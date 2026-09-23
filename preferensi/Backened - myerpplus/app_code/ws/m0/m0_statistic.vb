Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction

Imports System.Globalization
'<System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class m0_statistic
    Inherits System.Web.Services.WebService
    Dim ClsValidKey As New ClsSecurity
    Dim userid As String = ""     'User Id diisi dengan user yang melakukan proses transaksi
    Dim McUtama As String = ""
    Dim McDetail As String = ""

    <WebMethod()>
    Public Function M0_StatisticSearch(ByVal param As String) As String
        'M0_StatisticSearch ---------------------------------
        'paket1, paket2, paket3, dst...

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim wsResult As String = ""
        Dim strResult, strResultPaging As String
        Dim Filter As String = "", Sorting As String = ""

        Dim strSplit As String = ""
        Dim rsPaket() As String = strSplit.Split("")
        Dim dataPaket() As String, pagingPaket() As String

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


        'VALIDASI DAN SET DATA PAKET =======================================================
        'SPLIT PARAMETER DATA PAKET
        dataPaket = paramSplit(5).Split(sptField)
        rsPaket = paramSplit(5).Split(sptField)

        'SPLIT PARAMETER PAGING PAKET
        pagingPaket = paramSplit(2).Split(sptLogin)

        'CEK JML PAKET VS JML PAGING, HARUS SAMA
        If dataPaket.Length <> pagingPaket.Length Then
            result(2) = "Invalid count Data Packet and Paging Parameter." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA PAKET ================================================


        'PROSES WS SEBANYAK DATA PAKET =====================================================
        Dim Paket As String = ""
        For i = 1 To dataPaket.Length

            'SET NAMA PAKET -----------------------------------
            Paket = dataPaket(i - 1)
            'END OF SET NAMA PAKET ----------------------------


            'VALIDASI PAGING SESUAI PAKET ---------------------
            pagingSplit = pagingPaket(i - 1).Split(sptSubParam)

            'CEK ARRAY PAGING
            If (pagingSplit.Length <> 6) Then
                result(2) = "Packet " & (i - 1) & " - " & Paket & " : Invalid paging parameter." : GoTo selesai
            End If

            'CEK PAGENUMBER
            If (IsNumeric(pagingSplit(0)) = False) Then
                result(2) = "Packet " & (i - 1) & " - " & Paket & " : pageNumber required numeric." : GoTo selesai
            End If

            'CEK ITEMLIMIT
            If (IsNumeric(pagingSplit(1)) = False) Then
                result(2) = "Packet " & (i - 1) & " - " & Paket & " : itemLimit required numeric." : GoTo selesai
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
            'END OF VALIDASI PAGING SESUAI PAKET --------------


            'AMBIL DATA SESUAI PAKET --------------------------
            Select Case Paket
                Case "M0S_Area"
                    rsPaket(i - 1) = M0S_Example("Area", paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M0S_Bar"
                    rsPaket(i - 1) = M0S_Example("Bar", paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M0S_Bubble"
                    rsPaket(i - 1) = M0S_Example("Bubble", paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M0S_Column"
                    rsPaket(i - 1) = M0S_Example("Column", paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M0S_Pie"
                    rsPaket(i - 1) = M0S_Example("Pie", paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M0S_Line"
                    rsPaket(i - 1) = M0S_Example("Line", paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M0S_HLOC"
                    rsPaket(i - 1) = M0S_Example("HLOC", paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M0S_CandleStick"
                    rsPaket(i - 1) = M0S_Example("CandleStick", paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M0S_Plot"
                    rsPaket(i - 1) = M0S_Example("Plot", paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M2S_CashBank"
                    rsPaket(i - 1) = M2S_CashBank(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M2S_GiroData"
                    rsPaket(i - 1) = M2S_GiroData(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M2S_GiroAging"
                    rsPaket(i - 1) = M2S_GiroAging(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M2S_AP"
                    rsPaket(i - 1) = M2S_AP(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M2S_AR"
                    rsPaket(i - 1) = M2S_AR(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M3S_ProdukOmzet"
                    rsPaket(i - 1) = M3S_ProdukOmzet(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M3S_ProdukProfit"
                    rsPaket(i - 1) = M3S_ProdukProfit(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M3S_ProdukLaris"
                    rsPaket(i - 1) = M3S_ProdukLaris(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M3S_ProdukStokMinim"
                    rsPaket(i - 1) = M3S_ProdukStokMinim(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M4_RiSearch"
                    Dim wsM4_Ri As New m4_ri
                    rsPaket(i - 1) = wsM4_Ri.M4_RiSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M5_SiSearch"
                    Dim wsM5_Si As New m5_si
                    rsPaket(i - 1) = wsM5_Si.M5_SiSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M2_SmSearch"
                    Dim wsM2_Sm As New m2_sm
                    rsPaket(i - 1) = wsM2_Sm.M2_SmSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M3_PaSearch"
                    Dim wsM3_Pa As New m3_pa
                    rsPaket(i - 1) = wsM3_Pa.M3_PaSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M4_PoSearch"
                    Dim wsM4_Po As New m4_po
                    rsPaket(i - 1) = wsM4_Po.M4_PoSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M4_ApSearch"
                    Dim wsM4_Ap As New m4_ap
                    rsPaket(i - 1) = wsM4_Ap.M4_ApSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M5_SoSearch"
                    Dim wsM5_So As New m5_so
                    rsPaket(i - 1) = wsM5_So.M5_SoSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M5_PiSearch"
                    Dim wsM5_Pi As New m5_pi
                    rsPaket(i - 1) = wsM5_Pi.M5_PiSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M5_RpSearch"
                    Dim wsM5_Rp As New m5_rp
                    rsPaket(i - 1) = wsM5_Rp.M5_RpSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case "M4_VpSearch"
                    Dim wsM4_Vp As New m4_vp
                    rsPaket(i - 1) = wsM4_Vp.M4_VpSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_NpSearch"
                    '    Dim m8_f_np As New m8_f_np
                    '    rsPaket(i - 1) = m8_f_np.M8_F_NpSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_NpGridSearch"
                    '    Dim m8_f_np As New m8_f_np
                    '    rsPaket(i - 1) = m8_f_np.M8_F_NpGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")


                    'Case "M8_F_NpmSearch"
                    '    Dim m8_f_npm As New m8_f_npm
                    '    rsPaket(i - 1) = m8_f_npm.M8_F_NpmSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_NpmGridSearch"
                    '    Dim m8_f_npm As New m8_f_npm
                    '    rsPaket(i - 1) = m8_f_npm.M8_F_NpmGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_GpmSearch"
                    '    Dim m8_f_gpm As New m8_f_gpm
                    '    rsPaket(i - 1) = m8_f_gpm.M8_F_GpmSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_GpmGridSearch"
                    '    Dim m8_f_gpm As New m8_f_gpm
                    '    rsPaket(i - 1) = m8_f_gpm.M8_F_GpmGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_OpmSearch"
                    '    Dim m8_f_opm As New m8_f_opm
                    '    rsPaket(i - 1) = m8_f_opm.M8_F_OpmSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_OpmGridSearch"
                    '    Dim m8_f_opm As New m8_f_opm
                    '    rsPaket(i - 1) = m8_f_opm.M8_F_OpmGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_EbtidaSearch"
                    '    Dim m8_f_ebtida As New m8_f_ebtida
                    '    rsPaket(i - 1) = m8_f_ebtida.M8_F_EbtidaSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_EbtidaGridSearch"
                    '    Dim m8_f_ebtida As New m8_f_ebtida
                    '    rsPaket(i - 1) = m8_f_ebtida.M8_F_EbtidaGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_RgrSearch"
                    '    Dim m8_f_rgr As New m8_f_rgr
                    '    rsPaket(i - 1) = m8_f_rgr.M8_RgrSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_RgrGridSearch"
                    '    Dim m8_f_rgr As New m8_f_rgr
                    '    rsPaket(i - 1) = m8_f_rgr.M8_RgrGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_EvaSearch"
                    '    Dim m8_f_eva As New m8_f_eva
                    '    rsPaket(i - 1) = m8_f_eva.M8_F_EvaSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_EvaGridSearch"
                    '    Dim m8_f_eva As New m8_f_eva
                    '    rsPaket(i - 1) = m8_f_eva.M8_F_EvaGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_RoiSearch"
                    '    Dim m8_f_roi As New m8_f_roi
                    '    rsPaket(i - 1) = m8_f_roi.M8_F_RoiSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_RoiGridSearch"
                    '    Dim m8_f_roi As New m8_f_roi
                    '    rsPaket(i - 1) = m8_f_roi.M8_F_RoiGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_RoceSearch"
                    '    Dim m8_f_roce As New m8_f_roce
                    '    rsPaket(i - 1) = m8_f_roce.M8_F_RoceSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_RoceGridSearch"
                    '    Dim m8_f_roce As New m8_f_roce
                    '    rsPaket(i - 1) = m8_f_roce.M8_F_RoceGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_RoaSearch"
                    '    Dim m8_f_roa As New m8_f_roa
                    '    rsPaket(i - 1) = m8_f_roa.M8_F_RoaSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_RoaGridSearch"
                    '    Dim m8_f_roa As New m8_f_roa
                    '    rsPaket(i - 1) = m8_f_roa.M8_F_RoaGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_RoeSearch"
                    '    Dim m8_f_roe As New m8_f_roe
                    '    rsPaket(i - 1) = m8_f_roe.M8_F_RoeSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_RoeGridSearch"
                    '    Dim m8_f_roe As New m8_f_roe
                    '    rsPaket(i - 1) = m8_f_roe.M8_F_RoeGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_DteSearch"
                    '    Dim m8_f_dte As New m8_f_dte
                    '    rsPaket(i - 1) = m8_f_dte.M8_F_DteSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_DteGridSearch"
                    '    Dim m8_f_dte As New m8_f_dte
                    '    rsPaket(i - 1) = m8_f_dte.M8_F_DteGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_DsoSearch"
                    '    Dim m8_f_dso As New m8_f_dso
                    '    rsPaket(i - 1) = m8_f_dso.M8_F_DsoSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_DsoGridSearch"
                    '    Dim m8_f_dso As New m8_f_dso
                    '    rsPaket(i - 1) = m8_f_dso.M8_F_DsoGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_DsiSearch"
                    '    Dim m8_f_dsi As New m8_f_dsi
                    '    rsPaket(i - 1) = m8_f_dsi.M8_F_DsiSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_DsiGridSearch"
                    '    Dim m8_f_dsi As New m8_f_dsi
                    '    rsPaket(i - 1) = m8_f_dsi.M8_F_DsiGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_DpoSearch"
                    '    Dim m8_f_dpo As New m8_f_dpo
                    '    rsPaket(i - 1) = m8_f_dpo.M8_F_DpoSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_DpoGridSearch"
                    '    Dim m8_f_dpo As New m8_f_dpo
                    '    rsPaket(i - 1) = m8_f_dpo.M8_F_DpoGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_CccSearch"
                    '    Dim m8_f_ccc As New m8_f_ccc
                    '    rsPaket(i - 1) = m8_f_ccc.M8_F_CccSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_CccGridSearch"
                    '    Dim m8_f_ccc As New m8_f_ccc
                    '    rsPaket(i - 1) = m8_f_ccc.M8_F_CccGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_CtcSearch"
                    '    Dim m8_f_ctc As New m8_f_ctc
                    '    rsPaket(i - 1) = m8_f_ctc.M8_F_CtcSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_CtcGridSearch"
                    '    Dim m8_f_ctc As New m8_f_ctc
                    '    rsPaket(i - 1) = m8_f_ctc.M8_F_CtcGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_CapexSearch"
                    '    Dim m8_f_capex As New m8_f_capex
                    '    rsPaket(i - 1) = m8_f_capex.M8_F_CapexSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_CapexGridSearch"
                    '    Dim m8_f_capex As New m8_f_capex
                    '    rsPaket(i - 1) = m8_f_capex.M8_F_CapexGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_OtsSearch"
                    '    Dim m8_f_ots As New m8_f_ots
                    '    rsPaket(i - 1) = m8_f_ots.M8_F_OtsSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_OtsGridSearch"
                    '    Dim m8_f_ots As New m8_f_ots
                    '    rsPaket(i - 1) = m8_f_ots.M8_F_OtsGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                    'Case "M8_F_WctrSearch"
                    '    Dim m8_f_wtcr As New m8_f_wtcr
                    '    rsPaket(i - 1) = m8_f_wtcr.M8_F_WctrSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")
                    'Case "M8_F_WctrGridSearch"
                    '    Dim m8_f_wtcr As New m8_f_wtcr
                    '    rsPaket(i - 1) = m8_f_wtcr.M8_F_WctrGridSearch(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")


                    'Case "M0S_Example"
                    'rsPaket(i - 1) = M0S_produkstokminim(paramSplit(0) & "★" & Paket & "★" & pagingSplit(0) & "△" & pagingSplit(1) & "△" & pagingSplit(2) & "△" & pagingSplit(3) & "△" & formatTgl & "△" & formatTglWaktu & "★" & paramSplit(3) & "★" & paramSplit(4) & "★")

                Case Else : result(2) = Paket & " : Invalid Statistic Data Packet." : GoTo selesai
            End Select
            'END OF AMBIL DATA SESUAI PAKET -------------------

        Next
        'END OF PROSES WS SEBANYAK DATA PAKET ==============================================

        result(1) = 1

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "

            strResult = String.Join(sptSubParam, result)
            strResultPaging = String.Join(sptSubParam, resultPaging)
            wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam)
        Else
            strResult = String.Join(sptLogin, rsPaket)
            wsResult = String.Concat(strResult)
        End If

        Return wsResult
    End Function

#Region "M0"

    <WebMethod()>
    Public Function M0S_Example(ByVal type As String, ByVal param As String) As String
        'M2S_CashBank --------------------------------------------------------
        'cid, cnomor, cnama, cmatauang, csaldo, csaldovalas

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", search2 As String = ""

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
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        dt = New DataTable
        If type = "Column" Or type = "Bar" Or type = "Pie" Then
            With dt
                .Clear()
                .Columns.Clear()
                .Columns.Add("Country")
                .Columns.Add("Gold")
                .Columns.Add("Silver")
                .Columns.Add("Bronze")
            End With
            isi(dt, "USA", 35, 39, 29, 0)
            isi(dt, "China", 32, 17, 14, 0)
            isi(dt, "Russia", 27, 27, 38, 0)
        ElseIf type = "Area" Or type = "Line" Or type = "Bubble" Then
            With dt
                .Clear()
                .Columns.Clear()
                .Columns.Add("Month")
                .Columns.Add("Profit")
                .Columns.Add("Expenses")
                .Columns.Add("Amount")
            End With
            isi(dt, "Jan", 2000, 1500, 450, 0)
            isi(dt, "Feb", 1000, 200, 600, 0)
            isi(dt, "Mar", 1500, 500, 300, 0)
            isi(dt, "Apr", 1800, 1200, 900, 0)
            isi(dt, "May", 2400, 575, 500, 0)
        ElseIf type = "Plot" Then
            With dt
                .Clear()
                .Columns.Clear()
                .Columns.Add("Month")
                .Columns.Add("Profit")
                .Columns.Add("Expenses")
                .Columns.Add("Amount")
            End With
            isi(dt, "Jan", 2000, 1500, 450, 0)
            isi(dt, "Feb", 1000, 200, 600, 0)
            isi(dt, "Mar", 1500, 500, 300, 0)
        ElseIf type = "CandleStick" Or type = "HLOC" Then
            With dt
                .Clear()
                .Columns.Clear()
                .Columns.Add("Date")
                .Columns.Add("Open")
                .Columns.Add("High")
                .Columns.Add("Low")
                .Columns.Add("Close")
            End With
            isi(dt, "25-Jul", 40.55, 40.75, 40.24, 40.31)
            isi(dt, "26-Jul", 40.15, 40.78, 39.97, 40.34)
            isi(dt, "27-Jul", 40.38, 40.66, 40, 40.63)
            isi(dt, "28-Jul", 40.49, 40.99, 40.3, 40.98)
            isi(dt, "29-Jul", 40.13, 40.4, 39.65, 39.95)
            isi(dt, "1-Aug", 39.0, 39.5, 38.7, 38.6)
            isi(dt, "2-Aug", 38.68, 39.34, 37.75, 38.84)
            isi(dt, "3-Aug", 38.76, 38.76, 38.03, 38.12)
            isi(dt, "4-Aug", 37.98, 37.98, 36.56, 36.69)
            isi(dt, "5-Aug", 36.61, 37, 36.48, 36.86)
        End If

        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                If type = "Column" Or type = "Bar" Or type = "Pie" Then
                    search = String.Concat(search,
                     FxDB(dr("Country"), ""), sptField,
                     FxDB(dr("Gold"), 0), sptField,
                     FxDB(dr("Silver"), 0), sptField,
                     FxDB(dr("Bronze"), 0), sptRow)
                ElseIf type = "Area" Or type = "Line" Or type = "Bubble" Then
                    search = String.Concat(search,
                     FxDB(dr("Month"), ""), sptField,
                     FxDB(dr("Profit"), 0), sptField,
                     FxDB(dr("Expenses"), 0), sptField,
                     FxDB(dr("Amount"), 0), sptRow)
                ElseIf type = "Plot" Then
                    search = String.Concat(search,
                     FxDB(dr("Month"), ""), sptField,
                     FxDB(dr("Profit"), 0), sptField,
                     FxDB(dr("Expenses"), 0), sptField,
                     FxDB(dr("Amount"), 0), sptRow)
                ElseIf type = "CandleStick" Or type = "HLOC" Then
                    search = String.Concat(search,
                     FxDB(dr("Date"), ""), sptField,
                     FxDB(dr("Open"), 0), sptField,
                     FxDB(dr("High"), 0), sptField,
                     FxDB(dr("Low"), 0), sptField,
                     FxDB(dr("Close"), 0), sptRow)
                End If


            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(False))
            resultPaging(1) = Math.Abs(Val(False))
            resultPaging(2) = Math.Abs(Val(False))
            resultPaging(3) = 1
            resultPaging(4) = 1
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

        If type = "Column" Or type = "Bar" Or type = "Pie" Then
            wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("Country, Gold, Silver, Bronze"))
        ElseIf type = "Area" Or type = "Line" Or type = "Bubble" Then
            wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("Month, Profit, Expenses, Amount"))
        ElseIf type = "Plot" Then
            wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("Month, Profit, Expenses, Amount"))
        ElseIf type = "CandleStick" Or type = "HLOC" Then
            wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("Date, Open, High, Low, Close"))
        End If
        Return wsResult


    End Function

    Sub isi(ByVal dt As DataTable, ByVal nama As String, ByVal a1 As Double, ByVal a2 As Double, ByVal a3 As Double, ByVal a4 As Double)
        Dim drow As DataRow
        drow = dt.NewRow()
        drow(0) = nama
        drow(1) = a1
        drow(2) = a2
        drow(3) = a3
        If a4 > 0 Then
            drow(4) = a4
        End If
        dt.Rows.Add(drow)
    End Sub

#End Region

#Region "M2"

    <WebMethod()>
    Public Function M2S_CashBank(ByVal param As String) As String
        'M2S_CashBank --------------------------------------------------------
        'cid, cnomor, cnama, cmatauang, csaldo, csaldovalas

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = "", search2 As String = ""

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
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2s_cashbank")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "co.cnomor", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("cid"), ""), sptField,
                     FxDB(dr("cnomor"), ""), sptField,
                     FxDB(dr("cnama"), ""), sptField,
                     FxDB(dr("cmatauang"), ""), sptField,
                     FxDB(dr("csaldo"), 0), sptField,
                     FxDB(dr("csaldovalas"), 0), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
            'Else
            '    result(2) = "Transaction data not found. #1"
        End If


selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("cid, cnomor, cnama, cmatauang, csaldo, csaldovalas"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2S_GiroData(ByVal param As String) As String
        'M2S_Giro Grid --------------------------------------------------------
        'glnogiro, glnotransaksi, glkontak, gljenis, glbank, glnoacbank, glmatauang, 
        'glkurs, gljumlah, gljumlahvalas, gltgljthtempo, glstatus, glkontakkode, glkontaknama, 
        'glbanknama, glumur, glumurklasifikasi, glumurklasifikasinama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim search As String = ""

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
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2s_giro")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "glnogiro", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("glnogiro"), ""), sptField,
                     FxDB(dr("glnotransaksi"), ""), sptField,
                     FxDB(dr("glkontak"), ""), sptField,
                     FxDB(dr("gljenis"), 0), sptField,
                     FxDB(dr("glbank"), ""), sptField,
                     FxDB(dr("glnoacbank"), ""), sptField,
                     FxDB(dr("glmatauang"), ""), sptField,
                     FxDB(dr("glkurs"), 0), sptField,
                     FxDB(dr("gljumlah"), 0), sptField,
                     FxDB(dr("gljumlahvalas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("gltgljthtempo"), ""), formatTgl), sptField,
                     FxDB(dr("glstatus"), 0), sptField,
                     FxDB(dr("glkontakkode"), ""), sptField,
                     FxDB(dr("glkontaknama"), ""), sptField,
                     FxDB(dr("glbanknama"), ""), sptField,
                     FxDB(dr("glumur"), 0), sptField,
                     FxDB(dr("glumurklasifikasi"), 0), sptField,
                     FxDB(dr("glumurklasifikasinama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
            'Else
            '    result(2) = "Transaction data not found. #1"
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("glnogiro, glnotransaksi, glkontak, gljenis, glbank, glnoacbank, glmatauang, glkurs, gljumlah, gljumlahvalas, gltgljthtempo, glstatus, glkontakkode, glkontaknama, glbanknama, glumur, glumurklasifikasi, glumurklasifikasinama"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2S_GiroAging(ByVal param As String) As String
        'M2S_Giro Grid --------------------------------------------------------
        'glnogiro, glnotransaksi, glkontak, gljenis, glbank, glnoacbank, glmatauang, 
        'glkurs, gljumlah, gljumlahvalas, gltgljthtempo, glstatus, glkontakkode, glkontaknama, 
        'glbanknama, glumur, glumurklasifikasi, glumurklasifikasinama

        On Error GoTo selesai
        Dim formatTgl As String = "", formatTglWaktu As String = ""
        Dim search As String = ""

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
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2s_giro")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "glumurklasifikasi", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("glnogiro"), ""), sptField,
                     FxDB(dr("glnotransaksi"), ""), sptField,
                     FxDB(dr("glkontak"), ""), sptField,
                     FxDB(dr("gljenis"), 0), sptField,
                     FxDB(dr("glbank"), ""), sptField,
                     FxDB(dr("glnoacbank"), ""), sptField,
                     FxDB(dr("glmatauang"), ""), sptField,
                     FxDB(dr("glkurs"), 0), sptField,
                     FxDB(dr("gljumlah"), 0), sptField,
                     FxDB(dr("gljumlahvalas"), 0), sptField,
                     AsFormatTanggal(FxDB(dr("gltgljthtempo"), ""), formatTgl), sptField,
                     FxDB(dr("glstatus"), 0), sptField,
                     FxDB(dr("glkontakkode"), ""), sptField,
                     FxDB(dr("glkontaknama"), ""), sptField,
                     FxDB(dr("glbanknama"), ""), sptField,
                     FxDB(dr("glumur"), 0), sptField,
                     FxDB(dr("glumurklasifikasi"), 0), sptField,
                     FxDB(dr("glumurklasifikasinama"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
            'Else
            '    result(2) = "Transaction data not found. #1"
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("glnogiro, glnotransaksi, glkontak, gljenis, glbank, glnoacbank, glmatauang, glkurs, gljumlah, gljumlahvalas, gltgljthtempo, glstatus, glkontakkode, glkontaknama, glbanknama, glumur, glumurklasifikasi, glumurklasifikasinama"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2S_AP(ByVal param As String) As String
        'Hutang --------------------------------------------------------
        'tkontak, tkontakkode, tkontaknama, tmatauang, tsaldo, tnorek

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
            Filter = "(" & Filter & ") AND t.tstatus IN(2, 3, 4, 7)"
        Else
            Filter = "t.tstatus IN(2, 3, 4, 7)"
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2s_hutang")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "t.tkontak", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("tkontak"), ""), sptField,
                     FxDB(dr("tkontakkode"), ""), sptField,
                     FxDB(dr("tkontaknama"), ""), sptField,
                     FxDB(dr("tmatauang"), ""), sptField,
                     FxDB(dr("tsaldo"), 0), sptField,
                     FxDB(dr("tnorek"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
            'Else
            '    result(2) = "Transaction data not found. #1"
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("tkontak, tkontakkode, tkontaknama, tmatauang, tsaldo, tnorek"))
        Return wsResult
    End Function

    <WebMethod()>
    Public Function M2S_AR(ByVal param As String) As String
        'Piutang --------------------------------------------------------
        'tkontak, tkontakkode, tkontaknama, tmatauang, tsaldo, tnorek

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
            Filter = "(" & Filter & ") AND t.tstatus IN(2, 3, 4, 7)"
        Else
            Filter = "t.tstatus IN(2, 3, 4, 7)"
        End If

        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m2s_piutang")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M2_Cr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "t.tkontak", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("tkontak"), ""), sptField,
                     FxDB(dr("tkontakkode"), ""), sptField,
                     FxDB(dr("tkontaknama"), ""), sptField,
                     FxDB(dr("tmatauang"), ""), sptField,
                     FxDB(dr("tsaldo"), 0), sptField,
                     FxDB(dr("tnorek"), ""), sptRow)
            Next
            search = search.Substring(0, search.Length - sptRow.Length)

            result(1) = 1
            resultPaging(0) = Math.Abs(Val(pg1.isPaging))
            resultPaging(1) = Math.Abs(Val(pg1.isNext))
            resultPaging(2) = Math.Abs(Val(pg1.isPrev))
            resultPaging(3) = pg1.countPage
            resultPaging(4) = pg1.countRow
            'Else
            '    result(2) = "Transaction data not found. #1"
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("tkontak, tkontakkode, tkontaknama, tmatauang, tsaldo, tnorek"))
        Return wsResult
    End Function

#End Region

#Region "M3"

    <WebMethod()>
    Public Function M3S_ProdukOmzet(ByVal param As String) As String
        'M3S_ProdukOmzet --------------------------------------------------------
        'otahun, obulan, oidbarang, onilai, okodebarang, otipebarang, onamabarang

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
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m3s_produk_omzet")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'dt = AmbilData("aplikasi1-M3_Mr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "YEAR(si.sitgl), MONTH(si.sitgl), sid.idbarang", sql) ' Ambil data ke databases
        dt = AmbilData("aplikasi1-M3_Mr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "sid.idbarang", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("otahun"), 0), sptField,
                     FxDB(dr("obulan"), 0), sptField,
                     FxDB(dr("oidbarang"), 0), sptField,
                     FxDB(dr("onilai"), 0), sptField,
                     FxDB(dr("okodebarang"), ""), sptField,
                     FxDB(dr("otipebarang"), ""), sptField,
                     FxDB(dr("onamabarang"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("otahun, obulan, oidbarang, onilai, okodebarang, otipebarang, onamabarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3S_ProdukProfit(ByVal param As String) As String
        'M3S_ProdukProfit --------------------------------------------------------
        'otahun, obulan, oidbarang, onilaijual, onilaihpp, oprofit, okodebarang, otipebarang, onamabarang

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
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m3s_produk_profit")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'dt = AmbilData("aplikasi1-M3_Mr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "YEAR(si.sitgl), MONTH(si.sitgl), sid.idbarang", sql) ' Ambil data ke databases
        dt = AmbilData("aplikasi1-M3_Mr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "sid.idbarang", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("otahun"), 0), sptField,
                     FxDB(dr("obulan"), 0), sptField,
                     FxDB(dr("oidbarang"), 0), sptField,
                     FxDB(dr("onilaijual"), 0), sptField,
                     FxDB(dr("onilaihpp"), 0), sptField,
                     FxDB(dr("oprofit"), 0), sptField,
                     FxDB(dr("okodebarang"), ""), sptField,
                     FxDB(dr("otipebarang"), ""), sptField,
                     FxDB(dr("onamabarang"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("otahun, obulan, oidbarang, onilaijual, onilaihpp, oprofit, okodebarang, otipebarang, onamabarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3S_ProdukLaris(ByVal param As String) As String
        'M3S_ProdukLaris --------------------------------------------------------
        'otahun, obulan, oidbarang, ojmlbarang, osatuanbarang, okodebarang, otipebarang, onamabarang

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
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m3s_produk_laris")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        'dt = AmbilData("aplikasi1-M3_Mr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "YEAR(si.sitgl), MONTH(si.sitgl), sid.idbarang", sql) ' Ambil data ke databases
        dt = AmbilData("aplikasi1-M3_Mr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , "sid.idbarang", sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("otahun"), 0), sptField,
                     FxDB(dr("obulan"), 0), sptField,
                     FxDB(dr("oidbarang"), 0), sptField,
                     FxDB(dr("ojmlbarang"), 0), sptField,
                     FxDB(dr("osatuanbarang"), ""), sptField,
                     FxDB(dr("okodebarang"), ""), sptField,
                     FxDB(dr("otipebarang"), ""), sptField,
                     FxDB(dr("onamabarang"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("otahun, obulan, oidbarang, ojmlbarang, osatuanbarang, okodebarang, otipebarang, onamabarang"))

        Return wsResult
    End Function

    <WebMethod()>
    Public Function M3S_ProdukStokMinim(ByVal param As String) As String
        'M3S_ProdukStokMinim --------------------------------------------------------
        'bid, bkode, btipe, bnama, bstokminimal, bstok, bsatuan

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
        End If
        If (pagingSplit(3).Length > 0) Then
            Sorting = pagingSplit(3)
            '#Taruh fungsi replace disini...
        End If

        'PANGGIL QUERY
        Dim query As New m0_query
        sql = query.PanggilQuery("m3s_produk_stokminim")

        'BUKA KONEKSI
        Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
        Con1.Open()

        dt = AmbilData("aplikasi1-M3_Mr", Filter, Sorting, True, , , pagingSplit(0), pagingSplit(1), pg1, , , , sql) ' Ambil data ke databases
        pg1 = pg1
        If dt.Rows.Count > 0 Then
            For Each dr As DataRow In dt.Rows
                search = String.Concat(search,
                     FxDB(dr("bid"), 0), sptField,
                     FxDB(dr("bkode"), ""), sptField,
                     FxDB(dr("btipe"), ""), sptField,
                     FxDB(dr("bnama"), ""), sptField,
                     FxDB(dr("bstokminimal"), 0), sptField,
                     FxDB(dr("bstok"), 0), sptField,
                     FxDB(dr("bsatuan"), ""), sptRow)
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

        wsResult = String.Concat(wsResult, sptParam, ReplaceMapping("bid, bkode, btipe, bnama, bstokminimal, bstok, bsatuan"))

        Return wsResult
    End Function

#End Region


End Class