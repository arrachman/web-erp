Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class mob_m0_login

    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""
    Public Function MD5CalcString(ByVal strData As String) As String

        Dim objMD5 As New System.Security.Cryptography.MD5CryptoServiceProvider
        Dim arrData() As Byte
        Dim arrHash() As Byte

        ' first convert the string to bytes (using UTF8 encoding for unicode characters)
        arrData = System.Text.Encoding.UTF8.GetBytes(strData)

        ' hash contents of this byte array
        arrHash = objMD5.ComputeHash(arrData)

        ' thanks objects
        objMD5 = Nothing

        ' return formatted hash
        Return ByteArrayToString(arrHash)

    End Function

    ' utility function to convert a byte array into a hex string
    Private Function ByteArrayToString(ByVal arrInput() As Byte) As String

        Dim strOutput As New System.Text.StringBuilder(arrInput.Length)

        For i As Integer = 0 To arrInput.Length - 1
            strOutput.Append(arrInput(i).ToString("X2"))
        Next

        Return strOutput.ToString().ToLower

    End Function

    <WebMethod()>
    Public Function MobM0_LoginMin(ByVal param As String) As String

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "yyyy-MM-dd H:mm:ss", search As String = "", bahasa As String = "", ukontak As String = "", sql As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", _AccessKey As String = ""
        Dim strResult As String = "", strResultPaging As String = "", mappUser As String = "", strUser As String = ""

        Dim username As String = "", password As String = "", AppKey As String = "", AppSecret As String = "", AppCode As String = "", LoginReplace As Integer = 0, MacAddress As String = "33:33"

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

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

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
        If (dataSplit.Length <> 7) Then
            result(2) = "Invalid login data parameter." + paramSplit(5) : GoTo selesai
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
        'If (Len(dataSplit(4)) = 0) Then
        '    result(2) = "App Code can't be empty" : GoTo selesai
        'Else
        AppCode = Application("AppCode").ToString
        'End If

        'LOGIN REPLACE
        If (IsNumeric(dataSplit(5)) = False) Then
            result(2) = "Login Replace required numeric" : GoTo selesai
        Else
            LoginReplace = dataSplit(5)
        End If

        'LOGIN REPLACE
        If (Len(dataSplit(6)) = 0) Then
            result(2) = "UUID Mobile can't be empty" : GoTo selesai
        Else
            MacAddress = dataSplit(6).ToString
        End If
        'END OF SET DAN VALIDASI VARIABEL USER =============================================


        'PROSES LOGIN ======================================================================
        'CEK APPKEY DAN APPSECRET
        AppKey = AsAntiSQLInjection(AppKey)
        AppSecret = AsAntiSQLInjection(AppSecret)
        'result(2) = MD5CalcString("f955ea15c3f676bf" + sptField + "2" + sptField + "demobeta.myerpplus.com") : GoTo selesai
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()


        Dim myApp As DataTable
        myApp = AsDataTableAmbilDariDB("SELECT (appid) FROM m0_app WHERE appkey='" & AppKey & "' AND appsecret='" & AppSecret & "'")
        If myApp.Rows.Count = 0 Then
            result(2) = "Invalid App Key or App Secret." : GoTo selesai
        End If

        'CEK DATA USER
        Dim myUser As DataTable
        myUser = AsDataTableAmbilDariDB("SELECT userid, ukode, unama, upassword, uaktif, utglexpired, ubahasa, ukontak FROM m0_user WHERE ukode='" & username & "'")
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
            ukontak = drUser("ukontak")

            'CEK PASSWORD
            If drUser("upassword") = CreateSHAHash(password, "AlEuPj13") Then

                If drUser("uaktif") = 1 Then
                    'CEK TGL EXPIRED
                    If Date.Compare(dateNow, dateExpired) < 0 Then

                        'JIKA LOGIN REPLACE MAKA REPLACE LOGIN YANG LAMA DAN PAKAI LOGIN YANG TERBARU
                        sql = "SELECT ulid, uluser, ulcomputerip, ultgl FROM m0_userlogin WHERE uluser = '" & FixDouble(userid) & "'"
                        Dim dtUserLogin As DataTable = AsDataTableAmbilDariDB(sql)
                        If dtUserLogin.Rows.Count > 0 Then
                            Dim rsLogout As String = MobM0_Logout(dtUserLogin(0)("ulid") & sptParam & "M0_Logout" & sptParam & "0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss" & sptParam & dtUserLogin(0)("uluser") & sptParam & 0 & sptParam & AppCode)
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

                        'SET MEMCACHED WEBSITE ACCESS KEY
                        'If AsMemcached.SetCache("myerpplus-" & _AccessKey, myWAK, TimeSpan) = False Then
                        'If AsMemcached.SetCache("myerpplus-" & _AccessKey, myWAK) = False Then
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

        'Default No Berikutnya 1
        Dim noberikutnya As Integer = 1
        '----

        'Query (cek userid apa sudah ada di tabel)
        sql = "SELECT * FROM m0_nomor_mobile WHERE userid = " + userid
        Dim dt As DataTable = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count > 0 Then
            If (dt.Rows(0)("macaddress") = MacAddress) Then
                noberikutnya = dt.Rows(0)("noberikutnya")
            Else
                If LoginReplace = 1 Then
                    result(2) = "Cannot login another device."
                    GoTo selesai
                ElseIf LoginReplace = 2 Then
                    sql = "UPDATE m0_nomor_mobile SET macaddress = '" + MacAddress + "' WHERE userid = " + userid
                    If AsEksekusiSQL(sql) = False Then
                        result(2) = "UPDATE failed m0_nomor_mobile. Try again" : GoTo selesai
                    End If
                    noberikutnya = dt.Rows(0)("noberikutnya")
                End If
            End If
        Else
            'Jika user belum ada di tabel 
            'Query Insert user, masukkan macaddress dan userid
            sql = "INSERT INTO  m0_nomor_mobile (macaddress, userid) VALUES ('" + MacAddress + "', '" + userid + "')"
            If AsEksekusiSQL(sql) = False Then
                result(2) = "Insert failed m0_nomor_mobile. Try again" : GoTo selesai
            End If
        End If

        'AMBIL M0_UserSearch =============================================================
        Dim wsM0_User As New mob_m0_user
        Dim arrUser As String = wsM0_User.MobM0_UserSearch(_AccessKey & "★MobM0_UserSearch★0△0△userid=" & userid & "△△" & formatTgl & "△" & formatTglWaktu & "★0★1★")
        Dim arrUsr() As String = arrUser.Split(sptParam)

        strUser = arrUsr(2)
        mappUser = arrUsr(3)
        'END OF AMBIL M0_UserSearch ======================================================

        result(1) = 1
selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)

        'DATA
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strUser, sptLogin, _AccessKey, sptLogin, AppCode, sptLogin, noberikutnya)

        'MAPPING
        wsResult = String.Concat(wsResult, sptParam, mappUser, sptLogin, "WebsiteAccessKey", sptLogin, "AppCode", sptLogin, "noberikutnya")

        Return wsResult
    End Function

    <WebMethod()>
    Public Function MobM0_aktifkanUser(ByVal param As String) As String
        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "yyyy-MM-dd H:mm:ss", search As String = "", bahasa As String = "", ukontak As String = "", sql As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", _AccessKey As String = ""
        Dim strResult As String = "", strResultPaging As String = ""

        Dim username As String = "", password As String = "", AppKey As String = "", AppSecret As String = "", AppCode As String = "", LoginReplace As Integer = 0, MacAddress As String = "33:33"

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

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

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
        If (dataSplit.Length <> 7) Then
            result(2) = "Invalid login data parameter." + paramSplit(5) : GoTo selesai
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
            result(2) = "Domain can't be empty" : GoTo selesai
        End If
        AppCode = Application("AppCode").ToString

        'LOGIN REPLACE
        If (IsNumeric(dataSplit(5)) = False) Then
            result(2) = "Kode Aktivasi required numeric" : GoTo selesai
        Else
            MacAddress = Integer.Parse(dataSplit(5))
        End If

        'LOGIN REPLACE
        If (Len(dataSplit(6)) = 0) Then
            result(2) = "UUID Mobil can't be empty" : GoTo selesai
        Else
            MacAddress = dataSplit(6).ToString
        End If
        'END OF SET DAN VALIDASI VARIABEL USER =============================================

        If MacAddress = CreateSHAHash(dataSplit(6) + sptField + userid + sptField + dataSplit(4), "M@sUns4K") Then
            Return MobM0_Login(param)
        Else
            result(2) = "Activation code does not match" : GoTo selesai
        End If

        'PROSES LOGIN ======================================================================
        'result(2) = MD5CalcString("f955ea15c3f676bf" + sptField + "2" + sptField + "demobeta.myerpplus.com") : GoTo selesai

        result(1) = 1
selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)

        'DATA
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1))

        'MAPPING
        wsResult = String.Concat(wsResult)

        Return wsResult
    End Function


    <WebMethod()>
    Public Function MobM0_Login(ByVal param As String) As String

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "yyyy-MM-dd H:mm:ss", search As String = "", bahasa As String = "", ukontak As String = "", sql As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = "", strUser As String = "", strUserModule As String = "", strHakAkses As String = "", strSetting As String = ""
        Dim strUserMenu As String = "", strUserCustom As String = "", strUserReport As String = "", strNomor As String = "", strAccPeriod As String = ""
        Dim strReport As String = "", strContactCat As String = "", strFormSetGlobal As String = "", strSentence As String = "", _AccessKey As String = ""
        Dim strResult As String = "", strResultPaging As String = ""

        Dim strItem As String = "", strContact = "", strSo As String = "", strSoDetail As String = "", strTax As String = ""

        Dim username As String = "", password As String = "", AppKey As String = "", AppSecret As String = "", AppCode As String = "", LoginReplace As Integer = 0

        Dim mappUser As String = "", mappItem As String = "", mappContact As String = "", mappSoUtama As String = "", mappSoDetail As String = "", mappTax As String = "", mappSetting As String = ""

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

        'CEK FORMATTGL
        If Len(pagingSplit(4)) = 0 Then
            formatTgl = "yyyy-MM-dd"
        Else
            formatTgl = pagingSplit(4)
        End If

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
        'If (Len(dataSplit(4)) = 0) Then
        '    result(2) = "App Code can't be empty" : GoTo selesai
        'Else
        AppCode = Application("AppCode").ToString
        'End If

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
        myUser = AsDataTableAmbilDariDB("SELECT userid, ukode, unama, upassword, uaktif, utglexpired, ubahasa, ukontak FROM m0_user WHERE ukode='" & username & "'")
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
            ukontak = drUser("ukontak")

            'CEK PASSWORD
            If drUser("upassword") = CreateSHAHash(password, "AlEuPj13") Then
                'CEK USER ROLE
                Dim myRole As DataTable = AsDataTableAmbilDariDB("SELECT COUNT(userid) FROM m0_user_role WHERE userid='" & userid & "'")
                If Val(myRole.Rows(0)(0)) = 0 Then
                    result(2) = "User '" & userkode & "' doesn't have any role." : GoTo selesai
                End If

                'CEK AKTIF
                If drUser("uaktif") = 1 Then
                    'CEK TGL EXPIRED
                    If Date.Compare(dateNow, dateExpired) < 0 Then

                        'CEK USER SUDAH LOGIN ATAU BELUM
                        If LoginReplace = 1 Then
                            'JIKA LOGIN REPLACE MAKA REPLACE LOGIN YANG LAMA DAN PAKAI LOGIN YANG TERBARU
                            sql = "SELECT ulid, uluser, ulcomputerip, ultgl FROM m0_userlogin WHERE uluser = '" & FixDouble(userid) & "'"
                            Dim dtUserLogin As DataTable = AsDataTableAmbilDariDB(sql)
                            If dtUserLogin.Rows.Count > 0 Then
                                Dim rsLogout As String = MobM0_Logout(dtUserLogin(0)("ulid") & sptParam & "M0_Logout" & sptParam & "0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss" & sptParam & dtUserLogin(0)("uluser") & sptParam & 0 & sptParam & AppCode)
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

                        'SET MEMCACHED WEBSITE ACCESS KEY
                        'If AsMemcached.SetCache("myerpplus-" & _AccessKey, myWAK, TimeSpan) = False Then
                        'If AsMemcached.SetCache("myerpplus-" & _AccessKey, myWAK) = False Then
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


        'AMBIL M0_UserSearch =============================================================
        Dim wsM0_User As New mob_m0_user
        Dim arrUser() As String = wsM0_User.MobM0_UserSearch(_AccessKey & "★MobM0_UserSearch★0△0△userid=" & userid & "△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strUser = arrUser(2)
        mappUser = arrUser(3)
        'END OF AMBIL M0_UserSearch ======================================================


        'AMBIL MobM1_ItemSearch =============================================================
        Dim wsMobM1_Item As New mob_m1_item
        Dim arrItem() As String = wsMobM1_Item.MobM1_ItemSearch(_AccessKey & "★MobM1_ItemSearch★0△0△bmobile = " & 1 & "△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strItem = arrItem(2)
        mappItem = arrItem(3)
        'END OF AMBIL MobM1_ItemSearch ======================================================


        'AMBIL MobM1_ContactSearch =============================================================
        Dim wsMobM1_Contact As New mob_m1_contact
        Dim arrContact() As String = wsMobM1_Contact.MobM1_ContactSearch(_AccessKey & "★MobM1_ContactSearch★0△0△kkategori = 'C' AND ksalesman = " & FixDouble(ukontak) & "△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strContact = arrContact(2)
        mappContact = arrContact(3)
        'END OF AMBIL MobM1_ContactSearch ======================================================


        'AMBIL MobM5_SoSearch =============================================================
        Dim wsMobM5_So As New mob_m5_so
        Dim arrSo() As String = wsMobM5_So.MobM5_SoSearch(_AccessKey & "★MobM5_SoSearch★0△0△sobagianpenjualan = " & FixDouble(ukontak) & "△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strSo = arrSo(2)
        mappSoUtama = arrSo(3)
        'END OF AMBIL MobM5_SoSearch ======================================================


        'AMBIL MobM5_So_DetailSearch =============================================================
        Dim arrSoDetail() As String = wsMobM5_So.MobM5_So_DetailSearch(_AccessKey & "★MobM5_So_DetailSearch★0△0△sobagianpenjualan = " & FixDouble(ukontak) & "△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strSoDetail = arrSoDetail(2)
        mappSoDetail = arrSoDetail(3)
        'END OF AMBIL MobM5_So_DetailSearch ======================================================


        'AMBIL MobM5_So_DetailSearch =============================================================
        Dim wsM1_Tax As New m1_tax
        Dim arrTax() As String = wsM1_Tax.M1_TaxSearch(_AccessKey & "★M1_TaxSearch★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strTax = arrSoDetail(2)
        mappTax = arrSoDetail(3)
        'END OF AMBIL MobM5_So_DetailSearch ======================================================


        ''AMBIL M0_Usermodule_Search =======================================================
        'Dim wsM0_Usermodule As New m0_usermodule
        'Dim arrUserModule() As String = wsM0_Usermodule.M0_UsermoduleSearch(_AccessKey & "★M0_Usermodule_Search★0△0△userid=" & userid & " AND mactive=1△m.murutan△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        'strUserModule = arrUserModule(2)
        ''END OF AMBIL M0_Usermodule_Search ================================================


        ''AMBIL M0_HakAkses =================================================================
        'Dim wsM0_HakAkses As New m0_hakAkses
        'strHakAkses = wsM0_HakAkses.M0_HakAkses(_AccessKey & "★M0_HakAkses★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★" & userid & "★1★")
        ''END OF AMBIL M0_HakAkses ==========================================================


        'AMBIL M0_SettingSearch ============================================================
        Dim wsM0_Setting As New m0_setting
        Dim arrSetting() As String = wsM0_Setting.M0_SettingSearch(_AccessKey & "★M0_SettingSearch★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        strSetting = arrSetting(2)
        mappSetting = arrSetting(3)
        'END OF AMBIL M0_SettingSearch =====================================================


        ''AMBIL M0_UsermenuSearch ===========================================================
        'Dim wsM0_Usermenu As New Wsm0_usermenu
        'Dim arrUsermenu() As String = wsM0_Usermenu.M0_UsermenuSearch(_AccessKey & "★M0_UsermenuSearch★0△0△userid=" & userid & "△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        'strUserMenu = arrUsermenu(2)
        ''END OF AMBIL M0_UsermenuSearch ====================================================


        ''AMBIL M0_UsercustomSearch =======================================================
        'Dim wsM0_Usercustom As New wsm0_usercustom
        'Dim arrUsercustom() As String = wsM0_Usercustom.M0_UsercustomSearch(_AccessKey & "★M0_UsercustomSearch★0△0△userid=" & userid & "△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        'strUserCustom = arrUsercustom(2)
        ''END OF AMBIL M0_UsercustomSearch ================================================


        ''AMBIL M0_UserreportSearch =======================================================
        'Dim wsM0_Userreport As New wsm0_userreport
        'Dim arrUserreport() As String = wsM0_Userreport.M0_UserreportSearch(_AccessKey & "★M0_UserreportSearch★0△0△userid=" & userid & "△`u`.`userid`,`rr`.`rrmoduleid`,`rr`.`rrmenuid`,`rr`.`rritem`△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        'strUserReport = arrUserreport(2)
        ''END OF AMBIL M0_UserreportSearch ================================================


        ''AMBIL M0_NomorSearch ==============================================================
        'Dim wsM0_Nomor As New m0_nomor
        'Dim arrNomor() As String = wsM0_Nomor.M0_NomorSearch(_AccessKey & "★M0_NomorSearch★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        'strNomor = arrNomor(2)
        ''END OF AMBIL M0_NomorSearch =======================================================


        ''AMBIL M2_Accounting_PeriodSearch ==================================================
        'Dim wsM2_AccPeriod As New m2_accounting_period
        'Dim arrAccPeriod() As String = wsM2_AccPeriod.M2_Accounting_PeriodSearch(_AccessKey & "★M2_Accounting_PeriodSearch★0△0△apaktif = 1△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        'strAccPeriod = arrAccPeriod(2)
        ''END OF AMBIL M2_Accounting_PeriodSearch ===========================================


        ''AMBIL M0_ReportByLanguage =========================================================
        'Dim wsM0_Report As New m0_report
        'Dim arrReport() As String = wsM0_Report.M0_ReportByLanguage(_AccessKey & "★M0_ReportByLanguage★0△0△" & bahasa & "△`r`.`rmoduleid`,`r`.`rmenuid`,`r`.`rurutan`△" & formatTgl & "△" & formatTglWaktu & "★★1★").Split(sptParam)
        'strReport = arrReport(2)
        ''END OF AMBIL M0_ReportByLanguage ==================================================


        ''WebsiteAccessKey


        ''AMBIL M0_SentenceSearch ===========================================================
        'Dim wsM0_Sentence As New m0_sentence
        'Dim arrSentence() As String = wsM0_Sentence.M0_SentenceSearch(_AccessKey & "★M0_SentenceSearch★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        'strSentence = arrSentence(2)
        ''END OF AMBIL M0_SentenceSearch ====================================================


        ' ''AMBIL Cd_M1_Contact_Category ======================================================
        ''Dim wsM0_Caridata As New m0_caridata
        ''Dim arrContactCat() As String = wsM0_Caridata.CdM1_Contact_Category(_AccessKey & "★Cd_M1_Contact_Category★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        ''strContactCat = arrContactCat(2)
        ' ''END OF AMBIL Cd_M1_Contact_Category ===============================================


        ' ''AMBIL M0_Form_Setting_GlobalSearch ================================================
        ''Dim wsM0_Form_Setting_Global As New m0_form_setting_global
        ''Dim arrFormSetGlobal() As String = wsM0_Form_Setting_Global.M0_Form_Setting_GlobalSearch(_AccessKey & "★M0_Form_Setting_GlobalSearch★0△0△△△" & formatTgl & "△" & formatTglWaktu & "★0★1★").Split(sptParam)
        ''strFormSetGlobal = arrFormSetGlobal(2)
        ' ''END OF AMBIL M0_Form_Setting_GlobalSearch =========================================


        result(1) = 1

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)

        'DATA
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam,
                                 strUser, sptLogin, _AccessKey, sptLogin, AppCode, sptLogin, strItem, sptLogin, strContact, sptLogin, strSo, sptLogin, strSoDetail, sptLogin, strSetting)

        'MAPPING
        wsResult = String.Concat(wsResult, sptParam, mappUser, sptLogin, "WebsiteAccessKey", sptLogin, "AppCode", sptLogin, mappItem, sptLogin, mappContact, sptLogin, mappSoUtama, sptLogin, mappSoDetail, sptLogin, mappSetting)

        Return wsResult
    End Function

    <WebMethod()>
    Public Function MobM0_Logout(ByVal param As String) As String

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


        'REMOVE MEMCACHED WEBSITE ACCESS KEY ===============================================
        'If Not IsNothing(AsMemcached.GetCache("myerpplus-" & paramSplit(0))) Then
        '    AsMemcached.Remove("myerpplus-" & paramSplit(0))
        'End If
        'END OF REMOVE MEMCACHED WEBSITE ACCESS KEY ========================================


        'TRANSAKSI KE DATABASE =============================================================
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()


        ''HAPUS TABEL TEMPORARY REPORT -------------------------------
        ''m2r_anggaran, m2r_appostage_card, m2r_appostage_voucher, m2r_ap_card, m2r_ap_voucher, m2r_ap_voucher_aging, 
        ''m2r_ap_card_detail, m2r_ap_voucher_detail, m2r_ap_voucher_aging_detail, 
        ''m2r_arpostage_card, m2r_arpostage_voucher, m2r_aruskas, m2r_ar_card, m2r_ar_voucher, 
        ''m2r_ar_voucher_aging, m2r_ar_card_detail, m2r_ar_voucher_detail, m2r_ar_voucher_aging_detail, 
        ''m2r_bp_card, m2r_general_ledger, m2r_general_ledger_detail, m2r_giro_voucher, 
        ''m2r_giro_voucher_aging, m2r_ip_card, m2r_ip_list, m2r_ip_voucher, m2r_kartu_stok, 
        ''m2r_kasbank_harian, m2r_lr_invoice_detail, m2r_lr_invoice_global, m2r_mutasi_keuangan, m2r_mutasi_stok, m2r_mutasi_stok_detail, 
        ''m2r_neraca_mutasi, m2r_perincian_biaya, m2r_persediaan, m2r_persediaan_detail, m2r_posisi_keuangan, 
        ''m2r_posisi_keuangan_detail, m2r_posisi_keuangan_t, m2r_posisi_keuangan_t_detail, m2r_salesman_point, m2r_umpembelian_card, 
        ''m2r_umpembelian_voucher, m2r_umpenjualan_card, m2r_umpenjualan_voucher

        'sql = "DELETE FROM m2r_anggaran WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_anggaran data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_appostage_card WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_appostage_card data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_appostage_voucher WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_appostage_voucher data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_ap_card WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_ap_card data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_ap_card_detail WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_ap_card_detail data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_ap_voucher WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_ap_voucher data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_ap_voucher_detail WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_ap_voucher_detail data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_ap_voucher_aging WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_ap_voucher_aging data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_ap_voucher_aging_detail WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_ap_voucher_aging_detail data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_arpostage_card WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_arpostage_card data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_arpostage_voucher WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_arpostage_voucher data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_aruskas WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_aruskas data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_ar_card WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_ar_card data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_ar_card_detail WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_ar_card_detail data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_ar_voucher WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_ar_voucher data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_ar_voucher_detail WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_ar_voucher_detail data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_ar_voucher_aging WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_ar_voucher_aging data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_ar_voucher_aging_detail WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_ar_voucher_aging_detail data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_bp_card WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_bp_card data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_general_ledger WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_general_ledger data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_general_ledger_detail WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_general_ledger_detail data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_giro_voucher WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_giro_voucher data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_giro_voucher_aging WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_giro_voucher_aging data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_ip_card WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_ip_card data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_ip_list WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_ip_list data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_ip_voucher WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_ip_voucher data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_kartu_stok WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_kartu_stok data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_kasbank_harian WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_kasbank_harian data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_lr_invoice_detail WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_lr_invoice_detail data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_lr_invoice_global WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_lr_invoice_global data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_mutasi_keuangan WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_mutasi_keuangan data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_mutasi_stok WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_mutasi_stok data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_mutasi_stok_detail WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_mutasi_stok_detail data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_neraca_mutasi WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_neraca_mutasi data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_perincian_biaya WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_perincian_biaya data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_persediaan WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_persediaan data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_persediaan_detail WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_persediaan_detail data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_posisi_keuangan WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_posisi_keuangan data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_posisi_keuangan_detail WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_posisi_keuangan_detail data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_posisi_keuangan_t WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_posisi_keuangan_t data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_posisi_keuangan_t_detail WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_posisi_keuangan_t_detail data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_salesman_point WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_salesman_point data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_umpembelian_card WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_umpembelian_card data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_umpembelian_voucher WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_umpembelian_voucher data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_umpenjualan_card WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_umpenjualan_card data." : GoTo selesai
        'End If

        'sql = "DELETE FROM m2r_umpenjualan_voucher WHERE idlogin = '" & FixQuotes(paramSplit(0)) & "'"
        'If AsEksekusiSQL(sql) = False Then
        '    result(2) = "Failed remove m2r_umpenjualan_voucher data." : GoTo selesai
        'End If
        ''END OF HAPUS TABEL TEMPORARY REPORT ------------------------


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

End Class
