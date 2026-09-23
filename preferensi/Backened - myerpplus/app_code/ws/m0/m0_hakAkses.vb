Imports System.Web
Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports AsModuleMySQL.CommonFunction
Imports System.Data

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m0_hakAkses
    Inherits System.Web.Services.WebService
    Dim userid As String = ""
    Dim DtAnak As DataTable

    <WebMethod()> _
    Public Function M0_HakAkses2(ByVal param As String) As String
        KataKunci = "Source code ini punya Alfasoft"

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)

        Dim dtfav As DataTable
        Dim dt1 As DataTable
        Dim dt2 As DataTable
        Dim dt3 As DataTable
        Dim dtModule As DataTable
        Dim PakaiTutup1 As Boolean = False
        Dim PakaiTutup2 As Boolean = False
        Dim PakaiTutup3 As Boolean = False
        Dim sql As String = ""
        Dim sb As New StringBuilder

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m0_usermenu_v")

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPLIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            sb.Append("Invalid parameter.") : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================


        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            sb.Append("WebsiteAccessKey can't be empty.") : GoTo selesai
        End If

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        Dim validKey As RsValidKey
        validKey = ValidateKey(paramSplit(0))
        If Not validKey.success Then sb.Append(validKey.errmessage) : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            sb.Append("Access denied for insert/update data")
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            sb.Append("userid required numeric.") : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dtModule = AsDataTableAmbilDariDB("SELECT mid FROM m0_module")

        If dtModule.Rows.Count > 0 Then

            For Each drModule As DataRow In dtModule.Rows
                sb.Append("<node l='root'>")
                Dim ModuleID As Integer = drModule(0)
                'Buat Favorite
                dtfav = AsDataTableAmbilDariDB(sql & " where ummoduleid=" & ModuleID & " and umuserid=" & userid & " and umfavorite=1 and mnactive=1 ORDER BY mnid")
                If dtfav.Rows.Count > 0 Then

                    sb.Append("<node l='Favorite' t='99'>")
                    For Each drfav As DataRow In dtfav.Rows

                        'If JmlAnak(ModuleID, drfav("mnid")) > 0 Then
                        '    sb.Append("<node label='" & drfav("mnname") & "' tipe='99'>")
                        'Else
                        'sb.Append("<node l='" & drfav("mnname") & "' id='" & drfav("mnid") & "' u='" & drfav("mnurl") & "' t='t" & drfav("mntype") & "' p='" & drfav("mnpopup") & "' lb='" & drfav("mnlebar") & "' tg='" & drfav("mntinggi") & "' a='" & drfav("umakses") & "'/>")
                        'End If

                        'BARU, MENU BERDASARKAN BAHASA USER
                        If (drfav("ubahasa") = "IND") Then
                            sb.Append("<node l='" & drfav("mnnameind") & "' id='" & drfav("mnid") & "' u='" & drfav("mnurl") & "' t='t" & drfav("mntype") & "' p='" & drfav("mnpopup") & "' lb='" & drfav("mnlebar") & "' tg='" & drfav("mntinggi") & "' a='" & drfav("umakses") & "'/>")
                        Else
                            sb.Append("<node l='" & drfav("mnname") & "' id='" & drfav("mnid") & "' u='" & drfav("mnurl") & "' t='t" & drfav("mntype") & "' p='" & drfav("mnpopup") & "' lb='" & drfav("mnlebar") & "' tg='" & drfav("mntinggi") & "' a='" & drfav("umakses") & "'/>")
                        End If

                    Next
                    sb.Append("</node>")
                End If

                'Buat Menu Level-1
                dt1 = AsDataTableAmbilDariDB(sql & " where ummoduleid=" & ModuleID & " and umuserid=" & userid & " and mnlevel=1 and mnactive=1 ORDER BY mnurutan")
                If dt1.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dt1.Rows

                        If JmlAnak(ModuleID, dr1("mnid")) > 0 Then
                            'BARU, MENU BERDASARKAN BAHASA USER
                            If (dr1("ubahasa") = "IND") Then
                                sb.Append("<node l='" & dr1("mnnameind") & "' t='99'>")
                            Else
                                sb.Append("<node l='" & dr1("mnname") & "' t='99'>")
                            End If

                            PakaiTutup1 = True
                        Else
                            'BARU, MENU BERDASARKAN BAHASA USER
                            If (dr1("ubahasa") = "IND") Then
                                sb.Append("<node l='" & dr1("mnnameind") & "' id='" & dr1("mnid") & "' u='" & dr1("mnurl") & "' t='t" & dr1("mntype") & "' p='" & dr1("mnpopup") & "' lb='" & dr1("mnlebar") & "' tg='" & dr1("mntinggi") & "' a='" & dr1("umakses") & "'/>")
                            Else
                                sb.Append("<node l='" & dr1("mnname") & "' id='" & dr1("mnid") & "' u='" & dr1("mnurl") & "' t='t" & dr1("mntype") & "' p='" & dr1("mnpopup") & "' lb='" & dr1("mnlebar") & "' tg='" & dr1("mntinggi") & "' a='" & dr1("umakses") & "'/>")
                            End If
                            PakaiTutup1 = False
                        End If

                        'Buat Menu Level-2
                        dt2 = AsDataTableAmbilDariDB(sql & " where ummoduleid=" & ModuleID & " and mnparent='" & ModuleID & "-" & dr1("mnid") & "' and umuserid=" & userid & " and mnlevel=2 and mnactive=1 ORDER BY mnurutan")
                        If dt2.Rows.Count > 0 Then
                            For Each dr2 As DataRow In dt2.Rows

                                If JmlAnak(ModuleID, dr2("mnid")) > 0 Then
                                    'BARU, MENU BERDASARKAN BAHASA USER
                                    If (dr1("ubahasa") = "IND") Then
                                        sb.Append("<node l='" & dr2("mnnameind") & "' l='99'>")
                                    Else
                                        sb.Append("<node l='" & dr2("mnname") & "' l='99'>")
                                    End If
                                    PakaiTutup2 = True
                                Else
                                    'BARU, MENU BERDASARKAN BAHASA USER
                                    If (dr1("ubahasa") = "IND") Then
                                        sb.Append("<node l='" & dr2("mnnameind") & "' id='" & dr2("mnid") & "' u='" & dr2("mnurl") & "' t='t" & dr2("mntype") & "' p='" & dr2("mnpopup") & "' lb='" & dr2("mnlebar") & "' tg='" & dr2("mntinggi") & "' a='" & dr2("umakses") & "'/>")
                                    Else
                                        sb.Append("<node l='" & dr2("mnname") & "' id='" & dr2("mnid") & "' u='" & dr2("mnurl") & "' t='t" & dr2("mntype") & "' p='" & dr2("mnpopup") & "' lb='" & dr2("mnlebar") & "' tg='" & dr2("mntinggi") & "' a='" & dr2("umakses") & "'/>")
                                    End If
                                    PakaiTutup2 = False
                                End If

                                'Buat Menu Level-3
                                dt3 = AsDataTableAmbilDariDB(sql & " where ummoduleid=" & ModuleID & " and mnparent='" & ModuleID & "-" & dr2("mnid") & "' and umuserid=" & userid & " and mnlevel=3 and mnactive=3 ORDER BY mnurutan")
                                If dt3.Rows.Count > 0 Then
                                    For Each dr3 As DataRow In dt3.Rows

                                        If JmlAnak(ModuleID, dr3("mnid")) > 0 Then
                                            'BARU, MENU BERDASARKAN BAHASA USER
                                            If (dr3("ubahasa") = "IND") Then
                                                sb.Append("<node l='" & dr3("mnnameind") & "' t='99'>")
                                            Else
                                                sb.Append("<node l='" & dr3("mnname") & "' t='99'>")
                                            End If
                                            PakaiTutup3 = True
                                        Else
                                            'BARU, MENU BERDASARKAN BAHASA USER
                                            If (dr3("ubahasa") = "IND") Then
                                                sb.Append("<node l='" & dr3("mnnameind") & "' id='" & dr3("mnid") & "' u='" & dr3("mnurl") & "' t='t" & dr3("mntype") & "' p='" & dr3("mnpopup") & "' lb='" & dr3("mnlebar") & "' tg='" & dr3("mntinggi") & "' a='" & dr3("umakses") & "'/>")
                                            Else
                                                sb.Append("<node l='" & dr3("mnname") & "' id='" & dr3("mnid") & "' u='" & dr3("mnurl") & "' t='t" & dr3("mntype") & "' p='" & dr3("mnpopup") & "' lb='" & dr3("mnlebar") & "' tg='" & dr3("mntinggi") & "' a='" & dr3("umakses") & "'/>")
                                            End If
                                            PakaiTutup3 = False
                                        End If

                                        If PakaiTutup3 = True Then
                                            sb.Append("</node>")
                                        End If

                                    Next
                                End If 'Dt3

                                If PakaiTutup2 = True Then
                                    sb.Append("</node>")
                                End If

                            Next
                        End If 'Dt2

                        If PakaiTutup1 = True Then
                            sb.Append("</node>")
                        End If
                    Next
                End If 'Dt1

                sb.Append("</node>")

                'Pemisah antar Module
                sb.Append("|")
            Next
        End If

        ''TUTUP KONEKSI
        'myCon.Close()
        'myCon = Nothing

