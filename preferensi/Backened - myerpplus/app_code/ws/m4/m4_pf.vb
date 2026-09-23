Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports System.Net.Mail
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m4_pf
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M4_PfSimpan(ByVal param As String) As String
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
        'pfid(0) As Integer, pfcabang(1) As String, pflokasi(2) As String, pfgudang(3) As String, pfasalbarang(4) As String, 
        'pfasalbarangkategori(5) As Integer, pfjenispembelian(6) As String, pfjenispembeliankategori(7) As Integer, pfcarabayar(8) As Integer, pfsumber(9) As String, 
        'pfautonotransaksi(10) As Integer, pfnotransaksi(11) As String, pftgl(12) As Date, pfkodepa(13) As Integer, pfsupplier(14) As Integer, 
        'pfsupplierkontak(15) As String, pf1alamat1(16) As String, pf1alamat2(17) As String, pf1alamat3(18) As String, pf2alamat1(19) As String, 
        'pf2alamat2(20) As String, pf2alamat3(21) As String, pfbagianpembelian(22) As Integer, pftgldipenuhi(23) As Date, pftermin(24) As String, 
        'pftgljatuhtempo(25) As Date, pfuraian(26) As String, pfcatatan(27) As String, pfnoref(28) As String, pftglnoref(29) As Date, 
        'pftglpenutupan(30) As Date, pfmatauang(31) As String, pfkurs(32) As Double, pfhargatermasukpajak(33) As Integer, pftotal(34) As Double, 
        'pfdiskonpersen(35) As String, pfjmldiskon(36) As Double, pftotalpajak1detail(37) As Double, pftotalpajak2detail(38) As Double, pfbiayalainpersen(39) As String, 
        'pfbiayalain(40) As Double, pftotaltransaksi(41) As Double, pfjmlbayar(42) As Double, pfrekdiskon(43) As String, pfrekpajak1(44) As String, 
        'pfrekpajak2(45) As String, pfrekbiayalain(46) As String, pfrekbayar(47) As String, pfidpr(48) As Integer, pfidcs(49) As Integer, 
        'pfidrq(50) As Integer, pfidbs(51) As Integer, pfstatusipc(52) As Integer, pfstatusgrn(53) As Integer, pfstatusri(54) As Integer, 
        'pfstatusdnr(55) As Integer, pfstatusprt(56) As Integer, pfstatus(57) As Integer, pfstatussebelumnya(58) As Integer, pfjmlrevisi(59) As Integer, 
        'pfcetakanke(60) As Integer, pfinputuser(61) As Integer, pfinputtgl(62) As DateTime, pfmodifikasiuser(63) As Integer, pfmodifikasitgl(64) As DateTime, 
        'pfisclose(65) As Integer, pfcustomtext1(66) As String, pfcustomtext2(67) As String, pfcustomtext3(68) As String, pfcustomtext4(69) As String, 
        'pfcustomtext5(70) As String, pfcustomint1(71) As Integer, pfcustomint2(72) As Integer, pfcustomint3(73) As Integer, pfcustomdbl1(74) As Double, 
        'pfcustomdbl2(75) As Double, pfcustomdbl3(76) As Double, pfcustomdate1(77) As Date, pfcustomdate2(78) As Date, pfcustomdate3(79) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'pfid, pfcabang, pflokasi, pfgudang, pfasalbarang, pfasalbarangkategori, pfjenispembelian, 
        'pfjenispembeliankategori, pfcarabayar, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl, pfkodepa, 
        'pfsupplier, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, 
        'pf2alamat3, pfbagianpembelian, pftgldipenuhi, pftermin, pftgljatuhtempo, pfuraian, pfcatatan, 
        'pfnoref, pftglnoref, pftglpenutupan, pfmatauang, pfkurs, pfhargatermasukpajak, pftotal, 
        'pfdiskonpersen, pfjmldiskon, pftotalpajak1detail, pftotalpajak2detail, pfbiayalainpersen, pfbiayalain, pftotaltransaksi, 
        'pfjmlbayar, pfrekdiskon, pfrekpajak1, pfrekpajak2, pfrekbiayalain, pfrekbayar, pfidpr, 
        'pfidcs, pfidrq, pfidbs, pfstatusipc, pfstatusgrn, pfstatusri, pfstatusdnr, 
        'pfstatusprt, pfstatus, pfstatussebelumnya, pfjmlrevisi, pfcetakanke, pfinputuser, pfinputtgl, 
        'pfmodifikasiuser, pfmodifikasitgl, pfisclose, pfcustomtext1, pfcustomtext2, pfcustomtext3, pfcustomtext4, 
        'pfcustomtext5, pfcustomint1, pfcustomint2, pfcustomint3, pfcustomdbl1, pfcustomdbl2, pfcustomdbl3, 
        'pfcustomdate1, pfcustomdate2, pfcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 80) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'pfid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "pfid required numeric." : GoTo selesai
        End If
        'pfasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "pfasalbarangkategori required numeric." : GoTo selesai
        End If
        'pfjenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "pfjenispembeliankategori required numeric." : GoTo selesai
        End If
        'pfcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "pfcarabayar required numeric." : GoTo selesai
        End If
        'pfautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "pfautonotransaksi required numeric." : GoTo selesai
        End If
        'pftgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "pftgl required date." : GoTo selesai
        End If
        'pfkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "pfkodepa required numeric." : GoTo selesai
        End If
        'pfsupplier(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "pfsupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "pfsupplier can't be empty." : GoTo selesai
        End If
        'pfbagianpembelian(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "pfbagianpembelian required numeric." : GoTo selesai
        End If
        'pftgldipenuhi(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "pftgldipenuhi required date." : GoTo selesai
        End If
        'pftgljatuhtempo(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "pftgljatuhtempo required date." : GoTo selesai
        End If
        'pftglnoref(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "pftglnoref required date." : GoTo selesai
        End If
        'pftglpenutupan(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "pftglpenutupan required date." : GoTo selesai
        End If
        'pfkurs(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "pfkurs required numeric." : GoTo selesai
        End If
        'pfhargatermasukpajak(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "pfhargatermasukpajak required numeric." : GoTo selesai
        End If
        'pftotal(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "pftotal required numeric." : GoTo selesai
        End If
        'pfjmldiskon(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "pfjmldiskon required numeric." : GoTo selesai
        End If
        'pftotalpajak1detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "pftotalpajak1detail required numeric." : GoTo selesai
        End If
        'pftotalpajak2detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "pftotalpajak2detail required numeric." : GoTo selesai
        End If
        'pfbiayalain(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "pfbiayalain required numeric." : GoTo selesai
        End If
        'pftotaltransaksi(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "pftotaltransaksi required numeric." : GoTo selesai
        End If
        'pfjmlbayar(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "pfjmlbayar required numeric." : GoTo selesai
        End If
        'pfidpr(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "pfidpr required numeric." : GoTo selesai
        End If
        'pfidcs(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "pfidcs required numeric." : GoTo selesai
        End If
        'pfidrq(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "pfidrq required numeric." : GoTo selesai
        End If
        'pfidbs(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "pfidbs required numeric." : GoTo selesai
        End If
        'pfstatusipc(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "pfstatusipc required numeric." : GoTo selesai
        End If
        'pfstatusgrn(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "pfstatusgrn required numeric." : GoTo selesai
        End If
        'pfstatusri(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "pfstatusri required numeric." : GoTo selesai
        End If
        'pfstatusdnr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "pfstatusdnr required numeric." : GoTo selesai
        End If
        'pfstatusprt(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "pfstatusprt required numeric." : GoTo selesai
        End If
        'pfstatus(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "pfstatus required numeric." : GoTo selesai
        End If
        'pfstatussebelumnya(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "pfstatussebelumnya required numeric." : GoTo selesai
        End If
        'pfjmlrevisi(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "pfjmlrevisi required numeric." : GoTo selesai
        End If
        'pfcetakanke(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "pfcetakanke required numeric." : GoTo selesai
        End If
        'pfinputuser(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "pfinputuser required numeric." : GoTo selesai
        End If
        'pfinputtgl(62) As DateTime
        If (IsDate(dataUtama(62)) = False) Then
            result(2) = "pfinputtgl required date." : GoTo selesai
        End If
        'pfmodifikasiuser(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "pfmodifikasiuser required numeric." : GoTo selesai
        End If
        'pfmodifikasitgl(64) As DateTime
        If (IsDate(dataUtama(64)) = False) Then
            result(2) = "pfmodifikasitgl required date." : GoTo selesai
        End If
        'pfisclose(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "pfisclose required numeric." : GoTo selesai
        End If
        'pfcustomint1(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "pfcustomint1 required numeric." : GoTo selesai
        End If
        'pfcustomint2(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "pfcustomint2 required numeric." : GoTo selesai
        End If
        'pfcustomint3(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "pfcustomint3 required numeric." : GoTo selesai
        End If
        'pfcustomdbl1(74) As Double
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "pfcustomdbl1 required numeric." : GoTo selesai
        End If
        'pfcustomdbl2(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "pfcustomdbl2 required numeric." : GoTo selesai
        End If
        'pfcustomdbl3(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "pfcustomdbl3 required numeric." : GoTo selesai
        End If
        'pfcustomdate1(77) As Date
        If (IsDate(dataUtama(77)) = False) Then
            result(2) = "pfcustomdate1 required date." : GoTo selesai
        End If
        'pfcustomdate2(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "pfcustomdate2 required date." : GoTo selesai
        End If
        'pfcustomdate3(79) As Date
        If (IsDate(dataUtama(79)) = False) Then
            result(2) = "pfcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'pfcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "pfcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "pfcabang should not be more than 25 character." : GoTo selesai
        End If

        'pflokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "pflokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "pflokasi should not be more than 25 character." : GoTo selesai
        End If

        'pfgudang(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "pfgudang can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "pfgudang should not be more than 25 character." : GoTo selesai
        End If

        'pfsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "pfsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "pfsumber should not be more than 10 character." : GoTo selesai
        End If

        'pfnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "pfnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "pfnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'pftgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "pftgl can't be empty" : GoTo selesai
        End If

        'pftgldipenuhi(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "pftgldipenuhi can't be empty" : GoTo selesai
        End If

        'pftgljatuhtempo(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "pftgljatuhtempo can't be empty" : GoTo selesai
        End If

        'pftglnoref(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "pftglnoref can't be empty" : GoTo selesai
        End If

        'pftglpenutupan(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "pftglpenutupan can't be empty" : GoTo selesai
        End If

        'pfmatauang(31) As String
        If Len(dataUtama(31)) = 0 Then
            result(2) = "pfmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(31)) > 25 Then
            result(2) = "pfmatauang should not be more than 25 character." : GoTo selesai
        End If

        'pfkurs(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "pfkurs can't be empty" : GoTo selesai
        End If

        'pftotal(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "pftotal can't be empty" : GoTo selesai
        End If

        'pfdiskonpersen(35) As String
        If Len(dataUtama(35)) = 0 Then
            result(2) = "pfdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(35)) > 25 Then
            result(2) = "pfdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'pfjmldiskon(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "pfjmldiskon can't be empty" : GoTo selesai
        End If

        'pftotalpajak1detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "pftotalpajak1detail can't be empty" : GoTo selesai
        End If

        'pftotalpajak2detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "pftotalpajak2detail can't be empty" : GoTo selesai
        End If

        'pfbiayalainpersen(39) As String
        If Len(dataUtama(39)) = 0 Then
            result(2) = "pfbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(39)) > 25 Then
            result(2) = "pfbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'pfbiayalain(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "pfbiayalain can't be empty" : GoTo selesai
        End If

        'pftotaltransaksi(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "pftotaltransaksi can't be empty" : GoTo selesai
        End If

        'pfjmlbayar(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "pfjmlbayar can't be empty" : GoTo selesai
        End If

        'pfinputtgl(62) As DateTime
        If Len(dataUtama(62)) = 0 Then
            result(2) = "pfinputtgl can't be empty" : GoTo selesai
        End If

        'pfmodifikasitgl(64) As DateTime
        If Len(dataUtama(64)) = 0 Then
            result(2) = "pfmodifikasitgl can't be empty" : GoTo selesai
        End If

        'pfcustomdbl1(74) As Double
        If Len(dataUtama(74)) = 0 Then
            result(2) = "pfcustomdbl1 can't be empty" : GoTo selesai
        End If

        'pfcustomdbl2(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "pfcustomdbl2 can't be empty" : GoTo selesai
        End If

        'pfcustomdbl3(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "pfcustomdbl3 can't be empty" : GoTo selesai
        End If

        'pfcustomdate1(77) As Date
        If Len(dataUtama(77)) = 0 Then
            result(2) = "pfcustomdate1 can't be empty" : GoTo selesai
        End If

        'pfcustomdate2(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "pfcustomdate2 can't be empty" : GoTo selesai
        End If

        'pfcustomdate3(79) As Date
        If Len(dataUtama(79)) = 0 Then
            result(2) = "pfcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "pfid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pflokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfsupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pf1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pf1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pf1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pf2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pf2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pf2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfbagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pftgldipenuhi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfkurs", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "pfhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pftotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftotaltransaksi", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtutama, "pfjmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfrekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfidrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfidbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfstatusipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfstatusgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfstatusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfstatusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfstatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "pfid~pfcabang~pflokasi~pfgudang~pfasalbarang~pfasalbarangkategori~pfjenispembelian~pfjenispembeliankategori~pfcarabayar~pfsumber~pfautonotransaksi~pfnotransaksi~pftgl~pfkodepa~pfsupplier~pfsupplierkontak~pf1alamat1~pf1alamat2~pf1alamat3~pf2alamat1~pf2alamat2~pf2alamat3~pfbagianpembelian~pftgldipenuhi~pftermin~pftgljatuhtempo~pfuraian~pfcatatan~pfnoref~pftglnoref~pftglpenutupan~pfmatauang~pfkurs~pfhargatermasukpajak~pftotal~pfdiskonpersen~pfjmldiskon~pftotalpajak1detail~pftotalpajak2detail~pfbiayalainpersen~pfbiayalain~pftotaltransaksi~pfjmlbayar~pfrekdiskon~pfrekpajak1~pfrekpajak2~pfrekbiayalain~pfrekbayar~pfidpr~pfidcs~pfidrq~pfidbs~pfstatusipc~pfstatusgrn~pfstatusri~pfstatusdnr~pfstatusprt~pfstatus~pfstatussebelumnya~pfjmlrevisi~pfcetakanke~pfinputuser~pfinputtgl~pfmodifikasiuser~pfmodifikasitgl~pfisclose~pfcustomtext1~pfcustomtext2~pfcustomtext3~pfcustomtext4~pfcustomtext5~pfcustomint1~pfcustomint2~pfcustomint3~pfcustomdbl1~pfcustomdbl2~pfcustomdbl3~pfcustomdate1~pfcustomdate2~pfcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idpfdetail(0) As Integer, idpf(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
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
        'idpfdetail, idpf, idbarang, namabarang, tipebarang, jml, satuan, 
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
        AsDataTableTambahField(dtdetail, "idpfdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpf", AsEnumTypeData.AsInt64)
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
            'idpfdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idpfdetail required numeric." : GoTo selesai
            End If
            'idpf(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idpf required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idpfdetail~idpf~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargafix~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idprdetail~idcsdetail~idrqdetail~idbsdetail~jmlipc~statusipc~jmlgrn~statusgrn~jmlri~statusri~jmldnr~statusdnr~jmlprt~statusprt~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52)) = False Then
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
        'idpfcost(0) As Integer, idpf(1) As Integer, kodecost(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, rekdebit(6) As String, rekkredit(7) As String, kontak(8) As Integer, termasukhpp(9) As Integer, 
        'catatan(10) As String, costcenter(11) As String, divisi(12) As String, subdivisi(13) As String, proyek(14) As String, 
        'urutan(15) As Integer, idprcost(16) As Integer, idcscost(17) As Integer, idrqcost(18) As Integer, idbscost(19) As Integer, 
        'jumlahipc(20) As Double, statusipc(21) As Integer, jumlahgrn(22) As Double, statusgrn(23) As Integer, jumlahri(24) As Double, 
        'statusri(25) As Integer, jumlahbayar(26) As Double, statusbayar(27) As Integer, isclose(28) As Integer, customtext1(29) As String, 
        'customtext2(30) As String, customtext3(31) As String, customdbl1(32) As Double, customdbl2(33) As Double, customdbl3(34) As Double, 
        'customdate1(35) As Date, customdate2(36) As Date, customdate3(37) As Date

        'MAPPING BUAT FLEX DATA COST -----------------------------------------------------
        'idpfcost, idpf, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, 
        'proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, 
        'statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

        'Buat datatable cost
        Dim dtcost As New DataTable
        AsDataTableTambahField(dtcost, "idpfcost", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "idpf", AsEnumTypeData.AsInt64)
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
                'idpfcost(0) As Integer
                If (IsNumeric(dataRowCost(0)) = False) Then
                    result(2) = "Cost Row : " & i & " - idpfcost required numeric." : GoTo selesai
                End If
                'idpf(1) As Integer
                If (IsNumeric(dataRowCost(1)) = False) Then
                    result(2) = "Cost Row : " & i & " - idpf required numeric." : GoTo selesai
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

                If AsDataTableTambahData(dtcost, "idpfcost~idpf~kodecost~matauang~kurs~jumlah~rekdebit~rekkredit~kontak~termasukhpp~catatan~costcenter~divisi~subdivisi~proyek~urutan~idprcost~idcscost~idrqcost~idbscost~jumlahipc~statusipc~jumlahgrn~statusgrn~jumlahri~statusri~jumlahbayar~statusbayar~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowCost(0) & "~" & dataRowCost(1) & "~" & dataRowCost(2) & "~" & dataRowCost(3) & "~" & dataRowCost(4) & "~" & dataRowCost(5) & "~" & dataRowCost(6) & "~" & dataRowCost(7) & "~" & dataRowCost(8) & "~" & dataRowCost(9) & "~" & dataRowCost(10) & "~" & dataRowCost(11) & "~" & dataRowCost(12) & "~" & dataRowCost(13) & "~" & dataRowCost(14) & "~" & dataRowCost(15) & "~" & dataRowCost(16) & "~" & dataRowCost(17) & "~" & dataRowCost(18) & "~" & dataRowCost(19) & "~" & dataRowCost(20) & "~" & dataRowCost(21) & "~" & dataRowCost(22) & "~" & dataRowCost(23) & "~" & dataRowCost(24) & "~" & dataRowCost(25) & "~" & dataRowCost(26) & "~" & dataRowCost(27) & "~" & dataRowCost(28) & "~" & dataRowCost(29) & "~" & dataRowCost(30) & "~" & dataRowCost(31) & "~" & dataRowCost(32) & "~" & dataRowCost(33) & "~" & dataRowCost(34) & "~" & dataRowCost(35) & "~" & dataRowCost(36) & "~" & dataRowCost(37)) = False Then
                    result(2) = "Cost Row : " & i & " - insert into datatable failed." : GoTo selesai
                End If

            Next
            'END OF VALIDASI DAN SET ROW DATA COST ===========================================

        End If


        'MAPPING BUAT WS DATA TRANS -------------------------------------------------------
        'idpftrans(0) As Integer, idpf(1) As Integer, sumber(2) As String, idtransaksi(3) As Integer, catatan(4) As String, 
        'urutan(5) As Integer, isclose(6) As Integer, customtext1(7) As String, customtext2(8) As String, customtext3(9) As String, 
        'customtext4(10) As String, customtext5(11) As String, customdbl1(12) As Double, customdbl2(13) As Double, customdbl3(14) As Double, 
        'customdbl4(15) As Double, customdbl5(16) As Double, customdate1(17) As Date, customdate2(18) As Date, customdate3(19) As Date, 
        'customdate4(20) As Date, customdate5(21) As Date

        'MAPPING BUAT FLEX DATA TRANS -----------------------------------------------------
        'idpftrans, idpf, sumber, idtransaksi, catatan, urutan, isclose, 
        'customtext1, customtext2, customtext3, customtext4, customtext5, customdbl1, customdbl2, 
        'customdbl3, customdbl4, customdbl5, customdate1, customdate2, customdate3, customdate4, 
        'customdate5

        'Buat datatable trans
        Dim dttrans As New DataTable
        AsDataTableTambahField(dttrans, "idpftrans", AsEnumTypeData.AsString)
        AsDataTableTambahField(dttrans, "idpf", AsEnumTypeData.AsInt64)
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
                    'idpftrans(0) As Integer
                    If Len(dataRowTrans(0)) = 0 Then
                        result(2) = "Trans Row : " & i & " - idpftrans can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowTrans(0)) > 20 Then
                        result(2) = "Trans Row : " & i & " - idpftrans should not be more than 20 character." : GoTo selesai
                    End If

                    'idpf(1) As Integer
                    If Len(dataRowTrans(1)) = 0 Then
                        result(2) = "Trans Row : " & i & " - idpf can't be empty" : GoTo selesai
                    End If
                    If Len(dataRowTrans(1)) > 20 Then
                        result(2) = "Trans Row : " & i & " - idpf should not be more than 20 character." : GoTo selesai
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

                    If AsDataTableTambahData(dttrans, "idpftrans~idpf~sumber~idtransaksi~catatan~urutan~isclose~customtext1~customtext2~customtext3~customtext4~customtext5~customdbl1~customdbl2~customdbl3~customdbl4~customdbl5~customdate1~customdate2~customdate3~customdate4~customdate5", dataRowTrans(0) & "~" & dataRowTrans(1) & "~" & dataRowTrans(2) & "~" & dataRowTrans(3) & "~" & dataRowTrans(4) & "~" & dataRowTrans(5) & "~" & dataRowTrans(6) & "~" & dataRowTrans(7) & "~" & dataRowTrans(8) & "~" & dataRowTrans(9) & "~" & dataRowTrans(10) & "~" & dataRowTrans(11) & "~" & dataRowTrans(12) & "~" & dataRowTrans(13) & "~" & dataRowTrans(14) & "~" & dataRowTrans(15) & "~" & dataRowTrans(16) & "~" & dataRowTrans(17) & "~" & dataRowTrans(18) & "~" & dataRowTrans(19) & "~" & dataRowTrans(20) & "~" & dataRowTrans(21)) = False Then
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
                Select Case drutama("pfstatus")
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


                'HAK AKSES CUSTOM TOTAL TRANSAKSI PF ====================
                If drutama("pfstatus") = 2 Then
                    Dim validasiTotal As String = 0
                    Dim dtSetting As DataTable = AsDataTableAmbilDariDBCon("SELECT snilai FROM m0_setting WHERE (smodule='4') AND (sgrup='options') AND (skode='ValidasiTotalPF')", myConn)
                    If dtSetting.Rows.Count > 0 Then
                        validasiTotal = dtSetting.Rows(0)(0)
                    End If

                    If validasiTotal = 1 Then
                        sql = "SELECT IFNULL(rc.rcmoduleid,0) as rcmoduleid, IFNULL(rc.rcidpc,0) as rcidpc, IFNULL(rc.rcrole,'') as rcrole, IFNULL(rc.rcakses,0) as rcakses, pc.pckode, pc.pcnama FROM m0_permissions_custom pc LEFT JOIN m0_role_custom rc ON pc.pcmodule = rc.rcmoduleid AND pc.pcid = rc.rcidpc LEFT JOIN m0_user_role ur ON rc.rcrole = ur.role AND ur.userid = '" & userid & "' WHERE pc.pcmodule = 4 AND pc.pcid IN (8,9,10) HAVING rcakses = 0 ORDER BY pc.pcmodule, pc.pcid"
                        Dim dtHACustom As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                        If dtHACustom.Rows.Count > 0 Then
                            Dim dtVal As New DataTable
                            For Each dr1 As DataRow In dtHACustom.Rows
                                dtVal = AsDataTableFilterSortDt(dtutama, dr1("pckode"))
                                If dtVal.Rows.Count > 0 Then
                                    result(2) = "This role doesn't have permission to Approved : " & dr1("pcnama") : Trans.Rollback() : GoTo selesai
                                End If
                            Next
                        End If
                    End If
                End If
                'END OF HAK AKSES CUSTOM TOTAL TRANSAKSI PF =============


                ''CEK PERIODE AKUNTANSI ==================================
                'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(Asformattanggal(drutama("pftgl")), Asformattanggal(drutama("pftgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                If drutama("pfstatus") = 2 Or drutama("pfstatus") = 1 Or drutama("pfstatus") = 8 Or drutama("pfstatus") = 9 Or drutama("pfstatus") = 10 Or drutama("pfstatus") = 11 Then
                    'CEK HAK AKSES
                    '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
                    '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid

                    'Dim rsCekHakAkses As String = HakAkses(4, 7, 8, userid) 'MODULEID, MENUID, INDEKS AKSES, USERID SESUAI TRANSAKSI
                    'If Len(rsCekHakAkses) <> 0 Then result(2) = rsCekHakAkses : Trans.Rollback() : GoTo selesai

                    'ValidasiSimpan
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstandingPR, ftOutstandingPR, ftExistOutstandingRQ, ftOutstandingRQ, ftRQ, drutama("pfhargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH tempo ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("pftermin").ToString, Asformattanggal(drutama("pftgl")), "pftgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("pftgljatuhtempo") = Asformattanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH tempo =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("pftotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("pftotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("pftotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("pfhargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("pftotaltransaksi") = Double.Parse(drutama("pftotal")) - Double.Parse(drutama("pfjmldiskon")) + Double.Parse(drutama("pftotalpajak1detail")) + Double.Parse(drutama("pftotalpajak2detail")) + Double.Parse(drutama("pfbiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("pftotaltransaksi") = Double.Parse(drutama("pftotal")) - Double.Parse(drutama("pfjmldiskon")) + Double.Parse(drutama("pftotalpajak2detail")) + Double.Parse(drutama("pfbiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("pfid")
                    notransaksi = drutama("pfnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(pfid), pfnotransaksi FROM M4_pf WHERE pfid='" & result(4) & "' AND pfstatus NOT IN(2,3,4,7)", myConn)
                    'dtupdate = AsDataTableAmbilDariDBCon("SELECT COUNT(pfid), pfnotransaksi FROM M4_pf WHERE pfid='" & result(4) & "'", myConn)
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        If drutama("pfautonotransaksi") = 1 And notransaksi = "Auto" Then

                            'GENERATE NOTRANSAKSI =========================================
                            Dim wsM0_Nomor As New m0_nomor
                            Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("pfcabang"), drutama("pflokasi"), drutama("pfsumber"), drutama("pftgl"), drutama("pfsumber"), 4)
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
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(pfid) FROM m4_pf WHERE pfnotransaksi='" & notransaksi & "'", myConn)
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        ''SIMPAN HISTORY ========================
                        'Dim SimpanHistory As New m4_pf_history
                        'Dim rsSimpanHistory As String = SimpanHistory.M4_Pf_HistorySimpan("" & paramSplit(0) & "?M4_Pf_HistorySimpan?0?0???dd/MM/yyyy?dd/MM/yyyy H:mms?" & paramSplit(3) & "?0?" & FixQuotes(drutama("pfsumber")) & "?" & FixQuotes(drutama("pfid")) & "")
                        'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        'If (rsSplitResult(1) = 0) Then
                        '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        'End If
                        ''END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Pf set pfcabang  = '" & FixQuotes(drutama("pfcabang")) & "', pflokasi  = '" & FixQuotes(drutama("pflokasi")) & "', pfgudang  = '" & FixQuotes(drutama("pfgudang")) & "', pfasalbarang  = '" & FixQuotes(drutama("pfasalbarang")) & "', pfasalbarangkategori  = " & drutama("pfasalbarangkategori") & ", pfjenispembelian  = '" & FixQuotes(drutama("pfjenispembelian")) & "', pfjenispembeliankategori  = " & drutama("pfjenispembeliankategori") & ", pfcarabayar  = " & drutama("pfcarabayar") & ", pfsumber  = '" & FixQuotes(drutama("pfsumber")) & "', pfautonotransaksi  = " & drutama("pfautonotransaksi") & ", pfnotransaksi  = '" & notransaksi & "', pftgl  = '" & FixQuotes(AsFormatTanggal(drutama("pftgl"))) & "', pfkodepa  = " & drutama("pfkodepa") & ", pfsupplier  = " & drutama("pfsupplier") & ", pfsupplierkontak  = '" & FixQuotes(drutama("pfsupplierkontak")) & "', pf1alamat1  = '" & FixQuotes(drutama("pf1alamat1")) & "', pf1alamat2  = '" & FixQuotes(drutama("pf1alamat2")) & "', pf1alamat3  = '" & FixQuotes(drutama("pf1alamat3")) & "', pf2alamat1  = '" & FixQuotes(drutama("pf2alamat1")) & "', pf2alamat2  = '" & FixQuotes(drutama("pf2alamat2")) & "', pf2alamat3  = '" & FixQuotes(drutama("pf2alamat3")) & "', pfbagianpembelian  = " & drutama("pfbagianpembelian") & ", pftgldipenuhi  = '" & FixQuotes(AsFormatTanggal(drutama("pftgldipenuhi"))) & "', pftermin  = '" & FixQuotes(drutama("pftermin")) & "', pftgljatuhtempo  = '" & FixQuotes(AsFormatTanggal(drutama("pftgljatuhtempo"))) & "', pfuraian  = '" & FixQuotes(drutama("pfuraian")) & "', pfcatatan  = '" & FixQuotes(drutama("pfcatatan")) & "', pfnoref  = '" & FixQuotes(drutama("pfnoref")) & "', pftglnoref  = '" & FixQuotes(AsFormatTanggal(drutama("pftglnoref"))) & "', pftglpenutupan  = '" & FixQuotes(AsFormatTanggal(drutama("pftglpenutupan"))) & "', pfmatauang  = '" & FixQuotes(drutama("pfmatauang")) & "', pfkurs  = '" & FixDouble(drutama("pfkurs")) & "', pfhargatermasukpajak  = " & drutama("pfhargatermasukpajak") & ", pftotal  = '" & FixDouble(drutama("pftotal")) & "', pfdiskonpersen  = '" & FixQuotes(drutama("pfdiskonpersen")) & "', pfjmldiskon  = '" & FixDouble(drutama("pfjmldiskon")) & "', pftotalpajak1detail  = '" & FixDouble(drutama("pftotalpajak1detail")) & "', pftotalpajak2detail  = '" & FixDouble(drutama("pftotalpajak2detail")) & "', pfbiayalainpersen  = '" & FixQuotes(drutama("pfbiayalainpersen")) & "', pfbiayalain  = '" & FixDouble(drutama("pfbiayalain")) & "', pftotaltransaksi  = '" & FixDouble(drutama("pftotaltransaksi")) & "', pfjmlbayar  = '" & FixDouble(drutama("pfjmlbayar")) & "', pfrekdiskon  = '" & FixQuotes(drutama("pfrekdiskon")) & "', pfrekpajak1  = '" & FixQuotes(drutama("pfrekpajak1")) & "', pfrekpajak2  = '" & FixQuotes(drutama("pfrekpajak2")) & "', pfrekbiayalain  = '" & FixQuotes(drutama("pfrekbiayalain")) & "', pfrekbayar  = '" & FixQuotes(drutama("pfrekbayar")) & "', pfidpr  = " & drutama("pfidpr") & ", pfidcs  = " & drutama("pfidcs") & ", pfidrq  = " & drutama("pfidrq") & ", pfidbs  = " & drutama("pfidbs") & ", pfstatusipc  = " & drutama("pfstatusipc") & ", pfstatusgrn  = " & drutama("pfstatusgrn") & ", pfstatusri  = " & drutama("pfstatusri") & ", pfstatusdnr  = " & drutama("pfstatusdnr") & ", pfstatusprt  = " & drutama("pfstatusprt") & ", pfstatus  = " & drutama("pfstatus") & ", pfstatussebelumnya  = " & drutama("pfstatussebelumnya") & ", pfjmlrevisi  = pfjmlrevisi+1, pfcetakanke  = " & drutama("pfcetakanke") & ", pfmodifikasiuser  = " & drutama("pfmodifikasiuser") & ", pfmodifikasitgl  = NOW(), pfcustomtext1  = '" & FixQuotes(drutama("pfcustomtext1")) & "', pfcustomtext2  = '" & FixQuotes(drutama("pfcustomtext2")) & "', pfcustomtext3  = '" & FixQuotes(drutama("pfcustomtext3")) & "', pfcustomtext4  = '" & FixQuotes(drutama("pfcustomtext4")) & "', pfcustomtext5  = '" & FixQuotes(drutama("pfcustomtext5")) & "', pfcustomint1  = " & drutama("pfcustomint1") & ", pfcustomint2  = " & drutama("pfcustomint2") & ", pfcustomint3  = " & drutama("pfcustomint3") & ", pfcustomdbl1  = '" & FixDouble(drutama("pfcustomdbl1")) & "', pfcustomdbl2  = '" & FixDouble(drutama("pfcustomdbl2")) & "', pfcustomdbl3  = '" & FixDouble(drutama("pfcustomdbl3")) & "', pfcustomdate1  = '" & FixQuotes(AsFormatTanggal(drutama("pfcustomdate1"))) & "', pfcustomdate2  = '" & FixQuotes(AsFormatTanggal(drutama("pfcustomdate2"))) & "', pfcustomdate3  = '" & FixQuotes(AsFormatTanggal(drutama("pfcustomdate3"))) & "' where pfid = '" & drutama("pfid") & "'"
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

                    If drutama("pfautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("pfcabang"), drutama("pflokasi"), drutama("pfsumber"), drutama("pftgl"), drutama("pfsumber"), 4)
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
                        notransaksi = drutama("pfnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDBCon("SELECT COUNT(pfid) FROM m4_pf WHERE pfnotransaksi='" & notransaksi & "'", myConn)
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Pf (pfcabang, pflokasi, pfgudang, pfasalbarang, pfasalbarangkategori, pfjenispembelian, pfjenispembeliankategori, pfcarabayar, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl, pfkodepa, pfsupplier, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, pf2alamat3, pfbagianpembelian, pftgldipenuhi, pftermin, pftgljatuhtempo, pfuraian, pfcatatan, pfnoref, pftglnoref, pftglpenutupan, pfmatauang, pfkurs, pfhargatermasukpajak, pftotal, pfdiskonpersen, pfjmldiskon, pftotalpajak1detail, pftotalpajak2detail, pfbiayalainpersen, pfbiayalain, pftotaltransaksi, pfjmlbayar, pfrekdiskon, pfrekpajak1, pfrekpajak2, pfrekbiayalain, pfrekbayar, pfidpr, pfidcs, pfidrq, pfidbs, pfstatusipc, pfstatusgrn, pfstatusri, pfstatusdnr, pfstatusprt, pfstatus, pfstatussebelumnya, pfjmlrevisi, pfcetakanke, pfinputuser, pfinputtgl, pfmodifikasiuser, pfmodifikasitgl, pfisclose, pfcustomtext1, pfcustomtext2, pfcustomtext3, pfcustomtext4, pfcustomtext5, pfcustomint1, pfcustomint2, pfcustomint3, pfcustomdbl1, pfcustomdbl2, pfcustomdbl3, pfcustomdate1, pfcustomdate2, pfcustomdate3) values('" & FixQuotes(drutama("pfcabang")) & "', '" & FixQuotes(drutama("pflokasi")) & "', '" & FixQuotes(drutama("pfgudang")) & "', '" & FixQuotes(drutama("pfasalbarang")) & "', " & drutama("pfasalbarangkategori") & ", '" & FixQuotes(drutama("pfjenispembelian")) & "', " & drutama("pfjenispembeliankategori") & ", " & drutama("pfcarabayar") & ", '" & FixQuotes(drutama("pfsumber")) & "', " & drutama("pfautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(Asformattanggal(drutama("pftgl"))) & "', " & drutama("pfkodepa") & ", " & drutama("pfsupplier") & ", '" & FixQuotes(drutama("pfsupplierkontak")) & "', '" & FixQuotes(drutama("pf1alamat1")) & "', '" & FixQuotes(drutama("pf1alamat2")) & "', '" & FixQuotes(drutama("pf1alamat3")) & "', '" & FixQuotes(drutama("pf2alamat1")) & "', '" & FixQuotes(drutama("pf2alamat2")) & "', '" & FixQuotes(drutama("pf2alamat3")) & "', " & drutama("pfbagianpembelian") & ", '" & FixQuotes(Asformattanggal(drutama("pftgldipenuhi"))) & "', '" & FixQuotes(drutama("pftermin")) & "', '" & FixQuotes(Asformattanggal(drutama("pftgljatuhtempo"))) & "', '" & FixQuotes(drutama("pfuraian")) & "', '" & FixQuotes(drutama("pfcatatan")) & "', '" & FixQuotes(drutama("pfnoref")) & "', '" & FixQuotes(Asformattanggal(drutama("pftglnoref"))) & "', '" & FixQuotes(Asformattanggal(drutama("pftglpenutupan"))) & "', '" & FixQuotes(drutama("pfmatauang")) & "', '" & FixDouble(drutama("pfkurs")) & "', " & drutama("pfhargatermasukpajak") & ", '" & FixDouble(drutama("pftotal")) & "', '" & FixQuotes(drutama("pfdiskonpersen")) & "', '" & FixDouble(drutama("pfjmldiskon")) & "', '" & FixDouble(drutama("pftotalpajak1detail")) & "', '" & FixDouble(drutama("pftotalpajak2detail")) & "', '" & FixQuotes(drutama("pfbiayalainpersen")) & "', '" & FixDouble(drutama("pfbiayalain")) & "', '" & FixDouble(drutama("pftotaltransaksi")) & "', '" & FixDouble(drutama("pfjmlbayar")) & "', '" & FixQuotes(drutama("pfrekdiskon")) & "', '" & FixQuotes(drutama("pfrekpajak1")) & "', '" & FixQuotes(drutama("pfrekpajak2")) & "', '" & FixQuotes(drutama("pfrekbiayalain")) & "', '" & FixQuotes(drutama("pfrekbayar")) & "', " & drutama("pfidpr") & ", " & drutama("pfidcs") & ", " & drutama("pfidrq") & ", " & drutama("pfidbs") & ", " & drutama("pfstatusipc") & ", " & drutama("pfstatusgrn") & ", " & drutama("pfstatusri") & ", " & drutama("pfstatusdnr") & ", " & drutama("pfstatusprt") & ", " & drutama("pfstatus") & ", " & drutama("pfstatussebelumnya") & ", " & drutama("pfjmlrevisi") & ", " & drutama("pfcetakanke") & ", " & drutama("pfinputuser") & ", NOW(), " & drutama("pfmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("pfisclose") & ", '" & FixQuotes(drutama("pfcustomtext1")) & "', '" & FixQuotes(drutama("pfcustomtext2")) & "', '" & FixQuotes(drutama("pfcustomtext3")) & "', '" & FixQuotes(drutama("pfcustomtext4")) & "', '" & FixQuotes(drutama("pfcustomtext5")) & "', " & drutama("pfcustomint1") & ", " & drutama("pfcustomint2") & ", " & drutama("pfcustomint3") & ", '" & FixDouble(drutama("pfcustomdbl1")) & "', '" & FixDouble(drutama("pfcustomdbl2")) & "', '" & FixDouble(drutama("pfcustomdbl3")) & "', '" & FixQuotes(Asformattanggal(drutama("pfcustomdate1"))) & "', '" & FixQuotes(Asformattanggal(drutama("pfcustomdate2"))) & "', '" & FixQuotes(Asformattanggal(drutama("pfcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDBCon("select pfid from M4_pf where pfnotransaksi='" & notransaksi & "' AND pfinputuser= '" & userid & "' order by pfmodifikasitgl desc limit 1", myConn)
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Pf_Detail where idpf = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idpfdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("hargafix") & ", '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idprdetail") & ", " & dr1("idcsdetail") & ", " & dr1("idrqdetail") & ", " & dr1("idbsdetail") & ", '" & FixDouble(dr1("jmlipc")) & "', " & dr1("statusipc") & ", '" & FixDouble(dr1("jmlgrn")) & "', " & dr1("statusgrn") & ", '" & FixDouble(dr1("jmlri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jmldnr")) & "', " & dr1("statusdnr") & ", '" & FixDouble(dr1("jmlprt")) & "', " & dr1("statusprt") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(Asformattanggal(dr1("customdate1"))) & "', '" & FixQuotes(Asformattanggal(dr1("customdate2"))) & "', '" & FixQuotes(Asformattanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Pf_Detail(idpfdetail, idpf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M4_Pf_Cost where idpf = " & result(4)
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
                        strValue2.Append("(" & dr1("idpfcost") & ", " & result(4) & ", '" & FixQuotes(dr1("kodecost")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixQuotes(dr1("rekdebit")) & "', '" & FixQuotes(dr1("rekkredit")) & "', " & dr1("kontak") & ", " & dr1("termasukhpp") & ", '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("idprcost") & ", " & dr1("idcscost") & ", " & dr1("idrqcost") & ", " & dr1("idbscost") & ", '" & FixDouble(dr1("jumlahipc")) & "', " & dr1("statusipc") & ", '" & FixDouble(dr1("jumlahgrn")) & "', " & dr1("statusgrn") & ", '" & FixDouble(dr1("jumlahri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jumlahbayar")) & "', " & dr1("statusbayar") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(Asformattanggal(dr1("customdate1"))) & "', '" & FixQuotes(Asformattanggal(dr1("customdate2"))) & "', '" & FixQuotes(Asformattanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Pf_Cost(idpfcost, idpf, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M4_pf_Trans where idpf = '" & result(4) & "'"
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
                        strValue2.Append("('" & FixQuotes(dr1("idpftrans")) & "', " & result(4) & ", '" & FixQuotes(dr1("sumber")) & "', '" & FixQuotes(dr1("idtransaksi")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixQuotes(dr1("customtext4")) & "', '" & FixQuotes(dr1("customtext5")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixDouble(dr1("customdbl4")) & "', '" & FixDouble(dr1("customdbl5")) & "', '" & FixQuotes(Asformattanggal(dr1("customdate1"))) & "', '" & FixQuotes(Asformattanggal(dr1("customdate2"))) & "', '" & FixQuotes(Asformattanggal(dr1("customdate3"))) & "', '" & FixQuotes(Asformattanggal(dr1("customdate4"))) & "', '" & FixQuotes(Asformattanggal(dr1("customdate5"))) & "')")
                    Next
                    sql = "Insert into M4_pf_Trans(idpftrans, idpf, sumber, idtransaksi, catatan, urutan, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdate1, customdate2, customdate3, customdate4, customdate5) values" & strValue2.ToString & ""
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
                If drutama("pfstatus") = 2 Then
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


                End If
                'END OF UPDATE OUTSTANDING TRANSAKSI ================================================

                'Dim dtNotifikasi As DataTable = AsDataTableAmbilDariDBCon("SELECT * FROM m0_notifikasi_email WHERE moduleid = 4 AND menuid = 7 AND userid = " & drutama("pfinputuser"), myConn)
                'If dtNotifikasi.Rows.Count > 0 Then
                '    'For Each dr1 As DataRow In dtNotifikasi.Rows
                '    '    ftDetaill = IIf(Len(ftDetaill.ToString) = 0, "", ftDetaill & " OR ")
                '    '    ftDetaill = String.Concat(ftDetaill, "(idsq = '" & dr1("idsq") & "')")
                '    'Next
                '    If dtNotifikasi.Rows(0)(3) = drutama("pfstatus") Then
                '        Dim fromAddress = New MailAddress(dtNotifikasi.Rows(0)(7).ToString, "Notifikasi PF")
                '        Dim toAddress = New MailAddress(dtNotifikasi.Rows(0)(6).ToString, "Notifikasi PF")
                '        Dim fromPassword As String = dtNotifikasi.Rows(0)(8).ToString
                '        Const subject As String = "PF Status"
                '        Const body As String = "PF Status"

                '        Dim smtp = New SmtpClient()
                '        smtp.Host = "smtp.gmail.com"
                '        smtp.Pfrt = 587
                '        smtp.EnableSsl = True
                '        smtp.DeliveryMethod = SmtpDeliveryMethod.Network
                '        smtp.UseDefaultCredentials = True
                '        smtp.Credentials = New System.Net.NetworkCredential(fromAddress.Address, fromPassword)

                '        Using message = New MailMessage(fromAddress, toAddress)
                '            message.Subject = subject
                '            message.Body = body
                '            smtp.Send(message)
                '        End Using
                '    End If
                'End If

                'INSERT USER LOG ====================================================================
                Dim sumber As String = "PF", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M4_PfCloseItem(ByVal param As String) As String
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
        myConn = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        myConn.Open()

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataSplit(), dataDetail(), dataRowDetail() As String

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
        If (dataSplit.Length <> 1) Then
            result(2) = "Invalid transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================

       


        'MAPPING BUAT FLEX DATA DETAIL -----------------------------------------------------
        'idpfdetail, customdbl2, 

        'VALIDASI DAN SET DATA DETAIL ======================================================
        'SPLIT PARAMETER DATA DETAIL
        dataDetail = dataSplit(0).Split(sptRow)
        'END OF VALIDASI DAN SET DATA DETAIL ===============================================

        'Buat datatable detail
        Dim dtdetail As New DataTable
        AsDataTableTambahField(dtdetail, "idpfdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "customdbl2", AsEnumTypeData.AsString)
        

        'VALIDASI DAN SET DATA ROW DETAIL ==================================================
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

            'VALIDASI TIPE DATA DETAIL ------------------------------------------
            'idpfdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idpfdetail required numeric." : GoTo selesai
            End If
            
            'customdbl2(48) As Double
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - customdbl2 required numeric." : GoTo selesai
            End If
            
            'END OF VALIDASI TIPE DATA DETAIL -----------------------------------

            'VALIDASI DATA DETAIL ---------------------------------------
            If Len(dataRowDetail(0)) = 0 Then
                result(2) = "Row : " & i & " - idpfdetail can't be empty" : GoTo selesai
            End If
            'customdbl2(48) As Double
            If Len(dataRowDetail(1)) = 0 Then
                result(2) = "Row : " & i & " - customdbl2 can't be empty" : GoTo selesai
            End If

            
            'END OF VALIDASI DATA DETAIL --------------------------------

            If AsDataTableTambahData(dtdetail, "idpfdetail~customdbl2", dataRowDetail(0) & "~" & dataRowDetail(1)) = False Then
                result(2) = "Row : " & i & " - insert into datatable failed." : GoTo selesai
            End If
        Next
        'END OF VALIDASI DAN SET ROW DATA DETAIL ===========================================


        'SIMPAN KE DATABASE =================================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Dim dtupdate As New DataTable
        Dim rowUpdate As Integer = 0

        Try
            'Proses utama
            If (dtdetail.Rows.Count > 0) Then
                'Proses detail
                If (dtdetail.Rows.Count > 0) Then
                    Dim strValue2 As New StringBuilder
                    For Each dr1 As DataRow In dtdetail.Rows
                        strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", "; "))
                        strValue2.Append("update m4_pf_detail set customdbl2 = " & dr1("customdbl2") & " where idpfdetail = " & dr1("idpfdetail") & "")
                    Next
                    sql = strValue2.ToString
                    'result(2) = sql : GoTo selesai
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






                'INSERT USER LOG ====================================================================
                Dim sumber As String = "PF", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M4_PfUpdateStatus(ByVal param As String) As String

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
            Filter = Filter.Replace("pfsupplierkode", "c1.kkode")
            Filter = Filter.Replace("pfsuppliernama", "c1.knama")
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
            Dim sumber As String = "Pf", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Pftgl, Pfnotransaksi, Pfstatus FROM M4_Pf WHERE Pfid='" & idtransaksi & "'", myConn)
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Pfstatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            ''CEK PERIODE AKUNTANSI ==============================================================
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(Asformattanggal(tglTransaksi), Asformattanggal(tglTransaksi))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            ''END OF CEK PERIODE AKUNTANSI =======================================================

            'SIMPAN HISTORY ========================
            'Dim SimpanHistory As New m4_pf_history
            'Dim rsSimpanHistory As String = SimpanHistory.M4_Pf_HistorySimpan("" & paramSplit(0) & "?M4_Pf_HistorySimpan?0?0???dd/MM/yyyy?dd/MM/yyyy H:mms?" & paramSplit(3) & "?0?" & FixQuotes(sumber) & "?" & FixQuotes(idtransaksi) & "")
            'Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
            'Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
            ''JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
            'If (rsSplitResult(1) = 0) Then
            '    result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
            'End If
            'END OF SIMPAN HISTORY ==================

            If isDelete Then
                'CEK TERKAIT ====================================================================
                'PANGGIL QUERY TERKAIT
                Dim query As New m0_query
                sql = query.PanggilQuery("m4_pf_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idprdetail As Integer = 0, idrqdetail As Integer = 0
                Dim updNilaiPR As String = "", updFilterPR As String = "", updNilaiRQ As String = "", updFilterRQ As String = ""
                Dim gudang As String = "", updStokBooking As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDBCon("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, gudang, idprdetail, idrqdetail, urutan FROM m4_pf_detail WHERE idpf = '" & idtransaksi & "'", myConn)
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


            End If

            'update status utama
            sql = "UPDATE M4_Pf SET Pfstatus = " & nilaiStatus & ", Pfmodifikasiuser='" & userid & "', Pfmodifikasitgl = NOW(), pfposting = 0, pfpostingtgl = '1971-01-01 00:00:00', Pfjmlrevisi = Pfjmlrevisi + 1 WHERE Pfid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PfSearch(PostWsSearch(paramSplit(0), "M4_PfSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PfDelete(ByVal param As String) As String

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
            Filter = Filter.Replace("pfsupplierkode", "c1.kkode")
            Filter = Filter.Replace("pfsuppliernama", "c1.knama")
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
            Dim sumber As String = "Pf", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDBCon("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Pfid, Pfnotransaksi FROM M4_Pf WHERE Pfid='" & idtransaksi & "'", myConn)
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT pfcabang, pflokasi, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl"
            sql &= " FROM M4_pf"
            sql &= " WHERE pfid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDBCon(sql, myConn)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("pfcabang")
                lokasi = dtNomorNext.Rows(0)("pflokasi")
                sumber = dtNomorNext.Rows(0)("pfsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("pfautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("pfnotransaksi")
                tgl = Asformattanggal(dtNomorNext.Rows(0)("pftgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================

            'DELETE TRANS
            sql = "DELETE FROM M4_pf_Trans WHERE idpf ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE COST
            sql = "DELETE FROM M4_pf_Cost WHERE idpf ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M4_Pf_Detail WHERE idpf = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Pf WHERE pfid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PfSearch(PostWsSearch(paramSplit(0), "M4_PfSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PfGetdataById(ByVal param As String) As String

        'M4_PfGetdataById Utama --------------------------------------------------------
        'pfid, pfcabang, pflokasi, pfgudang, pfasalbarang, pfasalbarangkategori, pfjenispembelian, 
        'pfjenispembeliankategori, pfcarabayar, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl, pfkodepa, 
        'pfsupplier, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, 
        'pf2alamat3, pfbagianpembelian, pftgldipenuhi, pftermin, pftgljatuhtempo, pfuraian, pfcatatan, 
        'pfnoref, pftglnoref, pftglpenutupan, pfmatauang, pfkurs, pfhargatermasukpajak, pftotal, 
        'pfdiskonpersen, pfjmldiskon, pftotalpajak1detail, pftotalpajak2detail, pfbiayalainpersen, pfbiayalain, pftotaltransaksi, 
        'pfjmlbayar, pfrekdiskon, pfrekpajak1, pfrekpajak2, pfrekbiayalain, pfrekbayar, pfidpr, 
        'pfidcs, pfidrq, pfidbs, pfstatusipc, pfstatusgrn, pfstatusri, pfstatusdnr, 
        'pfstatusprt, pfstatusrealisasi, pfstatus, pfstatussebelumnya, pfjmlrevisi, pfcetakanke, pfinputuser, 
        'pfinputtgl, pfmodifikasiuser, pfmodifikasitgl, pfposting, pfpostingtgl, pfisclose, pfcustomtext1, 
        'pfcustomtext2, pfcustomtext3, pfcustomtext4, pfcustomtext5, pfcustomint1, pfcustomint2, pfcustomint3, 
        'pfcustomdbl1, pfcustomdbl2, pfcustomdbl3, pfcustomdate1, pfcustomdate2, pfcustomdate3, pfcabangnama, 
        'pflokasinama, pfgudangnama, pfsupplierkode, pfsuppliernama, pfbagianpembeliankode, pfbagianpembeliannama, pfterminnama, 
        'pfterminharijatuhtempo, pfrekdiskonnama, pfrekpajak1nama, pfrekpajak2nama, pfrekbiayalainnama, pfrekbayarnama, pfnotransaksipr, 
        'pfnotransaksics, pfnotransaksirq, pfnotransaksibs, pfstatusnama, pfstatussebelumnyanama, pfinputusernama, pfmodifikasiusernama , kpkp

        'M4_PfGetdataById Detail -------------------------------------------------------
        'idpfdetail, idpf, idbarang, namabarang, tipebarang, jml, satuan, 
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

        'M4_PfGetdataById Cost -------------------------------------------------------
        'idpfcost, idpf, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, 
        'proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, 
        'statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, 
        'kontaknama, costcenternama, divisinama, subdivisinama

        'M4_PfGetdataById Trans -------------------------------------------------------
        'idpftrans, idpf, sumber, idtransaksi, catatan, urutan, isclose, customtext1, 
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

        Dim NmMemcached As String = "aplikasi1-M4_Pf~M4_Pf_Detail-" & idtransaksi

        'Replace disesuaikan dengan kebutuhan
        'If (pagingSplit(2).Length > 0) Then
        '    Filter = pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If

        ' set filter
        If Len(pagingSplit(2)) = 0 Then ' jika filter tidak diisi
            ' filter id
            Filter = "pfid = " & idtransaksi
        Else ' jika filter diisi
            Filter = "pfid = " & idtransaksi & " and " & pagingSplit(2)
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        'sql = query.PanggilQuery("m4_pf_getdata")
        sql = "select `pf`.`pfid` AS `pfid`,`pf`.`pfcabang` AS `pfcabang`,`pf`.`pflokasi` AS `pflokasi`,`pf`.`pfgudang` AS `pfgudang`,`pf`.`pfasalbarang` AS `pfasalbarang`,`pf`.`pfasalbarangkategori` AS `pfasalbarangkategori`,`pf`.`pfjenispembelian` AS `pfjenispembelian`,`pf`.`pfjenispembeliankategori` AS `pfjenispembeliankategori`,`pf`.`pfcarabayar` AS `pfcarabayar`,`pf`.`pfsumber` AS `pfsumber`,`pf`.`pfautonotransaksi` AS `pfautonotransaksi`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,`pf`.`pftgl` AS `pftgl`,`pf`.`pfkodepa` AS `pfkodepa`,`pf`.`pfsupplier` AS `pfsupplier`,`pf`.`pfsupplierkontak` AS `pfsupplierkontak`,`pf`.`pf1alamat1` AS `pf1alamat1`,`pf`.`pf1alamat2` AS `pf1alamat2`,`pf`.`pf1alamat3` AS `pf1alamat3`,`pf`.`pf2alamat1` AS `pf2alamat1`,`pf`.`pf2alamat2` AS `pf2alamat2`,`pf`.`pf2alamat3` AS `pf2alamat3`,`pf`.`pfbagianpembelian` AS `pfbagianpembelian`,`pf`.`pftgldipenuhi` AS `pftgldipenuhi`,`pf`.`pftermin` AS `pftermin`,`pf`.`pftgljatuhtempo` AS `pftgljatuhtempo`,`pf`.`pfuraian` AS `pfuraian`,`pf`.`pfcatatan` AS `pfcatatan`,`pf`.`pfnoref` AS `pfnoref`,`pf`.`pftglnoref` AS `pftglnoref`,`pf`.`pftglpenutupan` AS `pftglpenutupan`,`pf`.`pfmatauang` AS `pfmatauang`,`pf`.`pfkurs` AS `pfkurs`,`pf`.`pfhargatermasukpajak` AS `pfhargatermasukpajak`,`pf`.`pftotal` AS `pftotal`,`pf`.`pfdiskonpersen` AS `pfdiskonpersen`,`pf`.`pfjmldiskon` AS `pfjmldiskon`,`pf`.`pftotalpajak1detail` AS `pftotalpajak1detail`,`pf`.`pftotalpajak2detail` AS `pftotalpajak2detail`,`pf`.`pfbiayalainpersen` AS `pfbiayalainpersen`,`pf`.`pfbiayalain` AS `pfbiayalain`,`pf`.`pftotaltransaksi` AS `pftotaltransaksi`,`pf`.`pfjmlbayar` AS `pfjmlbayar`,`pf`.`pfrekdiskon` AS `pfrekdiskon`,`pf`.`pfrekpajak1` AS `pfrekpajak1`,`pf`.`pfrekpajak2` AS `pfrekpajak2`,`pf`.`pfrekbiayalain` AS `pfrekbiayalain`,`pf`.`pfrekbayar` AS `pfrekbayar`,`pf`.`pfidpr` AS `pfidpr`,`pf`.`pfidcs` AS `pfidcs`,`pf`.`pfidrq` AS `pfidrq`,`pf`.`pfidbs` AS `pfidbs`,`pf`.`pfstatusipc` AS `pfstatusipc`,`pf`.`pfstatusgrn` AS `pfstatusgrn`,`pf`.`pfstatusri` AS `pfstatusri`,`pf`.`pfstatusdnr` AS `pfstatusdnr`,`pf`.`pfstatusprt` AS `pfstatusprt`,`pf`.`pfstatusrealisasi` AS `pfstatusrealisasi`,`pf`.`pfstatus` AS `pfstatus`,`pf`.`pfstatussebelumnya` AS `pfstatussebelumnya`,`pf`.`pfjmlrevisi` AS `pfjmlrevisi`,`pf`.`pfcetakanke` AS `pfcetakanke`,`pf`.`pfinputuser` AS `pfinputuser`,`pf`.`pfinputtgl` AS `pfinputtgl`,`pf`.`pfmodifikasiuser` AS `pfmodifikasiuser`,`pf`.`pfmodifikasitgl` AS `pfmodifikasitgl`,`pf`.`pfposting` AS `pfposting`,`pf`.`pfpostingtgl` AS `pfpostingtgl`,`pf`.`pfisclose` AS `pfisclose`,`pf`.`pfcustomtext1` AS `pfcustomtext1`,`pf`.`pfcustomtext2` AS `pfcustomtext2`,`pf`.`pfcustomtext3` AS `pfcustomtext3`,`pf`.`pfcustomtext4` AS `pfcustomtext4`,`pf`.`pfcustomtext5` AS `pfcustomtext5`,`pf`.`pfcustomint1` AS `pfcustomint1`,`pf`.`pfcustomint2` AS `pfcustomint2`,`pf`.`pfcustomint3` AS `pfcustomint3`,`pf`.`pfcustomdbl1` AS `pfcustomdbl1`,`pf`.`pfcustomdbl2` AS `pfcustomdbl2`,`pf`.`pfcustomdbl3` AS `pfcustomdbl3`,`pf`.`pfcustomdate1` AS `pfcustomdate1`,`pf`.`pfcustomdate2` AS `pfcustomdate2`,`pf`.`pfcustomdate3` AS `pfcustomdate3`,`br`.`bnama` AS `pfcabangnama`,`lc`.`lnama` AS `pflokasinama`,`wh`.`wnama` AS `pfgudangnama`,`c1`.`kkode` AS `pfsupplierkode`,`c1`.`knama` AS `pfsuppliernama`,`c2`.`kkode` AS `pfbagianpembeliankode`,`c2`.`knama` AS `pfbagianpembeliannama`,`tr`.`trnama` AS `pfterminnama`,`tr`.`trharijatuhtempo` AS `pfterminharijatuhtempo`,`coa1`.`cnama` AS `pfrekdiskonnama`,`coa2`.`cnama` AS `pfrekpajak1nama`,`coa3`.`cnama` AS `pfrekpajak2nama`,`coa4`.`cnama` AS `pfrekbiayalainnama`,`coa5`.`cnama` AS `pfrekbayarnama`,`pr`.`prnotransaksi` AS `pfnotransaksipr`,`cs`.`csnotransaksi` AS `pfnotransaksics`,`rq`.`rqnotransaksi` AS `pfnotransaksirq`,`bs`.`bsnotransaksi` AS `pfnotransaksibs`,`st1`.`nama` AS `pfstatusnama`,`st2`.`nama` AS `pfstatussebelumnyanama`,`u1`.`unama` AS `pfinputusernama`,`u2`.`unama` AS `pfmodifikasiusernama`,`pfd`.`idpfdetail` AS `idpfdetail`,`pfd`.`idpf` AS `idpf`,`pfd`.`idbarang` AS `idbarang`,`pfd`.`namabarang` AS `namabarang`,`pfd`.`tipebarang` AS `tipebarang`,`pfd`.`jml` AS `jml`,`pfd`.`satuan` AS `satuan`,`pfd`.`nilaisatuan` AS `nilaisatuan`,`pfd`.`jmlbarang` AS `jmlbarang`,`pfd`.`satuanbarang` AS `satuanbarang`,`pfd`.`matauang` AS `matauang`,`pfd`.`kurs` AS `kurs`,`pfd`.`hargafix` AS `hargafix`,`pfd`.`harga` AS `harga`,`pfd`.`diskon` AS `diskon`,`pfd`.`jmldiskon` AS `jmldiskon`,`pfd`.`pajak1` AS `pajak1`,`pfd`.`jmlpajak1` AS `jmlpajak1`,`pfd`.`pajak2` AS `pajak2`,`pfd`.`jmlpajak2` AS `jmlpajak2`,`pfd`.`cabang` AS `cabang`,`pfd`.`lokasi` AS `lokasi`,`pfd`.`gudang` AS `gudang`,`pfd`.`costcenter` AS `costcenter`,`pfd`.`divisi` AS `divisi`,`pfd`.`subdivisi` AS `subdivisi`,`pfd`.`proyek` AS `proyek`,`pfd`.`catatan` AS `catatan`,`pfd`.`urutan` AS `urutan`,`pfd`.`idprdetail` AS `idprdetail`,`pfd`.`idcsdetail` AS `idcsdetail`,`pfd`.`idrqdetail` AS `idrqdetail`,`pfd`.`idbsdetail` AS `idbsdetail`,`pfd`.`jmlipc` AS `jmlipc`,`pfd`.`statusipc` AS `statusipc`,`pfd`.`jmlgrn` AS `jmlgrn`,`pfd`.`statusgrn` AS `statusgrn`,`pfd`.`jmlri` AS `jmlri`,`pfd`.`statusri` AS `statusri`,`pfd`.`jmldnr` AS `jmldnr`,`pfd`.`statusdnr` AS `statusdnr`,`pfd`.`jmlprt` AS `jmlprt`,`pfd`.`statusprt` AS `statusprt`,`pfd`.`jmlrealisasi` AS `jmlrealisasi`,`pfd`.`statusrealisasi` AS `statusrealisasi`,`pfd`.`isclose` AS `isclose`,`pfd`.`customtext1` AS `customtext1`,`pfd`.`customtext2` AS `customtext2`,`pfd`.`customtext3` AS `customtext3`,`pfd`.`customdbl1` AS `customdbl1`,`pfd`.`customdbl2` AS `customdbl2`,`pfd`.`customdbl3` AS `customdbl3`,`pfd`.`customdate1` AS `customdate1`,`pfd`.`customdate2` AS `customdate2`,`pfd`.`customdate3` AS `customdate3`,`i`.`bkode` AS `kodebarang`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,`brd`.`bnama` AS `cabangnama`,`lcd`.`lnama` AS `lokasinama`,`whd`.`wnama` AS `gudangnama`,`cc`.`ccnama` AS `costcenternama`,`d`.`dnama` AS `divisinama`,`sd`.`sdnama` AS `subdivisinama`,`p`.`pnama` AS `proyeknama`,`pr2`.`prnotransaksi` AS `prnotransaksi`,`cs2`.`csnotransaksi` AS `csnotransaksi`,`rq2`.`rqnotransaksi` AS `rqnotransaksi`,`bs2`.`bsnotransaksi` AS `bsnotransaksi`, c1.kpkp, i.bapanjang, i.balebar, i.batinggi, i.bjmllapangan, i.bsatuanlapangan from ((((((((((((((((((((((((((((((((((((((`m4_pf` `pf` join `m4_pf_detail` `pfd` on((`pf`.`pfid` = `pfd`.`idpf`))) left join `m1_branch` `br` on((`br`.`bkode` = `pf`.`pfcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `pf`.`pflokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `pf`.`pfgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `pf`.`pfsupplier`))) left join `m1_contact` `c2` on((`c2`.`kid` = `pf`.`pfbagianpembelian`))) left join `m1_terms` `tr` on((`pf`.`pftermin` = `tr`.`trkode`))) left join `m1_coa` `coa1` on((`pf`.`pfrekdiskon` = `coa1`.`cnomor`))) left join `m1_coa` `coa2` on((`pf`.`pfrekpajak1` = `coa2`.`cnomor`))) left join `m1_coa` `coa3` on((`pf`.`pfrekpajak2` = `coa3`.`cnomor`))) left join `m1_coa` `coa4` on((`pf`.`pfrekbiayalain` = `coa4`.`cnomor`))) left join `m1_coa` `coa5` on((`pf`.`pfrekbayar` = `coa5`.`cnomor`))) left join `m4_pr` `pr` on((`pf`.`pfidpr` = `pr`.`prid`))) left join `m4_cs` `cs` on((`pf`.`pfidcs` = `cs`.`csid`))) left join `m4_rq` `rq` on((`pf`.`pfidrq` = `rq`.`rqid`))) left join `m4_bs` `bs` on((`pf`.`pfidbs` = `bs`.`bsid`))) left join `m0_status` `st1` on((`st1`.`kode` = `pf`.`pfstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `pf`.`pfstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `pf`.`pfinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `pf`.`pfmodifikasiuser`))) left join `m1_item` `i` on((`i`.`bid` = `pfd`.`idbarang`))) left join `m1_tax` `t1` on((`pfd`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`pfd`.`pajak2` = `t2`.`tkode`))) left join `m1_branch` `brd` on((`pfd`.`cabang` = `brd`.`bkode`))) left join `m1_location` `lcd` on((`pfd`.`lokasi` = `lcd`.`lkode`))) left join `m1_warehouse` `whd` on((`pfd`.`gudang` = `whd`.`wkode`))) left join `m1_cost_center` `cc` on((`pfd`.`costcenter` = `cc`.`cckode`))) left join `m1_division` `d` on((`pfd`.`divisi` = `d`.`dkode`))) left join `m1_subdivision` `sd` on((`pfd`.`subdivisi` = `sd`.`sdkode`))) left join `m1_project` `p` on((`pfd`.`proyek` = `p`.`pkode`))) left join `m4_pr_detail` `prd` on((`pfd`.`idprdetail` = `prd`.`idprdetail`))) left join `m4_pr` `pr2` on((`prd`.`idpr` = `pr2`.`prid`))) left join `m4_bs_detail` `bsd` on((`pfd`.`idbsdetail` = `bsd`.`idbsdetail`))) left join `m4_bs` `bs2` on((`bsd`.`idbs` = `bs2`.`bsid`))) left join `m4_cs_detail` `csd` on((`pfd`.`idcsdetail` = `csd`.`idcsdetail`))) left join `m4_cs` `cs2` on((`csd`.`idcs` = `cs2`.`csid`))) left join `m4_rq_detail` `rqd` on((`pfd`.`idrqdetail` = `rqd`.`idrqdetail`))) left join `m4_rq` `rq2` on((`rqd`.`idrq` = `rq2`.`rqid`)))"
        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData(NmMemcached, Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql)

        pg1 = pg1
        If dt.Rows.Count > 0 Then
            Dim drutama As DataRow = dt.Rows(0)
            utama = String.Concat(FxDB(drutama("pfid"), 0), sptField,
                     FxDB(drutama("pfcabang"), ""), sptField,
                     FxDB(drutama("pflokasi"), ""), sptField,
                     FxDB(drutama("pfgudang"), ""), sptField,
                     FxDB(drutama("pfasalbarang"), ""), sptField,
                     FxDB(drutama("pfasalbarangkategori"), 0), sptField,
                     FxDB(drutama("pfjenispembelian"), ""), sptField,
                     FxDB(drutama("pfjenispembeliankategori"), 0), sptField,
                     FxDB(drutama("pfcarabayar"), 0), sptField,
                     FxDB(drutama("pfsumber"), ""), sptField,
                     FxDB(drutama("pfautonotransaksi"), 0), sptField,
                     FxDB(drutama("pfnotransaksi"), ""), sptField,
                     Asformattanggal(FxDB(drutama("pftgl"), ""), formatTgl), sptField,
                     FxDB(drutama("pfkodepa"), 0), sptField,
                     FxDB(drutama("pfsupplier"), 0), sptField,
                     FxDB(drutama("pfsupplierkontak"), ""), sptField,
                     FxDB(drutama("pf1alamat1"), ""), sptField,
                     FxDB(drutama("pf1alamat2"), ""), sptField,
                     FxDB(drutama("pf1alamat3"), ""), sptField,
                     FxDB(drutama("pf2alamat1"), ""), sptField,
                     FxDB(drutama("pf2alamat2"), ""), sptField,
                     FxDB(drutama("pf2alamat3"), ""), sptField,
                     FxDB(drutama("pfbagianpembelian"), 0), sptField,
                     Asformattanggal(FxDB(drutama("pftgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(drutama("pftermin"), ""), sptField,
                     Asformattanggal(FxDB(drutama("pftgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(drutama("pfuraian"), ""), sptField,
                     FxDB(drutama("pfcatatan"), ""), sptField,
                     FxDB(drutama("pfnoref"), ""), sptField,
                     Asformattanggal(FxDB(drutama("pftglnoref"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(drutama("pftglpenutupan"), ""), formatTgl), sptField,
                     FxDB(drutama("pfmatauang"), ""), sptField,
                     FxDB(drutama("pfkurs"), 0), sptField,
                     FxDB(drutama("pfhargatermasukpajak"), 0), sptField,
                     FxDB(drutama("pftotal"), 0), sptField,
                     FxDB(drutama("pfdiskonpersen"), ""), sptField,
                     FxDB(drutama("pfjmldiskon"), 0), sptField,
                     FxDB(drutama("pftotalpajak1detail"), 0), sptField,
                     FxDB(drutama("pftotalpajak2detail"), 0), sptField,
                     FxDB(drutama("pfbiayalainpersen"), ""), sptField,
                     FxDB(drutama("pfbiayalain"), 0), sptField,
                     FxDB(drutama("pftotaltransaksi"), 0), sptField,
                     FxDB(drutama("pfjmlbayar"), 0), sptField,
                     FxDB(drutama("pfrekdiskon"), ""), sptField,
                     FxDB(drutama("pfrekpajak1"), ""), sptField,
                     FxDB(drutama("pfrekpajak2"), ""), sptField,
                     FxDB(drutama("pfrekbiayalain"), ""), sptField,
                     FxDB(drutama("pfrekbayar"), ""), sptField,
                     FxDB(drutama("pfidpr"), 0), sptField,
                     FxDB(drutama("pfidcs"), 0), sptField,
                     FxDB(drutama("pfidrq"), 0), sptField,
                     FxDB(drutama("pfidbs"), 0), sptField,
                     FxDB(drutama("pfstatusipc"), 0), sptField,
                     FxDB(drutama("pfstatusgrn"), 0), sptField,
                     FxDB(drutama("pfstatusri"), 0), sptField,
                     FxDB(drutama("pfstatusdnr"), 0), sptField,
                     FxDB(drutama("pfstatusprt"), 0), sptField,
                     FxDB(drutama("pfstatusrealisasi"), 0), sptField,
                     FxDB(drutama("pfstatus"), 0), sptField,
                     FxDB(drutama("pfstatussebelumnya"), 0), sptField,
                     FxDB(drutama("pfjmlrevisi"), 0), sptField,
                     FxDB(drutama("pfcetakanke"), 0), sptField,
                     FxDB(drutama("pfinputuser"), 0), sptField,
                     Asformattanggal(FxDB(drutama("pfinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pfmodifikasiuser"), 0), sptField,
                     Asformattanggal(FxDB(drutama("pfmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pfposting"), 0), sptField,
                     Asformattanggal(FxDB(drutama("pfpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(drutama("pfisclose"), 0), sptField,
                     FxDB(drutama("pfcustomtext1"), ""), sptField,
                     FxDB(drutama("pfcustomtext2"), ""), sptField,
                     FxDB(drutama("pfcustomtext3"), ""), sptField,
                     FxDB(drutama("pfcustomtext4"), ""), sptField,
                     FxDB(drutama("pfcustomtext5"), ""), sptField,
                     FxDB(drutama("pfcustomint1"), 0), sptField,
                     FxDB(drutama("pfcustomint2"), 0), sptField,
                     FxDB(drutama("pfcustomint3"), 0), sptField,
                     FxDB(drutama("pfcustomdbl1"), 0), sptField,
                     FxDB(drutama("pfcustomdbl2"), 0), sptField,
                     FxDB(drutama("pfcustomdbl3"), 0), sptField,
                     Asformattanggal(FxDB(drutama("pfcustomdate1"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(drutama("pfcustomdate2"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(drutama("pfcustomdate3"), ""), formatTgl), sptField,
                     FxDB(drutama("pfcabangnama"), ""), sptField,
                     FxDB(drutama("pflokasinama"), ""), sptField,
                     FxDB(drutama("pfgudangnama"), ""), sptField,
                     FxDB(drutama("pfsupplierkode"), ""), sptField,
                     FxDB(drutama("pfsuppliernama"), ""), sptField,
                     FxDB(drutama("pfbagianpembeliankode"), ""), sptField,
                     FxDB(drutama("pfbagianpembeliannama"), ""), sptField,
                     FxDB(drutama("pfterminnama"), ""), sptField,
                     FxDB(drutama("pfterminharijatuhtempo"), 0), sptField,
                     FxDB(drutama("pfrekdiskonnama"), ""), sptField,
                     FxDB(drutama("pfrekpajak1nama"), ""), sptField,
                     FxDB(drutama("pfrekpajak2nama"), ""), sptField,
                     FxDB(drutama("pfrekbiayalainnama"), ""), sptField,
                     FxDB(drutama("pfrekbayarnama"), ""), sptField,
                     FxDB(drutama("pfnotransaksipr"), ""), sptField,
                     FxDB(drutama("pfnotransaksics"), ""), sptField,
                     FxDB(drutama("pfnotransaksirq"), ""), sptField,
                     FxDB(drutama("pfnotransaksibs"), ""), sptField,
                     FxDB(drutama("pfstatusnama"), ""), sptField,
                     FxDB(drutama("pfstatussebelumnyanama"), ""), sptField,
                     FxDB(drutama("pfinputusernama"), ""), sptField,
                     FxDB(drutama("pfmodifikasiusernama"), ""), sptField,
                     FxDB(drutama("kpkp"), 0))

            For Each dr As DataRow In dt.Rows
                detail = String.Concat(detail, FxDB(dr("idpfdetail"), 0), sptField,
                     FxDB(dr("idpf"), 0), sptField,
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
                     Asformattanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
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
            sql = "SELECT pfc.idpfcost, pfc.idpf, pfc.kodecost, pfc.matauang, pfc.kurs, pfc.jumlah, pfc.rekdebit, pfc.rekkredit, pfc.kontak, pfc.termasukhpp, pfc.catatan, pfc.costcenter, pfc.divisi, pfc.subdivisi, pfc.proyek, pfc.urutan, pfc.idprcost, pfc.idcscost, pfc.idrqcost, pfc.idbscost, pfc.jumlahipc, pfc.statusipc, pfc.jumlahgrn, pfc.statusgrn, pfc.jumlahri, pfc.statusri, pfc.jumlahbayar, pfc.statusbayar, pfc.isclose, pfc.customtext1, pfc.customtext2, pfc.customtext3, pfc.customdbl1, pfc.customdbl2, pfc.customdbl3, pfc.customdate1, pfc.customdate2, pfc.customdate3, oc.ocnama as kodecostnama, coa1.cnama as rekdebitnama, coa2.cnama as rekkreditnama,  c.kkode as kontakkode, c.knama as kontaknama, cc.ccnama as costcenternama, d.dnama as divisinama, sd.sddivisi as subdivisinama FROM m4_pf_cost pfc JOIN m4_pf pf ON pfc.idpf = pf.pfid LEFT JOIN m1_other_cost oc ON pfc.kodecost = oc.ockode LEFT JOIN m1_coa coa1 ON pfc.rekdebit = coa1.cnomor LEFT JOIN m1_coa coa2 ON pfc.rekkredit = coa2.cnomor LEFT JOIN m1_contact c ON pfc.kontak = c.kid LEFT JOIN m1_cost_center cc ON pfc.costcenter = cc.cckode LEFT JOIN m1_division d ON pfc.divisi = d.dkode LEFT JOIN m1_subdivision sd ON pfc.subdivisi = sd.sdkode"
            Dim dtcost As New DataTable
            dtcost = AmbilData("aplikasi1-m4_pf_cost", Filter, "pfc.urutan", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcost.Rows
                cost = String.Concat(cost,
                     FxDB(dr("idpfcost"), ""), sptField,
                     FxDB(dr("idpf"), ""), sptField,
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
                     Asformattanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
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
            sql = "SELECT pftrans.idpftrans, pftrans.idpf, pftrans.sumber, pftrans.idtransaksi, pftrans.catatan, pftrans.urutan, pftrans.isclose, pftrans.customtext1, pftrans.customtext2, pftrans.customtext3, pftrans.customtext4, pftrans.customtext5, pftrans.customdbl1, pftrans.customdbl2, pftrans.customdbl3, pftrans.customdbl4, pftrans.customdbl5, pftrans.customdate1, pftrans.customdate2, pftrans.customdate3, pftrans.customdate4, pftrans.customdate5, m5si.sinotransaksi as notransaksi, m5si.sitgl as tgltransaksi, m5si.sicustomer as kontak, c.kkode as kontakkode,  c.knama as kontaknama FROM m4_pf_trans pftrans LEFT JOIN m5_si m5si  ON pftrans.sumber = m5si.sisumber AND pftrans.idtransaksi = m5si.siid LEFT JOIN m1_contact c ON m5si.sicustomer = c.kid"
            Dim dttrans As New DataTable
            dttrans = AmbilData("aplikasi1-m1_no_trans_out", "pftrans.idpf = '" & idtransaksi & "'", "pftrans.urutan ASC", True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dttrans.Rows
                trans = String.Concat(trans,
                     FxDB(dr("idpftrans"), 0), sptField,
                     FxDB(dr("idpf"), 0), sptField,
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
                     Asformattanggal(FxDB(dr("customdate1"), "1900-01-01"), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("customdate2"), "1900-01-01"), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("customdate3"), "1900-01-01"), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("customdate4"), "1900-01-01"), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("customdate5"), "1900-01-01"), formatTgl), sptField,
                     FxDB(dr("notransaksi"), ""), sptField,
                     Asformattanggal(FxDB(dr("tgltransaksi"), "1900-01-01"), formatTgl), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pfid, pfcabang, pflokasi, pfgudang, pfasalbarang, pfasalbarangkategori, pfjenispembelian, pfjenispembeliankategori, pfcarabayar, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl, pfkodepa, pfsupplier, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, pf2alamat3, pfbagianpembelian, pftgldipenuhi, pftermin, pftgljatuhtempo, pfuraian, pfcatatan, pfnoref, pftglnoref, pftglpenutupan, pfmatauang, pfkurs, pfhargatermasukpajak, pftotal, pfdiskonpersen, pfjmldiskon, pftotalpajak1detail, pftotalpajak2detail, pfbiayalainpersen, pfbiayalain, pftotaltransaksi, pfjmlbayar, pfrekdiskon, pfrekpajak1, pfrekpajak2, pfrekbiayalain, pfrekbayar, pfidpr, pfidcs, pfidrq, pfidbs, pfstatusipc, pfstatusgrn, pfstatusri, pfstatusdnr, pfstatusprt, pfstatusrealisasi, pfstatus, pfstatussebelumnya, pfjmlrevisi, pfcetakanke, pfinputuser, pfinputtgl, pfmodifikasiuser, pfmodifikasitgl, pfposting, pfpostingtgl, pfisclose, pfcustomtext1, pfcustomtext2, pfcustomtext3, pfcustomtext4, pfcustomtext5, pfcustomint1, pfcustomint2, pfcustomint3, pfcustomdbl1, pfcustomdbl2, pfcustomdbl3, pfcustomdate1, pfcustomdate2, pfcustomdate3, pfcabangnama, pflokasinama, pfgudangnama, pfsupplierkode, pfsuppliernama, pfbagianpembeliankode, pfbagianpembeliannama, pfterminnama, pfterminharijatuhtempo, pfrekdiskonnama, pfrekpajak1nama, pfrekpajak2nama, pfrekbiayalainnama, pfrekbayarnama, pfnotransaksipr, pfnotransaksics, pfnotransaksirq, pfnotransaksibs, pfstatusnama, pfstatussebelumnyanama, pfinputusernama, pfmodifikasiusernama, kpkp" & sptSubParam & "idpfdetail, idpf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, jmlrealisasi, statusrealisasi, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodebarang, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, cabangnama, lokasinama, gudangnama, costcenternama, divisinama, subdivisinama, proyeknama, prnotransaksi, csnotransaksi, rqnotransaksi, bsnotransaksi, bapanjang, balebar, batinggi, bjmllapangan, bsatuanlapangan" & sptSubParam & "idpfcost, idpf, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, kontaknama, costcenternama, divisinama, subdivisinama" & sptSubParam & "idpftrans, idpf, sumber, idtransaksi, catatan, urutan, isclose, customtext1, customtext2, customtext3, customtext4, customtext5, customdbl1, customdbl2, customdbl3, customdbl4, customdbl5, customdate1, customdate2, customdate3, customdate4, customdate5, notransaksi, tgltransaksi, kontak, kontakkode, kontaknama"))

        Return wsResult
    End Function

    '    <WebMethod()>
    '    Public Function M4_PfSearch(ByVal param As String) As String
    '        'M4_PfSearch --------------------------------------------------------
    '        'pfid, pfcabang, pflokasi, pfgudang, pfasalbarang, pfasalbarangkategori, pfjenispembelian, 
    '        'pfjenispembeliankategori, pfcarabayar, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl, pfkodepa, 
    '        'pfsupplier, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, 
    '        'pf2alamat3, pfbagianpembelian, pftgldipenuhi, pftermin, pftgljatuhtempo, pfuraian, pfcatatan, 
    '        'pfnoref, pftglnoref, pftglpenutupan, pfmatauang, pfkurs, pfhargatermasukpajak, pftotal, 
    '        'pfdiskonpersen, pfjmldiskon, pftotalpajak1detail, pftotalpajak2detail, pfbiayalainpersen, pfbiayalain, pftotaltransaksi, 
    '        'pfjmlbayar, pfrekdiskon, pfrekpajak1, pfrekpajak2, pfrekbiayalain, pfrekbayar, pfidpr, 
    '        'pfidcs, pfidrq, pfidbs, pfstatusipc, pfstatusgrn, pfstatusri, pfstatusdnr, 
    '        'pfstatusprt, pfstatusrealisasi, pfstatus, pfstatussebelumnya, pfjmlrevisi, pfcetakanke, pfinputuser, 
    '        'pfinputtgl, pfmodifikasiuser, pfmodifikasitgl, pfposting, pfpostingtgl, pfisclose, pfcabangnama, 
    '        'pflokasinama, pfgudangnama, pfsupplierkode, pfsuppliernama, pfbagianpembeliankode, pfbagianpembeliannama, prnotransaksi, 
    '        'csnotransaksi, rqnotransaksi, bsnotransaksi, pfstatusnama, pfstatussebelumnyanama, pfinputusernama, pfmodifikasiusernama

    '        On Error GoTo selesai
    '        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

    '        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
    '        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

    '        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
    '        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

    '        Dim wsResult As String = ""
    '        Dim strResult, strResultPaging As String

    '        Dim sql As String = ""

    '        Dim pg1 As New RsPaging
    '        Dim Filter As String = "", Sorting As String = ""
    '        Dim dt As New DataTable

    '        'SET DEFAULT 
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
    '        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
    '            result(2) = "Access denied for insert/update data"
    '        End If
    '        'END OF VALIDASI WEBSITEACCESSKEY ==================================================

    '        'VALIDASI PARAMETER PAGING =========================================================
    '        'SPLIT PARAMETER PAGING
    '        pagingSplit = paramSplit(2).Split(sptSubParam)

    '        'CEK ARRAY PAGING
    '        If (pagingSplit.Length <> 6) Then
    '            result(2) = "Invalid paging parameter." : GoTo selesai
    '        End If

    '        'CEK PAGENUMBER
    '        If (IsNumeric(pagingSplit(0)) = False) Then
    '            result(2) = "pageNumber required numeric." : GoTo selesai
    '        End If

    '        'CEK ITEMLIMIT
    '        If (IsNumeric(pagingSplit(1)) = False) Then
    '            result(2) = "itemLimit required numeric." : GoTo selesai
    '        End If

    '        'CEK FORMATTGL
    '        If Len(pagingSplit(4)) = 0 Then
    '            formatTgl = "yyyy-MM-dd"
    '        Else
    '            formatTgl = pagingSplit(4)
    '        End If

    '        'CEK FORMATTGLWAKTU
    '        If Len(pagingSplit(5)) = 0 Then
    '            formatTglWaktu = "yyyy-MM-dd H:mm:ss"
    '        Else
    '            formatTglWaktu = pagingSplit(5)
    '        End If
    '        'END OF VALIDASI PARAMETER PAGING ==================================================

    '        'Replace disesuaikan dengan kebutuhan
    '        If (pagingSplit(2).Length > 0) Then
    '            Filter = pagingSplit(2)
    '            '#Taruh fungsi replace disini...
    '            Filter = Filter.Replace("pfsupplierkode", "c1.kkode")
    '            Filter = Filter.Replace("pfsuppliernama", "c1.knama")
    '        End If
    '        If (pagingSplit(3).Length > 0) Then
    '            Sorting = pagingSplit(3)
    '            '#Taruh fungsi replace disini...
    '        End If

    '        'PANGGIL QUERY
    '        Dim query As New m0_query
    '        sql = query.PanggilQuery("m4_pf_v")

    '        result(2) = sql : GoTo selesai

    '        'BUKA KONEKSI
    '        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
    '        Con1.Open()



    '        dt = AmbilData("aplikasi1-M4_Pf", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
    '        pg1 = pg1
    '        result(2) = dt.Rows.Count.ToString : GoTo selesai
    '        If dt.Rows.Count > 0 Then
    '            For Each dr As DataRow In dt.Rows
    '                search = String.Concat(search,
    '                     FxDB(dr("pfid"), 0), sptField,
    '                     FxDB(dr("pfcabang"), ""), sptField,
    '                     FxDB(dr("pflokasi"), ""), sptField,
    '                     FxDB(dr("pfgudang"), ""), sptField,
    '                     FxDB(dr("pfasalbarang"), ""), sptField,
    '                     FxDB(dr("pfasalbarangkategori"), 0), sptField,
    '                     FxDB(dr("pfjenispembelian"), ""), sptField,
    '                     FxDB(dr("pfjenispembeliankategori"), 0), sptField,
    '                     FxDB(dr("pfcarabayar"), 0), sptField,
    '                     FxDB(dr("pfsumber"), ""), sptField,
    '                     FxDB(dr("pfautonotransaksi"), 0), sptField,
    '                     FxDB(dr("pfnotransaksi"), ""), sptField,
    '                     Asformattanggal(FxDB(dr("pftgl"), ""), formatTgl), sptField,
    '                     FxDB(dr("pfkodepa"), 0), sptField,
    '                     FxDB(dr("pfsupplier"), 0), sptField,
    '                     FxDB(dr("pfsupplierkontak"), ""), sptField,
    '                     FxDB(dr("pf1alamat1"), ""), sptField,
    '                     FxDB(dr("pf1alamat2"), ""), sptField,
    '                     FxDB(dr("pf1alamat3"), ""), sptField,
    '                     FxDB(dr("pf2alamat1"), ""), sptField,
    '                     FxDB(dr("pf2alamat2"), ""), sptField,
    '                     FxDB(dr("pf2alamat3"), ""), sptField,
    '                     FxDB(dr("pfbagianpembelian"), 0), sptField,
    '                     Asformattanggal(FxDB(dr("pftgldipenuhi"), ""), formatTgl), sptField,
    '                     FxDB(dr("pftermin"), ""), sptField,
    '                     Asformattanggal(FxDB(dr("pftgljatuhtempo"), ""), formatTgl), sptField,
    '                     FxDB(dr("pfuraian"), ""), sptField,
    '                     FxDB(dr("pfcatatan"), ""), sptField,
    '                     FxDB(dr("pfnoref"), ""), sptField,
    '                     Asformattanggal(FxDB(dr("pftglnoref"), ""), formatTgl), sptField,
    '                     Asformattanggal(FxDB(dr("pftglpenutupan"), ""), formatTgl), sptField,
    '                     FxDB(dr("pfmatauang"), ""), sptField,
    '                     FxDB(dr("pfkurs"), 0), sptField,
    '                     FxDB(dr("pfhargatermasukpajak"), 0), sptField,
    '                     FxDB(dr("pftotal"), 0), sptField,
    '                     FxDB(dr("pfdiskonpersen"), ""), sptField,
    '                     FxDB(dr("pfjmldiskon"), 0), sptField,
    '                     FxDB(dr("pftotalpajak1detail"), 0), sptField,
    '                     FxDB(dr("pftotalpajak2detail"), 0), sptField,
    '                     FxDB(dr("pfbiayalainpersen"), ""), sptField,
    '                     FxDB(dr("pfbiayalain"), 0), sptField,
    '                     FxDB(dr("pftotaltransaksi"), 0), sptField,
    '                     FxDB(dr("pfjmlbayar"), 0), sptField,
    '                     FxDB(dr("pfrekdiskon"), ""), sptField,
    '                     FxDB(dr("pfrekpajak1"), ""), sptField,
    '                     FxDB(dr("pfrekpajak2"), ""), sptField,
    '                     FxDB(dr("pfrekbiayalain"), ""), sptField,
    '                     FxDB(dr("pfrekbayar"), ""), sptField,
    '                     FxDB(dr("pfidpr"), 0), sptField,
    '                     FxDB(dr("pfidcs"), 0), sptField,
    '                     FxDB(dr("pfidrq"), 0), sptField,
    '                     FxDB(dr("pfidbs"), 0), sptField,
    '                     FxDB(dr("pfstatusipc"), 0), sptField,
    '                     FxDB(dr("pfstatusgrn"), 0), sptField,
    '                     FxDB(dr("pfstatusri"), 0), sptField,
    '                     FxDB(dr("pfstatusdnr"), 0), sptField,
    '                     FxDB(dr("pfstatusprt"), 0), sptField,
    '                     FxDB(dr("pfstatusrealisasi"), 0), sptField,
    '                     FxDB(dr("pfstatus"), 0), sptField,
    '                     FxDB(dr("pfstatussebelumnya"), 0), sptField,
    '                     FxDB(dr("pfjmlrevisi"), 0), sptField,
    '                     FxDB(dr("pfcetakanke"), 0), sptField,
    '                     FxDB(dr("pfinputuser"), 0), sptField,
    '                     Asformattanggal(FxDB(dr("pfinputtgl"), ""), formatTglWaktu), sptField,
    '                     FxDB(dr("pfmodifikasiuser"), 0), sptField,
    '                     Asformattanggal(FxDB(dr("pfmodifikasitgl"), ""), formatTglWaktu), sptField,
    '                     FxDB(dr("pfposting"), 0), sptField,
    '                     Asformattanggal(FxDB(dr("pfpostingtgl"), ""), formatTglWaktu), sptField,
    '                     FxDB(dr("pfisclose"), 0), sptField,
    '                     FxDB(dr("pfcabangnama"), ""), sptField,
    '                     FxDB(dr("pflokasinama"), ""), sptField,
    '                     FxDB(dr("pfgudangnama"), ""), sptField,
    '                     FxDB(dr("pfsupplierkode"), ""), sptField,
    '                     FxDB(dr("pfsuppliernama"), ""), sptField,
    '                     FxDB(dr("pfbagianpembeliankode"), ""), sptField,
    '                     FxDB(dr("pfbagianpembeliannama"), ""), sptField,
    '                     FxDB(dr("prnotransaksi"), ""), sptField,
    '                     FxDB(dr("csnotransaksi"), ""), sptField,
    '                     FxDB(dr("rqnotransaksi"), ""), sptField,
    '                     FxDB(dr("bsnotransaksi"), ""), sptField,
    '                     FxDB(dr("pfstatusnama"), ""), sptField,
    '                     FxDB(dr("pfstatussebelumnyanama"), ""), sptField,
    '                     FxDB(dr("pfinputusernama"), ""), sptField,
    '                     FxDB(dr("pfmodifikasiusernama"), ""), sptRow)
    '            Next
    '            search = search.Substring(0, search.Length - sptRow.Length)

    '            result(1) = 1
    '            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
    '            resultPaging(1) = Math.Abs(Val(pg1.isNext))
    '            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
    '            resultPaging(3) = pg1.countPage
    '            resultPaging(4) = pg1.countRow
    '        Else
    '            result(2) = "Transaction data not found."
    '        End If

    'selesai:
    '        If result(1) = 0 Then
    '            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
    '        End If

    '        strResult = String.Join(sptSubParam, result)
    '        strResultPaging = String.Join(sptSubParam, resultPaging)
    '        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

    '        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pfid, pfcabang, pflokasi, pfgudang, pfasalbarang, pfasalbarangkategori, pfjenispembelian, pfjenispembeliankategori, pfcarabayar, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl, pfkodepa, pfsupplier, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, pf2alamat3, pfbagianpembelian, pftgldipenuhi, pftermin, pftgljatuhtempo, pfuraian, pfcatatan, pfnoref, pftglnoref, pftglpenutupan, pfmatauang, pfkurs, pfhargatermasukpajak, pftotal, pfdiskonpersen, pfjmldiskon, pftotalpajak1detail, pftotalpajak2detail, pfbiayalainpersen, pfbiayalain, pftotaltransaksi, pfjmlbayar, pfrekdiskon, pfrekpajak1, pfrekpajak2, pfrekbiayalain, pfrekbayar, pfidpr, pfidcs, pfidrq, pfidbs, pfstatusipc, pfstatusgrn, pfstatusri, pfstatusdnr, pfstatusprt, pfstatusrealisasi, pfstatus, pfstatussebelumnya, pfjmlrevisi, pfcetakanke, pfinputuser, pfinputtgl, pfmodifikasiuser, pfmodifikasitgl, pfposting, pfpostingtgl, pfisclose, pfcabangnama, pflokasinama, pfgudangnama, pfsupplierkode, pfsuppliernama, pfbagianpembeliankode, pfbagianpembeliannama, prnotransaksi, csnotransaksi, rqnotransaksi, bsnotransaksi, pfstatusnama, pfstatussebelumnyanama, pfinputusernama, pfmodifikasiusernama"))

    '        Return wsResult
    '    End Function

    <WebMethod()>
    Public Function M4_PfSearch(ByVal param As String) As String
        'M4_PfSearch --------------------------------------------------------
        'pfid, pfcabang, pflokasi, pfgudang, pfasalbarang, pfasalbarangkategori, pfjenispembelian, 
        'pfjenispembeliankategori, pfcarabayar, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl, pfkodepa, 
        'pfsupplier, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, 
        'pf2alamat3, pfbagianpembelian, pftgldipenuhi, pftermin, pftgljatuhtempo, pfuraian, pfcatatan, 
        'pfnoref, pftglnoref, pftglpenutupan, pfmatauang, pfkurs, pfhargatermasukpajak, pftotal, 
        'pfdiskonpersen, pfjmldiskon, pftotalpajak1detail, pftotalpajak2detail, pfbiayalainpersen, pfbiayalain, pftotaltransaksi, 
        'pfjmlbayar, pfrekdiskon, pfrekpajak1, pfrekpajak2, pfrekbiayalain, pfrekbayar, pfidpr, 
        'pfidcs, pfidrq, pfidbs, pfstatusipc, pfstatusgrn, pfstatusri, pfstatusdnr, 
        'pfstatusprt, pfstatusrealisasi, pfstatus, pfstatussebelumnya, pfjmlrevisi, pfcetakanke, pfinputuser, 
        'pfinputtgl, pfmodifikasiuser, pfmodifikasitgl, pfposting, pfpostingtgl, pfisclose, pfcabangnama, 
        'pflokasinama, pfgudangnama, pfsupplierkode, pfsuppliernama, pfbagianpembeliankode, pfbagianpembeliannama, prnotransaksi, 
        'csnotransaksi, rqnotransaksi, bsnotransaksi, pfstatusnama, pfstatussebelumnyanama, pfinputusernama, pfmodifikasiusernama

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
            Filter = Filter.Replace("pfsupplierkode", "c1.kkode")
            Filter = Filter.Replace("pfsuppliernama", "c1.knama")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = "select `pf`.`pfid` AS `pfid`,`pf`.`pfcabang` AS `pfcabang`,`pf`.`pflokasi` AS `pflokasi`,`pf`.`pfgudang` AS `pfgudang`,`pf`.`pfasalbarang` AS `pfasalbarang`,`pf`.`pfasalbarangkategori` AS `pfasalbarangkategori`,`pf`.`pfjenispembelian` AS `pfjenispembelian`,`pf`.`pfjenispembeliankategori` AS `pfjenispembeliankategori`,`pf`.`pfcarabayar` AS `pfcarabayar`,`pf`.`pfsumber` AS `pfsumber`,`pf`.`pfautonotransaksi` AS `pfautonotransaksi`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,`pf`.`pftgl` AS `pftgl`,`pf`.`pfkodepa` AS `pfkodepa`,`pf`.`pfsupplier` AS `pfsupplier`,`pf`.`pfsupplierkontak` AS `pfsupplierkontak`,`pf`.`pf1alamat1` AS `pf1alamat1`,`pf`.`pf1alamat2` AS `pf1alamat2`,`pf`.`pf1alamat3` AS `pf1alamat3`,`pf`.`pf2alamat1` AS `pf2alamat1`,`pf`.`pf2alamat2` AS `pf2alamat2`,`pf`.`pf2alamat3` AS `pf2alamat3`,`pf`.`pfbagianpembelian` AS `pfbagianpembelian`,`pf`.`pftgldipenuhi` AS `pftgldipenuhi`,`pf`.`pftermin` AS `pftermin`,`pf`.`pftgljatuhtempo` AS `pftgljatuhtempo`,`pf`.`pfuraian` AS `pfuraian`,`pf`.`pfcatatan` AS `pfcatatan`,`pf`.`pfnoref` AS `pfnoref`,`pf`.`pftglnoref` AS `pftglnoref`,`pf`.`pftglpenutupan` AS `pftglpenutupan`,`pf`.`pfmatauang` AS `pfmatauang`,`pf`.`pfkurs` AS `pfkurs`,`pf`.`pfhargatermasukpajak` AS `pfhargatermasukpajak`,`pf`.`pftotal` AS `pftotal`,`pf`.`pfdiskonpersen` AS `pfdiskonpersen`,`pf`.`pfjmldiskon` AS `pfjmldiskon`,`pf`.`pftotalpajak1detail` AS `pftotalpajak1detail`,`pf`.`pftotalpajak2detail` AS `pftotalpajak2detail`,`pf`.`pfbiayalainpersen` AS `pfbiayalainpersen`,`pf`.`pfbiayalain` AS `pfbiayalain`,`pf`.`pftotaltransaksi` AS `pftotaltransaksi`,`pf`.`pfjmlbayar` AS `pfjmlbayar`,`pf`.`pfrekdiskon` AS `pfrekdiskon`,`pf`.`pfrekpajak1` AS `pfrekpajak1`,`pf`.`pfrekpajak2` AS `pfrekpajak2`,`pf`.`pfrekbiayalain` AS `pfrekbiayalain`,`pf`.`pfrekbayar` AS `pfrekbayar`,`pf`.`pfidpr` AS `pfidpr`,`pf`.`pfidcs` AS `pfidcs`,`pf`.`pfidrq` AS `pfidrq`,`pf`.`pfidbs` AS `pfidbs`,`pf`.`pfstatusipc` AS `pfstatusipc`,`pf`.`pfstatusgrn` AS `pfstatusgrn`,`pf`.`pfstatusri` AS `pfstatusri`,`pf`.`pfstatusdnr` AS `pfstatusdnr`,`pf`.`pfstatusprt` AS `pfstatusprt`,`pf`.`pfstatusrealisasi` AS `pfstatusrealisasi`,`pf`.`pfstatus` AS `pfstatus`,`pf`.`pfstatussebelumnya` AS `pfstatussebelumnya`,`pf`.`pfjmlrevisi` AS `pfjmlrevisi`,`pf`.`pfcetakanke` AS `pfcetakanke`,`pf`.`pfinputuser` AS `pfinputuser`,`pf`.`pfinputtgl` AS `pfinputtgl`,`pf`.`pfmodifikasiuser` AS `pfmodifikasiuser`,`pf`.`pfmodifikasitgl` AS `pfmodifikasitgl`,`pf`.`pfposting` AS `pfposting`,`pf`.`pfpostingtgl` AS `pfpostingtgl`,`pf`.`pfisclose` AS `pfisclose`,`br`.`bnama` AS `pfcabangnama`,`lc`.`lnama` AS `pflokasinama`,`wh`.`wnama` AS `pfgudangnama`,`c1`.`kkode` AS `pfsupplierkode`,`c1`.`knama` AS `pfsuppliernama`,`c2`.`kkode` AS `pfbagianpembeliankode`,`c2`.`knama` AS `pfbagianpembeliannama`,`pr`.`prnotransaksi` AS `prnotransaksi`,`cs`.`csnotransaksi` AS `csnotransaksi`,`rq`.`rqnotransaksi` AS `rqnotransaksi`,`bs`.`bsnotransaksi` AS `bsnotransaksi`,`st1`.`nama` AS `pfstatusnama`,`st2`.`nama` AS `pfstatussebelumnyanama`,`u1`.`unama` AS `pfinputusernama`,`u2`.`unama` AS `pfmodifikasiusernama` from (((((((((((((`m4_pf` `pf` left join `m1_branch` `br` on((`br`.`bkode` = `pf`.`pfcabang`))) left join `m1_location` `lc` on((`lc`.`lkode` = `pf`.`pflokasi`))) left join `m1_warehouse` `wh` on((`wh`.`wkode` = `pf`.`pfgudang`))) left join `m1_contact` `c1` on((`c1`.`kid` = `pf`.`pfsupplier`))) left join `m1_contact` `c2` on((`c2`.`kid` = `pf`.`pfbagianpembelian`))) left join `m4_pr` `pr` on((`pf`.`pfidpr` = `pr`.`prid`))) left join `m4_cs` `cs` on((`pf`.`pfidcs` = `cs`.`csid`))) left join `m4_rq` `rq` on((`pf`.`pfidrq` = `rq`.`rqid`))) left join `m4_bs` `bs` on((`pf`.`pfidbs` = `bs`.`bsid`))) left join `m0_status` `st1` on((`st1`.`kode` = `pf`.`pfstatus`))) left join `m0_status` `st2` on((`st2`.`kode` = `pf`.`pfstatussebelumnya`))) left join `m0_user` `u1` on((`u1`.`userid` = `pf`.`pfinputuser`))) left join `m0_user` `u2` on((`u2`.`userid` = `pf`.`pfmodifikasiuser`)))"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Pf", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        'result(2) = dt.Rows.Count.ToString & " - " & sql & " where " & Filter : GoTo selesai
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pfid"), 0), sptField,
                     FxDB(dr("pfcabang"), ""), sptField,
                     FxDB(dr("pflokasi"), ""), sptField,
                     FxDB(dr("pfgudang"), ""), sptField,
                     FxDB(dr("pfasalbarang"), ""), sptField,
                     FxDB(dr("pfasalbarangkategori"), 0), sptField,
                     FxDB(dr("pfjenispembelian"), ""), sptField,
                     FxDB(dr("pfjenispembeliankategori"), 0), sptField,
                     FxDB(dr("pfcarabayar"), 0), sptField,
                     FxDB(dr("pfsumber"), ""), sptField,
                     FxDB(dr("pfautonotransaksi"), 0), sptField,
                     FxDB(dr("pfnotransaksi"), ""), sptField,
                     Asformattanggal(FxDB(dr("pftgl"), ""), formatTgl), sptField,
                     FxDB(dr("pfkodepa"), 0), sptField,
                     FxDB(dr("pfsupplier"), 0), sptField,
                     FxDB(dr("pfsupplierkontak"), ""), sptField,
                     FxDB(dr("pf1alamat1"), ""), sptField,
                     FxDB(dr("pf1alamat2"), ""), sptField,
                     FxDB(dr("pf1alamat3"), ""), sptField,
                     FxDB(dr("pf2alamat1"), ""), sptField,
                     FxDB(dr("pf2alamat2"), ""), sptField,
                     FxDB(dr("pf2alamat3"), ""), sptField,
                     FxDB(dr("pfbagianpembelian"), 0), sptField,
                     Asformattanggal(FxDB(dr("pftgldipenuhi"), ""), formatTgl), sptField,
                     FxDB(dr("pftermin"), ""), sptField,
                     Asformattanggal(FxDB(dr("pftgljatuhtempo"), ""), formatTgl), sptField,
                     FxDB(dr("pfuraian"), ""), sptField,
                     FxDB(dr("pfcatatan"), ""), sptField,
                     FxDB(dr("pfnoref"), ""), sptField,
                     Asformattanggal(FxDB(dr("pftglnoref"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("pftglpenutupan"), ""), formatTgl), sptField,
                     FxDB(dr("pfmatauang"), ""), sptField,
                     FxDB(dr("pfkurs"), 0), sptField,
                     FxDB(dr("pfhargatermasukpajak"), 0), sptField,
                     FxDB(dr("pftotal"), 0), sptField,
                     FxDB(dr("pfdiskonpersen"), ""), sptField,
                     FxDB(dr("pfjmldiskon"), 0), sptField,
                     FxDB(dr("pftotalpajak1detail"), 0), sptField,
                     FxDB(dr("pftotalpajak2detail"), 0), sptField,
                     FxDB(dr("pfbiayalainpersen"), ""), sptField,
                     FxDB(dr("pfbiayalain"), 0), sptField,
                     FxDB(dr("pftotaltransaksi"), 0), sptField,
                     FxDB(dr("pfjmlbayar"), 0), sptField,
                     FxDB(dr("pfrekdiskon"), ""), sptField,
                     FxDB(dr("pfrekpajak1"), ""), sptField,
                     FxDB(dr("pfrekpajak2"), ""), sptField,
                     FxDB(dr("pfrekbiayalain"), ""), sptField,
                     FxDB(dr("pfrekbayar"), ""), sptField,
                     FxDB(dr("pfidpr"), 0), sptField,
                     FxDB(dr("pfidcs"), 0), sptField,
                     FxDB(dr("pfidrq"), 0), sptField,
                     FxDB(dr("pfidbs"), 0), sptField,
                     FxDB(dr("pfstatusipc"), 0), sptField,
                     FxDB(dr("pfstatusgrn"), 0), sptField,
                     FxDB(dr("pfstatusri"), 0), sptField,
                     FxDB(dr("pfstatusdnr"), 0), sptField,
                     FxDB(dr("pfstatusprt"), 0), sptField,
                     FxDB(dr("pfstatusrealisasi"), 0), sptField,
                     FxDB(dr("pfstatus"), 0), sptField,
                     FxDB(dr("pfstatussebelumnya"), 0), sptField,
                     FxDB(dr("pfjmlrevisi"), 0), sptField,
                     FxDB(dr("pfcetakanke"), 0), sptField,
                     FxDB(dr("pfinputuser"), 0), sptField,
                     Asformattanggal(FxDB(dr("pfinputtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pfmodifikasiuser"), 0), sptField,
                     Asformattanggal(FxDB(dr("pfmodifikasitgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pfposting"), 0), sptField,
                     Asformattanggal(FxDB(dr("pfpostingtgl"), ""), formatTglWaktu), sptField,
                     FxDB(dr("pfisclose"), 0), sptField,
                     FxDB(dr("pfcabangnama"), ""), sptField,
                     FxDB(dr("pflokasinama"), ""), sptField,
                     FxDB(dr("pfgudangnama"), ""), sptField,
                     FxDB(dr("pfsupplierkode"), ""), sptField,
                     FxDB(dr("pfsuppliernama"), ""), sptField,
                     FxDB(dr("pfbagianpembeliankode"), ""), sptField,
                     FxDB(dr("pfbagianpembeliannama"), ""), sptField,
                     FxDB(dr("prnotransaksi"), ""), sptField,
                     FxDB(dr("csnotransaksi"), ""), sptField,
                     FxDB(dr("rqnotransaksi"), ""), sptField,
                     FxDB(dr("bsnotransaksi"), ""), sptField,
                     FxDB(dr("pfstatusnama"), ""), sptField,
                     FxDB(dr("pfstatussebelumnyanama"), ""), sptField,
                     FxDB(dr("pfinputusernama"), ""), sptField,
                     FxDB(dr("pfmodifikasiusernama"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("pfid, pfcabang, pflokasi, pfgudang, pfasalbarang, pfasalbarangkategori, pfjenispembelian, pfjenispembeliankategori, pfcarabayar, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl, pfkodepa, pfsupplier, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, pf2alamat3, pfbagianpembelian, pftgldipenuhi, pftermin, pftgljatuhtempo, pfuraian, pfcatatan, pfnoref, pftglnoref, pftglpenutupan, pfmatauang, pfkurs, pfhargatermasukpajak, pftotal, pfdiskonpersen, pfjmldiskon, pftotalpajak1detail, pftotalpajak2detail, pfbiayalainpersen, pfbiayalain, pftotaltransaksi, pfjmlbayar, pfrekdiskon, pfrekpajak1, pfrekpajak2, pfrekbiayalain, pfrekbayar, pfidpr, pfidcs, pfidrq, pfidbs, pfstatusipc, pfstatusgrn, pfstatusri, pfstatusdnr, pfstatusprt, pfstatusrealisasi, pfstatus, pfstatussebelumnya, pfjmlrevisi, pfcetakanke, pfinputuser, pfinputtgl, pfmodifikasiuser, pfmodifikasitgl, pfposting, pfpostingtgl, pfisclose, pfcabangnama, pflokasinama, pfgudangnama, pfsupplierkode, pfsuppliernama, pfbagianpembeliankode, pfbagianpembeliannama, prnotransaksi, csnotransaksi, rqnotransaksi, bsnotransaksi, pfstatusnama, pfstatussebelumnyanama, pfinputusernama, pfmodifikasiusernama"))

        Return wsResult
    End Function


    <WebMethod()>
    Public Function M4_Pf_Detail_VSearch(ByVal param As String) As String
        'M4_Pf_Detail_VSearch --------------------------------------------------------
        'idpfdetail, idpf, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, 
        'jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, 
        'statusrealisasi, jmlrealisasi, statusprt, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, pfnotransaksi, 
        'pfuraian, pfcatatan, pfnoref, pftgl, pftglnoref, pfsupplierkontak, pf1alamat1, pf1alamat2, 
        'pf1alamat3, pf2alamat1, pf2alamat2, pf2alamat3, pftermin, pfterminnama, pfterminharijatuhtempo, 
        'pfbagianpembelian, pfbagianpembeliankode, pfbagianpembeliannama, kodebarang, bhpp, bjenis, brekpersediaan, 
        'brekdiskonpembelian, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, 
        'jmlsisaipc, jmlsisagrn, jmlsisari, jmlsisarealisasi, pfsupplier, pfsupplierkode, pfsuppliernama, 
        'bjmllapangan, bsatuanlapangan, basset, ambilnotransaksi, pfhargatermasukpajak, pfcustomtext1, pfcustomtext2,
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
            Filter = Filter.Replace("idbarang", "pfd.idbarang")
            Filter = Filter.Replace("statusrealisasi", "pfd.statusrealisasi")
            Filter = Filter.Replace("isclose", "pfd.isclose")
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.PanggilQuery("m4_pf_detail_v")
        'sql = "select `pfd`.`idpfdetail` AS `idpfdetail`,`pfd`.`idpf` AS `idpf`,`pfd`.`idbarang` AS `idbarang`,`pfd`.`namabarang` AS `namabarang`,`pfd`.`tipebarang` AS `tipebarang`,`pfd`.`jml` AS `jml`,`pfd`.`satuan` AS `satuan`,`pfd`.`nilaisatuan` AS `nilaisatuan`,`pfd`.`jmlbarang` AS `jmlbarang`,`pfd`.`satuanbarang` AS `satuanbarang`,`pfd`.`matauang` AS `matauang`,`pfd`.`kurs` AS `kurs`,`pfd`.`hargafix` AS `hargafix`,`pfd`.`harga` AS `harga`,`pfd`.`diskon` AS `diskon`,`pfd`.`jmldiskon` AS `jmldiskon`,`pfd`.`pajak1` AS `pajak1`,`pfd`.`jmlpajak1` AS `jmlpajak1`,`pfd`.`pajak2` AS `pajak2`,`pfd`.`jmlpajak2` AS `jmlpajak2`,`pfd`.`cabang` AS `cabang`,`pfd`.`lokasi` AS `lokasi`,`pfd`.`gudang` AS `gudang`,`pfd`.`costcenter` AS `costcenter`,`pfd`.`divisi` AS `divisi`,`pfd`.`subdivisi` AS `subdivisi`,`pfd`.`proyek` AS `proyek`,`pfd`.`catatan` AS `catatan`,`pfd`.`urutan` AS `urutan`,`pfd`.`idprdetail` AS `idprdetail`,`pfd`.`idcsdetail` AS `idcsdetail`,`pfd`.`idrqdetail` AS `idrqdetail`,`pfd`.`idbsdetail` AS `idbsdetail`,`pfd`.`jmlipc` AS `jmlipc`,`pfd`.`statusipc` AS `statusipc`,`pfd`.`jmlgrn` AS `jmlgrn`,`pfd`.`statusgrn` AS `statusgrn`,`pfd`.`jmlri` AS `jmlri`,`pfd`.`statusri` AS `statusri`,`pfd`.`jmldnr` AS `jmldnr`,`pfd`.`statusdnr` AS `statusdnr`,`pfd`.`jmlprt` AS `jmlprt`,`pfd`.`statusrealisasi` AS `statusrealisasi`,`pfd`.`jmlrealisasi` AS `jmlrealisasi`,`pfd`.`statusprt` AS `statusprt`,`pfd`.`isclose` AS `isclose`,`pfd`.`customtext1` AS `customtext1`,`pfd`.`customtext2` AS `customtext2`,`pfd`.`customtext3` AS `customtext3`,`pfd`.`customdbl1` AS `customdbl1`,`pfd`.`customdbl2` AS `customdbl2`,`pfd`.`customdbl3` AS `customdbl3`,`pfd`.`customdate1` AS `customdate1`,`pfd`.`customdate2` AS `customdate2`,`pfd`.`customdate3` AS `customdate3`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,`pf`.`pfuraian` AS `pfuraian`,`pf`.`pfcatatan` AS `pfcatatan`,`pf`.`pfnoref` AS `pfnoref`,`pf`.`pftgl` AS `pftgl`,`pf`.`pftglnoref` AS `pftglnoref`,`pf`.`pfsupplierkontak` AS `pfsupplierkontak`,`pf`.`pf1alamat1` AS `pf1alamat1`,`pf`.`pf1alamat2` AS `pf1alamat2`,`pf`.`pf1alamat3` AS `pf1alamat3`,`pf`.`pf2alamat1` AS `pf2alamat1`,`pf`.`pf2alamat2` AS `pf2alamat2`,`pf`.`pf2alamat3` AS `pf2alamat3`,`pf`.`pftermin` AS `pftermin`,`tr`.`trnama` AS `pfterminnama`,`tr`.`trharijatuhtempo` AS `pfterminharijatuhtempo`,`pf`.`pfbagianpembelian` AS `pfbagianpembelian`,`c1`.`kkode` AS `pfbagianpembeliankode`,`c1`.`knama` AS `pfbagianpembeliannama`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`brekpersediaan` AS `brekpersediaan`,`i`.`brekdiskonpembelian` AS `brekdiskonpembelian`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`pfd`.`jmlbarang` - `pfd`.`jmlipc`) / `pfd`.`nilaisatuan`) AS `jmlsisaipc`,((`pfd`.`jmlbarang` - `pfd`.`jmlgrn`) / `pfd`.`nilaisatuan`) AS `jmlsisagrn`,((`pfd`.`jmlbarang` - `pfd`.`jmlri`) / `pfd`.`nilaisatuan`) AS `jmlsisari`,((`pfd`.`jmlbarang` - `pfd`.`jmlrealisasi`) / `pfd`.`nilaisatuan`) AS `jmlsisarealisasi`,`pf`.`pfsupplier` AS `pfsupplier`,`c`.`kkode` AS `pfsupplierkode`,`c`.`knama` AS `pfsuppliernama`, i.bjmllapangan, i.bsatuanlapangan, pf.pfhargatermasukpajak, pf.pfcustomtext1, pf.pfcustomtext2, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama from (((((((`m4_pf_detail` `pfd` join `m4_pf` `pf` on((`pfd`.`idpf` = `pf`.`pfid`))) left join `m1_terms` `tr` on((`pf`.`pftermin` = `tr`.`trkode`))) left join `m1_contact` `c1` on((`pf`.`pfbagianpembelian` = `c1`.`kid`))) left join `m1_item` `i` on((`pfd`.`idbarang` = `i`.`bid`))) left join `m1_tax` `t1` on((`pfd`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`pfd`.`pajak2` = `t2`.`tkode`))) left join `m1_contact` `c` on((`pf`.`pfsupplier` = `c`.`kid`)))"
        sql = "select `pfd`.`idpfdetail` AS `idpfdetail`,`pfd`.`idpf` AS `idpf`,`pfd`.`idbarang` AS `idbarang`,`pfd`.`namabarang` AS `namabarang`,`pfd`.`tipebarang` AS `tipebarang`,`pfd`.`jml` AS `jml`,`pfd`.`satuan` AS `satuan`,`pfd`.`nilaisatuan` AS `nilaisatuan`,`pfd`.`jmlbarang` AS `jmlbarang`,`pfd`.`satuanbarang` AS `satuanbarang`,`pfd`.`matauang` AS `matauang`,`pfd`.`kurs` AS `kurs`,`pfd`.`hargafix` AS `hargafix`,`pfd`.`harga` AS `harga`,`pfd`.`diskon` AS `diskon`,`pfd`.`jmldiskon` AS `jmldiskon`,`pfd`.`pajak1` AS `pajak1`,`pfd`.`jmlpajak1` AS `jmlpajak1`,`pfd`.`pajak2` AS `pajak2`,`pfd`.`jmlpajak2` AS `jmlpajak2`,`pfd`.`cabang` AS `cabang`,`pfd`.`lokasi` AS `lokasi`,`pfd`.`gudang` AS `gudang`,`pfd`.`costcenter` AS `costcenter`,`pfd`.`divisi` AS `divisi`,`pfd`.`subdivisi` AS `subdivisi`,`pfd`.`proyek` AS `proyek`,`pfd`.`catatan` AS `catatan`,`pfd`.`urutan` AS `urutan`,`pfd`.`idprdetail` AS `idprdetail`,`pfd`.`idcsdetail` AS `idcsdetail`,`pfd`.`idrqdetail` AS `idrqdetail`,`pfd`.`idbsdetail` AS `idbsdetail`,`pfd`.`jmlipc` AS `jmlipc`,`pfd`.`statusipc` AS `statusipc`,`pfd`.`jmlgrn` AS `jmlgrn`,`pfd`.`statusgrn` AS `statusgrn`,`pfd`.`jmlri` AS `jmlri`,`pfd`.`statusri` AS `statusri`,`pfd`.`jmldnr` AS `jmldnr`,`pfd`.`statusdnr` AS `statusdnr`,`pfd`.`jmlprt` AS `jmlprt`,`pfd`.`statusrealisasi` AS `statusrealisasi`,`pfd`.`jmlrealisasi` AS `jmlrealisasi`,`pfd`.`statusprt` AS `statusprt`,`pfd`.`isclose` AS `isclose`,`pfd`.`customtext1` AS `customtext1`,`pfd`.`customtext2` AS `customtext2`,`pfd`.`customtext3` AS `customtext3`,`pfd`.`customdbl1` AS `customdbl1`,`pfd`.`customdbl2` AS `customdbl2`,`pfd`.`customdbl3` AS `customdbl3`,`pfd`.`customdate1` AS `customdate1`,`pfd`.`customdate2` AS `customdate2`,`pfd`.`customdate3` AS `customdate3`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,`pf`.`pfuraian` AS `pfuraian`,`pf`.`pfcatatan` AS `pfcatatan`,`pf`.`pfnoref` AS `pfnoref`,`pf`.`pftgl` AS `pftgl`,`pf`.`pftglnoref` AS `pftglnoref`,`pf`.`pfsupplierkontak` AS `pfsupplierkontak`,`pf`.`pf1alamat1` AS `pf1alamat1`,`pf`.`pf1alamat2` AS `pf1alamat2`,`pf`.`pf1alamat3` AS `pf1alamat3`,`pf`.`pf2alamat1` AS `pf2alamat1`,`pf`.`pf2alamat2` AS `pf2alamat2`,`pf`.`pf2alamat3` AS `pf2alamat3`,`pf`.`pftermin` AS `pftermin`,`tr`.`trnama` AS `pfterminnama`,`tr`.`trharijatuhtempo` AS `pfterminharijatuhtempo`,`pf`.`pfbagianpembelian` AS `pfbagianpembelian`,`c1`.`kkode` AS `pfbagianpembeliankode`,`c1`.`knama` AS `pfbagianpembeliannama`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`brekpersediaan` AS `brekpersediaan`,`i`.`brekdiskonpembelian` AS `brekdiskonpembelian`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`pfd`.`jmlbarang` - `pfd`.`jmlipc`) / `pfd`.`nilaisatuan`) AS `jmlsisaipc`,((`pfd`.`jmlbarang` - `pfd`.`jmlgrn`) / `pfd`.`nilaisatuan`) AS `jmlsisagrn`,((`pfd`.`jmlbarang` - `pfd`.`jmlri`) / `pfd`.`nilaisatuan`) AS `jmlsisari`,((`pfd`.`jmlbarang` - `pfd`.`jmlrealisasi`) / `pfd`.`nilaisatuan`) AS `jmlsisarealisasi`,`pf`.`pfsupplier` AS `pfsupplier`,`c`.`kkode` AS `pfsupplierkode`,`c`.`knama` AS `pfsuppliernama`, i.bjmllapangan, i.bsatuanlapangan, pf.pfhargatermasukpajak, pf.pfcustomtext1, pf.pfcustomtext2, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama from `m4_pf_detail` `pfd` join `m4_pf` `pf` on `pfd`.`idpf` = `pf`.`pfid` left join `m1_terms` `tr` on `pf`.`pftermin` = `tr`.`trkode` left join `m1_contact` `c1` on `pf`.`pfbagianpembelian` = `c1`.`kid` left join `m1_item` `i` on `pfd`.`idbarang` = `i`.`bid` left join `m1_tax` `t1` on `pfd`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `pfd`.`pajak2` = `t2`.`tkode` left join `m1_contact` `c` on `pf`.`pfsupplier` = `c`.`kid` left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M5_Sq_Detail", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idpfdetail"), 0), sptField,
                     FxDB(dr("idpf"), 0), sptField,
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
                     Asformattanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("pfnotransaksi"), ""), sptField,
                     FxDB(dr("pfuraian"), ""), sptField,
                     FxDB(dr("pfcatatan"), ""), sptField,
                     FxDB(dr("pfnoref"), ""), sptField,
                     Asformattanggal(FxDB(dr("pftgl"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("pftglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("pfsupplierkontak"), ""), sptField,
                     FxDB(dr("pf1alamat1"), ""), sptField,
                     FxDB(dr("pf1alamat2"), ""), sptField,
                     FxDB(dr("pf1alamat3"), ""), sptField,
                     FxDB(dr("pf2alamat1"), ""), sptField,
                     FxDB(dr("pf2alamat2"), ""), sptField,
                     FxDB(dr("pf2alamat3"), ""), sptField,
                     FxDB(dr("pftermin"), ""), sptField,
                     FxDB(dr("pfterminnama"), ""), sptField,
                     FxDB(dr("pfterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("pfbagianpembelian"), 0), sptField,
                     FxDB(dr("pfbagianpembeliankode"), ""), sptField,
                     FxDB(dr("pfbagianpembeliannama"), ""), sptField,
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
                     FxDB(dr("pfsupplier"), ""), sptField,
                     FxDB(dr("pfsupplierkode"), ""), sptField,
                     FxDB(dr("pfsuppliernama"), ""), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(dr("pfnotransaksi"), ""), sptField,
                     FxDB(dr("pfhargatermasukpajak"), 0), sptField,
                     FxDB(dr("pfcustomtext1"), ""), sptField,
                     FxDB(dr("pfcustomtext2"), ""), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idpfdetail, idpf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusrealisasi, jmlrealisasi, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, pfnotransaksi, pfuraian, pfcatatan, pfnoref, pftgl, pftglnoref, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, pf2alamat3, pftermin, pfterminnama, pfterminharijatuhtempo, pfbagianpembelian, pfbagianpembeliankode, pfbagianpembeliannama, kodebarang, bhpp, bjenis, brekpersediaan, brekdiskonpembelian, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisaipc, jmlsisagrn, jmlsisari, jmlsisarealisasi, pfsupplier, pfsupplierkode, pfsuppliernama, bjmllapangan, bsatuanlapangan, basset, ambilnotransaksi, pfhargatermasukpajak, pfcustomtext1, pfcustomtext2, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_Pf_Detail_Cost(ByVal param As String) As String
        'M4_Pf_Detail_Cost --------------------------------------------------------
        'Detail
        'idpfdetail, idpf, idbarang, namabarang, tipebarang, jml, satuan, 
        'nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, 
        'diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, 
        'lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, 
        'urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, 
        'jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, 
        'statusrealisasi, jmlrealisasi, statusprt, isclose, customtext1, customtext2, customtext3, 
        'customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, pfnotransaksi, 
        'pfuraian, pfcatatan, pfnoref, pftgl, pftglnoref, pfsupplierkontak, pf1alamat1, pf1alamat2, 
        'pf1alamat3, pf2alamat1, pf2alamat2, pf2alamat3, pftermin, pfterminnama, pfterminharijatuhtempo, 
        'pfbagianpembelian, pfbagianpembeliankode, pfbagianpembeliannama, kodebarang, bhpp, bjenis, brekpersediaan, 
        'brekdiskonpembelian, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, 
        'jmlsisaipc, jmlsisagrn, jmlsisari, jmlsisarealisasi, pfsupplier, pfsupplierkode, pfsuppliernama, 
        'bjmllapangan, bsatuanlapangan, basset, ambilnotransaksi, pfhargatermasukpajak, pfcustomtext1, pfcustomtext2,
        'pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, 
        'pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama

        'Cost
        'idpfcost, idpf, kodecost, matauang, kurs, jumlah, rekdebit, 
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
            Filter1 = Filter1.Replace("idbarang", "pfd.idbarang")
            Filter1 = Filter1.Replace("statusrealisasi", "pfd.statusrealisasi")
            Filter1 = Filter1.Replace("isclose", "pfd.isclose")
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
        'sql = "select `pfd`.`idpfdetail` AS `idpfdetail`,`pfd`.`idpf` AS `idpf`,`pfd`.`idbarang` AS `idbarang`,`pfd`.`namabarang` AS `namabarang`,`pfd`.`tipebarang` AS `tipebarang`,`pfd`.`jml` AS `jml`,`pfd`.`satuan` AS `satuan`,`pfd`.`nilaisatuan` AS `nilaisatuan`,CONCAT(`pfd`.`jmlbarang`, '#$') AS `jmlbarang`,`pfd`.`satuanbarang` AS `satuanbarang`,`pfd`.`matauang` AS `matauang`,`pfd`.`kurs` AS `kurs`,`pfd`.`hargafix` AS `hargafix`,`pfd`.`harga` AS `harga`,`pfd`.`diskon` AS `diskon`,`pfd`.`jmldiskon` AS `jmldiskon`,`pfd`.`pajak1` AS `pajak1`,`pfd`.`jmlpajak1` AS `jmlpajak1`,`pfd`.`pajak2` AS `pajak2`,`pfd`.`jmlpajak2` AS `jmlpajak2`,`pfd`.`cabang` AS `cabang`,`pfd`.`lokasi` AS `lokasi`,`pfd`.`gudang` AS `gudang`,`pfd`.`costcenter` AS `costcenter`,`pfd`.`divisi` AS `divisi`,`pfd`.`subdivisi` AS `subdivisi`,`pfd`.`proyek` AS `proyek`,`pfd`.`catatan` AS `catatan`,`pfd`.`urutan` AS `urutan`,`pfd`.`idprdetail` AS `idprdetail`,`pfd`.`idcsdetail` AS `idcsdetail`,`pfd`.`idrqdetail` AS `idrqdetail`,`pfd`.`idbsdetail` AS `idbsdetail`,`pfd`.`jmlipc` AS `jmlipc`,`pfd`.`statusipc` AS `statusipc`,`pfd`.`jmlgrn` AS `jmlgrn`,`pfd`.`statusgrn` AS `statusgrn`,`pfd`.`jmlri` AS `jmlri`,`pfd`.`statusri` AS `statusri`,`pfd`.`jmldnr` AS `jmldnr`,`pfd`.`statusdnr` AS `statusdnr`,`pfd`.`jmlprt` AS `jmlprt`,`pfd`.`statusrealisasi` AS `statusrealisasi`,`pfd`.`jmlrealisasi` AS `jmlrealisasi`,`pfd`.`statusprt` AS `statusprt`,`pfd`.`isclose` AS `isclose`,`pfd`.`customtext1` AS `customtext1`,`pfd`.`customtext2` AS `customtext2`,`pfd`.`customtext3` AS `customtext3`,`pfd`.`customdbl1` AS `customdbl1`,`pfd`.`customdbl2` AS `customdbl2`,`pfd`.`customdbl3` AS `customdbl3`,`pfd`.`customdate1` AS `customdate1`,`pfd`.`customdate2` AS `customdate2`,`pfd`.`customdate3` AS `customdate3`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,`pf`.`pfuraian` AS `pfuraian`,`pf`.`pfcatatan` AS `pfcatatan`,`pf`.`pfnoref` AS `pfnoref`,`pf`.`pftgl` AS `pftgl`,`pf`.`pftglnoref` AS `pftglnoref`,`pf`.`pfsupplierkontak` AS `pfsupplierkontak`,`pf`.`pf1alamat1` AS `pf1alamat1`,`pf`.`pf1alamat2` AS `pf1alamat2`,`pf`.`pf1alamat3` AS `pf1alamat3`,`pf`.`pf2alamat1` AS `pf2alamat1`,`pf`.`pf2alamat2` AS `pf2alamat2`,`pf`.`pf2alamat3` AS `pf2alamat3`,`pf`.`pftermin` AS `pftermin`,`tr`.`trnama` AS `pfterminnama`,`tr`.`trharijatuhtempo` AS `pfterminharijatuhtempo`,`pf`.`pfbagianpembelian` AS `pfbagianpembelian`,`c1`.`kkode` AS `pfbagianpembeliankode`,`c1`.`knama` AS `pfbagianpembeliannama`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`brekpersediaan` AS `brekpersediaan`,`i`.`brekdiskonpembelian` AS `brekdiskonpembelian`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`pfd`.`jmlbarang` - `pfd`.`jmlipc`) / `pfd`.`nilaisatuan`) AS `jmlsisaipc`,((`pfd`.`jmlbarang` - `pfd`.`jmlgrn`) / `pfd`.`nilaisatuan`) AS `jmlsisagrn`,((`pfd`.`jmlbarang` - `pfd`.`jmlri`) / `pfd`.`nilaisatuan`) AS `jmlsisari`, CONCAT(((`pfd`.`jmlbarang` - `pfd`.`jmlrealisasi`) / `pfd`.`nilaisatuan`), '#$') AS `jmlsisarealisasi`,`pf`.`pfsupplier` AS `pfsupplier`,`c`.`kkode` AS `pfsupplierkode`,`c`.`knama` AS `pfsuppliernama`, i.bjmllapangan, i.bsatuanlapangan from (((((((`m4_pf_detail` `pfd` left join `m4_pf` `pf` on((`pfd`.`idpf` = `pf`.`pfid`))) left join `m1_terms` `tr` on((`pf`.`pftermin` = `tr`.`trkode`))) left join `m1_contact` `c1` on((`pf`.`pfbagianpembelian` = `c1`.`kid`))) left join `m1_item` `i` on((`pfd`.`idbarang` = `i`.`bid`))) left join `m1_tax` `t1` on((`pfd`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`pfd`.`pajak2` = `t2`.`tkode`))) left join `m1_contact` `c` on((`pf`.`pfsupplier` = `c`.`kid`)))"
        'sql = " select `pfd`.`idpfdetail` AS `idpfdetail`,`pfd`.`idpf` AS `idpf`,`pfd`.`idbarang` AS `idbarang`,`pfd`.`namabarang` AS `namabarang`,`pfd`.`tipebarang` AS `tipebarang`,`pfd`.`jml` AS `jml`,`pfd`.`satuan` AS `satuan`,`pfd`.`nilaisatuan` AS `nilaisatuan`,`pfd`.`jmlbarang` AS `jmlbarang`,`pfd`.`satuanbarang` AS `satuanbarang`,`pfd`.`matauang` AS `matauang`,`pfd`.`kurs` AS `kurs`,`pfd`.`hargafix` AS `hargafix`,`pfd`.`harga` AS `harga`,`pfd`.`diskon` AS `diskon`,`pfd`.`jmldiskon` AS `jmldiskon`,`pfd`.`pajak1` AS `pajak1`,`pfd`.`jmlpajak1` AS `jmlpajak1`,`pfd`.`pajak2` AS `pajak2`,`pfd`.`jmlpajak2` AS `jmlpajak2`,`pfd`.`cabang` AS `cabang`,`pfd`.`lokasi` AS `lokasi`,`pfd`.`gudang` AS `gudang`,`pfd`.`costcenter` AS `costcenter`,`pfd`.`divisi` AS `divisi`,`pfd`.`subdivisi` AS `subdivisi`,`pfd`.`proyek` AS `proyek`,`pfd`.`catatan` AS `catatan`,`pfd`.`urutan` AS `urutan`,`pfd`.`idprdetail` AS `idprdetail`,`pfd`.`idcsdetail` AS `idcsdetail`,`pfd`.`idrqdetail` AS `idrqdetail`,`pfd`.`idbsdetail` AS `idbsdetail`,`pfd`.`jmlipc` AS `jmlipc`,`pfd`.`statusipc` AS `statusipc`,`pfd`.`jmlgrn` AS `jmlgrn`,`pfd`.`statusgrn` AS `statusgrn`,`pfd`.`jmlri` AS `jmlri`,`pfd`.`statusri` AS `statusri`,`pfd`.`jmldnr` AS `jmldnr`,`pfd`.`statusdnr` AS `statusdnr`,`pfd`.`jmlprt` AS `jmlprt`,`pfd`.`statusrealisasi` AS `statusrealisasi`,`pfd`.`jmlrealisasi` AS `jmlrealisasi`,`pfd`.`statusprt` AS `statusprt`,`pfd`.`isclose` AS `isclose`,`pfd`.`customtext1` AS `customtext1`,`pfd`.`customtext2` AS `customtext2`,`pfd`.`customtext3` AS `customtext3`,`pfd`.`customdbl1` AS `customdbl1`,`pfd`.`customdbl2` AS `customdbl2`,`pfd`.`customdbl3` AS `customdbl3`,`pfd`.`customdate1` AS `customdate1`,`pfd`.`customdate2` AS `customdate2`,`pfd`.`customdate3` AS `customdate3`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,`pf`.`pfuraian` AS `pfuraian`,`pf`.`pfcatatan` AS `pfcatatan`,`pf`.`pfnoref` AS `pfnoref`,`pf`.`pftgl` AS `pftgl`,`pf`.`pftglnoref` AS `pftglnoref`,`pf`.`pfsupplierkontak` AS `pfsupplierkontak`,`pf`.`pf1alamat1` AS `pf1alamat1`,`pf`.`pf1alamat2` AS `pf1alamat2`,`pf`.`pf1alamat3` AS `pf1alamat3`,`pf`.`pf2alamat1` AS `pf2alamat1`,`pf`.`pf2alamat2` AS `pf2alamat2`,`pf`.`pf2alamat3` AS `pf2alamat3`,`pf`.`pftermin` AS `pftermin`,`tr`.`trnama` AS `pfterminnama`,`tr`.`trharijatuhtempo` AS `pfterminharijatuhtempo`,`pf`.`pfbagianpembelian` AS `pfbagianpembelian`,`c1`.`kkode` AS `pfbagianpembeliankode`,`c1`.`knama` AS `pfbagianpembeliannama`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`brekpersediaan` AS `brekpersediaan`,`i`.`brekdiskonpembelian` AS `brekdiskonpembelian`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`pfd`.`jmlbarang` - `pfd`.`jmlipc`) / `pfd`.`nilaisatuan`) AS `jmlsisaipc`,((`pfd`.`jmlbarang` - `pfd`.`jmlgrn`) / `pfd`.`nilaisatuan`) AS `jmlsisagrn`,((`pfd`.`jmlbarang` - `pfd`.`jmlri`) / `pfd`.`nilaisatuan`) AS `jmlsisari`,((`pfd`.`jmlbarang` - `pfd`.`jmlrealisasi`) / `pfd`.`nilaisatuan`) AS `jmlsisarealisasi`,`pf`.`pfsupplier` AS `pfsupplier`,`c`.`kkode` AS `pfsupplierkode`,`c`.`knama` AS `pfsuppliernama`, i.bjmllapangan, i.bsatuanlapangan, pf.pfhargatermasukpajak, pf.pfcustomtext1, pf.pfcustomtext2, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama from (((((((`m4_pf_detail` `pfd` join `m4_pf` `pf` on((`pfd`.`idpf` = `pf`.`pfid`))) left join `m1_terms` `tr` on((`pf`.`pftermin` = `tr`.`trkode`))) left join `m1_contact` `c1` on((`pf`.`pfbagianpembelian` = `c1`.`kid`))) left join `m1_item` `i` on((`pfd`.`idbarang` = `i`.`bid`))) left join `m1_tax` `t1` on((`pfd`.`pajak1` = `t1`.`tkode`))) left join `m1_tax` `t2` on((`pfd`.`pajak2` = `t2`.`tkode`))) left join `m1_contact` `c` on((`pf`.`pfsupplier` = `c`.`kid`)))"
        sql = "select `pfd`.`idpfdetail` AS `idpfdetail`,`pfd`.`idpf` AS `idpf`,`pfd`.`idbarang` AS `idbarang`,`pfd`.`namabarang` AS `namabarang`,`pfd`.`tipebarang` AS `tipebarang`,`pfd`.`jml` AS `jml`,`pfd`.`satuan` AS `satuan`,`pfd`.`nilaisatuan` AS `nilaisatuan`,`pfd`.`jmlbarang` AS `jmlbarang`,`pfd`.`satuanbarang` AS `satuanbarang`,`pfd`.`matauang` AS `matauang`,`pfd`.`kurs` AS `kurs`,`pfd`.`hargafix` AS `hargafix`,`pfd`.`harga` AS `harga`,`pfd`.`diskon` AS `diskon`,`pfd`.`jmldiskon` AS `jmldiskon`,`pfd`.`pajak1` AS `pajak1`,`pfd`.`jmlpajak1` AS `jmlpajak1`,`pfd`.`pajak2` AS `pajak2`,`pfd`.`jmlpajak2` AS `jmlpajak2`,`pfd`.`cabang` AS `cabang`,`pfd`.`lokasi` AS `lokasi`,`pfd`.`gudang` AS `gudang`,`pfd`.`costcenter` AS `costcenter`,`pfd`.`divisi` AS `divisi`,`pfd`.`subdivisi` AS `subdivisi`,`pfd`.`proyek` AS `proyek`,`pfd`.`catatan` AS `catatan`,`pfd`.`urutan` AS `urutan`,`pfd`.`idprdetail` AS `idprdetail`,`pfd`.`idcsdetail` AS `idcsdetail`,`pfd`.`idrqdetail` AS `idrqdetail`,`pfd`.`idbsdetail` AS `idbsdetail`,`pfd`.`jmlipc` AS `jmlipc`,`pfd`.`statusipc` AS `statusipc`,`pfd`.`jmlgrn` AS `jmlgrn`,`pfd`.`statusgrn` AS `statusgrn`,`pfd`.`jmlri` AS `jmlri`,`pfd`.`statusri` AS `statusri`,`pfd`.`jmldnr` AS `jmldnr`,`pfd`.`statusdnr` AS `statusdnr`,`pfd`.`jmlprt` AS `jmlprt`,`pfd`.`statusrealisasi` AS `statusrealisasi`,`pfd`.`jmlrealisasi` AS `jmlrealisasi`,`pfd`.`statusprt` AS `statusprt`,`pfd`.`isclose` AS `isclose`,`pfd`.`customtext1` AS `customtext1`,`pfd`.`customtext2` AS `customtext2`,`pfd`.`customtext3` AS `customtext3`,`pfd`.`customdbl1` AS `customdbl1`,`pfd`.`customdbl2` AS `customdbl2`,`pfd`.`customdbl3` AS `customdbl3`,`pfd`.`customdate1` AS `customdate1`,`pfd`.`customdate2` AS `customdate2`,`pfd`.`customdate3` AS `customdate3`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,`pf`.`pfuraian` AS `pfuraian`,`pf`.`pfcatatan` AS `pfcatatan`,`pf`.`pfnoref` AS `pfnoref`,`pf`.`pftgl` AS `pftgl`,`pf`.`pftglnoref` AS `pftglnoref`,`pf`.`pfsupplierkontak` AS `pfsupplierkontak`,`pf`.`pf1alamat1` AS `pf1alamat1`,`pf`.`pf1alamat2` AS `pf1alamat2`,`pf`.`pf1alamat3` AS `pf1alamat3`,`pf`.`pf2alamat1` AS `pf2alamat1`,`pf`.`pf2alamat2` AS `pf2alamat2`,`pf`.`pf2alamat3` AS `pf2alamat3`,`pf`.`pftermin` AS `pftermin`,`tr`.`trnama` AS `pfterminnama`,`tr`.`trharijatuhtempo` AS `pfterminharijatuhtempo`,`pf`.`pfbagianpembelian` AS `pfbagianpembelian`,`c1`.`kkode` AS `pfbagianpembeliankode`,`c1`.`knama` AS `pfbagianpembeliannama`,`i`.`bkode` AS `kodebarang`,`i`.`bhpp` AS `bhpp`,`i`.`bjenis` AS `bjenis`,`i`.`brekpersediaan` AS `brekpersediaan`,`i`.`brekdiskonpembelian` AS `brekdiskonpembelian`,`i`.`bserial` AS `bserial`,`i`.`bbatch` AS `bbatch`,`i`.`basset` AS `basset`,`t1`.`tnama` AS `pajak1nama`,`t1`.`tnilai` AS `pajak1nilai`,`t2`.`tnama` AS `pajak2nama`,`t2`.`tnilai` AS `pajak2nilai`,((`pfd`.`jmlbarang` - `pfd`.`jmlipc`) / `pfd`.`nilaisatuan`) AS `jmlsisaipc`,((`pfd`.`jmlbarang` - `pfd`.`jmlgrn`) / `pfd`.`nilaisatuan`) AS `jmlsisagrn`,((`pfd`.`jmlbarang` - `pfd`.`jmlri`) / `pfd`.`nilaisatuan`) AS `jmlsisari`,((`pfd`.`jmlbarang` - `pfd`.`jmlrealisasi`) / `pfd`.`nilaisatuan`) AS `jmlsisarealisasi`,`pf`.`pfsupplier` AS `pfsupplier`,`c`.`kkode` AS `pfsupplierkode`,`c`.`knama` AS `pfsuppliernama`, i.bjmllapangan, i.bsatuanlapangan, pf.pfhargatermasukpajak, pf.pfcustomtext1, pf.pfcustomtext2, t1.takunbeli as pajak1akunbeli, t1c1.cnama as pajak1akunbelinama, t1.takunjual as pajak1akunjual, t1c2.cnama as pajak1akunjualnama, t2.takunbeli as pajak2akunbeli, t2c1.cnama as pajak2akunbelinama, t2.takunjual as pajak2akunjual, t2c2.cnama as pajak2akunjualnama from `m4_pf_detail` `pfd` join `m4_pf` `pf` on `pfd`.`idpf` = `pf`.`pfid` left join `m1_terms` `tr` on `pf`.`pftermin` = `tr`.`trkode` left join `m1_contact` `c1` on `pf`.`pfbagianpembelian` = `c1`.`kid` left join `m1_item` `i` on `pfd`.`idbarang` = `i`.`bid` left join `m1_tax` `t1` on `pfd`.`pajak1` = `t1`.`tkode` left join `m1_tax` `t2` on `pfd`.`pajak2` = `t2`.`tkode` left join `m1_contact` `c` on `pf`.`pfsupplier` = `c`.`kid` left join m1_coa t1c1 on t1.takunbeli = t1c1.cnomor left join m1_coa t1c2 on t1.takunjual = t1c2.cnomor left join m1_coa t2c1 on t2.takunbeli = t2c1.cnomor left join m1_coa t2c2 on t2.takunjual = t2c2.cnomor"

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M4_Pf_Detail", Filter1, Sorting1, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("idpfdetail"), 0), sptField,
                     FxDB(dr("idpf"), 0), sptField,
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
                     Asformattanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
                     FxDB(dr("pfnotransaksi"), ""), sptField,
                     FxDB(dr("pfuraian"), ""), sptField,
                     FxDB(dr("pfcatatan"), ""), sptField,
                     FxDB(dr("pfnoref"), ""), sptField,
                     Asformattanggal(FxDB(dr("pftgl"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("pftglnoref"), ""), formatTgl), sptField,
                     FxDB(dr("pfsupplierkontak"), ""), sptField,
                     FxDB(dr("pf1alamat1"), ""), sptField,
                     FxDB(dr("pf1alamat2"), ""), sptField,
                     FxDB(dr("pf1alamat3"), ""), sptField,
                     FxDB(dr("pf2alamat1"), ""), sptField,
                     FxDB(dr("pf2alamat2"), ""), sptField,
                     FxDB(dr("pf2alamat3"), ""), sptField,
                     FxDB(dr("pftermin"), ""), sptField,
                     FxDB(dr("pfterminnama"), ""), sptField,
                     FxDB(dr("pfterminharijatuhtempo"), 0), sptField,
                     FxDB(dr("pfbagianpembelian"), 0), sptField,
                     FxDB(dr("pfbagianpembeliankode"), ""), sptField,
                     FxDB(dr("pfbagianpembeliannama"), ""), sptField,
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
                     FxDB(dr("pfsupplier"), ""), sptField,
                     FxDB(dr("pfsupplierkode"), ""), sptField,
                     FxDB(dr("pfsuppliernama"), ""), sptField,
                     FxDB(dr("bjmllapangan"), 0), sptField,
                     FxDB(dr("bsatuanlapangan"), ""), sptField,
                     FxDB(dr("basset"), 0), sptField,
                     FxDB(dr("pfnotransaksi"), ""), sptField,
                     FxDB(dr("pfhargatermasukpajak"), 0), sptField,
                     FxDB(dr("pfcustomtext1"), ""), sptField,
                     FxDB(dr("pfcustomtext2"), ""), sptField,
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
            sql = "SELECT pfc.idpfcost, pfc.idpf, pfc.kodecost, pfc.matauang, pfc.kurs, pfc.jumlah, pfc.rekdebit, pfc.rekkredit, pfc.kontak, pfc.termasukhpp, pfc.catatan, pfc.costcenter, pfc.divisi, pfc.subdivisi, pfc.proyek, pfc.urutan, pfc.idprcost, pfc.idcscost, pfc.idrqcost, pfc.idbscost, pfc.jumlahipc, pfc.statusipc, pfc.jumlahgrn, pfc.statusgrn, pfc.jumlahri, pfc.statusri, pfc.jumlahbayar, pfc.statusbayar, pfc.isclose, pfc.customtext1, pfc.customtext2, pfc.customtext3, pfc.customdbl1, pfc.customdbl2, pfc.customdbl3, pfc.customdate1, pfc.customdate2, pfc.customdate3, oc.ocnama as kodecostnama, coa1.cnama as rekdebitnama, coa2.cnama as rekkreditnama,  c.kkode as kontakkode, c.knama as kontaknama, cc.ccnama as costcenternama, d.dnama as divisinama, sd.sddivisi as subdivisinama FROM m4_pf_cost pfc JOIN m4_pf pf ON pfc.idpf = pf.pfid LEFT JOIN m1_other_cost oc ON pfc.kodecost = oc.ockode LEFT JOIN m1_coa coa1 ON pfc.rekdebit = coa1.cnomor LEFT JOIN m1_coa coa2 ON pfc.rekkredit = coa2.cnomor LEFT JOIN m1_contact c ON pfc.kontak = c.kid LEFT JOIN m1_cost_center cc ON pfc.costcenter = cc.cckode LEFT JOIN m1_division d ON pfc.divisi = d.dkode LEFT JOIN m1_subdivision sd ON pfc.subdivisi = sd.sdkode"
            Dim dtcost As New DataTable
            dtcost = AmbilData("aplikasi1-m4_pf_cost", Filter2, Sorting2, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
            For Each dr As DataRow In dtcost.Rows
                cost = String.Concat(cost,
                     FxDB(dr("idpfcost"), ""), sptField,
                     FxDB(dr("idpf"), ""), sptField,
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
                     Asformattanggal(FxDB(dr("customdate1"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("customdate2"), ""), formatTgl), sptField,
                     Asformattanggal(FxDB(dr("customdate3"), ""), formatTgl), sptField,
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("idpfdetail, idpf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusrealisasi, jmlrealisasi, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, pfnotransaksi, pfuraian, pfcatatan, pfnoref, pftgl, pftglnoref, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, pf2alamat3, pftermin, pfterminnama, pfterminharijatuhtempo, pfbagianpembelian, pfbagianpembeliankode, pfbagianpembeliannama, kodebarang, bhpp, bjenis, brekpersediaan, brekdiskonpembelian, bserial, bbatch, pajak1nama, pajak1nilai, pajak2nama, pajak2nilai, jmlsisaipc, jmlsisagrn, jmlsisari, jmlsisarealisasi, pfsupplier, pfsupplierkode, pfsuppliernama, bjmllapangan, bsatuanlapangan, basset, ambilnotransaksi, pfhargatermasukpajak, pfcustomtext1, pfcustomtext2, pajak1akunbeli, pajak1akunbelinama, pajak1akunjual, pajak1akunjualnama, pajak2akunbeli, pajak2akunbelinama, pajak2akunjual, pajak2akunjualnama" & sptSubParam & "idpfcost, idpf, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3, kodecostnama, rekdebitnama, rekkreditnama, kontakkode, kontaknama, costcenternama, divisinama, subdivisinama"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M4_PfTerkait(ByVal param As String) As String
        'M4_PfTerkait --------------------------------------------------------
        'pfid, pfnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, 
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

        'VALIDASI DAN SET IDTRANSAKSI ======================================================
        Dim idtransaksi As String = ""
        'CEK IDTRANSAKSI
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "sfid required numeric." : GoTo selesai
        End If

        'SET IDTRANSAKSI
        idtransaksi = paramSplit(3)
        'END OF VALIDASI DAN SET IDTRANSAKSI ===============================================

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
            Filter = pagingSplit(2) & " AND pfid=" & idtransaksi
            '#Taruh fungsi replace disini...
        Else
            Filter = "pfid=" & idtransaksi
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        ''PANGGIL QUERY
        'Dim query As New m0_query
        'sql = query.m5_sf_terkait(Filter)
        sql = m4_pf_terkait(Filter)
        'result(2) = sql : GoTo selesai

        dt = AmbilData("aplikasi1-m4_pf_terkait", , Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("pfid"), 0), sptField,
                     FxDB(dr("pfnotransaksi"), ""), sptField,
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
            result(2) = "Related PF data not found."
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("sfid, sfnotransaksi, sumber, idterkait, noterkait, tglterkait, inputtglterkait, modifikasitglterkait, jenisterkait"))

        Return wsResult
    End Function

    Private Function ValidasiSimpan(ByVal dtdetail As DataTable, ByVal ftExistOutstandingPR As String, ByVal ftOutstandingPR As String, ByVal ftExistOutstandingRQ As String, ByVal ftOutstandingRQ As String, ByVal ftRQ As String, ByVal termasukPajak As String) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtval As New DataTable

        Dim dtLookup As New DataTable, kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuan As String = "", nilaiSatuan As Double = 0, sisa As Double = 0
        Dim filterLookup As String = "", urutan As String = ""

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
    Public Function M4_PfSimpanOld(ByVal param As String) As String
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
        'pfid(0) As Integer, pfcabang(1) As String, pflokasi(2) As String, pfgudang(3) As String, pfasalbarang(4) As String, 
        'pfasalbarangkategori(5) As Integer, pfjenispembelian(6) As String, pfjenispembeliankategori(7) As Integer, pfcarabayar(8) As Integer, pfsumber(9) As String, 
        'pfautonotransaksi(10) As Integer, pfnotransaksi(11) As String, pftgl(12) As Date, pfkodepa(13) As Integer, pfsupplier(14) As Integer, 
        'pfsupplierkontak(15) As String, pf1alamat1(16) As String, pf1alamat2(17) As String, pf1alamat3(18) As String, pf2alamat1(19) As String, 
        'pf2alamat2(20) As String, pf2alamat3(21) As String, pfbagianpembelian(22) As Integer, pftgldipenuhi(23) As Date, pftermin(24) As String, 
        'pftgljatuhtempo(25) As Date, pfuraian(26) As String, pfcatatan(27) As String, pfnoref(28) As String, pftglnoref(29) As Date, 
        'pftglpenutupan(30) As Date, pfmatauang(31) As String, pfkurs(32) As Double, pfhargatermasukpajak(33) As Integer, pftotal(34) As Double, 
        'pfdiskonpersen(35) As String, pfjmldiskon(36) As Double, pftotalpajak1detail(37) As Double, pftotalpajak2detail(38) As Double, pfbiayalainpersen(39) As String, 
        'pfbiayalain(40) As Double, pftotaltransaksi(41) As Double, pfjmlbayar(42) As Double, pfrekdiskon(43) As String, pfrekpajak1(44) As String, 
        'pfrekpajak2(45) As String, pfrekbiayalain(46) As String, pfrekbayar(47) As String, pfidpr(48) As Integer, pfidcs(49) As Integer, 
        'pfidrq(50) As Integer, pfidbs(51) As Integer, pfstatusipc(52) As Integer, pfstatusgrn(53) As Integer, pfstatusri(54) As Integer, 
        'pfstatusdnr(55) As Integer, pfstatusprt(56) As Integer, pfstatus(57) As Integer, pfstatussebelumnya(58) As Integer, pfjmlrevisi(59) As Integer, 
        'pfcetakanke(60) As Integer, pfinputuser(61) As Integer, pfinputtgl(62) As DateTime, pfmodifikasiuser(63) As Integer, pfmodifikasitgl(64) As DateTime, 
        'pfisclose(65) As Integer, pfcustomtext1(66) As String, pfcustomtext2(67) As String, pfcustomtext3(68) As String, pfcustomtext4(69) As String, 
        'pfcustomtext5(70) As String, pfcustomint1(71) As Integer, pfcustomint2(72) As Integer, pfcustomint3(73) As Integer, pfcustomdbl1(74) As Double, 
        'pfcustomdbl2(75) As Double, pfcustomdbl3(76) As Double, pfcustomdate1(77) As Date, pfcustomdate2(78) As Date, pfcustomdate3(79) As Date


        'MAPPING BUAT FLEX ----------------------------------------------------------
        'pfid, pfcabang, pflokasi, pfgudang, pfasalbarang, pfasalbarangkategori, pfjenispembelian, 
        'pfjenispembeliankategori, pfcarabayar, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl, pfkodepa, 
        'pfsupplier, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, 
        'pf2alamat3, pfbagianpembelian, pftgldipenuhi, pftermin, pftgljatuhtempo, pfuraian, pfcatatan, 
        'pfnoref, pftglnoref, pftglpenutupan, pfmatauang, pfkurs, pfhargatermasukpajak, pftotal, 
        'pfdiskonpersen, pfjmldiskon, pftotalpajak1detail, pftotalpajak2detail, pfbiayalainpersen, pfbiayalain, pftotaltransaksi, 
        'pfjmlbayar, pfrekdiskon, pfrekpajak1, pfrekpajak2, pfrekbiayalain, pfrekbayar, pfidpr, 
        'pfidcs, pfidrq, pfidbs, pfstatusipc, pfstatusgrn, pfstatusri, pfstatusdnr, 
        'pfstatusprt, pfstatus, pfstatussebelumnya, pfjmlrevisi, pfcetakanke, pfinputuser, pfinputtgl, 
        'pfmodifikasiuser, pfmodifikasitgl, pfisclose, pfcustomtext1, pfcustomtext2, pfcustomtext3, pfcustomtext4, 
        'pfcustomtext5, pfcustomint1, pfcustomint2, pfcustomint3, pfcustomdbl1, pfcustomdbl2, pfcustomdbl3, 
        'pfcustomdate1, pfcustomdate2, pfcustomdate3

        'VALIDASI DAN SET DATA UTAMA =======================================================
        dataUtama = dataSplit(0).Split(sptField)    'SPLIT PARAMETER DATA UTAMA

        'CEK ARRAY DATA UTAMA
        If (dataUtama.Length <> 80) Then
            result(2) = "Invalid main transaction data parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA UTAMA ================================================

        'VALIDASI TIPE DATA UTAMA ==========================================================
        'pfid(0) As Integer
        If (IsNumeric(dataUtama(0)) = False) Then
            result(2) = "pfid required numeric." : GoTo selesai
        End If
        'pfasalbarangkategori(5) As Integer
        If (IsNumeric(dataUtama(5)) = False) Then
            result(2) = "pfasalbarangkategori required numeric." : GoTo selesai
        End If
        'pfjenispembeliankategori(7) As Integer
        If (IsNumeric(dataUtama(7)) = False) Then
            result(2) = "pfjenispembeliankategori required numeric." : GoTo selesai
        End If
        'pfcarabayar(8) As Integer
        If (IsNumeric(dataUtama(8)) = False) Then
            result(2) = "pfcarabayar required numeric." : GoTo selesai
        End If
        'pfautonotransaksi(10) As Integer
        If (IsNumeric(dataUtama(10)) = False) Then
            result(2) = "pfautonotransaksi required numeric." : GoTo selesai
        End If
        'pftgl(12) As Date
        If (IsDate(dataUtama(12)) = False) Then
            result(2) = "pftgl required date." : GoTo selesai
        End If
        'pfkodepa(13) As Integer
        If (IsNumeric(dataUtama(13)) = False) Then
            result(2) = "pfkodepa required numeric." : GoTo selesai
        End If
        'pfsupplier(14) As Integer
        If (IsNumeric(dataUtama(14)) = False) Then
            result(2) = "pfsupplier required numeric." : GoTo selesai
        End If
        If (dataUtama(14) < 1) Then
            result(2) = "pfsupplier can't be empty." : GoTo selesai
        End If
        'pfbagianpembelian(22) As Integer
        If (IsNumeric(dataUtama(22)) = False) Then
            result(2) = "pfbagianpembelian required numeric." : GoTo selesai
        End If
        'pftgldipenuhi(23) As Date
        If (IsDate(dataUtama(23)) = False) Then
            result(2) = "pftgldipenuhi required date." : GoTo selesai
        End If
        'pftgljatuhtempo(25) As Date
        If (IsDate(dataUtama(25)) = False) Then
            result(2) = "pftgljatuhtempo required date." : GoTo selesai
        End If
        'pftglnoref(29) As Date
        If (IsDate(dataUtama(29)) = False) Then
            result(2) = "pftglnoref required date." : GoTo selesai
        End If
        'pftglpenutupan(30) As Date
        If (IsDate(dataUtama(30)) = False) Then
            result(2) = "pftglpenutupan required date." : GoTo selesai
        End If
        'pfkurs(32) As Double
        If (IsNumeric(dataUtama(32)) = False) Then
            result(2) = "pfkurs required numeric." : GoTo selesai
        End If
        'pfhargatermasukpajak(33) As Integer
        If (IsNumeric(dataUtama(33)) = False) Then
            result(2) = "pfhargatermasukpajak required numeric." : GoTo selesai
        End If
        'pftotal(34) As Double
        If (IsNumeric(dataUtama(34)) = False) Then
            result(2) = "pftotal required numeric." : GoTo selesai
        End If
        'pfjmldiskon(36) As Double
        If (IsNumeric(dataUtama(36)) = False) Then
            result(2) = "pfjmldiskon required numeric." : GoTo selesai
        End If
        'pftotalpajak1detail(37) As Double
        If (IsNumeric(dataUtama(37)) = False) Then
            result(2) = "pftotalpajak1detail required numeric." : GoTo selesai
        End If
        'pftotalpajak2detail(38) As Double
        If (IsNumeric(dataUtama(38)) = False) Then
            result(2) = "pftotalpajak2detail required numeric." : GoTo selesai
        End If
        'pfbiayalain(40) As Double
        If (IsNumeric(dataUtama(40)) = False) Then
            result(2) = "pfbiayalain required numeric." : GoTo selesai
        End If
        'pftotaltransaksi(41) As Double
        If (IsNumeric(dataUtama(41)) = False) Then
            result(2) = "pftotaltransaksi required numeric." : GoTo selesai
        End If
        'pfjmlbayar(42) As Double
        If (IsNumeric(dataUtama(42)) = False) Then
            result(2) = "pfjmlbayar required numeric." : GoTo selesai
        End If
        'pfidpr(48) As Integer
        If (IsNumeric(dataUtama(48)) = False) Then
            result(2) = "pfidpr required numeric." : GoTo selesai
        End If
        'pfidcs(49) As Integer
        If (IsNumeric(dataUtama(49)) = False) Then
            result(2) = "pfidcs required numeric." : GoTo selesai
        End If
        'pfidrq(50) As Integer
        If (IsNumeric(dataUtama(50)) = False) Then
            result(2) = "pfidrq required numeric." : GoTo selesai
        End If
        'pfidbs(51) As Integer
        If (IsNumeric(dataUtama(51)) = False) Then
            result(2) = "pfidbs required numeric." : GoTo selesai
        End If
        'pfstatusipc(52) As Integer
        If (IsNumeric(dataUtama(52)) = False) Then
            result(2) = "pfstatusipc required numeric." : GoTo selesai
        End If
        'pfstatusgrn(53) As Integer
        If (IsNumeric(dataUtama(53)) = False) Then
            result(2) = "pfstatusgrn required numeric." : GoTo selesai
        End If
        'pfstatusri(54) As Integer
        If (IsNumeric(dataUtama(54)) = False) Then
            result(2) = "pfstatusri required numeric." : GoTo selesai
        End If
        'pfstatusdnr(55) As Integer
        If (IsNumeric(dataUtama(55)) = False) Then
            result(2) = "pfstatusdnr required numeric." : GoTo selesai
        End If
        'pfstatusprt(56) As Integer
        If (IsNumeric(dataUtama(56)) = False) Then
            result(2) = "pfstatusprt required numeric." : GoTo selesai
        End If
        'pfstatus(57) As Integer
        If (IsNumeric(dataUtama(57)) = False) Then
            result(2) = "pfstatus required numeric." : GoTo selesai
        End If
        'pfstatussebelumnya(58) As Integer
        If (IsNumeric(dataUtama(58)) = False) Then
            result(2) = "pfstatussebelumnya required numeric." : GoTo selesai
        End If
        'pfjmlrevisi(59) As Integer
        If (IsNumeric(dataUtama(59)) = False) Then
            result(2) = "pfjmlrevisi required numeric." : GoTo selesai
        End If
        'pfcetakanke(60) As Integer
        If (IsNumeric(dataUtama(60)) = False) Then
            result(2) = "pfcetakanke required numeric." : GoTo selesai
        End If
        'pfinputuser(61) As Integer
        If (IsNumeric(dataUtama(61)) = False) Then
            result(2) = "pfinputuser required numeric." : GoTo selesai
        End If
        'pfinputtgl(62) As DateTime
        If (IsDate(dataUtama(62)) = False) Then
            result(2) = "pfinputtgl required date." : GoTo selesai
        End If
        'pfmodifikasiuser(63) As Integer
        If (IsNumeric(dataUtama(63)) = False) Then
            result(2) = "pfmodifikasiuser required numeric." : GoTo selesai
        End If
        'pfmodifikasitgl(64) As DateTime
        If (IsDate(dataUtama(64)) = False) Then
            result(2) = "pfmodifikasitgl required date." : GoTo selesai
        End If
        'pfisclose(65) As Integer
        If (IsNumeric(dataUtama(65)) = False) Then
            result(2) = "pfisclose required numeric." : GoTo selesai
        End If
        'pfcustomint1(71) As Integer
        If (IsNumeric(dataUtama(71)) = False) Then
            result(2) = "pfcustomint1 required numeric." : GoTo selesai
        End If
        'pfcustomint2(72) As Integer
        If (IsNumeric(dataUtama(72)) = False) Then
            result(2) = "pfcustomint2 required numeric." : GoTo selesai
        End If
        'pfcustomint3(73) As Integer
        If (IsNumeric(dataUtama(73)) = False) Then
            result(2) = "pfcustomint3 required numeric." : GoTo selesai
        End If
        'pfcustomdbl1(74) As Double
        If (IsNumeric(dataUtama(74)) = False) Then
            result(2) = "pfcustomdbl1 required numeric." : GoTo selesai
        End If
        'pfcustomdbl2(75) As Double
        If (IsNumeric(dataUtama(75)) = False) Then
            result(2) = "pfcustomdbl2 required numeric." : GoTo selesai
        End If
        'pfcustomdbl3(76) As Double
        If (IsNumeric(dataUtama(76)) = False) Then
            result(2) = "pfcustomdbl3 required numeric." : GoTo selesai
        End If
        'pfcustomdate1(77) As Date
        If (IsDate(dataUtama(77)) = False) Then
            result(2) = "pfcustomdate1 required date." : GoTo selesai
        End If
        'pfcustomdate2(78) As Date
        If (IsDate(dataUtama(78)) = False) Then
            result(2) = "pfcustomdate2 required date." : GoTo selesai
        End If
        'pfcustomdate3(79) As Date
        If (IsDate(dataUtama(79)) = False) Then
            result(2) = "pfcustomdate3 required date." : GoTo selesai
        End If

        'END OF VALIDASI TIPE DATA UTAMA ===================================================

        'VALIDASI DATA UTAMA =======================================================
        'pfcabang(1) As String
        If Len(dataUtama(1)) = 0 Then
            result(2) = "pfcabang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(1)) > 25 Then
            result(2) = "pfcabang should not be more than 25 character." : GoTo selesai
        End If

        'pflokasi(2) As String
        If Len(dataUtama(2)) = 0 Then
            result(2) = "pflokasi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(2)) > 25 Then
            result(2) = "pflokasi should not be more than 25 character." : GoTo selesai
        End If

        'pfgudang(3) As String
        'If Len(dataUtama(3)) = 0 Then
        '    result(2) = "pfgudang can't be empty" : GoTo selesai
        'End If
        If Len(dataUtama(3)) > 25 Then
            result(2) = "pfgudang should not be more than 25 character." : GoTo selesai
        End If

        'pfsumber(9) As String
        If Len(dataUtama(9)) = 0 Then
            result(2) = "pfsumber can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(9)) > 10 Then
            result(2) = "pfsumber should not be more than 10 character." : GoTo selesai
        End If

        'pfnotransaksi(11) As String
        If Len(dataUtama(11)) = 0 Then
            result(2) = "pfnotransaksi can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(11)) > 50 Then
            result(2) = "pfnotransaksi should not be more than 50 character." : GoTo selesai
        End If

        'pftgl(12) As Date
        If Len(dataUtama(12)) = 0 Then
            result(2) = "pftgl can't be empty" : GoTo selesai
        End If

        'pftgldipenuhi(23) As Date
        If Len(dataUtama(23)) = 0 Then
            result(2) = "pftgldipenuhi can't be empty" : GoTo selesai
        End If

        'pftgljatuhtempo(25) As Date
        If Len(dataUtama(25)) = 0 Then
            result(2) = "pftgljatuhtempo can't be empty" : GoTo selesai
        End If

        'pftglnoref(29) As Date
        If Len(dataUtama(29)) = 0 Then
            result(2) = "pftglnoref can't be empty" : GoTo selesai
        End If

        'pftglpenutupan(30) As Date
        If Len(dataUtama(30)) = 0 Then
            result(2) = "pftglpenutupan can't be empty" : GoTo selesai
        End If

        'pfmatauang(31) As String
        If Len(dataUtama(31)) = 0 Then
            result(2) = "pfmatauang can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(31)) > 25 Then
            result(2) = "pfmatauang should not be more than 25 character." : GoTo selesai
        End If

        'pfkurs(32) As Double
        If Len(dataUtama(32)) = 0 Then
            result(2) = "pfkurs can't be empty" : GoTo selesai
        End If

        'pftotal(34) As Double
        If Len(dataUtama(34)) = 0 Then
            result(2) = "pftotal can't be empty" : GoTo selesai
        End If

        'pfdiskonpersen(35) As String
        If Len(dataUtama(35)) = 0 Then
            result(2) = "pfdiskonpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(35)) > 25 Then
            result(2) = "pfdiskonpersen should not be more than 25 character." : GoTo selesai
        End If

        'pfjmldiskon(36) As Double
        If Len(dataUtama(36)) = 0 Then
            result(2) = "pfjmldiskon can't be empty" : GoTo selesai
        End If

        'pftotalpajak1detail(37) As Double
        If Len(dataUtama(37)) = 0 Then
            result(2) = "pftotalpajak1detail can't be empty" : GoTo selesai
        End If

        'pftotalpajak2detail(38) As Double
        If Len(dataUtama(38)) = 0 Then
            result(2) = "pftotalpajak2detail can't be empty" : GoTo selesai
        End If

        'pfbiayalainpersen(39) As String
        If Len(dataUtama(39)) = 0 Then
            result(2) = "pfbiayalainpersen can't be empty" : GoTo selesai
        End If
        If Len(dataUtama(39)) > 25 Then
            result(2) = "pfbiayalainpersen should not be more than 25 character." : GoTo selesai
        End If

        'pfbiayalain(40) As Double
        If Len(dataUtama(40)) = 0 Then
            result(2) = "pfbiayalain can't be empty" : GoTo selesai
        End If

        'pftotaltransaksi(41) As Double
        If Len(dataUtama(41)) = 0 Then
            result(2) = "pftotaltransaksi can't be empty" : GoTo selesai
        End If

        'pfjmlbayar(42) As Double
        If Len(dataUtama(42)) = 0 Then
            result(2) = "pfjmlbayar can't be empty" : GoTo selesai
        End If

        'pfinputtgl(62) As DateTime
        If Len(dataUtama(62)) = 0 Then
            result(2) = "pfinputtgl can't be empty" : GoTo selesai
        End If

        'pfmodifikasitgl(64) As DateTime
        If Len(dataUtama(64)) = 0 Then
            result(2) = "pfmodifikasitgl can't be empty" : GoTo selesai
        End If

        'pfcustomdbl1(74) As Double
        If Len(dataUtama(74)) = 0 Then
            result(2) = "pfcustomdbl1 can't be empty" : GoTo selesai
        End If

        'pfcustomdbl2(75) As Double
        If Len(dataUtama(75)) = 0 Then
            result(2) = "pfcustomdbl2 can't be empty" : GoTo selesai
        End If

        'pfcustomdbl3(76) As Double
        If Len(dataUtama(76)) = 0 Then
            result(2) = "pfcustomdbl3 can't be empty" : GoTo selesai
        End If

        'pfcustomdate1(77) As Date
        If Len(dataUtama(77)) = 0 Then
            result(2) = "pfcustomdate1 can't be empty" : GoTo selesai
        End If

        'pfcustomdate2(78) As Date
        If Len(dataUtama(78)) = 0 Then
            result(2) = "pfcustomdate2 can't be empty" : GoTo selesai
        End If

        'pfcustomdate3(79) As Date
        If Len(dataUtama(79)) = 0 Then
            result(2) = "pfcustomdate3 can't be empty" : GoTo selesai
        End If

        'END OF VALIDASI DATA UTAMA ================================================

        'Buat datatable dtutama
        Dim dtutama As New DataTable
        AsDataTableTambahField(dtutama, "pfid", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcabang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pflokasi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfgudang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfasalbarang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfasalbarangkategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfjenispembelian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfjenispembeliankategori", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfcarabayar", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfsumber", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfautonotransaksi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfnotransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfkodepa", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfsupplier", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfsupplierkontak", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pf1alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pf1alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pf1alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pf2alamat1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pf2alamat2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pf2alamat3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfbagianpembelian", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pftgldipenuhi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftermin", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftgljatuhtempo", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfuraian", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcatatan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftglnoref", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftglpenutupan", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfmatauang", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfkurs", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfhargatermasukpajak", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pftotal", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfdiskonpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfjmldiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftotalpajak1detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftotalpajak2detail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfbiayalainpersen", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pftotaltransaksi", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfjmlbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfrekdiskon", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfrekpajak1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfrekpajak2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfrekbiayalain", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfrekbayar", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfidpr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfidcs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfidrq", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfidbs", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfstatusipc", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfstatusgrn", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfstatusri", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfstatusdnr", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfstatusprt", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfstatus", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfstatussebelumnya", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfjmlrevisi", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfcetakanke", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfinputuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfinputtgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfmodifikasiuser", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfmodifikasitgl", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfisclose", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfcustomtext1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomtext2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomtext3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomtext4", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomtext5", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomint1", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfcustomint2", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfcustomint3", AsEnumTypeData.AsInt64)
        AsDataTableTambahField(dtutama, "pfcustomdbl1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomdbl2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomdbl3", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomdate1", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomdate2", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtutama, "pfcustomdate3", AsEnumTypeData.AsString)
        If AsDataTableTambahData(dtutama, "pfid~pfcabang~pflokasi~pfgudang~pfasalbarang~pfasalbarangkategori~pfjenispembelian~pfjenispembeliankategori~pfcarabayar~pfsumber~pfautonotransaksi~pfnotransaksi~pftgl~pfkodepa~pfsupplier~pfsupplierkontak~pf1alamat1~pf1alamat2~pf1alamat3~pf2alamat1~pf2alamat2~pf2alamat3~pfbagianpembelian~pftgldipenuhi~pftermin~pftgljatuhtempo~pfuraian~pfcatatan~pfnoref~pftglnoref~pftglpenutupan~pfmatauang~pfkurs~pfhargatermasukpajak~pftotal~pfdiskonpersen~pfjmldiskon~pftotalpajak1detail~pftotalpajak2detail~pfbiayalainpersen~pfbiayalain~pftotaltransaksi~pfjmlbayar~pfrekdiskon~pfrekpajak1~pfrekpajak2~pfrekbiayalain~pfrekbayar~pfidpr~pfidcs~pfidrq~pfidbs~pfstatusipc~pfstatusgrn~pfstatusri~pfstatusdnr~pfstatusprt~pfstatus~pfstatussebelumnya~pfjmlrevisi~pfcetakanke~pfinputuser~pfinputtgl~pfmodifikasiuser~pfmodifikasitgl~pfisclose~pfcustomtext1~pfcustomtext2~pfcustomtext3~pfcustomtext4~pfcustomtext5~pfcustomint1~pfcustomint2~pfcustomint3~pfcustomdbl1~pfcustomdbl2~pfcustomdbl3~pfcustomdate1~pfcustomdate2~pfcustomdate3", dataUtama(0) & "~" & dataUtama(1) & "~" & dataUtama(2) & "~" & dataUtama(3) & "~" & dataUtama(4) & "~" & dataUtama(5) & "~" & dataUtama(6) & "~" & dataUtama(7) & "~" & dataUtama(8) & "~" & dataUtama(9) & "~" & dataUtama(10) & "~" & dataUtama(11) & "~" & dataUtama(12) & "~" & dataUtama(13) & "~" & dataUtama(14) & "~" & dataUtama(15) & "~" & dataUtama(16) & "~" & dataUtama(17) & "~" & dataUtama(18) & "~" & dataUtama(19) & "~" & dataUtama(20) & "~" & dataUtama(21) & "~" & dataUtama(22) & "~" & dataUtama(23) & "~" & dataUtama(24) & "~" & dataUtama(25) & "~" & dataUtama(26) & "~" & dataUtama(27) & "~" & dataUtama(28) & "~" & dataUtama(29) & "~" & dataUtama(30) & "~" & dataUtama(31) & "~" & dataUtama(32) & "~" & dataUtama(33) & "~" & dataUtama(34) & "~" & dataUtama(35) & "~" & dataUtama(36) & "~" & dataUtama(37) & "~" & dataUtama(38) & "~" & dataUtama(39) & "~" & dataUtama(40) & "~" & dataUtama(41) & "~" & dataUtama(42) & "~" & dataUtama(43) & "~" & dataUtama(44) & "~" & dataUtama(45) & "~" & dataUtama(46) & "~" & dataUtama(47) & "~" & dataUtama(48) & "~" & dataUtama(49) & "~" & dataUtama(50) & "~" & dataUtama(51) & "~" & dataUtama(52) & "~" & dataUtama(53) & "~" & dataUtama(54) & "~" & dataUtama(55) & "~" & dataUtama(56) & "~" & dataUtama(57) & "~" & dataUtama(58) & "~" & dataUtama(59) & "~" & dataUtama(60) & "~" & dataUtama(61) & "~" & dataUtama(62) & "~" & dataUtama(63) & "~" & dataUtama(64) & "~" & dataUtama(65) & "~" & dataUtama(66) & "~" & dataUtama(67) & "~" & dataUtama(68) & "~" & dataUtama(69) & "~" & dataUtama(70) & "~" & dataUtama(71) & "~" & dataUtama(72) & "~" & dataUtama(73) & "~" & dataUtama(74) & "~" & dataUtama(75) & "~" & dataUtama(76) & "~" & dataUtama(77) & "~" & dataUtama(78) & "~" & dataUtama(79)) = False Then
            result(2) = "Insert into main datatable failed." : GoTo selesai
        End If

        'MAPPING BUAT WS DATA DETAIL -------------------------------------------------------
        'idpfdetail(0) As Integer, idpf(1) As Integer, idbarang(2) As Integer, namabarang(3) As String, tipebarang(4) As String, 
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
        'idpfdetail, idpf, idbarang, namabarang, tipebarang, jml, satuan, 
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
        AsDataTableTambahField(dtdetail, "idpfdetail", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtdetail, "idpf", AsEnumTypeData.AsInt64)
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
            'idpfdetail(0) As Integer
            If (IsNumeric(dataRowDetail(0)) = False) Then
                result(2) = "Row : " & i & " - idpfdetail required numeric." : GoTo selesai
            End If
            'idpf(1) As Integer
            If (IsNumeric(dataRowDetail(1)) = False) Then
                result(2) = "Row : " & i & " - idpf required numeric." : GoTo selesai
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

            If AsDataTableTambahData(dtdetail, "idpfdetail~idpf~idbarang~namabarang~tipebarang~jml~satuan~nilaisatuan~jmlbarang~satuanbarang~matauang~kurs~hargafix~harga~diskon~jmldiskon~pajak1~jmlpajak1~pajak2~jmlpajak2~cabang~lokasi~gudang~costcenter~divisi~subdivisi~proyek~catatan~urutan~idprdetail~idcsdetail~idrqdetail~idbsdetail~jmlipc~statusipc~jmlgrn~statusgrn~jmlri~statusri~jmldnr~statusdnr~jmlprt~statusprt~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowDetail(0) & "~" & dataRowDetail(1) & "~" & dataRowDetail(2) & "~" & dataRowDetail(3) & "~" & dataRowDetail(4) & "~" & dataRowDetail(5) & "~" & dataRowDetail(6) & "~" & dataRowDetail(7) & "~" & dataRowDetail(8) & "~" & dataRowDetail(9) & "~" & dataRowDetail(10) & "~" & dataRowDetail(11) & "~" & dataRowDetail(12) & "~" & dataRowDetail(13) & "~" & dataRowDetail(14) & "~" & dataRowDetail(15) & "~" & dataRowDetail(16) & "~" & dataRowDetail(17) & "~" & dataRowDetail(18) & "~" & dataRowDetail(19) & "~" & dataRowDetail(20) & "~" & dataRowDetail(21) & "~" & dataRowDetail(22) & "~" & dataRowDetail(23) & "~" & dataRowDetail(24) & "~" & dataRowDetail(25) & "~" & dataRowDetail(26) & "~" & dataRowDetail(27) & "~" & dataRowDetail(28) & "~" & dataRowDetail(29) & "~" & dataRowDetail(30) & "~" & dataRowDetail(31) & "~" & dataRowDetail(32) & "~" & dataRowDetail(33) & "~" & dataRowDetail(34) & "~" & dataRowDetail(35) & "~" & dataRowDetail(36) & "~" & dataRowDetail(37) & "~" & dataRowDetail(38) & "~" & dataRowDetail(39) & "~" & dataRowDetail(40) & "~" & dataRowDetail(41) & "~" & dataRowDetail(42) & "~" & dataRowDetail(43) & "~" & dataRowDetail(44) & "~" & dataRowDetail(45) & "~" & dataRowDetail(46) & "~" & dataRowDetail(47) & "~" & dataRowDetail(48) & "~" & dataRowDetail(49) & "~" & dataRowDetail(50) & "~" & dataRowDetail(51) & "~" & dataRowDetail(52)) = False Then
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
        'idpfcost(0) As Integer, idpf(1) As Integer, kodecost(2) As String, matauang(3) As String, kurs(4) As Double, 
        'jumlah(5) As Double, rekdebit(6) As String, rekkredit(7) As String, kontak(8) As Integer, termasukhpp(9) As Integer, 
        'catatan(10) As String, costcenter(11) As String, divisi(12) As String, subdivisi(13) As String, proyek(14) As String, 
        'urutan(15) As Integer, idprcost(16) As Integer, idcscost(17) As Integer, idrqcost(18) As Integer, idbscost(19) As Integer, 
        'jumlahipc(20) As Double, statusipc(21) As Integer, jumlahgrn(22) As Double, statusgrn(23) As Integer, jumlahri(24) As Double, 
        'statusri(25) As Integer, jumlahbayar(26) As Double, statusbayar(27) As Integer, isclose(28) As Integer, customtext1(29) As String, 
        'customtext2(30) As String, customtext3(31) As String, customdbl1(32) As Double, customdbl2(33) As Double, customdbl3(34) As Double, 
        'customdate1(35) As Date, customdate2(36) As Date, customdate3(37) As Date

        'MAPPING BUAT FLEX DATA COST -----------------------------------------------------
        'idpfcost, idpf, kodecost, matauang, kurs, jumlah, rekdebit, 
        'rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, 
        'proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, 
        'statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, 
        'isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, 
        'customdate1, customdate2, customdate3

        'Buat datatable cost
        Dim dtcost As New DataTable
        AsDataTableTambahField(dtcost, "idpfcost", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtcost, "idpf", AsEnumTypeData.AsInt64)
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
                'idpfcost(0) As Integer
                If (IsNumeric(dataRowCost(0)) = False) Then
                    result(2) = "Cost Row : " & i & " - idpfcost required numeric." : GoTo selesai
                End If
                'idpf(1) As Integer
                If (IsNumeric(dataRowCost(1)) = False) Then
                    result(2) = "Cost Row : " & i & " - idpf required numeric." : GoTo selesai
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

                If AsDataTableTambahData(dtcost, "idpfcost~idpf~kodecost~matauang~kurs~jumlah~rekdebit~rekkredit~kontak~termasukhpp~catatan~costcenter~divisi~subdivisi~proyek~urutan~idprcost~idcscost~idrqcost~idbscost~jumlahipc~statusipc~jumlahgrn~statusgrn~jumlahri~statusri~jumlahbayar~statusbayar~isclose~customtext1~customtext2~customtext3~customdbl1~customdbl2~customdbl3~customdate1~customdate2~customdate3", dataRowCost(0) & "~" & dataRowCost(1) & "~" & dataRowCost(2) & "~" & dataRowCost(3) & "~" & dataRowCost(4) & "~" & dataRowCost(5) & "~" & dataRowCost(6) & "~" & dataRowCost(7) & "~" & dataRowCost(8) & "~" & dataRowCost(9) & "~" & dataRowCost(10) & "~" & dataRowCost(11) & "~" & dataRowCost(12) & "~" & dataRowCost(13) & "~" & dataRowCost(14) & "~" & dataRowCost(15) & "~" & dataRowCost(16) & "~" & dataRowCost(17) & "~" & dataRowCost(18) & "~" & dataRowCost(19) & "~" & dataRowCost(20) & "~" & dataRowCost(21) & "~" & dataRowCost(22) & "~" & dataRowCost(23) & "~" & dataRowCost(24) & "~" & dataRowCost(25) & "~" & dataRowCost(26) & "~" & dataRowCost(27) & "~" & dataRowCost(28) & "~" & dataRowCost(29) & "~" & dataRowCost(30) & "~" & dataRowCost(31) & "~" & dataRowCost(32) & "~" & dataRowCost(33) & "~" & dataRowCost(34) & "~" & dataRowCost(35) & "~" & dataRowCost(36) & "~" & dataRowCost(37)) = False Then
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
                'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(Asformattanggal(drutama("pftgl")), Asformattanggal(drutama("pftgl")))
                'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
                'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
                ''END OF CEK PERIODE AKUNTANSI ===========================


                'VALIDASI SIMPAN ========================================
                If drutama("pfstatus") = 2 Then
                    'CEK HAK AKSES
                    '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
                    '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid

                    Dim rsCekHakAkses As String = HakAkses(4, 7, 8, userid) 'MODULEID, MENUID, INDEKS AKSES, USERID SESUAI TRANSAKSI
                    If Len(rsCekHakAkses) <> 0 Then result(2) = rsCekHakAkses : Trans.Rollback() : GoTo selesai

                    'ValidasiSimpan
                    Dim rsValidasi As String = ValidasiSimpan(dtdetail, ftExistOutstandingPR, ftOutstandingPR, ftExistOutstandingRQ, ftOutstandingRQ, ftRQ, drutama("pfhargatermasukpajak"))
                    If Len(rsValidasi) > 0 Then result(2) = rsValidasi : Trans.Rollback() : GoTo selesai
                End If
                'END OF VALIDASI SIMPAN =================================


                ''SET TGL JATUH tempo ====================================
                'Dim rsTglJT(2) As String 'isSuccess(0), hasil(1)
                'rsTglJT = F_TglJT(drutama("pftermin").ToString, Asformattanggal(drutama("pftgl")), "pftgl").Split(sptSubParam)
                'If rsTglJT(0) = 0 Then
                '    result(2) = rsTglJT(1) : Trans.Rollback() : GoTo selesai
                'Else
                '    drutama("pftgljatuhtempo") = Asformattanggal(rsTglJT(1))
                'End If
                ''END OF SET TGL JATUH tempo =============================


                'PERHITUNGAN TOTAL UTAMA ================================
                'DIAMBILKAN DARI DATA DETAIL

                'TAMBAHKAN FIELD SUBTOTAL PADA DETAIL
                'SUBTOTAL = (jml * harga) - jmldiskon
                AsDataTableTambahField(dtdetail, "subtotal", AsEnumTypeData.AsDouble)
                dtdetail.Columns("subtotal").Expression = "(jml * harga) - jmldiskon"

                'TOTAL = subtotal
                drutama("pftotal") = AsDataTableDSum(dtdetail, "subtotal")

                'TOTALPAJAK1 = jmlpajak1
                drutama("pftotalpajak1detail") = AsDataTableDSum(dtdetail, "jmlpajak1")

                'TOTALPAJAK2 = jmlpajak2
                drutama("pftotalpajak2detail") = AsDataTableDSum(dtdetail, "jmlpajak2")

                'JIKA HARGA TIDAK TERMASUK PAJAK MAKA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                'JIKA HARGA TERMASUK PAJAK MAKA TANPA MENAMBAHKAN PAJAK PADA TOTAL TRANSAKSI
                If Integer.Parse(drutama("pfhargatermasukpajak")) = 0 Then
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + TOTALPAJAK1 + TOTALPAJAK2 + BIAYALAIN
                    drutama("pftotaltransaksi") = Double.Parse(drutama("pftotal")) - Double.Parse(drutama("pfjmldiskon")) + Double.Parse(drutama("pftotalpajak1detail")) + Double.Parse(drutama("pftotalpajak2detail")) + Double.Parse(drutama("pfbiayalain"))

                Else
                    'TOTAL TRANSAKSI = TOTAL - JMLDISKON + BIAYALAIN
                    drutama("pftotaltransaksi") = Double.Parse(drutama("pftotal")) - Double.Parse(drutama("pfjmldiskon")) + Double.Parse(drutama("pfbiayalain"))

                End If
                'END OF PERHITUNGAN TOTAL UTAMA =========================


                If isUpdate Then
                    result(4) = drutama("pfid")
                    notransaksi = drutama("pfnotransaksi")
                    'JIKA UPDATE CEK JML ROW PADA DATABASE
                    dtupdate = AsDataTableAmbilDariDB("SELECT COUNT(pfid), pfnotransaksi FROM M4_pf WHERE pfid='" & result(4) & "' AND pfstatus NOT IN(2,3,4,7)")
                    rowUpdate = dtupdate.Rows(0)(0)

                    If (rowUpdate > 0) Then

                        'CEK NO TRANSAKSI ======================
                        If notransaksi <> dtupdate.Rows(0)(1).ToString Then
                            Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(pfid) FROM m4_pf WHERE pfnotransaksi='" & notransaksi & "'")
                            Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                            If cekNo > 0 Then
                                result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                            End If
                        End If
                        'END OF CEK NO TRANSAKSI ===============

                        'SIMPAN HISTORY ========================
                        Dim SimpanHistory As New m4_pf_history
                        Dim rsSimpanHistory As String = SimpanHistory.M4_Pf_HistorySimpan("" & paramSplit(0) & "?M4_Pf_HistorySimpan?0?0???dd/MM/yyyy?dd/MM/yyyy H:mms?" & paramSplit(3) & "?0?" & FixQuotes(drutama("pfsumber")) & "?" & FixQuotes(drutama("pfid")) & "")
                        Dim rsSplit() As String = rsSimpanHistory.Split(sptParam)
                        Dim rsSplitResult() As String = rsSplit(0).Split(sptSubParam)
                        'JIKA ISSUCCES SIMPAN HISTORY = 0 MAKA TAMPILKAN ERRMESSAGE
                        If (rsSplitResult(1) = 0) Then
                            result(2) = "Insert history failed : " & rsSplitResult(2) : Trans.Rollback() : GoTo selesai
                        End If
                        'END OF SIMPAN HISTORY ==================

                        sql = "Update M4_Pf set pfcabang  = '" & FixQuotes(drutama("pfcabang")) & "', pflokasi  = '" & FixQuotes(drutama("pflokasi")) & "', pfgudang  = '" & FixQuotes(drutama("pfgudang")) & "', pfasalbarang  = '" & FixQuotes(drutama("pfasalbarang")) & "', pfasalbarangkategori  = " & drutama("pfasalbarangkategori") & ", pfjenispembelian  = '" & FixQuotes(drutama("pfjenispembelian")) & "', pfjenispembeliankategori  = " & drutama("pfjenispembeliankategori") & ", pfcarabayar  = " & drutama("pfcarabayar") & ", pfsumber  = '" & FixQuotes(drutama("pfsumber")) & "', pfautonotransaksi  = " & drutama("pfautonotransaksi") & ", pfnotransaksi  = '" & notransaksi & "', pftgl  = '" & FixQuotes(Asformattanggal(drutama("pftgl"))) & "', pfkodepa  = " & drutama("pfkodepa") & ", pfsupplier  = " & drutama("pfsupplier") & ", pfsupplierkontak  = '" & FixQuotes(drutama("pfsupplierkontak")) & "', pf1alamat1  = '" & FixQuotes(drutama("pf1alamat1")) & "', pf1alamat2  = '" & FixQuotes(drutama("pf1alamat2")) & "', pf1alamat3  = '" & FixQuotes(drutama("pf1alamat3")) & "', pf2alamat1  = '" & FixQuotes(drutama("pf2alamat1")) & "', pf2alamat2  = '" & FixQuotes(drutama("pf2alamat2")) & "', pf2alamat3  = '" & FixQuotes(drutama("pf2alamat3")) & "', pfbagianpembelian  = " & drutama("pfbagianpembelian") & ", pftgldipenuhi  = '" & FixQuotes(Asformattanggal(drutama("pftgldipenuhi"))) & "', pftermin  = '" & FixQuotes(drutama("pftermin")) & "', pftgljatuhtempo  = '" & FixQuotes(Asformattanggal(drutama("pftgljatuhtempo"))) & "', pfuraian  = '" & FixQuotes(drutama("pfuraian")) & "', pfcatatan  = '" & FixQuotes(drutama("pfcatatan")) & "', pfnoref  = '" & FixQuotes(drutama("pfnoref")) & "', pftglnoref  = '" & FixQuotes(Asformattanggal(drutama("pftglnoref"))) & "', pftglpenutupan  = '" & FixQuotes(Asformattanggal(drutama("pftglpenutupan"))) & "', pfmatauang  = '" & FixQuotes(drutama("pfmatauang")) & "', pfkurs  = '" & FixDouble(drutama("pfkurs")) & "', pfhargatermasukpajak  = " & drutama("pfhargatermasukpajak") & ", pftotal  = '" & FixDouble(drutama("pftotal")) & "', pfdiskonpersen  = '" & FixQuotes(drutama("pfdiskonpersen")) & "', pfjmldiskon  = '" & FixDouble(drutama("pfjmldiskon")) & "', pftotalpajak1detail  = '" & FixDouble(drutama("pftotalpajak1detail")) & "', pftotalpajak2detail  = '" & FixDouble(drutama("pftotalpajak2detail")) & "', pfbiayalainpersen  = '" & FixQuotes(drutama("pfbiayalainpersen")) & "', pfbiayalain  = '" & FixDouble(drutama("pfbiayalain")) & "', pftotaltransaksi  = '" & FixDouble(drutama("pftotaltransaksi")) & "', pfjmlbayar  = '" & FixDouble(drutama("pfjmlbayar")) & "', pfrekdiskon  = '" & FixQuotes(drutama("pfrekdiskon")) & "', pfrekpajak1  = '" & FixQuotes(drutama("pfrekpajak1")) & "', pfrekpajak2  = '" & FixQuotes(drutama("pfrekpajak2")) & "', pfrekbiayalain  = '" & FixQuotes(drutama("pfrekbiayalain")) & "', pfrekbayar  = '" & FixQuotes(drutama("pfrekbayar")) & "', pfidpr  = " & drutama("pfidpr") & ", pfidcs  = " & drutama("pfidcs") & ", pfidrq  = " & drutama("pfidrq") & ", pfidbs  = " & drutama("pfidbs") & ", pfstatusipc  = " & drutama("pfstatusipc") & ", pfstatusgrn  = " & drutama("pfstatusgrn") & ", pfstatusri  = " & drutama("pfstatusri") & ", pfstatusdnr  = " & drutama("pfstatusdnr") & ", pfstatusprt  = " & drutama("pfstatusprt") & ", pfstatus  = " & drutama("pfstatus") & ", pfstatussebelumnya  = " & drutama("pfstatussebelumnya") & ", pfjmlrevisi  = pfjmlrevisi+1, pfcetakanke  = " & drutama("pfcetakanke") & ", pfmodifikasiuser  = " & drutama("pfmodifikasiuser") & ", pfmodifikasitgl  = NOW(), pfcustomtext1  = '" & FixQuotes(drutama("pfcustomtext1")) & "', pfcustomtext2  = '" & FixQuotes(drutama("pfcustomtext2")) & "', pfcustomtext3  = '" & FixQuotes(drutama("pfcustomtext3")) & "', pfcustomtext4  = '" & FixQuotes(drutama("pfcustomtext4")) & "', pfcustomtext5  = '" & FixQuotes(drutama("pfcustomtext5")) & "', pfcustomint1  = " & drutama("pfcustomint1") & ", pfcustomint2  = " & drutama("pfcustomint2") & ", pfcustomint3  = " & drutama("pfcustomint3") & ", pfcustomdbl1  = '" & FixDouble(drutama("pfcustomdbl1")) & "', pfcustomdbl2  = '" & FixDouble(drutama("pfcustomdbl2")) & "', pfcustomdbl3  = '" & FixDouble(drutama("pfcustomdbl3")) & "', pfcustomdate1  = '" & FixQuotes(Asformattanggal(drutama("pfcustomdate1"))) & "', pfcustomdate2  = '" & FixQuotes(Asformattanggal(drutama("pfcustomdate2"))) & "', pfcustomdate3  = '" & FixQuotes(Asformattanggal(drutama("pfcustomdate3"))) & "' where pfid = '" & drutama("pfid") & "'"
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

                    If drutama("pfautonotransaksi") = 1 Then

                        'GENERATE NOTRANSAKSI =========================================
                        Dim wsM0_Nomor As New m0_nomor
                        Dim rsNotransaksi As String = wsM0_Nomor.M0_Notransaksi(drutama("pfcabang"), drutama("pflokasi"), drutama("pfsumber"), drutama("pftgl"))
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
                        notransaksi = drutama("pfnotransaksi")
                    End If

                    'CEK NO TRANSAKSI ======================
                    Dim dtCekNo As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(pfid) FROM m4_pf WHERE pfnotransaksi='" & notransaksi & "'")
                    Dim cekNo As Double = Val(dtCekNo.Rows(0)(0))
                    If cekNo > 0 Then
                        result(2) = "No. : '" & notransaksi & "' - has been used." : Trans.Rollback() : GoTo selesai
                    End If
                    'END OF CEK NO TRANSAKSI ===============

                    sql = "Insert into M4_Pf (pfcabang, pflokasi, pfgudang, pfasalbarang, pfasalbarangkategori, pfjenispembelian, pfjenispembeliankategori, pfcarabayar, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl, pfkodepa, pfsupplier, pfsupplierkontak, pf1alamat1, pf1alamat2, pf1alamat3, pf2alamat1, pf2alamat2, pf2alamat3, pfbagianpembelian, pftgldipenuhi, pftermin, pftgljatuhtempo, pfuraian, pfcatatan, pfnoref, pftglnoref, pftglpenutupan, pfmatauang, pfkurs, pfhargatermasukpajak, pftotal, pfdiskonpersen, pfjmldiskon, pftotalpajak1detail, pftotalpajak2detail, pfbiayalainpersen, pfbiayalain, pftotaltransaksi, pfjmlbayar, pfrekdiskon, pfrekpajak1, pfrekpajak2, pfrekbiayalain, pfrekbayar, pfidpr, pfidcs, pfidrq, pfidbs, pfstatusipc, pfstatusgrn, pfstatusri, pfstatusdnr, pfstatusprt, pfstatus, pfstatussebelumnya, pfjmlrevisi, pfcetakanke, pfinputuser, pfinputtgl, pfmodifikasiuser, pfmodifikasitgl, pfisclose, pfcustomtext1, pfcustomtext2, pfcustomtext3, pfcustomtext4, pfcustomtext5, pfcustomint1, pfcustomint2, pfcustomint3, pfcustomdbl1, pfcustomdbl2, pfcustomdbl3, pfcustomdate1, pfcustomdate2, pfcustomdate3) values('" & FixQuotes(drutama("pfcabang")) & "', '" & FixQuotes(drutama("pflokasi")) & "', '" & FixQuotes(drutama("pfgudang")) & "', '" & FixQuotes(drutama("pfasalbarang")) & "', " & drutama("pfasalbarangkategori") & ", '" & FixQuotes(drutama("pfjenispembelian")) & "', " & drutama("pfjenispembeliankategori") & ", " & drutama("pfcarabayar") & ", '" & FixQuotes(drutama("pfsumber")) & "', " & drutama("pfautonotransaksi") & ", '" & notransaksi & "', '" & FixQuotes(Asformattanggal(drutama("pftgl"))) & "', " & drutama("pfkodepa") & ", " & drutama("pfsupplier") & ", '" & FixQuotes(drutama("pfsupplierkontak")) & "', '" & FixQuotes(drutama("pf1alamat1")) & "', '" & FixQuotes(drutama("pf1alamat2")) & "', '" & FixQuotes(drutama("pf1alamat3")) & "', '" & FixQuotes(drutama("pf2alamat1")) & "', '" & FixQuotes(drutama("pf2alamat2")) & "', '" & FixQuotes(drutama("pf2alamat3")) & "', " & drutama("pfbagianpembelian") & ", '" & FixQuotes(Asformattanggal(drutama("pftgldipenuhi"))) & "', '" & FixQuotes(drutama("pftermin")) & "', '" & FixQuotes(Asformattanggal(drutama("pftgljatuhtempo"))) & "', '" & FixQuotes(drutama("pfuraian")) & "', '" & FixQuotes(drutama("pfcatatan")) & "', '" & FixQuotes(drutama("pfnoref")) & "', '" & FixQuotes(Asformattanggal(drutama("pftglnoref"))) & "', '" & FixQuotes(Asformattanggal(drutama("pftglpenutupan"))) & "', '" & FixQuotes(drutama("pfmatauang")) & "', '" & FixDouble(drutama("pfkurs")) & "', " & drutama("pfhargatermasukpajak") & ", '" & FixDouble(drutama("pftotal")) & "', '" & FixQuotes(drutama("pfdiskonpersen")) & "', '" & FixDouble(drutama("pfjmldiskon")) & "', '" & FixDouble(drutama("pftotalpajak1detail")) & "', '" & FixDouble(drutama("pftotalpajak2detail")) & "', '" & FixQuotes(drutama("pfbiayalainpersen")) & "', '" & FixDouble(drutama("pfbiayalain")) & "', '" & FixDouble(drutama("pftotaltransaksi")) & "', '" & FixDouble(drutama("pfjmlbayar")) & "', '" & FixQuotes(drutama("pfrekdiskon")) & "', '" & FixQuotes(drutama("pfrekpajak1")) & "', '" & FixQuotes(drutama("pfrekpajak2")) & "', '" & FixQuotes(drutama("pfrekbiayalain")) & "', '" & FixQuotes(drutama("pfrekbayar")) & "', " & drutama("pfidpr") & ", " & drutama("pfidcs") & ", " & drutama("pfidrq") & ", " & drutama("pfidbs") & ", " & drutama("pfstatusipc") & ", " & drutama("pfstatusgrn") & ", " & drutama("pfstatusri") & ", " & drutama("pfstatusdnr") & ", " & drutama("pfstatusprt") & ", " & drutama("pfstatus") & ", " & drutama("pfstatussebelumnya") & ", " & drutama("pfjmlrevisi") & ", " & drutama("pfcetakanke") & ", " & drutama("pfinputuser") & ", NOW(), " & drutama("pfmodifikasiuser") & ", '1971-01-01 00:00:00', " & drutama("pfisclose") & ", '" & FixQuotes(drutama("pfcustomtext1")) & "', '" & FixQuotes(drutama("pfcustomtext2")) & "', '" & FixQuotes(drutama("pfcustomtext3")) & "', '" & FixQuotes(drutama("pfcustomtext4")) & "', '" & FixQuotes(drutama("pfcustomtext5")) & "', " & drutama("pfcustomint1") & ", " & drutama("pfcustomint2") & ", " & drutama("pfcustomint3") & ", '" & FixDouble(drutama("pfcustomdbl1")) & "', '" & FixDouble(drutama("pfcustomdbl2")) & "', '" & FixDouble(drutama("pfcustomdbl3")) & "', '" & FixQuotes(Asformattanggal(drutama("pfcustomdate1"))) & "', '" & FixQuotes(Asformattanggal(drutama("pfcustomdate2"))) & "', '" & FixQuotes(Asformattanggal(drutama("pfcustomdate3"))) & "')"
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
                    dt2 = AsDataTableAmbilDariDB("select pfid from M4_pf where pfnotransaksi='" & notransaksi & "' AND pfinputuser= '" & userid & "' order by pfmodifikasitgl desc limit 1")
                    If dt2.Rows.Count > 0 Then result(4) = dt2.Rows(0)(0) Else result(2) = "Main transaction data not found." : Trans.Rollback() : GoTo selesai
                End If

                'Hapus detail ketika update
                If (isUpdate) Then
                    sql = "Delete from M4_Pf_Detail where idpf = '" & result(4) & "'"
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
                        strValue2.Append("(" & dr1("idpfdetail") & ", " & result(4) & ", " & dr1("idbarang") & ", '" & FixQuotes(dr1("namabarang")) & "', '" & FixQuotes(dr1("tipebarang")) & "', '" & FixDouble(dr1("jml")) & "', '" & FixQuotes(dr1("satuan")) & "', '" & FixDouble(dr1("nilaisatuan")) & "', '" & FixDouble(dr1("jmlbarang")) & "', '" & FixQuotes(dr1("satuanbarang")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', " & dr1("hargafix") & ", '" & FixDouble(dr1("harga")) & "', '" & FixQuotes(dr1("diskon")) & "', '" & FixQuotes(dr1("jmldiskon")) & "', '" & FixQuotes(dr1("pajak1")) & "', '" & FixDouble(dr1("jmlpajak1")) & "', '" & FixQuotes(dr1("pajak2")) & "', '" & FixDouble(dr1("jmlpajak2")) & "', '" & FixQuotes(dr1("cabang")) & "', '" & FixQuotes(dr1("lokasi")) & "', '" & FixQuotes(dr1("gudang")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', '" & FixQuotes(dr1("catatan")) & "', " & dr1("urutan") & ", " & dr1("idprdetail") & ", " & dr1("idcsdetail") & ", " & dr1("idrqdetail") & ", " & dr1("idbsdetail") & ", '" & FixDouble(dr1("jmlipc")) & "', " & dr1("statusipc") & ", '" & FixDouble(dr1("jmlgrn")) & "', " & dr1("statusgrn") & ", '" & FixDouble(dr1("jmlri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jmldnr")) & "', " & dr1("statusdnr") & ", '" & FixDouble(dr1("jmlprt")) & "', " & dr1("statusprt") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(Asformattanggal(dr1("customdate1"))) & "', '" & FixQuotes(Asformattanggal(dr1("customdate2"))) & "', '" & FixQuotes(Asformattanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Pf_Detail(idpfdetail, idpf, idbarang, namabarang, tipebarang, jml, satuan, nilaisatuan, jmlbarang, satuanbarang, matauang, kurs, hargafix, harga, diskon, jmldiskon, pajak1, jmlpajak1, pajak2, jmlpajak2, cabang, lokasi, gudang, costcenter, divisi, subdivisi, proyek, catatan, urutan, idprdetail, idcsdetail, idrqdetail, idbsdetail, jmlipc, statusipc, jmlgrn, statusgrn, jmlri, statusri, jmldnr, statusdnr, jmlprt, statusprt, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                    sql = "Delete from M4_Pf_Cost where idpf = " & result(4)
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
                        strValue2.Append("(" & dr1("idpfcost") & ", " & result(4) & ", '" & FixQuotes(dr1("kodecost")) & "', '" & FixQuotes(dr1("matauang")) & "', '" & FixDouble(dr1("kurs")) & "', '" & FixDouble(dr1("jumlah")) & "', '" & FixQuotes(dr1("rekdebit")) & "', '" & FixQuotes(dr1("rekkredit")) & "', " & dr1("kontak") & ", " & dr1("termasukhpp") & ", '" & FixQuotes(dr1("catatan")) & "', '" & FixQuotes(dr1("costcenter")) & "', '" & FixQuotes(dr1("divisi")) & "', '" & FixQuotes(dr1("subdivisi")) & "', '" & FixQuotes(dr1("proyek")) & "', " & dr1("urutan") & ", " & dr1("idprcost") & ", " & dr1("idcscost") & ", " & dr1("idrqcost") & ", " & dr1("idbscost") & ", '" & FixDouble(dr1("jumlahipc")) & "', " & dr1("statusipc") & ", '" & FixDouble(dr1("jumlahgrn")) & "', " & dr1("statusgrn") & ", '" & FixDouble(dr1("jumlahri")) & "', " & dr1("statusri") & ", '" & FixDouble(dr1("jumlahbayar")) & "', " & dr1("statusbayar") & ", " & dr1("isclose") & ", '" & FixQuotes(dr1("customtext1")) & "', '" & FixQuotes(dr1("customtext2")) & "', '" & FixQuotes(dr1("customtext3")) & "', '" & FixDouble(dr1("customdbl1")) & "', '" & FixDouble(dr1("customdbl2")) & "', '" & FixDouble(dr1("customdbl3")) & "', '" & FixQuotes(Asformattanggal(dr1("customdate1"))) & "', '" & FixQuotes(Asformattanggal(dr1("customdate2"))) & "', '" & FixQuotes(Asformattanggal(dr1("customdate3"))) & "')")
                    Next
                    sql = "Insert into M4_Pf_Cost(idpfcost, idpf, kodecost, matauang, kurs, jumlah, rekdebit, rekkredit, kontak, termasukhpp, catatan, costcenter, divisi, subdivisi, proyek, urutan, idprcost, idcscost, idrqcost, idbscost, jumlahipc, statusipc, jumlahgrn, statusgrn, jumlahri, statusri, jumlahbayar, statusbayar, isclose, customtext1, customtext2, customtext3, customdbl1, customdbl2, customdbl3, customdate1, customdate2, customdate3) values" & strValue2.ToString & ""
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
                If drutama("pfstatus") = 2 Then
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
                        sql = "INSERT INTO m1_item_booking_pf (idbarang, gudang, jmlbooking) VALUES " & updStokBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
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
                Dim sumber As String = "PF", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
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
    Public Function M4_PfUpdateStatusOld(ByVal param As String) As String

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
            Filter = Filter.Replace("pfsupplierkode", "c1.kkode")
            Filter = Filter.Replace("pfsuppliernama", "c1.knama")
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
            Dim sumber As String = "Pf", tglTransaksi As String = ""
            Dim mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0, statusTransaksi As Integer = 0
            'ambil moduleid, menuid dari m0_nomor dan tgl, notransaksi, status dari transaksi
            dtdetail = AsDataTableAmbilDariDB("SELECT moduleid, menuid, 0 FROM m0_nomor WHERE kodetabel='" & sumber & _
                                              "' UNION SELECT Pftgl, Pfnotransaksi, Pfstatus FROM M4_Pf WHERE Pfid='" & idtransaksi & "'")
            If dtdetail.Rows.Count > 1 Then
                '       moduleid                     menuid                               tgl                                 notransaksi           status
                mdlid = dtdetail.Rows(0)(0) : mnid = dtdetail.Rows(0)(1) : tglTransaksi = dtdetail.Rows(1)(0) : notransaksi = dtdetail.Rows(1)(1) : statusTransaksi = dtdetail.Rows(1)(2)
            Else
                result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN INSERT USER LOG ===================================================

            'JIKA UNCLOSE MAKA SET NILAI STATUS = STATUSSEBELUMNYA, JNSAKTIVITAS = 17. ELSE JNSAKTIVITAS = NILAISTATUS
            If nilaiStatus = "unclose" Then
                nilaiStatus = "Pfstatussebelumnya" : jnsaktivitas = 17
                'CEK STATUS TRANSAKSI, JIKA <> 7 MAKA TIDAK BISA UNCLOSE
                If statusTransaksi <> 7 Then result(2) = "Transaction has not closed, it can't be unclose." : Trans.Rollback() : GoTo selesai
            Else
                jnsaktivitas = nilaiStatus
            End If

            'SET ISDELETE = TRUE JIKA STATUS TRANSAKSI = 2/3/4/7 DAN JNS AKTIVITAS <> 7(CLOSE) & 17(UNCLOSE)
            If ((statusTransaksi = 2 Or statusTransaksi = 3 Or statusTransaksi = 4 Or statusTransaksi = 7) And jnsaktivitas <> 7 And jnsaktivitas <> 17) Then isDelete = True

            ''CEK PERIODE AKUNTANSI ==============================================================
            'Dim arrCekPeriode(2) As String 'success(0), errmessage(1)
            'Dim rsCekPeriode As String = M2_Accounting_PeriodeCheck(Asformattanggal(tglTransaksi), Asformattanggal(tglTransaksi))
            'arrCekPeriode = rsCekPeriode.Split(sptSubParam)
            'If arrCekPeriode(0) = 0 Then result(2) = arrCekPeriode(1) : Trans.Rollback() : GoTo selesai
            ''END OF CEK PERIODE AKUNTANSI =======================================================

            'SIMPAN HISTORY ========================
            Dim SimpanHistory As New m4_pf_history
            Dim rsSimpanHistory As String = SimpanHistory.M4_Pf_HistorySimpan("" & paramSplit(0) & "?M4_Pf_HistorySimpan?0?0???dd/MM/yyyy?dd/MM/yyyy H:mms?" & paramSplit(3) & "?0?" & FixQuotes(sumber) & "?" & FixQuotes(idtransaksi) & "")
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
                sql = query.PanggilQuery("m4_pf_terkait")
                sql = sql.Replace("validtransaksi", idtransaksi)
                Dim dtTerkait As DataTable = AsDataTableAmbilDariDB(sql)
                dtTerkait = AsDataTableFilterLimit(dtTerkait, "jenisterkait = 1", , , 1)
                If dtTerkait.Rows.Count > 0 Then result(2) = "Can't update '" & notransaksi & "'. It has related transactions." : Trans.Rollback() : GoTo selesai
                'END OF CEK TERKAIT =============================================================

                Dim idbarang As Integer = 0, jmlbarang As Double = 0, idprdetail As Integer = 0, idrqdetail As Integer = 0
                Dim updNilaiPR As String = "", updFilterPR As String = "", updNilaiRQ As String = "", updFilterRQ As String = ""
                Dim gudang As String = "", updStokBooking As String = ""

                'AMBIL DATA DETAIL
                dtdetail = AsDataTableAmbilDariDB("SELECT idbarang, tipebarang, namabarang, satuan, nilaisatuan, jmlbarang, gudang, idprdetail, idrqdetail, urutan FROM m4_pf_detail WHERE idpf = '" & idtransaksi & "'")
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
                    sql = "INSERT INTO m1_item_booking_pf (idbarang, gudang, jmlbooking) VALUES " & updStokBooking & " ON DUPLICATE KEY UPDATE jmlbooking = jmlbooking + VALUES(jmlbooking)"
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
            sql = "UPDATE M4_Pf SET Pfstatus = " & nilaiStatus & ", Pfmodifikasiuser='" & userid & "', Pfmodifikasitgl = NOW(), pfposting = 0, pfpostingtgl = '1971-01-01 00:00:00', Pfjmlrevisi = Pfjmlrevisi + 1 WHERE Pfid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PfSearch(PostWsSearch(paramSplit(0), "M4_PfSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function M4_PfDeleteOld(ByVal param As String) As String

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
            Filter = Filter.Replace("pfsupplierkode", "c1.kkode")
            Filter = Filter.Replace("pfsuppliernama", "c1.knama")
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
            Dim sumber As String = "Pf", notransaksi As String = "", mdlid As Integer = 0, mnid As Integer = 0, jnsaktivitas As Integer = 0
            'ambil moduleid dan menuid dari m0_nomor
            Dim dtnomor As DataTable = AsDataTableAmbilDariDB("SELECT moduleid, menuid FROM m0_nomor WHERE kodetabel='" & sumber & "' UNION SELECT Pfid, Pfnotransaksi FROM M4_Pf WHERE Pfid='" & idtransaksi & "'")
            If dtnomor.Rows.Count > 1 Then mdlid = dtnomor.Rows(0)(0) : mnid = dtnomor.Rows(0)(1) : notransaksi = dtnomor.Rows(1)(1) Else result(2) = "#1. Transaction data not found." : Trans.Rollback() : GoTo selesai
            'hapus : jnsaktivitas = 12
            jnsaktivitas = 12
            'END OF PERSIAPAN INSERT USER LOG ===================================================


            'PERSIAPAN UPDATE NOMOR BERIKUTNYA ==================================================
            Dim cabang As String = "", lokasi As String = "", autonotransaksi As Integer = 0, tgl As String = ""
            sql = "  SELECT pfcabang, pflokasi, pfsumber, pfautonotransaksi, pfnotransaksi, pftgl"
            sql &= " FROM M4_pf"
            sql &= " WHERE pfid = '" & FixDouble(idtransaksi) & "'"
            Dim dtNomorNext As DataTable = AsDataTableAmbilDariDB(sql)
            If dtNomorNext.Rows.Count > 0 Then
                cabang = dtNomorNext.Rows(0)("pfcabang")
                lokasi = dtNomorNext.Rows(0)("pflokasi")
                sumber = dtNomorNext.Rows(0)("pfsumber")
                autonotransaksi = Double.Parse(dtNomorNext.Rows(0)("pfautonotransaksi"))
                notransaksi = dtNomorNext.Rows(0)("pfnotransaksi")
                tgl = Asformattanggal(dtNomorNext.Rows(0)("pftgl"))
            Else
                result(2) = "#2. Transaction data not found." : Trans.Rollback() : GoTo selesai
            End If
            'END OF PERSIAPAN UPDATE NOMOR BERIKUTNYA ===========================================


            'DELETE COST
            sql = "DELETE FROM M4_pf_Cost WHERE idpf ='" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE DETAIL
            sql = "DELETE FROM M4_Pf_Detail WHERE idpf = '" & idtransaksi & "'"
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = Con1
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            'DELETE UTAMA
            sql = "DELETE FROM M4_Pf WHERE pfid = '" & idtransaksi & "'"
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
            Dim paramSearch As String = M4_PfSearch(PostWsSearch(paramSplit(0), "M4_PfSearch", pagingSplit(0), pagingSplit(1), Filter, Sorting, formatTgl, formatTglWaktu))
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
    Public Function m4_pf_terkait(ByVal strFilter As String) As String
        Dim sql As String
        Dim filter1 As String = "", filter2 As String = "", filter3 As String = "", filter4 As String = "", filter5 As String = "", filter6 As String = "", filter7 As String = "", filter8 As String = "", filter9 As String = ""

        'Replace Filter & Sort
        If (strFilter.Length > 0) Then
            'filter1 = strFilter
            'filter1 = filter1 & " AND ((`m4_pr`.`prstatus` = 2) or (`m4_pr`.`prstatus` = 3) or (`m4_pr`.`prstatus` = 4) or (`m4_pr`.`prstatus` = 7))"

            filter2 = strFilter
            filter2 = filter2 & " AND ((`m4_po`.`postatus` = 2) or (`m4_po`.`postatus` = 3) or (`m4_po`.`postatus` = 4) or (`m4_po`.`postatus` = 7))"

            'filter3 = strFilter
            'filter3 = filter3 & " AND ((`m5_pl`.`plstatus` = 2) or (`m5_pl`.`plstatus` = 3) or (`m5_pl`.`plstatus` = 4) or (`m5_pl`.`plstatus` = 7))"

            'filter4 = strFilter
            'filter4 = filter4 & " AND ((`m5_do`.`dostatus` = 2) or (`m5_do`.`dostatus` = 3) or (`m5_do`.`dostatus` = 4) or (`m5_do`.`dostatus` = 7))"

            'filter5 = strFilter
            'filter5 = filter5 & " AND ((`m5_dr`.`drstatus` = 2) or (`m5_dr`.`drstatus` = 3) or (`m5_dr`.`drstatus` = 4) or (`m5_dr`.`drstatus` = 7))"

            'filter6 = strFilter
            'filter6 = filter6 & " AND ((`m5_pi`.`pistatus` = 2) or (`m5_pi`.`pistatus` = 3) or (`m5_pi`.`pistatus` = 4) or (`m5_pi`.`pistatus` = 7))"

            'filter7 = strFilter
            'filter7 = filter7 & " AND ((`m5_si`.`sistatus` = 2) or (`m5_si`.`sistatus` = 3) or (`m5_si`.`sistatus` = 4) or (`m5_si`.`sistatus` = 7))"

            'filter8 = strFilter
            'filter8 = filter8 & " AND ((`m5_rnr`.`rnrstatus` = 2) or (`m5_rnr`.`rnrstatus` = 3) or (`m5_rnr`.`rnrstatus` = 4) or (`m5_rnr`.`rnrstatus` = 7))"

            'filter9 = strFilter
            'filter9 = filter9 & " AND ((`m5_sr`.`srstatus` = 2) or (`m5_sr`.`srstatus` = 3) or (`m5_sr`.`srstatus` = 4) or (`m5_sr`.`srstatus` = 7))"
        Else
            'Default filter
            'filter1 = "((`m4_pr`.`prstatus` = 2) or (`m4_pr`.`prstatus` = 3) or (`m4_pr`.`prstatus` = 4) or (`m4_pr`.`prstatus` = 7))"
            filter2 = "((`m4_po`.`postatus` = 2) or (`m4_po`.`postatus` = 3) or (`m4_po`.`postatus` = 4) or (`m4_po`.`postatus` = 7))"
            'filter3 = "((`m5_pl`.`plstatus` = 2) or (`m5_pl`.`plstatus` = 3) or (`m5_pl`.`plstatus` = 4) or (`m5_pl`.`plstatus` = 7))"
            'filter4 = "((`m5_do`.`dostatus` = 2) or (`m5_do`.`dostatus` = 3) or (`m5_do`.`dostatus` = 4) or (`m5_do`.`dostatus` = 7))"
            'filter5 = "((`m5_dr`.`drstatus` = 2) or (`m5_dr`.`drstatus` = 3) or (`m5_dr`.`drstatus` = 4) or (`m5_dr`.`drstatus` = 7))"
            'filter6 = "((`m5_pi`.`pistatus` = 2) or (`m5_pi`.`pistatus` = 3) or (`m5_pi`.`pistatus` = 4) or (`m5_pi`.`pistatus` = 7))"
            'filter7 = "((`m5_si`.`sistatus` = 2) or (`m5_si`.`sistatus` = 3) or (`m5_si`.`sistatus` = 4) or (`m5_si`.`sistatus` = 7))"
            'filter8 = "((`m5_rnr`.`rnrstatus` = 2) or (`m5_rnr`.`rnrstatus` = 3) or (`m5_rnr`.`rnrstatus` = 4) or (`m5_rnr`.`rnrstatus` = 7))"
            'filter9 = "((`m5_sr`.`srstatus` = 2) or (`m5_sr`.`srstatus` = 3) or (`m5_sr`.`srstatus` = 4) or (`m5_sr`.`srstatus` = 7))"
        End If

        'If Len(filter1) > 0 Then filter1 = " WHERE " & filter1
        If Len(filter2) > 0 Then filter2 = " WHERE " & filter2
        'If Len(filter3) > 0 Then filter3 = " WHERE " & filter3
        'If Len(filter4) > 0 Then filter4 = " WHERE " & filter4
        'If Len(filter5) > 0 Then filter5 = " WHERE " & filter5
        'If Len(filter6) > 0 Then filter6 = " WHERE " & filter6
        'If Len(filter7) > 0 Then filter7 = " WHERE " & filter7
        'If Len(filter8) > 0 Then filter8 = " WHERE " & filter8
        'If Len(filter9) > 0 Then filter9 = " WHERE " & filter9


        'sql = "select `so`.`pfid` AS `pfid`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,'PR' AS `sumber`,`m4_pr`.`prid` AS `idterkait`,`m4_pr`.`prnotransaksi` AS `noterkait`,`m4_pr`.`prtgl` AS `tglterkait`,`m4_pr`.`prinputtgl` AS `inputtglterkait`,`m4_pr`.`prmodifikasitgl` AS `modifikasitglterkait`, 0 as jenisterkait from (((`m4_pf_detail` `pfd` join `m4_pf` `pf` on((`pfd`.`idpf` = `pf`.`pfid`))) join `m4_pr_detail` on((`m4_pr_detail`.`idprdetail` = `pfd`.`idprdetail`))) join `m4_pr` on((`m4_pr_detail`.`idpr` = `m4_pr`.`prid`))) " & filter1 & "  group by `pf`.`pfid`, `m4_pr`.`prid` "
        'sql &= " UNION ALL "
        'sql &= "select `pf`.`pfid` AS `pfid`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,'PR' AS `sumber`,`m4_pr`.`prid` AS `idterkait`,`m4_pr`.`prnotransaksi` AS `noterkait`,`m4_pr`.`prtgl` AS `tglterkait`,`m4_pr`.`prinputtgl` AS `inputtglterkait`,`m4_pr`.`prmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m4_pf_detail` `pfd` join `m4_pf` `pf` on((`pfd`.`idpf` = `pf`.`pfid`))) join `m4_pr_detail` on((`m4_pr_detail`.`idpfdetail` = `pfd`.`idpfdetail`))) join `m4_pr` on((`m4_pr_detail`.`idpr` = `m4_pr`.`prid`))) " & filter1 & "  group by `pf`.`pfid`, `m4_pr`.`prid` "
        'sql &= " UNION ALL "
        sql = " select `pf`.`pfid` AS `pfid`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,'PO' AS `sumber`,`m4_po`.`poid` AS `idterkait`,`m4_po`.`ponotransaksi` AS `noterkait`, `m4_po`.`potgl` AS `tglterkait`,`m4_po`.`poinputtgl` AS `inputtglterkait`,`m4_po`.`pomodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from `m4_pf` `pf` join `m4_po` on `pf`.`pfid` = `m4_po`.`pocustomint1` " & filter2 & "  group by `pf`.`pfid`, `m4_po`.`poid`  "
        'sql &= " UNION ALL "
        'sql &= "select `pf`.`pfid` AS `pfid`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,'PL' AS `sumber`,`m5_pl`.`plid` AS `idterkait`,`m5_pl`.`plnotransaksi` AS `noterkait`,`m5_pl`.`pltgl` AS `tglterkait`,`m5_pl`.`plinputtgl` AS `inputtglterkait`,`m5_pl`.`plmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m4_pf_detail` `pfd` join `m4_pf` `pf` on((`pfd`.`idpf` = `pf`.`pfid`))) join `m5_pl_detail` on((`m5_pl_detail`.`idpfdetail` = `pfd`.`idpfdetail`))) join `m5_pl` on((`m5_pl_detail`.`idpl` = `m5_pl`.`plid`))) " & filter3 & "  group by `pf`.`pfid`, `m5_pl`.`plid` "
        'sql &= " UNION ALL "
        'sql &= "select `pf`.`pfid` AS `pfid`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,'DO' AS `sumber`,`m5_do`.`doid` AS `idterkait`,`m5_do`.`donotransaksi` AS `noterkait`,`m5_do`.`dotgl` AS `tglterkait`,`m5_do`.`doinputtgl` AS `inputtglterkait`,`m5_do`.`domodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m4_pf_detail` `pfd` join `m4_pf` `pf` on((`pfd`.`idpf` = `pf`.`pfid`))) join `m5_do_detail` on((`m5_do_detail`.`idpfdetail` = `pfd`.`idpfdetail`))) join `m5_do` on((`m5_do_detail`.`iddo` = `m5_do`.`doid`))) " & filter4 & "  group by `pf`.`pfid`, `m5_do`.`doid` "
        'sql &= " UNION ALL "
        'sql &= "select `pf`.`pfid` AS `pfid`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,'DR' AS `sumber`,`m5_dr`.`drid` AS `idterkait`,`m5_dr`.`drnotransaksi` AS `noterkait`,`m5_dr`.`drtgl` AS `tglterkait`,`m5_dr`.`drinputtgl` AS `inputtglterkait`,`m5_dr`.`drmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m4_pf_detail` `pfd` join `m4_pf` `pf` on((`pfd`.`idpf` = `pf`.`pfid`))) join `m5_dr_detail` on((`m5_dr_detail`.`idpfdetail` = `pfd`.`idpfdetail`))) join `m5_dr` on((`m5_dr_detail`.`iddr` = `m5_dr`.`drid`))) " & filter5 & "  group by `pf`.`pfid`, `m5_dr`.`drid` "
        'sql &= " UNION ALL "
        'sql &= "select `pf`.`pfid` AS `pfid`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,'PI' AS `sumber`,`m5_pi`.`piid` AS `idterkait`,`m5_pi`.`pinotransaksi` AS `noterkait`,`m5_pi`.`pitgl` AS `tglterkait`,`m5_pi`.`piinputtgl` AS `inputtglterkait`,`m5_pi`.`pimodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m4_pf_detail` `pfd` join `m4_pf` `pf` on((`pfd`.`idpf` = `pf`.`pfid`))) join `m5_pi_detail` on((`m5_pi_detail`.`idpfdetail` = `pfd`.`idpfdetail`))) join `m5_pi` on((`m5_pi_detail`.`idpi` = `m5_pi`.`piid`))) " & filter6 & "  group by `pf`.`pfid`, `m5_pi`.`piid` "
        'sql &= " UNION ALL "
        'sql &= "select `pf`.`pfid` AS `pfid`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,'SI' AS `sumber`,`m5_si`.`siid` AS `idterkait`,`m5_si`.`sinotransaksi` AS `noterkait`,`m5_si`.`sitgl` AS `tglterkait`,`m5_si`.`siinputtgl` AS `inputtglterkait`,`m5_si`.`simodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m4_pf_detail` `pfd` join `m4_pf` `pf` on((`pfd`.`idpf` = `pf`.`pfid`))) join `m5_si_detail` on((`m5_si_detail`.`idpfdetail` = `pfd`.`idpfdetail`))) join `m5_si` on((`m5_si_detail`.`idsi` = `m5_si`.`siid`))) " & filter7 & "  group by `pf`.`pfid`, `m5_si`.`siid` "
        'sql &= " UNION ALL "
        'sql &= "select `pf`.`pfid` AS `pfid`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,'RNR' AS `sumber`,`m5_rnr`.`rnrid` AS `idterkait`,`m5_rnr`.`rnrnotransaksi` AS `noterkait`,`m5_rnr`.`rnrtgl` AS `tglterkait`,`m5_rnr`.`rnrinputtgl` AS `inputtglterkait`,`m5_rnr`.`rnrmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m4_pf_detail` `pfd` join `m4_pf` `pf` on((`pfd`.`idpf` = `pf`.`pfid`))) join `m5_rnr_detail` on((`m5_rnr_detail`.`idpfdetail` = `pfd`.`idpfdetail`))) join `m5_rnr` on((`m5_rnr_detail`.`idrnr` = `m5_rnr`.`rnrid`))) " & filter8 & "  group by `pf`.`pfid`, `m5_rnr`.`rnrid` "
        'sql &= " UNION ALL "
        'sql &= "select `pf`.`pfid` AS `pfid`,`pf`.`pfnotransaksi` AS `pfnotransaksi`,'SR' AS `sumber`,`m5_sr`.`srid` AS `idterkait`,`m5_sr`.`srnotransaksi` AS `noterkait`,`m5_sr`.`srtgl` AS `tglterkait`,`m5_sr`.`srinputtgl` AS `inputtglterkait`,`m5_sr`.`srmodifikasitgl` AS `modifikasitglterkait`, 1 as jenisterkait from (((`m4_pf_detail` `pfd` join `m4_pf` `pf` on((`pfd`.`idpf` = `pf`.`pfid`))) join `m5_sr_detail` on((`m5_sr_detail`.`idpfdetail` = `pfd`.`idpfdetail`))) join `m5_sr` on((`m5_sr_detail`.`idsr` = `m5_sr`.`srid`))) " & filter9 & "  group by `pf`.`pfid`, `m5_sr`.`srid`"

        Return sql
    End Function
End Class