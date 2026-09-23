Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.IO

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")>
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Public Class m0_login_s
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M0_Logout(ByVal param As String) As String

        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", sql As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", strResult As String = "", strResultPaging As String = "", AppCode As String = ""

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
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

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


        'APP CODE
        If (Len(paramSplit(5)) = 0) Then
            result(2) = "App Code can't be empty" : GoTo selesai
        Else
            AppCode = paramSplit(5).ToString
        End If


        ''REMOVE MEMCACHED WEBSITE ACCESS KEY ===============================================
        'If Not IsNothing(AsMemcached.GetCache("myerpplus-" & Application("AppCode") & "-" & paramSplit(0))) Then
        '    AsMemcached.Remove("myerpplus-" & Application("AppCode") & "-" & paramSplit(0))
        'End If
        ''END OF REMOVE MEMCACHED WEBSITE ACCESS KEY ========================================


        'TRANSAKSI KE DATABASE =============================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'HAPUS TABEL TEMPORARY REPORT -------------------------------
        'AMBIL DAFTAR TABEL PENAMPUNG REPORT PROGRESS
        Dim dtReportTemp As DataTable = AsDataTableAmbilDariDB("SELECT tnama FROM m0_report_temp")
        If dtReportTemp.Rows.Count > 0 Then
            For Each dr As DataRow In dtReportTemp.Rows
                sql = "DELETE FROM " & dr("tnama") & " WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
                If AsEksekusiSQL(sql) = False Then
                    result(2) = "Failed remove " & dr("tnama") & " data." : GoTo selesai
                End If
            Next
        End If
        'END OF HAPUS TABEL TEMPORARY REPORT ------------------------


        'HAPUS DATA LOGIN -------------------------------------------
        sql = "DELETE FROM m0_userlogin WHERE ulid = '" & FixQuotes(paramSplit(0)) & "'"
        If AsEksekusiSQL(sql) = False Then
            result(2) = "Failed remove user login data." : GoTo selesai
        End If

        'TAMBAHKAN MSMQ
        'tipe = login/check/logout
        Dim tipeMsmq As String = "logout"
        Dim hasilMsmq As String = SendMsmqLogin(dirMsmqUserLogin, tipeMsmq, paramSplit(0), userid, AppCode)
        If Len(hasilMsmq) > 0 Then
            result(2) = hasilMsmq : GoTo selesai
        End If
        'END OF HAPUS DATA LOGIN ------------------------------------

        result(1) = 1

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_LoginUserS(ByVal param As String) As String

        On Error GoTo selesai
        Dim formatTgl As String = "yyyy-MM-dd", formatTglWaktu As String = "yyyy-MM-dd H:mm:ss", search As String = "", bahasa As String = "", sql As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", strUser As String = "", strUserModule As String = "", strHakAkses As String = "", strSetting As String = ""
        Dim strUserMenu As String = "", strUserCustom As String = "", strUserReport As String = "", strNomor As String = "", strAccPeriod As String = ""
        Dim strReport As String = "", strContactCat As String = "", strFormSetGlobal As String = "", strSentence As String = "", _AccessKey As String = "", strHardware As String = "", strMenuSerenity As String = "", strReportfilter As String = ""
        Dim strResult As String = "", strResultPaging As String = ""
        Dim username As String = "", password As String = "", AppKey As String = "", AppSecret As String = "", AppCode As String = "", LoginReplace As Integer = 0
        Dim contents As String = "", lokasi As String = ""

        'UNTUK POS
        Dim strSettingPOS As String = ""
        Dim strBonusItem As String = "", strBonusItemDetail As String = "", strSubsItem As String = "", strSubsItemDetail As String = "", strAddItem As String = "", strAddItemDetail As String = ""
        Dim strDiscItem As String = "", strDiscCatItem As String = "", strBonusTrans As String = "", strBonusTransDetail As String = ""
        Dim strPointItem As String = "", strPointCatItem As String = "", strPointNominal As String = ""

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
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

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

        ''CEK FORMATTGL
        'If Len(pagingSplit(4)) = 0 Then
        '    formatTgl = "yyyy-MM-dd"
        'Else
        '    formatTgl = pagingSplit(4)
        'End If

        'CEK FORMATTGLWAKTU
        'If Len(pagingSplit(5)) = 0 Then
        '    formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        'Else
        '    formatTglWaktu = pagingSplit(5)
        'End If
        'END OF VALIDASI PARAMETER PAGING ==================================================


        'SET DAN VALIDASI VARIABEL USER ====================================================
        Dim dataSplit() As String = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataSplit.Length <> 6) Then
            result(2) = "Invalid login data parameter." : GoTo selesai
        End If

        'APPKEY
        If (Len(dataSplit(0)) = 0) Then
            result(2) = "App Key can't be empty" : GoTo selesai
        Else
            AppKey = dataSplit(0).ToString
        End If

        'APPSECRET
        If (Len(dataSplit(1)) = 0) Then
            result(2) = "App Secret can't be empty" : GoTo selesai
        Else
            AppSecret = dataSplit(1).ToString
        End If

        'USERNAME
        If (Len(dataSplit(2)) = 0) Then
            result(2) = "Username can't be empty" : GoTo selesai
        Else
            username = dataSplit(2).ToString
        End If

        'PASSWORD
        If (Len(dataSplit(3)) = 0) Then
            result(2) = "Password can't be empty" : GoTo selesai
        Else
            password = dataSplit(3).ToString
        End If

        'APP CODE
        If (Len(dataSplit(4)) = 0) Then
            result(2) = "App Code can't be empty" : GoTo selesai
        Else
            AppCode = dataSplit(4).ToString
        End If

        'LOGIN REPLACE
        If (IsNumeric(dataSplit(5)) = False) Then
            result(2) = "Login Replace required numeric" : GoTo selesai
        Else
            LoginReplace = Integer.Parse(dataSplit(5))
        End If
        'END OF SET DAN VALIDASI VARIABEL USER =============================================


        'PROSES LOGIN ======================================================================
        'CEK APPKEY DAN APPSECRET
        AppKey = AsAntiSQLInjection(AppKey)
        AppSecret = AsAntiSQLInjection(AppSecret)

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim myApp As DataTable
        myApp = AsDataTableAmbilDariDB("SELECT (appid) FROM m0_app WHERE appkey='" & AppKey & "' AND appsecret='" & AppSecret & "'")
        If myApp.Rows.Count = 0 Then
            result(2) = "Invalid App Key or App Secret." : GoTo selesai
        End If

        'CEK DATA USER
        Dim myUser As DataTable
        myUser = AsDataTableAmbilDariDB("SELECT userid, ukode, unama, upassword, uaktif, utglexpired, ubahasa, ulevel, ulokasi FROM m0_user WHERE ukode='" & username & "'")
        If myUser.Rows.Count > 0 Then

            Dim drUser As DataRow = myUser.Rows(0)
            Dim dateNow As Date = Now, userkode As String = ""

            'SET TGL EXPIRED
            Dim expired() = Split(AsFormatTanggal(drUser("utglexpired"), "yyyy-MM-dd"), "-")
            Dim dateExpired As New Date(expired(0), expired(1), expired(2))

            'AMBIL USERID
            userid = drUser("userid")
            userkode = drUser("ukode")
            bahasa = drUser("ubahasa")
            lokasi = FxDB(drUser("ulokasi"), "")

            'CEK PASSWORD 
            If drUser("upassword") = CreateSHAHash(password, "AlEuPj13") Then

            Else
                result(2) = "Invalid password." : GoTo selesai
            End If

        Else
            result(2) = "Invalid username." : GoTo selesai
        End If
        'END OF PROSES LOGIN ===============================================================

        result(1) = 1

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(result(2))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M0_LoginDataS(ByVal param As String) As String

        On Error GoTo selesai
        Dim formatTgl As String = "yyyy-MM-dd", formatTglWaktu As String = "yyyy-MM-dd H:mm:ss", search As String = "", bahasa As String = "", sql As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", strUser As String = "", strUserModule As String = "", strHakAkses As String = "", strSetting As String = ""
        Dim strUserMenu As String = "", strUserCustom As String = "", strUserReport As String = "", strNomor As String = "", strAccPeriod As String = ""
        Dim strReport As String = "", strContactCat As String = "", strFormSetGlobal As String = "", strSentence As String = "", _AccessKey As String = "", strHardware As String = "", strMenuSerenity As String = "", strReportfilter As String = "", strTranslateS As String = "", strReportOrder As String = "", strReportGroup As String = ""
        Dim strResult As String = "", strResultPaging As String = ""
        Dim username As String = "", password As String = "", AppKey As String = "", AppSecret As String = "", AppCode As String = "", LoginReplace As Integer = 0
        Dim contents As String = "", lokasi As String = ""

        'UNTUK POS
        Dim strSettingPOS As String = ""
        Dim strBonusItem As String = "", strBonusItemDetail As String = "", strSubsItem As String = "", strSubsItemDetail As String = "", strAddItem As String = "", strAddItemDetail As String = ""
        Dim strDiscItem As String = "", strDiscCatItem As String = "", strBonusTrans As String = "", strBonusTransDetail As String = ""
        Dim strPointItem As String = "", strPointCatItem As String = "", strPointNominal As String = ""

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
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

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

        ''CEK FORMATTGL
        'If Len(pagingSplit(4)) = 0 Then
        '    formatTgl = "yyyy-MM-dd"
        'Else
        '    formatTgl = pagingSplit(4)
        'End If

        'CEK FORMATTGLWAKTU
        'If Len(pagingSplit(5)) = 0 Then
        '    formatTglWaktu = "yyyy-MM-dd H:mm:ss"
        'Else
        '    formatTglWaktu = pagingSplit(5)
        'End If
        'END OF VALIDASI PARAMETER PAGING ==================================================


        'SET DAN VALIDASI VARIABEL USER ====================================================
        Dim dataSplit() As String = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataSplit.Length <> 6) Then
            result(2) = "Invalid login data parameter." : GoTo selesai
        End If

        'APPKEY
        If (Len(dataSplit(0)) = 0) Then
            result(2) = "App Key can't be empty" : GoTo selesai
        Else
            AppKey = dataSplit(0).ToString
        End If

        'APPSECRET
        If (Len(dataSplit(1)) = 0) Then
            result(2) = "App Secret can't be empty" : GoTo selesai
        Else
            AppSecret = dataSplit(1).ToString
        End If

        'USERNAME
        If (Len(dataSplit(2)) = 0) Then
            result(2) = "Username can't be empty" : GoTo selesai
        Else
            username = dataSplit(2).ToString
        End If

        'PASSWORD
        If (Len(dataSplit(3)) = 0) Then
            result(2) = "Password can't be empty" : GoTo selesai
        Else
            password = dataSplit(3).ToString
        End If

        'APP CODE
        If (Len(dataSplit(4)) = 0) Then
            result(2) = "App Code can't be empty" : GoTo selesai
        Else
            AppCode = dataSplit(4).ToString
        End If

        'LOGIN REPLACE
        If (IsNumeric(dataSplit(5)) = False) Then
            result(2) = "Login Replace required numeric" : GoTo selesai
        Else
            LoginReplace = Integer.Parse(dataSplit(5))
        End If
        'END OF SET DAN VALIDASI VARIABEL USER =============================================


        'PROSES LOGIN ======================================================================
        'CEK APPKEY DAN APPSECRET
        AppKey = AsAntiSQLInjection(AppKey)
        AppSecret = AsAntiSQLInjection(AppSecret)

        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        Dim myApp As DataTable
        myApp = AsDataTableAmbilDariDB("SELECT (appid) FROM m0_app WHERE appkey='" & AppKey & "' AND appsecret='" & AppSecret & "'")
        If myApp.Rows.Count = 0 Then
            result(2) = "Invalid App Key or App Secret." : GoTo selesai
        End If

        'CEK DATA USER
        Dim myUser As DataTable
        myUser = AsDataTableAmbilDariDB("SELECT userid, ukode, unama, upassword, uaktif, utglexpired, ubahasa, ulevel, ulokasi FROM m0_user WHERE ukode='" & username & "'")
        If myUser.Rows.Count > 0 Then

            Dim drUser As DataRow = myUser.Rows(0)
            Dim dateNow As Date = Now, userkode As String = ""

            'SET TGL EXPIRED
            Dim expired() = Split(AsFormatTanggal(drUser("utglexpired"), "yyyy-MM-dd"), "-")
            Dim dateExpired As New Date(expired(0), expired(1), expired(2))

            'AMBIL USERID
            userid = drUser("userid")
            userkode = drUser("ukode")
            bahasa = drUser("ubahasa")
            lokasi = FxDB(drUser("ulokasi"), "")

            If drUser("ulevel") = 3 Then
                'JIKA user Serenity
            ElseIf drUser("ulevel") = 4 Then
                'JIKE user Pusat dan Serenity
            Else
                'JIKA Lainnya
                result(2) = "Level users can't log on Serenity" : GoTo selesai

            End If

            'CEK PASSWORD 
            If drUser("upassword") = drUser("upassword") Then
                'CEK USER ROLE
                Dim myRole As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(userid) FROM m0_user_role WHERE userid='" & userid & "'")
                If Val(myRole.Rows(0)(0)) = 0 Then
                    result(2) = "User '" & userkode & "' doesn't have any role." : GoTo selesai
                End If

                'CEK AKTIF
                If drUser("uaktif") = 1 Then
                    'CEK TGL EXPIRED
                    If Date.Compare(dateNow, dateExpired) < 0 Then

                        'CEK LEVEL USER (0 : user pos, 1 : user pusat, 2 : user pos dan pusat)
                        'DIBANDINGKAN DENGAN SETTING APP ISSERVER ATAU BUKAN
                        Dim IsServer As String = F_getSetting(0, "company", "IsServer")
                        Dim LokasiSetting As String = F_getSetting(0, "company", "Lokasi")
                        Dim LokasiSettingNama As String = F_getSetting(0, "company", "LokasiNama")

                        'If IsServer = 1 And drUser("ulevel") = 0 Then
                        '    'JIKA SERVER PUSAT DAN USER POS MAKA TIDAK BISA LOGIN
                        '    result(2) = "POS users can't log on the Main website." : GoTo selesai

                        'ElseIf IsServer = 0 And drUser("ulevel") = 1 Then
                        '    'JIKE SERVER POS DAN USER PUSAT MAKA TIDAK BISA LOGIN
                        '    result(2) = "Main users can't log on the POS website." : GoTo selesai

                        'ElseIf drUser("ulevel") = 3 Then
                        '    'JIKA USER BI MAKA TIDAK BISA LOGIN
                        '    result(2) = "BI users can't log on the Main website." : GoTo selesai

                        'ElseIf IsServer = 0 And drUser("ulevel") = 4 Then
                        '    'JIKE SERVER POS DAN USER BI + PUSAT MAKA TIDAK BISA LOGIN
                        '    result(2) = "Main/BI users can't log on the POS website." : GoTo selesai

                        'ElseIf IsServer = 0 And drUser("ulevel") = 0 And drUser("ulokasi") <> LokasiSetting Then
                        '    'JIKE SERVER POS DAN USER POS DAN LOKASI USER TIDAK SESUAI SETTING LOKASI MAKA TIDAK BISA LOGIN
                        '    result(2) = "User " & drUser("unama") & " can't login to " & LokasiSettingNama & " location" : GoTo selesai

                        'End If

                        'CEK USER SUDAH LOGIN ATAU BELUM
                        If LoginReplace = 1 Then
                            'JIKA LOGIN REPLACE MAKA REPLACE LOGIN YANG LAMA DAN PAKAI LOGIN YANG TERBARU
                            sql = "SELECT ulid, uluser, ulcomputerip, ultgl FROM m0_userlogin WHERE uluser = '" & FixDouble(userid) & "'"
                            Dim dtUserLogin As DataTable = AsDataTableAmbilDariDB(sql)
                            If dtUserLogin.Rows.Count > 0 Then
                                Dim rsLogout As String = M0_Logout(dtUserLogin(0)("ulid") & sptParam & "M0_Logout" & sptParam & "0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss" & sptParam & dtUserLogin(0)("uluser") & sptParam & 0 & sptParam & AppCode)
                            End If

                        Else
                            'JIKA TIDAK LOGIN REPLACE MAKA TAMPILKAN ALERT USER SUDAH LOGIN DI TEMPAT LAIN
                            sql = "SELECT ulid, uluser, ulcomputerip, ultgl FROM m0_userlogin WHERE uluser = '" & FixDouble(userid) & "'"
                            Dim dtUserLogin As DataTable = AsDataTableAmbilDariDB(sql)
                            If dtUserLogin.Rows.Count > 0 Then
                                result(2) = "User '" & username & "' was logged on " & dtUserLogin.Rows(0)("ulcomputerip") : result(3) = 1 : GoTo selesai
                            End If
                        End If

                        'GENERATE WEBSITE ACCESS KEY
                        Dim _DateCreated As Date = Now
                        Dim _DateExpired As Date = _DateCreated.AddMinutes(60) 'Asumsi 60 menit
                        Dim Security As New ClsSecurity
                        Dim intervalMinute As Integer = 60 * 600000 '60 Minutes
                        Dim ip As String = HttpContext.Current.Request.UserHostAddress

                        _AccessKey = Security.MD5CalcString(userid & AppKey & _DateCreated & _DateExpired & ip) 'RandomPassword.Generate(15)

                        Dim htable As New Hashtable
                        htable.Add("keyCreated", _DateCreated)
                        htable.Add("keyExpired", _DateExpired)
                        htable.Add("keyInterval", intervalMinute)
                        htable.Add("userid", userid)
                        htable.Add("ip", ip)

                        Dim myWAK As New DataTable
                        AsDataTableTambahField(myWAK, "keyCreated", AsEnumTypeData.AsString)
                        AsDataTableTambahField(myWAK, "keyExpired", AsEnumTypeData.AsString)
                        AsDataTableTambahField(myWAK, "keyInterval", AsEnumTypeData.AsString)
                        AsDataTableTambahField(myWAK, "userid", AsEnumTypeData.AsString)
                        AsDataTableTambahField(myWAK, "ip", AsEnumTypeData.AsString)
                        If AsDataTableTambahData(myWAK, "keyCreated~keyExpired~keyInterval~userid~ip", _DateCreated & "~" & _DateExpired & "~" & intervalMinute & "~" & userid & "~" & ip) = False Then
                            result(2) = "Failed creating acces key data. Try again" : GoTo selesai
                        End If

                        ''1 jam
                        'Dim TimeSpan As New TimeSpan
                        'TimeSpan.Add(New TimeSpan(0, 0, 1))

                        'SIMPAN KE TABEL USER LOGIN :                   ulid,                       uluser,            ulcomputerip,                ulaktif,       ultgl
                        sql = "INSERT INTO m0_userlogin VALUES ('" & FixQuotes(_AccessKey) & "', '" & FixDouble(userid) & "', '" & FixQuotes(ip) & "', '" & FixDouble(1) & "', NOW())"
                        If AsEksekusiSQL(sql) = False Then
                            result(2) = "Login Failed, failed creating user login. Try again" : GoTo selesai
                        End If

                        'TAMBAHKAN MSMQ
                        'tipe = login/check/logout
                        Dim tipeMsmq As String = "login"
                        Dim hasilMsmq As String = SendMsmqLogin(dirMsmqUserLogin, tipeMsmq, _AccessKey, userid, AppCode)
                        If Len(hasilMsmq) > 0 Then
                            result(2) = hasilMsmq : GoTo selesai
                        End If

                        ''SET MEMCACHED WEBSITE ACCESS KEY
                        ''If AsMemcached.SetCache("myerpplus-" & _AccessKey, myWAK, TimeSpan) = False Then
                        'If AsMemcached.SetCache("myerpplus-" & Application("AppCode") & "-" & _AccessKey, myWAK) = False Then
                        '    result(2) = "API Error, failed creating acces key. Try again" : GoTo selesai
                        'End If

                    Else
                        result(2) = "User '" & username & "' has been expired since " & AsFormatTanggal(drUser("utglexpired")) & "." : GoTo selesai
                    End If

                Else
                    result(2) = "User '" & username & "' hasn't active." : GoTo selesai
                End If

            Else
                result(2) = "Invalid password." : GoTo selesai
            End If

        Else
            result(2) = "Invalid username." : GoTo selesai
        End If
        'END OF PROSES LOGIN ===============================================================


        'AMBIL M0_User_VSearch =============================================================
        Dim wsM0_User As New m0_user
        Dim arrUser() As String = wsM0_User.M0_User_VSearch(_AccessKey & "★M0_User_VSearch★0△0△userid=" & userid & "△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strUser = arrUser(2)
        'END OF AMBIL M0_User_VSearch ======================================================


        'AMBIL M0_Usermodule_Search =======================================================
        Dim wsM0_Usermodule As New m0_usermodule
        Dim arrUserModule() As String = wsM0_Usermodule.M0_UsermoduleSearch(_AccessKey & "★M0_Usermodule_Search★0△0△userid=" & userid & " AND mactive=1△m.murutan△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strUserModule = arrUserModule(2)
        'END OF AMBIL M0_Usermodule_Search ================================================


        'AMBIL M0_HakAkses =================================================================
        Dim wsM0_HakAkses As New m0_hakAkses
        strHakAkses = wsM0_HakAkses.M0_HakAkses(_AccessKey & "★M0_HakAkses★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★" & userid & "★1★")
        'END OF AMBIL M0_HakAkses ==========================================================


        'AMBIL M0_SettingSearch ============================================================
        Dim wsM0_Setting As New m0_setting
        Dim arrSetting() As String = wsM0_Setting.M0_SettingSearch(_AccessKey & "★M0_SettingSearch★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★" & lokasi).Split(sptParam)
        strSetting = arrSetting(2)
        'END OF AMBIL M0_SettingSearch =====================================================


        'AMBIL M0_UsermenuSearch ===========================================================
        Dim wsM0_Usermenu As New Wsm0_usermenu
        Dim arrUsermenu() As String = wsM0_Usermenu.M0_UsermenuSearch(_AccessKey & "★M0_UsermenuSearch★0△0△userid=" & userid & "△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strUserMenu = arrUsermenu(2)
        'END OF AMBIL M0_UsermenuSearch ====================================================


        'AMBIL M0_UsercustomSearch =======================================================
        Dim wsM0_Usercustom As New wsm0_usercustom
        Dim arrUsercustom() As String = wsM0_Usercustom.M0_UsercustomSearch(_AccessKey & "★M0_UsercustomSearch★0△0△userid=" & userid & "△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strUserCustom = arrUsercustom(2)
        'END OF AMBIL M0_UsercustomSearch ================================================


        'AMBIL M0_UserreportSearch =======================================================
        Dim wsM0_Userreport As New wsm0_userreport
        Dim arrUserreport() As String = wsM0_Userreport.M0_UserreportSearch(_AccessKey & "★M0_UserreportSearch★0△0△userid=" & userid & "△`u`.`userid`,`rr`.`rrmoduleid`,`rr`.`rrmenuid`,`rr`.`rritem`△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strUserReport = arrUserreport(2)
        'END OF AMBIL M0_UserreportSearch ================================================


        'AMBIL M0_NomorSearch ==============================================================
        Dim wsM0_Nomor As New m0_nomor
        Dim arrNomor() As String = wsM0_Nomor.M0_NomorSearch(_AccessKey & "★M0_NomorSearch★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strNomor = arrNomor(2)
        'END OF AMBIL M0_NomorSearch =======================================================


        'AMBIL M2_Accounting_PeriodSearch ==================================================
        Dim wsM2_AccPeriod As New m2_accounting_period
        Dim arrAccPeriod() As String = wsM2_AccPeriod.M2_Accounting_PeriodSearch(_AccessKey & "★M2_Accounting_PeriodSearch★0△0△apaktif = 1△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strAccPeriod = arrAccPeriod(2)
        'END OF AMBIL M2_Accounting_PeriodSearch ===========================================


        'AMBIL M0_ReportByLanguage =========================================================
        Dim wsM0_Report As New m0_report
        Dim arrReport() As String = wsM0_Report.M0_ReportByLanguage(_AccessKey & "★M0_ReportByLanguage★0△0△" & bahasa & "~" & userid & "~△`r`.`rmoduleid`,`r`.`rmenuid`,`r`.`rurutan`△" & formatTgl & "△" & formatTglWaktu & "★★1★").Split(sptParam)
        strReport = arrReport(2)
        'END OF AMBIL M0_ReportByLanguage ==================================================


        'WebsiteAccessKey


        'AMBIL M0_SentenceSearch ===========================================================
        Dim wsM0_Sentence As New m0_sentence
        Dim arrSentence() As String = wsM0_Sentence.M0_SentenceSearch(_AccessKey & "★M0_SentenceSearch★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strSentence = arrSentence(2)
        'END OF AMBIL M0_SentenceSearch ====================================================


        ''AMBIL Cd_M1_Contact_Category ======================================================
        'Dim wsM0_Caridata As New m0_caridata
        'Dim arrContactCat() As String = wsM0_Caridata.CdM1_Contact_Category(_AccessKey & "★Cd_M1_Contact_Category★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        'strContactCat = arrContactCat(2)
        ''END OF AMBIL Cd_M1_Contact_Category ===============================================


        ''AMBIL M0_Form_Setting_GlobalSearch ================================================
        'Dim wsM0_Form_Setting_Global As New m0_form_setting_global
        'Dim arrFormSetGlobal() As String = wsM0_Form_Setting_Global.M0_Form_Setting_GlobalSearch(_AccessKey & "★M0_Form_Setting_GlobalSearch★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        'strFormSetGlobal = arrFormSetGlobal(2)
        ''END OF AMBIL M0_Form_Setting_GlobalSearch =========================================


        'AMBIL FILE JSON BAHASA =============================================================
        Dim myPath As String = HttpContext.Current.Server.MapPath("~/") & "app\libs\language\"
        Dim sr As StreamReader
        Dim fileName As String = bahasa & ".json"

        If bahasa.ToUpper <> "INA" Then
            'CEK FILE EXISTS
            If (File.Exists(myPath & fileName)) Then
                sr = File.OpenText(myPath & fileName)
                contents = sr.ReadToEnd()
                sr.Close()
            Else
                result(2) = fileName & " File doesn't exists." : GoTo selesai
            End If
        End If
        'END OF AMBIL FILE JSON BAHASA ======================================================


        'AMBIL SETTING POS ==================================================================
        'SETTING POS DILOAD JIKA USER LOGIN MEMILIKI HAK AKSES UNTUK INSERT/UPDATE TRANSAKSI POS
        'index hak akses : 0=Insert, 1=Update/Draft, 4=Approved1, 5=Approved2, 6=Approved3, 7=Approved4, 8=Approved
        Dim dtCekPOS As New DataTable
        Dim CatPOS As String = "", StrAksesPOS As String = ""
        sql = "SELECT rm.rmmoduleid, rm.rmmenuid, rm.rmrole, rm.rmakses, rm.rmfavourite, l.lkategoripos FROM m0_role_menu rm JOIN m0_user_role ur ON rm.rmrole = ur.role JOIN m0_user u ON ur.userid = u.userid JOIN m1_location l ON u.ulokasi = l.lkode WHERE rm.rmmoduleid = 12 AND rm.rmmenuid = 8 AND u.userid = '" & FixDouble(userid) & "'"
        dtCekPOS = AsDataTableAmbilDariDB(sql)
        If dtCekPOS.Rows.Count > 0 Then
            'CEK LEN KATEGORI POS
            If Len(FxDB(dtCekPOS.Rows(0)("lkategoripos").ToString, "")) > 0 Then
                'SET KATEGORI POS
                CatPOS = FxDB(dtCekPOS.Rows(0)("lkategoripos").ToString, "")

                'AMBIL SETTING POS SESUAI KATEGORI -------------------------
                Dim wsM12_PosCategorySetting As New m12_pos_category_setting
                Dim arrSettingPOS() As String = wsM12_PosCategorySetting.M12_Pos_Category_SettingGetdataById(_AccessKey & "★M12_Pos_Category_SettingGetdataById★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★" & CatPOS & "★1★").Split(sptParam)
                strSettingPOS = arrSettingPOS(2)
                'END OF AMBIL SETTING POS SESUAI KATEGORI ------------------

                'SET HAK STR AKSES POS
                StrAksesPOS = FxDB(dtCekPOS.Rows(0)("rmakses").ToString, "")

                'CEK HAK AKSES POS
                If StrAksesPOS.ElementAt(0) = "1" Or
                    StrAksesPOS.ElementAt(1) = "1" Or
                    StrAksesPOS.ElementAt(4) = "1" Or
                    StrAksesPOS.ElementAt(5) = "1" Or
                    StrAksesPOS.ElementAt(6) = "1" Or
                    StrAksesPOS.ElementAt(7) = "1" Or
                    StrAksesPOS.ElementAt(8) = "1" Then

                    'AMBIL SETTING BONUS ITEM
                    Dim wsM12_PosBonusItem As New m12_pos_bonus_item
                    Dim arrBonusItem() As String = wsM12_PosBonusItem.M12_Pos_Bonus_ItemSearch(_AccessKey & "★M12_Pos_Bonus_ItemSearch★0△0△bikategori = '" & CatPOS & "'△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
                    strBonusItem = arrBonusItem(2)

                    'AMBIL SETTING BONUS ITEM DETAIL
                    Dim wsM12_PosBonusItemDetail As New m12_pos_bonus_item
                    Dim arrBonusItemDetail() As String = wsM12_PosBonusItemDetail.M12_Pos_Bonus_Item_DetailSetting(_AccessKey & "★M12_Pos_Bonus_Item_DetailSetting★0△0△bikategori = '" & CatPOS & "'△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
                    strBonusItemDetail = arrBonusItemDetail(2)

                    'AMBIL SETTING SUBSTITUTION ITEM
                    Dim wsM12_PosSubsItem As New m12_pos_substitution_item
                    Dim arrSubsItem() As String = wsM12_PosSubsItem.M12_Pos_Substitution_ItemSearch(_AccessKey & "★M12_Pos_Substitution_ItemSearch★0△0△sikategori = '" & CatPOS & "'△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
                    strSubsItem = arrSubsItem(2)

                    'AMBIL SETTING SUBSTITUTION ITEM DETAIL
                    Dim wsM12_PosSubsItemDetail As New m12_pos_substitution_item
                    Dim arrSubsItemDetail() As String = wsM12_PosSubsItemDetail.M12_Pos_Substitution_Item_DetailSetting(_AccessKey & "★M12_Pos_Substitution_Item_DetailSetting★0△0△sikategori = '" & CatPOS & "'△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
                    strSubsItemDetail = arrSubsItemDetail(2)

                    'AMBIL SETTING ADDITIONAL ITEM
                    Dim wsM12_PosAddItem As New m12_pos_additional_item
                    Dim arrAddItem() As String = wsM12_PosAddItem.M12_Pos_Additional_ItemSearch(_AccessKey & "★M12_Pos_Additional_ItemSearch★0△0△aikategori = '" & CatPOS & "'△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
                    strAddItem = arrAddItem(2)

                    'AMBIL SETTING ADDITIONAL ITEM DETAIL
                    Dim wsM12_PosAddItemDetail As New m12_pos_additional_item
                    Dim arrAddItemDetail() As String = wsM12_PosAddItemDetail.M12_Pos_Additional_Item_DetailSetting(_AccessKey & "★M12_Pos_Additional_Item_DetailSetting★0△0△aikategori = '" & CatPOS & "'△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
                    strAddItemDetail = arrAddItemDetail(2)

                    'AMBIL SETTING DISCOUNT ITEM
                    Dim wsM12_PosDiscItem As New m12_pos_discount_item
                    Dim arrDiscItem() As String = wsM12_PosDiscItem.M12_Pos_Discount_ItemSearch(_AccessKey & "★M12_Pos_Discount_ItemSearch★0△0△dikategori = '" & CatPOS & "'△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
                    strDiscItem = arrDiscItem(2)

                    'AMBIL SETTING DISCOUNT CATEGORY ITEM
                    Dim wsM12_PosDiscCatItem As New m12_pos_discount_category_item
                    Dim arrDiscCatItem() As String = wsM12_PosDiscCatItem.M12_Pos_Discount_Category_ItemSearch(_AccessKey & "★M12_Pos_Discount_Category_ItemSearch★0△0△dcikategori = '" & CatPOS & "'△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
                    strDiscCatItem = arrDiscCatItem(2)

                    'AMBIL SETTING POINT ITEM
                    Dim wsM12_PosPointItem As New m12_pos_point_item
                    Dim arrPointItem() As String = wsM12_PosPointItem.M12_Pos_Point_ItemSearch(_AccessKey & "★M12_Pos_Point_ItemSearch★0△0△pikategori = '" & CatPOS & "'△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
                    strPointItem = arrPointItem(2)

                    'AMBIL SETTING POINT CATEGORY ITEM
                    Dim wsM12_PosPointCatItem As New m12_pos_point_category_item
                    Dim arrPointCatItem() As String = wsM12_PosPointCatItem.M12_Pos_Point_Category_ItemSearch(_AccessKey & "★M12_Pos_Point_Category_ItemSearch★0△0△pcikategori = '" & CatPOS & "'△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
                    strPointCatItem = arrPointCatItem(2)

                    'AMBIL SETTING POINT NOMINAL
                    Dim wsM12_PosPointNominal As New m12_pos_point_transaction
                    Dim arrPointNominal() As String = wsM12_PosPointNominal.M12_Pos_Point_TransactionSearch(_AccessKey & "★M12_Pos_Point_TransactionSearch★0△0△ptkategori = '" & CatPOS & "'△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
                    strPointNominal = arrPointNominal(2)

                    'AMBIL SETTING BONUS ITEM NOMINAL
                    Dim wsM12_PosBonusTrans As New m12_pos_bonus_trans
                    Dim arrBonusTrans() As String = wsM12_PosBonusTrans.M12_Pos_Bonus_TransSearch(_AccessKey & "★M12_Pos_Bonus_TransSearch★0△0△bikategori = '" & CatPOS & "'△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
                    strBonusTrans = arrBonusTrans(2)

                    'AMBIL SETTING BONUS ITEM NOMINAL
                    Dim wsM12_PosBonusTransDetail As New m12_pos_bonus_trans
                    Dim arrBonusTransDetail() As String = wsM12_PosBonusTransDetail.M12_Pos_Bonus_Trans_DetailSetting(_AccessKey & "★M12_Pos_Bonus_Trans_DetailSetting★0△0△bikategori = '" & CatPOS & "'△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
                    strBonusTransDetail = arrBonusTransDetail(2)
                End If
            End If
        End If
        'END OF AMBIL SETTING POS ===========================================================


        'AMBIL M12_Pos_HardwareSearch =======================================================
        Dim wsM12_Pos_Hardware As New m12_pos_hardware
        Dim arrHardware() As String = wsM12_Pos_Hardware.M12_Pos_HardwareSearch(_AccessKey & "★M12_Pos_HardwareSearch★0△0△phuserid=" & userid & "△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strHardware = arrHardware(2)
        'END OF AMBIL M12_Pos_HardwareSearch ================================================

        'result(2) = "Err " + _AccessKey & "★M0_Menu_SerenitySearch★0△0△" & bahasa & "△u.userid=" & userid & "△" & formatTgl & "△" & formatTglWaktu & "★0★1★" : GoTo selesai
        'AMBIL M0_Menu_S =======================================================
        Dim wsM0_menu As New m0_menu_s
        Dim arrMenuS() As String = wsM0_menu.M0_Menu_SerenitySearch(_AccessKey & "★M0_Menu_SerenitySearch★0△0△" + bahasa + "△u.userid=" & userid & "△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strMenuSerenity = arrMenuS(2)
        'END OF AMBIL M0_Menu_S ================================================

        'AMBIL M0_Report_Filter =======================================================
        Dim wsM0_reportfilter As New m0_report
        Dim arrReportfilter() As String = wsM0_reportfilter.M0_Report_FilterSearch(_AccessKey & "★M0_Report_FilterSearch★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strReportfilter = arrReportfilter(2)
        'END OF AMBIL M0_Report_Filter ================================================

        'AMBIL m0_language_detail =======================================================
        'Dim WsM0_language_s As New m0_language_s
        'Dim arrlanguage_s() As String = WsM0_language_s.M0_Language_SSearch(_AccessKey & "★M0_Language_SSearch★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        'strTranslateS = arrlanguage_s(2)
        'If (bahasa.Length > 0) Then
        '    bahasa = "INA"
        'End If
        Dim WsM0_sentence_s As New m0_sentence_s
        Dim arrlanguage_s() As String = WsM0_sentence_s.M0_sentence_SSearch(_AccessKey & "★M0_Language_SSearch★0△0△" + bahasa + "△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strTranslateS = arrlanguage_s(2)
        'END OF AMBIL m0_language_detail ================================================

        'AMBIL M0_Report_OrderSearch =======================================================
        Dim arrReportorder() As String = wsM0_reportfilter.M0_Report_OrderSearch(_AccessKey & "★M0_Report_OrderSearch★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strReportOrder = arrReportorder(2)
        'END OF AMBIL M0_Report_OrderSearch ================================================

        'AMBIL M0_Report_GroupSearch =======================================================
        Dim arrReportgroup() As String = wsM0_reportfilter.M0_Report_GroupSearch(_AccessKey & "★M0_Report_GroupSearch★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strReportGroup = arrReportgroup(2)
        'END OF AMBIL M0_Report_GroupSearch ================================================

        result(1) = 1

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam,
                                 strUser, sptLogin, strUserModule, sptLogin, strHakAkses, sptLogin, strSetting, sptLogin, strUserMenu, sptLogin, strUserCustom, sptLogin,
                                 strUserReport, sptLogin, strNomor, sptLogin, strAccPeriod, sptLogin, strReport, sptLogin, _AccessKey, sptLogin, strSentence, sptLogin,
                                 contents, sptLogin, strSettingPOS, sptLogin, strBonusItem, sptLogin, strBonusItemDetail, sptLogin, strSubsItem, sptLogin, strSubsItemDetail, sptLogin,
                                 strAddItem, sptLogin, strAddItemDetail, sptLogin, strDiscItem, sptLogin, strDiscCatItem, sptLogin, strPointItem, sptLogin, strPointCatItem, sptLogin,
                                 strPointNominal, sptLogin, strHardware, sptLogin, strBonusTrans, sptLogin, strBonusTransDetail, sptLogin, strMenuSerenity, sptLogin, strReportfilter, sptLogin, strTranslateS, sptLogin, strReportOrder, sptLogin, strReportGroup)
        'wsResult = _AccessKey & "★M0_ReportByLanguage★0△0△" & bahasa & "~" & userid & "~△`r`.`rmoduleid`,`r`.`rmenuid`,`r`.`rurutan`△" & formatTgl & "△" & formatTglWaktu & "★★1★"
        Return wsResult
    End Function

End Class