selesai:
        Return sb.ToString
    End Function

    <WebMethod()> _
    Public Function M0_HakAkses(ByVal param As String) As String
        'JIKA MENAMBAHKAN FIELD DISINI MAKA HARUS TAMBAHKAN JUGA LENGTH ARRAY PADA VALIDASI AMBIL USERID WS M0_LOGIN
        KataKunci = "Source code ini punya Alfasoft"

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)

        Dim dtfav As DataTable, dt1 As DataTable, dt2 As DataTable, dt3 As DataTable
        Dim dtUser As DataTable, dtModule As DataTable
        Dim PakaiTutup1 As Boolean = False
        Dim PakaiTutup2 As Boolean = False
        Dim PakaiTutup3 As Boolean = False
        Dim sql As String = "", groupBy As String = "", bahasa As String = ""
        Dim sb As New StringBuilder

        'PANGGIL QUERY
        'sql = "select `mn`.`mnmoduleid` AS `mnmoduleid`,`mn`.`mnid` AS `mnid`,`mn`.`mnname` AS `mnname`,`mn`.`mnnameind` AS `mnnameind`,`mn`.`mnurl` AS `mnurl`,`mn`.`mnparent` AS `mnparent`,`mn`.`mntype` AS `mntype`,`mn`.`mnlevel` AS `mnlevel`,`mn`.`mnurutan` AS `mnurutan`,`mn`.`mnactive` AS `mnactive`,`mn`.`mnviewopening` AS `mnviewopening`,`mn`.`mnpopup` AS `mnpopup`,`mn`.`mnlebar` AS `mnlebar`,`mn`.`mntinggi` AS `mntinggi`,`u`.`userid` AS `userid`,u.ubahasa as ubahasa, `rm`.`rmakses` AS `rmakses`,`rm`.`rmfavourite` AS `rmfavourite` from (((`m0_user` `u` join `m0_user_role` `ur` on((`u`.`userid` = `ur`.`userid`))) join `m0_role_menu` `rm` on((`ur`.`role` = `rm`.`rmrole`))) join `m0_menu` `mn` on(((`mn`.`mnmoduleid` = `rm`.`rmmoduleid`) and (`rm`.`rmmenuid` = `mn`.`mnid`))))"
        sql = "select `mn`.`mnmoduleid` AS `mnmoduleid`,`mn`.`mnid` AS `mnid`,`mn`.`mnname` AS `mnname`,`mnl`.`mnltranslate` AS `mnltranslate`,`mn`.`mnurl` AS `mnurl`,`mn`.`mnparent` AS `mnparent`,`mn`.`mntype` AS `mntype`,`mn`.`mnlevel` AS `mnlevel`,`mn`.`mnurutan` AS `mnurutan`,`mn`.`mnactive` AS `mnactive`,`mn`.`mnviewopening` AS `mnviewopening`,`mn`.`mnpopup` AS `mnpopup`,`mn`.`mnlebar` AS `mnlebar`,`mn`.`mntinggi` AS `mntinggi`,`mn`.`mnidtransaksi` AS `mnidtransaksi`,`u`.`userid` AS `userid`,`u`.`ubahasa` AS `ubahasa`,`rm`.`rmakses` AS `rmakses`,`rm`.`rmfavourite` AS `rmfavourite` from ((((`m0_user` `u` join `m0_user_role` `ur` on((`u`.`userid` = `ur`.`userid`))) join `m0_role_menu` `rm` on((convert(`ur`.`role` using utf8) = convert(`rm`.`rmrole` using utf8)))) join `m0_menu` `mn` on(((`mn`.`mnmoduleid` = `rm`.`rmmoduleid`) and (`rm`.`rmmenuid` = `mn`.`mnid`)))) left join `m0_menu_lang` `mnl` on(((`mn`.`mnmoduleid` = `mnl`.`mnlmoduleid`) and (`mn`.`mnid` = `mnl`.`mnlmnid`) and (`u`.`ubahasa` = `mnl`.`mnllanguage`))))"
        groupBy = "group by `u`.`userid`,`mn`.`mnmoduleid`,`mn`.`mnid`"

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPLIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            sb.Append("Invalid parameter.") : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================


        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            sb.Append("WebsiteAccessKey can't be empty.") : GoTo selesai
        End If

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        Dim validKey As RsValidKey
        validKey = ValidateKey(paramSplit(0))
        If Not validKey.success Then sb.Append(validKey.errmessage) : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            sb.Append("Access denied for insert/update data")
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            sb.Append("userid required numeric.") : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dtUser = AsDataTableAmbilDariDB("SELECT ubahasa FROM m0_user WHERE userid='" & userid & "'")
        bahasa = dtUser.Rows(0)("ubahasa").ToString
        dtModule = AsDataTableAmbilDariDB("SELECT mid FROM m0_module ORDER BY mid")

        If dtModule.Rows.Count > 0 Then

            For Each drModule As DataRow In dtModule.Rows
                sb.Append("<node l='root'>")
                Dim ModuleID As Integer = drModule(0)
                'Buat Favorite
                dtfav = AsDataTableAmbilDariDB(sql & " where mn.mnmoduleid=" & ModuleID & " and u.userid=" & userid & " and rmfavourite=1 and mn.mnactive=1 " & groupBy & " ORDER BY mn.mnurutan")

                If dtfav.Rows.Count > 0 Then

                    sb.Append("<node l='Favorite' t='99'>")
                    For Each drfav As DataRow In dtfav.Rows

                        'BARU, MENU BERDASARKAN BAHASA USER
                        If (Len(drfav("mnltranslate").ToString) = 0) Then
                            sb.Append("<node l='" & drfav("mnname") & "' id='" & drfav("mnid") & "' u='" & drfav("mnurl") & "' t='t" & drfav("mntype") & "' p='" & drfav("mnpopup") & "' lb='" & drfav("mnlebar") & "' tg='" & drfav("mntinggi") & "' a='" & drfav("rmakses") & "'")
                        Else
                            sb.Append("<node l='" & drfav("mnltranslate") & "' id='" & drfav("mnid") & "' u='" & drfav("mnurl") & "' t='t" & drfav("mntype") & "' p='" & drfav("mnpopup") & "' lb='" & drfav("mnlebar") & "' tg='" & drfav("mntinggi") & "' a='" & drfav("rmakses") & "'")
                        End If
                        'TAMBAHKAN IDTRANSAKSI UNTUK MENU DATA-DATA DAN LAPORAN
                        If drfav("mntype").ToString.Equals("3") Or drfav("mntype").ToString.Equals("4") Then
                            sb.Append(" idt='" & drfav("mnidtransaksi") & "'")
                        End If
                        'TUTUP NODE
                        sb.Append("/>")

                    Next
                    sb.Append("</node>")
                End If

                'Buat Menu Level-1
                dt1 = AsDataTableAmbilDariDB(sql & " where mn.mnmoduleid=" & ModuleID & " and u.userid=" & userid & " and mn.mnlevel=1 and mn.mnactive=1 " & groupBy & " ORDER BY mn.mnurutan")

                If dt1.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dt1.Rows

                        If JmlAnak(ModuleID, dr1("mnid")) > 0 Then
                            'BARU, MENU BERDASARKAN BAHASA USER
                            If (Len(dr1("mnltranslate").ToString) = 0) Then
                                sb.Append("<node l='" & dr1("mnname") & "' t='99'>")
                            Else
                                sb.Append("<node l='" & dr1("mnltranslate") & "' t='99'>")
                            End If

                            PakaiTutup1 = True
                        Else
                            'BARU, MENU BERDASARKAN BAHASA USER
                            If (Len(dr1("mnltranslate").ToString) = 0) Then
                                sb.Append("<node l='" & dr1("mnname") & "' id='" & dr1("mnid") & "' u='" & dr1("mnurl") & "' t='t" & dr1("mntype") & "' p='" & dr1("mnpopup") & "' lb='" & dr1("mnlebar") & "' tg='" & dr1("mntinggi") & "' a='" & dr1("rmakses") & "'")
                            Else
                                sb.Append("<node l='" & dr1("mnltranslate") & "' id='" & dr1("mnid") & "' u='" & dr1("mnurl") & "' t='t" & dr1("mntype") & "' p='" & dr1("mnpopup") & "' lb='" & dr1("mnlebar") & "' tg='" & dr1("mntinggi") & "' a='" & dr1("rmakses") & "'")
                            End If
                            'TAMBAHKAN IDTRANSAKSI UNTUK MENU DATA-DATA DAN LAPORAN
                            If dr1("mntype").ToString.Equals("3") Or dr1("mntype").ToString.Equals("4") Then
                                sb.Append(" idt='" & dr1("mnidtransaksi") & "'")
                            End If
                            'TUTUP NODE
                            sb.Append("/>")
                            PakaiTutup1 = False
                        End If

                        'Buat Menu Level-2
                        dt2 = AsDataTableAmbilDariDB(sql & " where mn.mnmoduleid=" & ModuleID & " and mn.mnparent='" & ModuleID & "-" & dr1("mnid") & "' and u.userid=" & userid & " and mn.mnlevel=2 and mn.mnactive=1 " & groupBy & " ORDER BY mn.mnurutan")

                        If dt2.Rows.Count > 0 Then
                            For Each dr2 As DataRow In dt2.Rows

                                If JmlAnak(ModuleID, dr2("mnid")) > 0 Then
                                    'BARU, MENU BERDASARKAN BAHASA USER
                                    If (Len(dr2("mnltranslate").ToString) = 0) Then
                                        sb.Append("<node l='" & dr2("mnname") & "' l='99'>")
                                    Else
                                        sb.Append("<node l='" & dr2("mnltranslate") & "' l='99'>")
                                    End If
                                    PakaiTutup2 = True
                                Else
                                    'BARU, MENU BERDASARKAN BAHASA USER
                                    If (Len(dr2("mnltranslate").ToString) = 0) Then
                                        sb.Append("<node l='" & dr2("mnname") & "' id='" & dr2("mnid") & "' u='" & dr2("mnurl") & "' t='t" & dr2("mntype") & "' p='" & dr2("mnpopup") & "' lb='" & dr2("mnlebar") & "' tg='" & dr2("mntinggi") & "' a='" & dr2("rmakses") & "'")
                                    Else
                                        sb.Append("<node l='" & dr2("mnltranslate") & "' id='" & dr2("mnid") & "' u='" & dr2("mnurl") & "' t='t" & dr2("mntype") & "' p='" & dr2("mnpopup") & "' lb='" & dr2("mnlebar") & "' tg='" & dr2("mntinggi") & "' a='" & dr2("rmakses") & "'")
                                    End If
                                    'TAMBAHKAN IDTRANSAKSI UNTUK MENU DATA-DATA DAN LAPORAN
                                    If dr2("mntype").ToString.Equals("3") Or dr2("mntype").ToString.Equals("4") Then
                                        sb.Append(" idt='" & dr2("mnidtransaksi") & "'")
                                    End If
                                    'TUTUP NODE
                                    sb.Append("/>")
                                    PakaiTutup2 = False
                                End If

                                'Buat Menu Level-3
                                dt3 = AsDataTableAmbilDariDB(sql & " where mn.mnmoduleid=" & ModuleID & " and mn.mnparent='" & ModuleID & "-" & dr2("mnid") & "' and u.userid=" & userid & " and mn.mnlevel=3 and mn.mnactive=1 " & groupBy & " ORDER BY mn.mnurutan")

                                If dt3.Rows.Count > 0 Then
                                    For Each dr3 As DataRow In dt3.Rows

                                        If JmlAnak(ModuleID, dr3("mnid")) > 0 Then
                                            'BARU, MENU BERDASARKAN BAHASA USER
                                            If (Len(dr3("mnltranslate").ToString) = 0) Then
                                                sb.Append("<node l='" & dr3("mnname") & "' t='99'>")
                                            Else
                                                sb.Append("<node l='" & dr3("mnltranslate") & "' t='99'>")
                                            End If
                                            PakaiTutup3 = True
                                        Else
                                            'BARU, MENU BERDASARKAN BAHASA USER
                                            If (Len(dr3("mnltranslate").ToString) = 0) Then
                                                sb.Append("<node l='" & dr3("mnname") & "' id='" & dr3("mnid") & "' u='" & dr3("mnurl") & "' t='t" & dr3("mntype") & "' p='" & dr3("mnpopup") & "' lb='" & dr3("mnlebar") & "' tg='" & dr3("mntinggi") & "' a='" & dr3("rmakses") & "'")
                                            Else
                                                sb.Append("<node l='" & dr3("mnltranslate") & "' id='" & dr3("mnid") & "' u='" & dr3("mnurl") & "' t='t" & dr3("mntype") & "' p='" & dr3("mnpopup") & "' lb='" & dr3("mnlebar") & "' tg='" & dr3("mntinggi") & "' a='" & dr3("rmakses") & "'")
                                            End If
                                            'TAMBAHKAN IDTRANSAKSI UNTUK MENU DATA-DATA DAN LAPORAN
                                            If dr3("mntype").ToString.Equals("3") Or dr3("mntype").ToString.Equals("4") Then
                                                sb.Append(" idt='" & dr3("mnidtransaksi") & "'")
                                            End If
                                            'TUTUP NODE
                                            sb.Append("/>")
                                            PakaiTutup3 = False
                                        End If

                                        If PakaiTutup3 = True Then
                                            sb.Append("</node>")
                                        End If

                                    Next
                                End If 'Dt3

                                If PakaiTutup2 = True Then
                                    sb.Append("</node>")
                                End If

                            Next
                        End If 'Dt2

                        If PakaiTutup1 = True Then
                            sb.Append("</node>")
                        End If
                    Next
                End If 'Dt1

                sb.Append("</node>")

                'Pemisah antar Module
                sb.Append("|")
            Next
        End If

selesai:
        Return sb.ToString
    End Function

    Private Function JmlAnak(ByVal ModuleID As Integer, ByVal MenuID As Integer) As Integer
        If IsNothing(DtAnak) Then
            DtAnak = AsDataTableAmbilDariDB("select mnmoduleid, mnid, mnparent from m0_menu")
        End If

        Dim JmlData As Integer = AsDataTableDCount(DtAnak, "mnmoduleid=" & ModuleID & " and mnparent='" & ModuleID & "-" & MenuID & "'")
        Return JmlData
    End Function

    <WebMethod()> _
    Public Function M0_MenuTree(ByVal param As String) As String
        KataKunci = "Source code ini punya Alfasoft"

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim dt1 As DataTable, dt2 As DataTable, dt3 As DataTable
        Dim dtModule As DataTable
        Dim PakaiTutup1 As Boolean = False
        Dim PakaiTutup2 As Boolean = False
        Dim PakaiTutup3 As Boolean = False
        Dim sql As String = "", bahasa As String = "", FilterModule As String = "", Filter As String = "", FilterTree As String = "", search As String = ""
        Dim sb As New StringBuilder

        'PANGGIL QUERY
        sql = "select `mn`.`mnmoduleid` AS `mnmoduleid`,`mn`.`mnid` AS `mnid`,`mn`.`mnname` AS `mnname`,`mnl`.`mnltranslate` AS `mnltranslate`,`mn`.`mnurl` AS `mnurl`,`mn`.`mntype` AS `mntype`,`mn`.`mnpopup` AS `mnpopup`,`mn`.`mnlebar` AS `mnlebar`,`mn`.`mntinggi` AS `mntinggi`,`mn`.`mnidtransaksi` AS `mnidtransaksi`,'1111111111111' AS `rmakses` from (`m0_menu` `mn` left join `m0_menu_lang` `mnl` on(((`mn`.`mnmoduleid` = `mnl`.`mnlmoduleid`) and (`mn`.`mnid` = `mnl`.`mnlmnid`) and (`mnl`.`mnllanguage` = 'valbahasa'))))"

        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPLIT PARAM
        paramSplit = param.Split(sptParam)

        'CEK ARRAY PARAM
        If (paramSplit.Length <> 6) Then
            sb.Append("Invalid parameter.") : GoTo selesai
        End If
        'END OF VALIDASI PARAMETER GLOBAL ==================================================


        'VALIDASI WEBSITEACCESSKEY =========================================================
        If Len(paramSplit(0)) = 0 Then
            sb.Append("WebsiteAccessKey can't be empty.") : GoTo selesai
        End If

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        Dim validKey As RsValidKey
        validKey = ValidateKey(paramSplit(0))
        If Not validKey.success Then sb.Append(validKey.errmessage) : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            sb.Append("Access denied for insert/update data")
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================

        'VALIDASI PARAMETER PAGING =========================================================
        'SPLIT PARAMETER PAGING
        pagingSplit = paramSplit(2).Split(sptSubParam)

        'CEK ARRAY PAGING
        If (pagingSplit.Length <> 6) Then
            sb.Append("Invalid paging parameter.") : GoTo selesai
        End If

        'SET BAHASA
        If (Len(paramSplit(3)) = 0) Then
            sb.Append("Language can't be empty.") : GoTo selesai
        Else
            bahasa = paramSplit(3)
            sql = sql.Replace("valbahasa", bahasa)
        End If

        ''SET FILTER MODULE
        'FilterModule = " mactive = 1 "
        'If (pagingSplit(2).Length > 0) Then
        '    FilterModule = FilterModule & " AND " & pagingSplit(2)
        '    '#Taruh fungsi replace disini...
        'End If
        'SET FILTER MODULE
        If (pagingSplit(2).Length > 0) Then
            FilterModule = pagingSplit(2)
            '#Taruh fungsi replace disini...
        End If

        'SET FILTER MENU
        If (pagingSplit(3).Length > 0) Then
            Filter = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'END OF VALIDASI PARAMETER PAGING ==================================================


        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        If Len(FilterModule) > 0 Then
            dtModule = AsDataTableAmbilDariDB("SELECT mid, 0 as userid, mname, mversion, mcompany, mauthor, mdescription, mactive, murutan FROM m0_module WHERE " & FilterModule & " ORDER BY mid")
        Else
            dtModule = AsDataTableAmbilDariDB("SELECT mid, 0 as userid, mname, mversion, mcompany, mauthor, mdescription, mactive, murutan FROM m0_module ORDER BY mid")
        End If

        'AMBIL DATA MODULE
        If dtModule.Rows.Count > 0 Then
            For Each dr As DataRow In dtModule.Rows
                search = String.Concat(search,
                             FxDB(dr("mid"), 0), sptField,
                             FxDB(dr("userid"), 0), sptField,
                             FxDB(dr("mname"), ""), sptField,
                             FxDB(dr("mversion"), ""), sptField,
                             FxDB(dr("mcompany"), ""), sptField,
                             FxDB(dr("mauthor"), ""), sptField,
                             FxDB(dr("mdescription"), ""), sptField,
                             FxDB(dr("mactive"), 0), sptField,
                             FxDB(dr("murutan"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)
            sb.Append(search & sptLogin)
        Else
            sb.Append("User module data not found.") : GoTo selesai
        End If
        Dim s As String = ""
        If dtModule.Rows.Count > 0 Then

            For Each drModule As DataRow In dtModule.Rows
                sb.Append("<node l='root'>")
                Dim ModuleID As Integer = drModule(0)

                'Buat Menu Level-1
                'If Len(Filter) > 0 Then FilterTree = " where (" & Filter & ") AND mn.mnmoduleid=" & ModuleID & " and mn.mnlevel=1 ORDER BY mn.mnurutan" Else FilterTree = " where mn.mnmoduleid=" & ModuleID & " and mn.mnlevel=1 ORDER BY mn.mnurutan"
                If Len(Filter) > 0 Then FilterTree = " where (" & Filter & ") AND mn.mnmoduleid=" & ModuleID & " and mn.mnlevel=1 and mn.mnactive=1 ORDER BY mn.mnurutan" Else FilterTree = " where mn.mnmoduleid=" & ModuleID & " and mn.mnlevel=1 and mn.mnactive=1 ORDER BY mn.mnurutan"
                dt1 = AsDataTableAmbilDariDB(sql & FilterTree)

                If dt1.Rows.Count > 0 Then
                    For Each dr1 As DataRow In dt1.Rows
                        If JmlAnak(ModuleID, dr1("mnid")) > 0 Then
                            'BARU, MENU BERDASARKAN BAHASA USER
                            If (Len(dr1("mnltranslate").ToString) = 0) Then
                                sb.Append("<node l='" & dr1("mnname") & "' t='99'>")
                            Else
                                sb.Append("<node l='" & dr1("mnltranslate") & "' t='99'>")
                            End If

                            PakaiTutup1 = True
                        Else
                            'BARU, MENU BERDASARKAN BAHASA USER
                            If (Len(dr1("mnltranslate").ToString) = 0) Then
                                sb.Append("<node l='" & dr1("mnname") & "' id='" & dr1("mnid") & "' t='t" & dr1("mntype") & "' idt='" & dr1("mnidtransaksi") & "'/>")
                            Else
                                sb.Append("<node l='" & dr1("mnltranslate") & "' id='" & dr1("mnid") & "' t='t" & dr1("mntype") & "' idt='" & dr1("mnidtransaksi") & "'/>")
                            End If
                            PakaiTutup1 = False
                        End If

                        'Buat Menu Level-2
                        If Len(Filter) > 0 Then FilterTree = " where (" & Filter & ") AND mn.mnmoduleid=" & ModuleID & " and mn.mnparent='" & ModuleID & "-" & dr1("mnid") & "' and mn.mnlevel=2 and mn.mnactive=1  ORDER BY mn.mnurutan" Else FilterTree = " where mn.mnmoduleid=" & ModuleID & " and mn.mnparent='" & ModuleID & "-" & dr1("mnid") & "' and mn.mnlevel=2 and mn.mnactive=1  ORDER BY mn.mnurutan"
                        dt2 = AsDataTableAmbilDariDB(sql & FilterTree)

                        If dt2.Rows.Count > 0 Then
                            For Each dr2 As DataRow In dt2.Rows

                                If JmlAnak(ModuleID, dr2("mnid")) > 0 Then
                                    'BARU, MENU BERDASARKAN BAHASA USER
                                    If (Len(dr2("mnltranslate").ToString) = 0) Then
                                        sb.Append("<node l='" & dr2("mnname") & "' l='99'>")
                                    Else
                                        sb.Append("<node l='" & dr2("mnltranslate") & "' l='99'>")
                                    End If
                                    PakaiTutup2 = True
                                Else
                                    'BARU, MENU BERDASARKAN BAHASA USER
                                    If (Len(dr2("mnltranslate").ToString) = 0) Then
                                        sb.Append("<node l='" & dr2("mnname") & "' id='" & dr2("mnid") & "' t='t" & dr2("mntype") & "' idt='" & dr2("mnidtransaksi") & "'/>")
                                    Else
                                        sb.Append("<node l='" & dr2("mnltranslate") & "' id='" & dr2("mnid") & "' t='t" & dr2("mntype") & "' idt='" & dr2("mnidtransaksi") & "'/>")
                                    End If
                                    PakaiTutup2 = False
                                End If

                                'Buat Menu Level-3
                                If Len(Filter) > 0 Then FilterTree = " where (" & Filter & ") AND mn.mnmoduleid=" & ModuleID & " and mn.mnparent='" & ModuleID & "-" & dr2("mnid") & "' and mn.mnlevel=3 and mn.mnactive=1  ORDER BY mn.mnurutan" Else FilterTree = " where mn.mnmoduleid=" & ModuleID & " and mn.mnparent='" & ModuleID & "-" & dr2("mnid") & "' and mn.mnlevel=3 and mn.mnactive=1  ORDER BY mn.mnurutan"
                                dt3 = AsDataTableAmbilDariDB(sql & FilterTree)

                                If dt3.Rows.Count > 0 Then
                                    For Each dr3 As DataRow In dt3.Rows

                                        If JmlAnak(ModuleID, dr3("mnid")) > 0 Then
                                            'BARU, MENU BERDASARKAN BAHASA USER
                                            If (Len(dr3("mnltranslate").ToString) = 0) Then
                                                sb.Append("<node l='" & dr3("mnname") & "' t='99'>")
                                            Else
                                                sb.Append("<node l='" & dr3("mnltranslate") & "' t='99'>")
                                            End If
                                            PakaiTutup3 = True
                                        Else
                                            'BARU, MENU BERDASARKAN BAHASA USER
                                            If (Len(dr3("mnltranslate").ToString) = 0) Then
                                                sb.Append("<node l='" & dr3("mnname") & "' id='" & dr3("mnid") & "' t='t" & dr3("mntype") & "' idt='" & dr3("mnidtransaksi") & "'/>")
                                            Else
                                                sb.Append("<node l='" & dr3("mnltranslate") & "' id='" & dr3("mnid") & "' t='t" & dr3("mntype") & "' idt='" & dr3("mnidtransaksi") & "'/>")
                                            End If
                                            PakaiTutup3 = False
                                        End If

                                        If PakaiTutup3 = True Then
                                            sb.Append("</node>")
                                        End If

                                    Next
                                End If 'Dt3

                                If PakaiTutup2 = True Then
                                    sb.Append("</node>")
                                End If

                            Next
                        End If 'Dt2

                        If PakaiTutup1 = True Then
                            sb.Append("</node>")
                        End If
                    Next
                End If 'Dt1

                sb.Append("</node>")

                'Pemisah antar Module
                sb.Append("|")
            Next
        End If

selesai:
        Return sb.ToString
    End Function

End Class