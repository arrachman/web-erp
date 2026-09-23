Imports System.IO
Imports System.Xml
Imports System.Net
Imports System.Text
Imports System.Security.Permissions
Imports System.Configuration
Imports System.Web.Configuration

<PermissionSet(SecurityAction.Demand, Name:="FullTrust")> _
<System.Runtime.InteropServices.ComVisibleAttribute(True)> _
Public Class formOtomatisUpload
    '<ConfigurationPropertyAttribute("maxRequestLength", DefaultValue:=999999)> <IntegerValidatorAttribute(MinValue:=5)> Public Property MaxRequestLength As Integer

    Public getlokasi As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)
    Public LokasiLog As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\LogUpload.txt"
    Public LokasiConfig As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\appUpload.config"

    Public strCon As String = "", strConStieReport As String = ""
    Public Req As String = "", Param As String = ""

    Public dtTransaksi As New DataTable

    Dim tmrDurasi As New System.Timers.Timer(), tmrJam As New System.Timers.Timer()
    Dim ctThread As Threading.Thread
    Private webBrowser1 As New WebBrowser()

    Dim noPerulangan As Integer = 0, strResultUpload As String = ""

    Dim sql As String = "", sumber As String = "", idUtama As Double = 0, noTransaksi As String = "", noRef As String = "", userId As Double = 0
    Dim dtUtama As New DataTable, dtDetail As New DataTable, dtBatch As New DataTable, dtSerial As New DataTable, dtPay As New DataTable
    Dim dtDetailCurr As New DataTable, dtBatchCurr As New DataTable, dtSerialCurr As New DataTable, dtPayCurr As New DataTable, drUtama As DataRow

    Dim strWs As String = "", strUtama As String = "", strDetail As String = "", strBatch As String = "", strSerial As String = "", strPay As String = ""
    Dim errMessage As String = "", wsResult As String = ""
    Dim rsWs() As String 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
    Dim rsTarget As String = "", rsSuccess As String = 0, rsMessage As String = "", rsStep As String = 0, rsId As String = 0


#Region "Form"

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        F_Switch("ClearLog")
        F_Switch("Load")
    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles ntfOtomatisUpload.MouseDoubleClick
        ntfOtomatisUpload.Visible = False
        WindowState = FormWindowState.Normal
        ShowInTaskbar = True
    End Sub

    Private Sub formOtoamtisUplaod_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Resize
        If WindowState = FormWindowState.Minimized Then
            ntfOtomatisUpload.Visible = True
            ShowInTaskbar = False
            ntfOtomatisUpload.ShowBalloonTip(1000)
        End If
    End Sub

    Private Sub btnstart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnstart.Click
        F_Switch("Start")
    End Sub

    Private Sub btnstop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnstop.Click
        F_Switch("Stop")
    End Sub

    Private Sub btnuploadnow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnuploadnow.Click
        F_ProsesUploadDownload("upload")
    End Sub

    Private Sub btndownloadnow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btndownloadnow.Click
        F_ProsesUploadDownload("download")
    End Sub

    Private Sub lblclearlog_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblclearlog.Click
        F_Switch("ClearLog")
    End Sub

    Private Sub cbxDurasi_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbxDurasi.CheckedChanged
        If cbxDurasi.Checked Then
            cbxjam.Checked = False
        End If
    End Sub

    Private Sub cbxjam_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbxjam.CheckedChanged
        If cbxjam.Checked Then
            cbxDurasi.Checked = False
        End If
    End Sub

#End Region

#Region "Fungsi"

    Public Sub F_Switch(ByVal param1 As String)

        Try
            SetProgress(param1)

            Select Case param1

                Case "SimpanConfig"
                    If Not File.Exists(LokasiConfig) Then
                        File.Create(LokasiConfig).Dispose()
                    End If

                    Dim data As String = "<setting>" + vbCrLf
                    data += "   <durasi>" + txtdurasi.Text + "</durasi>" + vbCrLf
                    data += "   <jam>" + txtjam.Text + "</jam>" + vbCrLf
                    data += "   <cbxdurasi>" + cbxDurasi.Checked.ToString + "</cbxdurasi>" + vbCrLf
                    data += "   <cbxjam>" + cbxjam.Checked.ToString + "</cbxjam>" + vbCrLf
                    data += "   <lokasi>" + txtlokasi.Text + "</lokasi>" + vbCrLf
                    data += "</setting>"

                    File.WriteAllText(LokasiConfig, data)

                Case "Load"
                    If File.Exists(LokasiLog) Then
                        txtlog.Text = File.ReadAllText(LokasiLog)
                    End If

                    If Not File.Exists(LokasiConfig) Then
                        cbxDurasi.Checked = True
                        cbxjam.Checked = False
                        txtdurasi.Text = "30"
                        txtjam.Text = "18:00"
                        txtjam.Enabled = False
                        txtlokasi.Text = "A"
                        F_Switch("SimpanConfig")

                    Else
                        Dim dataString = File.ReadAllText(LokasiConfig)
                        Using reader As XmlReader = XmlReader.Create(New StringReader(dataString))
                            While reader.Read()
                                Select Case reader.NodeType
                                    Case XmlNodeType.Element
                                        Select Case reader.Name
                                            Case "durasi" : txtdurasi.Text = reader.ReadElementContentAsString()
                                            Case "lokasi" : txtlokasi.Text = reader.ReadElementContentAsString()
                                            Case "jam" : txtjam.Text = reader.ReadElementContentAsString()
                                            Case "cbxjam"
                                                If reader.ReadElementContentAsString().Contains("True") Then
                                                    cbxjam.Checked = True
                                                Else
                                                    cbxjam.Checked = False
                                                End If

                                            Case "cbxdurasi"
                                                If reader.ReadElementContentAsString().Contains("True") Then
                                                    cbxDurasi.Checked = True
                                                Else
                                                    cbxDurasi.Checked = False
                                                End If
                                        End Select
                                End Select
                            End While
                        End Using

                    End If

                    'SET DATA TRANSAKSI
                    dtTransaksi.Columns.Add("Trans")
                    dtTransaksi.Columns.Add("IdTrans")
                    'dtTransaksi.Columns.Add("Tanggal")
                    dtTransaksi.Columns.Add("NoTrans")
                    dtTransaksi.Columns.Add("Uploaded")
                    dtTransaksi.Columns.Add("Desc")

                    Dim ReportConfig As String = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location).Replace("\OtomatisUpload", "") + "\app.config"
                    If Not File.Exists(ReportConfig) Then
                        MsgBox("File Koneksi tidak ada, segera hubungi tim support MyERPplus")
                        Me.Close()
                        Return
                    End If

                    Dim config As String
                    'download report.config
                    config = File.ReadAllText(ReportConfig)
                    'config = Encoding.UTF8.GetString(Convert.FromBase64String(Encoding.UTF8.GetString(Convert.FromBase64String(config)).Replace("UUhKeU5HTm9iV0Z1TURnd01qa3p3aXphcmQ5Mw==", "")))
                    config = Encoding.UTF8.GetString(Convert.FromBase64String(config))

                    'Set Koneksi, lokasi hasil report dan lokasi file mrt dari report.config
                    Using reader As XmlReader = XmlReader.Create(New StringReader(config))
                        While reader.Read()
                            Select Case reader.NodeType
                                Case XmlNodeType.Element
                                    Select Case reader.Name
                                        Case "ConStr" : strCon = reader.ReadElementContentAsString()
                                        Case "ReportConStr" : strConStieReport = reader.ReadElementContentAsString()
                                    End Select
                            End Select
                        End While
                    End Using

                    AddHandler tmrDurasi.Elapsed, AddressOf OnTimedEventDurasi
                    AddHandler tmrJam.Elapsed, AddressOf OnTimedEventJam
                    'MsgBox(strCon)
                   
                    btnstart.PerformClick()

                Case "Stop"
                    F_Switch("SimpanConfig")

                    cbxDurasi.Enabled = True
                    txtdurasi.Enabled = True
                    txtlokasi.Enabled = True

                    cbxjam.Enabled = True
                    txtjam.Enabled = True
                   

                    btnstart.Enabled = True
                    btnstop.Enabled = False
                    tmrDurasi.Enabled = False
                    tmrJam.Enabled = False
                    tmrDurasi.Stop()
                    tmrJam.Stop()

                Case "Start"
                    F_Switch("SimpanConfig")

                    cbxDurasi.Enabled = False
                    txtdurasi.Enabled = False
                    txtlokasi.Enabled = False

                    cbxjam.Enabled = False
                    txtjam.Enabled = False
                    btnstart.Enabled = False
                    btnstop.Enabled = True

                    tmrDurasi.Enabled = True
                    tmrJam.Enabled = True

                    Dim durasi As Integer = txtdurasi.Text

                    tmrDurasi.Interval = durasi * 60 * 1000

                    tmrJam.Interval = 60 * 1000

                    tmrDurasi.Start()
                    tmrJam.Start()

                Case "Upload"

                Case "ClearLog"
                    SetProgress("ClearLog")
                    Dim namafile As String = LokasiLog.Replace(".txt", Now.Year.ToString + Now.Month.ToString + Now.Day.ToString + Now.Hour.ToString + Now.Second.ToString + ".txt")
                    File.Create(namafile).Dispose()
                    File.WriteAllText(namafile, txtlog.Text)
                    txtlog.Text = ""
                    File.WriteAllText(LokasiLog, txtlog.Text)

                Case Else
                    MsgBox("Switch invalid packet.")

            End Select

        Catch ex As Exception
            SetProgress("F_Switch : " & param1 & " - " & ex.Message)

        End Try

    End Sub

    Public Sub Log(ByVal StrVal As String)

        If Len(StrVal) > 0 Then

            Try
                If Not File.Exists(LokasiLog) Then
                    File.Create(LokasiLog).Dispose()
                End If

                Dim streamWriter As StreamWriter = New StreamWriter(LokasiLog, True)
                With streamWriter
                    .Write(Now & " : " & StrVal & vbCrLf)
                    .Flush()
                    .Dispose()
                    .Close()
                End With
                txtlog.AppendText(Now & " : " & StrVal & vbCrLf)

            Catch ex As Exception
                MsgBox("Log : " & ex.Message)

            End Try

        End If

    End Sub

    Private Sub formOtomatisUpload_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        SetProgress("Closed")
    End Sub

    Private Sub OnTimedEventDurasi(ByVal source As Object, ByVal e As ElapsedEventArgs)

        Try
            If cbxDurasi.Checked Then
                SetProgress("Durasi GO")
                F_ProsesUploadDownload("upload")

            End If

        Catch ex As Exception
            SetProgress("Error OnTimedEventDurasi : " + Err.Description)

        End Try

    End Sub

    Private Sub OnTimedEventJam(ByVal source As Object, ByVal e As ElapsedEventArgs)

        Try
            If cbxjam.Checked Then
                If txtjam.Text = Now.ToString("HH:mm") Then
                    SetProgress("Durasi Jam")
                    F_ProsesUploadDownload("uploaddownload")

                End If

            End If

        Catch ex As Exception
            SetProgress("Error OnTimedEventJam : " + Err.Description)

        End Try

    End Sub

    Public Sub F_ProsesUploadDownload(ByVal method As String)

        'JALANKAN PROSES UPLOAD
        ctThread = New Threading.Thread(Sub() MyUploadDownload(method))
        ctThread.Start()

    End Sub

    Private Delegate Sub delegate_ProgressUpdate()

    Public Sub SetProgress(ByVal data As String)

        Try
            If Me.InvokeRequired Then
                Me.Invoke(New delegate_ProgressUpdate(Sub() Me.SetProgress(data)))
            Else
                If data <> "" Then
                    Log(data)
                End If
            End If

        Catch ex As Exception
            MsgBox("SetProgress : " & ex.Message)

        End Try

    End Sub

    'Private Delegate Sub delegate_DGTransaksi()

    'Public Sub SetDGTransaksi(ByVal data As Object)

    '    Try
    '        If Me.InvokeRequired Then
    '            Me.Invoke(New delegate_DGTransaksi(Sub() Me.SetDGTransaksi(data)))

    '        Else
    '            'dgTransaksi.Refresh()
    '            dgTransaksi.DataSource = Nothing
    '            dgTransaksi.DataSource = data
    '            'If dgTransaksi.Rows.Count > 0 Then dgTransaksi.FirstDisplayedScrollingRowIndex = dgTransaksi.RowCount - 1

    '        End If

    '    Catch ex As Exception
    '        SetProgress("SetDGTransaksi : " & ex.Message)

    '    End Try

    'End Sub

    Public Sub ClearVariable()

        Try
            sql = "" : sumber = "" : idUtama = 0 : noTransaksi = "" : noRef = "" : userId = 0
            dtUtama.Clear() : dtDetail.Clear() : dtBatch.Clear() : dtSerial.Clear() : dtPay.Clear()
            dtDetailCurr.Clear() : dtBatchCurr.Clear() : dtSerialCurr.Clear() : dtPayCurr.Clear()
            'drUtama.Delete()
            strWs = "" : strUtama = "" : strDetail = "" : strBatch = "" : strSerial = "" : strPay = ""
            errMessage = "" : wsResult = ""
            'Dim rsWs() As String 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
            rsTarget = "" : rsSuccess = 0 : rsMessage = "" : rsStep = 0 : rsId = 0

        Catch ex As Exception
            SetProgress("ClearVariable : " & ex.Message)

        End Try

    End Sub

    Public Function F_CallWs(ByVal url As String, ByVal param As String) As String

        Try
            'SetProgress(url + " " + param)
            webBrowser1.Document.InvokeScript("f_sendWS", New String() {url, param})

        Catch ex As Exception
            SetProgress("F_CallWs : " & ex.Message)

        End Try

        Return ""
    End Function

    Public Sub f_confirmWS(ByVal kondisi As Object, ByVal data As String)

        'FUNGSI RESULT WS
        Try
            SetProgress("kondisi : " + kondisi.ToString + " data : " + data)

        Catch ex As Exception
            SetProgress("Informasi Split WS : " + Err.Description)

        End Try

    End Sub

    Function WS_Request(ByVal URL As String, ByVal POSTdata As String, Optional ByVal method As String = "POST") As String
        Dim responseData As String = ""

        'TAMBAHKAN PARAM
        POSTdata = "param=" & System.Web.HttpUtility.UrlEncode(POSTdata)

        Try

            Dim cookieJar As New Net.CookieContainer()
            Dim hwrequest As Net.HttpWebRequest = Net.WebRequest.Create(URL)

            hwrequest.CookieContainer = cookieJar
            hwrequest.Accept = "*/*"
            hwrequest.AllowAutoRedirect = True
            hwrequest.UserAgent = "http_requester/0.1"
            hwrequest.Timeout = 6000000
            hwrequest.Method = method

            If hwrequest.Method = "POST" Then

                hwrequest.ContentType = "application/x-www-form-urlencoded"

                Dim encoding As New UTF8Encoding() 'Use UTF8Encoding for XML requests
                Dim postByteArray() As Byte = encoding.GetBytes(POSTdata)

                hwrequest.ContentLength = postByteArray.Length

                Dim postStream As IO.Stream = hwrequest.GetRequestStream()

                postStream.Write(postByteArray, 0, postByteArray.Length)
                postStream.Close()

            End If

            Dim hwresponse As Net.HttpWebResponse = hwrequest.GetResponse()

            If hwresponse.StatusCode = Net.HttpStatusCode.OK Then

                Dim responseStream As IO.StreamReader = _
                  New IO.StreamReader(hwresponse.GetResponseStream())
                responseData = responseStream.ReadToEnd()

            End If

            hwresponse.Close()

        Catch e As Exception
            responseData = "An error occurred: " & e.Message

        End Try

        Return responseData

    End Function

#End Region

#Region "UploadDownload"

    Public Sub MyUploadDownload(ByVal method As String)

        Try

            'AMBIL SETTING URL WS
            strUrlWs = F_getSetting(0, "company", "UrlWS", strCon)
            If Len(strUrlWs) = 0 Then
                MsgBox("Setting URL WS tidak ditemukan, segera hubungi tim support MyERPplus")
                SetProgress("Upload Download Failed : Setting URL WS not found, please contact MyERPplus team support")
                GoTo selesai
                'Me.Close()
                'Return

            Else
                strUrlWs &= "/ws/myerpplus.asmx/Ws"

            End If
            'SetProgress("URL Web Services : " & strUrlWs)

            Select Case method.ToLower
                Case "upload"
                    MyUpload()

                Case "download"
                    MyDownload()

                Case "uploaddownload"
                    MyUpload()
                    MyDownload()

                Case Else
                    SetProgress("MyUploadDownload : Invalid packet")
            End Select

        Catch ex As Exception
            SetProgress("MyUploadDownload : " & ex.Message)

        End Try

selesai:
    End Sub

#End Region

#Region "Upload"

    Public Sub MyUpload()

        Dim stepKe As Double = 0

        SetProgress("Starting upload data.")

        'DISABLE BUTTON DAN TIMER
        btnstop.Enabled = False
        btnuploadnow.Enabled = False
        btndownloadnow.Enabled = False
        tmrDurasi.Stop()
        tmrJam.Stop()

        Try

            'RESET GRID DATA TRANSAKSI
            dtTransaksi.Clear()
            strResultUpload = ""

            'PROSES UPLOAD SP
            'stepKe += 1 : MyUploadM3_SP()

            'PROSES UPLOAD SA
            'stepKe += 1 : MyUploadM3_SA()

            'PROSES UPLOAD SO
            'stepKe += 1 : MyUploadM5_SO()

            'PROSES UPLOAD KONTAK
            stepKe += 1 : MyUploadContact()

            'PROSES UPLOAD SI
            stepKe += 1 : MyUploadM5_SI()

            'SET PROGRESS HASIL UPLOAD
            SetProgress(strResultUpload)

        Catch ex As Exception
            SetProgress("Upload data failed - step : " & stepKe & ". " & ex.Message) : GoTo selesai

        End Try

        SetProgress("Upload data finished.")

selesai:

        'ENABLE BUTTON DAN TIMER
        btnstop.Enabled = True
        btnuploadnow.Enabled = True
        btndownloadnow.Enabled = True
        tmrDurasi.Start()
        tmrJam.Start()

    End Sub


    Public Sub MyUploadM5_SI()

        SetProgress("Start uploading Sales Invoice (SI)...")

        'PROSESNYA
        '1. AMBIL DATA YANG BELUM TERUPLOAD
        '2. CREATE FILE SQL
        '3. EXECUTE FILE SQL
        '4. INSERT KE TABEL PENJUALAN UTAMA, DETAIL, PAY
        '5. UPDATE STATUS UPLOAD PENJUALAN TOKO
        '6. HAPUS DATA YG TERUPLOAD DARI TABEL PENAMPUNG
        '7. INSERT TRANSAKSI BARANG
        '8. HITUNG STOK 

        Dim sql As String = "", idUtama As Double = 0, noTransaksi As String = "", noRef As String = "", userId As Double = 0
        Dim dtUtama As New DataTable, dtDetail As New DataTable, dtBatch As New DataTable, dtSerial As New DataTable, dtPay As New DataTable
        Dim dtDetailCurr As New DataTable, dtBatchCurr As New DataTable, dtSerialCurr As New DataTable, dtPayCurr As New DataTable
        Dim strWs As String = "", strUtama As String = "", strDetail As String = "", strBatch As String = "", strSerial As String = "", strPay As String = ""
        Dim errMessage As String = "", wsResult As String = ""
        Dim rsWs() As String 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim rsTarget As String = "", rsSuccess As String = 0, rsMessage As String = "", rsStep As String = 0, rsId As String = 0
        Dim utama As String = "", detail As String = "", pay As String = "", siid As String = ""
        Dim tglupload As DateTime = Now()
        Dim sqlname As String = txtlokasi.Text & "_" & tglupload.Day & "-" & tglupload.Month & "-" & tglupload.Year & "(" & tglupload.Hour & "_" & tglupload.Minute & "_" & tglupload.Second & ")"
        Try

            'AMBIL DATA BELUM TERUPLOAD
            'AMBIL DATA UTAMA
            'AMBIL DATA BELUM TERUPLOAD
            'AMBIL DATA UTAMA
            SetProgress("Selecting main transaction")

            'UTAMA

            sql = "SELECT si.siid, si.sicabang, si.silokasi, si.sigudang, si.siasalbarang, si.siasalbarangkategori, si.sijenispenjualan, si.sijenispenjualankategori, si.sisaldoawal, si.sicarabayar, si.sisumber, si.siautonotransaksi, si.sinotransaksi, si.sitgl, si.sikodepa, si.sicustomer, si.sicustomerkontak, si.si1alamat1, si.si1alamat2, si.si1alamat3, si.si2alamat1, si.si2alamat2, si.si2alamat3, si.sibagianpenjualan, si.siekspedisi, si.sitglkirim, si.sitermin, si.sitgljatuhtempo, si.siuraian, si.sicatatan, CONCAT('(',si.sinotransaksi,')')sinoref, si.sitglnoref, si.sitglpenutupan, si.simatauang, si.sikurs, si.sihargatermasukpajak, si.sitotal, si.sidiskonpersen, si.sijmldiskon, si.sitotalpajak1detail, si.sitotalpajak2detail, si.sibiayalainpersen, si.sibiayalain, si.sitotaltransaksi, si.sijmluangmuka, si.sijmlbayar, si.sibayartunai, si.sibayarkkredit, si.sibayarkdebit, si.sibayarvoucher, si.sibayarpoin, si.sibayarjmlpoin, si.sichargepersen, si.sicharge, si.sijmlkembali, si.sipoinsebelumnya, si.sipoindidapat, si.sistatuslunas, si.sitgllunas, si.sinofakturpajak, si.sisdhbayarpajak, si.sitglbayarpajak, si.sirekdiskon, si.sirekpajak1, si.sirekpajak2, si.sirekbiayalain, si.sirekuangmuka, si.sirekbayar, si.sirekcharge, si.sirekkembali, si.siidsq, si.siidso, si.siidas, si.siidpi, si.siidpl, si.siiddo, si.siiddr, si.sistatusrnr, si.sistatussr, si.sistatusrealisasi, 0 as sistatussie, '1900-01-01' as sitglsie, si.sistatus, si.sistatussebelumnya, si.sijmlrevisi, si.sicetakanke, si.siinputuser, si.siinputtgl, si.simodifikasiuser, si.simodifikasitgl, si.siposting, si.sipostingtgl, si.situtupperiode, si.siisclose, si.siuploaded, si.sicustomarea, si.sicustomtext1, si.sicustomtext2, si.sicustomtext3, si.sicustomtext4, si.sicustomtext5, si.sicustomtext6, si.sicustomtext7, si.sicustomtext8, si.sicustomtext9, si.sicustomtext10, si.sicustomint1, si.sicustomint2, si.sicustomint3, si.sicustomint4, si.sicustomint5, si.sicustomint6, si.sicustomint7, si.sicustomint8, si.sicustomint9, si.sicustomint10, si.sicustomdbl1, si.sicustomdbl2, si.sicustomdbl3, si.sicustomdbl4, si.sicustomdbl5, si.sicustomdbl6, si.sicustomdbl7, si.sicustomdbl8, si.sicustomdbl9, si.sicustomdbl10, si.sicustomdate1, si.sicustomdate2, si.sicustomdate3, si.sicustomdate4, si.sicustomdate5, si.sicustomdate6, si.sicustomdate7, si.sicustomdate8, si.sicustomdate9, si.sicustomdate10 FROM m5_si si where si.sistatus in (2,3,4,7) AND si.siuploaded = 0 AND si.silokasi = '" & txtlokasi.Text & "'"
            dtUtama = AsDataTableAmbilDariDB(sql, strCon) 'Ambil data ke databases
            'errMessage = dtUtama.Rows.Count & "" : GoTo selesai
            If dtUtama.Rows.Count > 0 Then
                For Each dr As DataRow In dtUtama.Rows
                    siid = String.Concat(siid, FxDB(dr("siid"), ""), ",")

                    utama = String.Concat(utama,
                                          "INSERT INTO `m0_si` VALUES(" &
                             FxDB(dr("siid"), ""), ",",
                            "'" & FxDB(dr("sicabang"), "") & "'", ",",
                            "'" & FxDB(dr("silokasi"), "") & "'", ",",
                            "'" & FxDB(dr("sigudang"), "") & "'", ",",
                            "'" & FxDB(dr("siasalbarang") & "'", ""), ",",
                            "'" & FxDB(dr("siasalbarangkategori") & "'", ""), ",",
                            "'" & FxDB(dr("sijenispenjualan"), "") & "'", ",",
                            "'" & FxDB(dr("sijenispenjualankategori") & "'", ""), ",",
                            "'" & FxDB(dr("sisaldoawal"), "") & "'", ",",
                            "'" & FxDB(dr("sicarabayar"), "") & "'", ",",
                            "'" & FxDB(dr("sisumber"), "") & "'", ",",
                            "'" & FxDB(dr("siautonotransaksi") & "'", ""), ",",
                            "'" & FxDB(dr("sinotransaksi"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitgl"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB(dr("sikodepa"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomer"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomerkontak"), "") & "'", ",",
                            "'" & FxDB(dr("si1alamat1"), "") & "'", ",",
                            "'" & FxDB(dr("si1alamat2"), "") & "'", ",",
                            "'" & FxDB(dr("si1alamat3"), "") & "'", ",",
                            "'" & FxDB(dr("si2alamat1"), "") & "'", ",",
                            "'" & FxDB(dr("si2alamat2"), "") & "'", ",",
                            "'" & FxDB(dr("si2alamat3"), "") & "'", ",",
                            "'" & FxDB(dr("sibagianpenjualan"), "") & "'", ",",
                            "'" & FxDB(dr("siekspedisi"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitglkirim"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB(dr("sitermin"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitgljatuhtempo"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB(dr("siuraian"), "") & "'", ",",
                            "'" & FxDB(dr("sicatatan"), "") & "'", ",",
                            "'" & FxDB(dr("sinoref"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitglnoref"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitglpenutupan"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB(dr("simatauang"), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sikurs")), "") & "'", ",",
                            "'" & FxDB(dr("sihargatermasukpajak"), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sitotal")), "") & "'", ",",
                            "'" & FxDB(dr("sidiskonpersen"), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sijmldiskon")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sitotalpajak1detail")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sitotalpajak2detail")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sibiayalainpersen")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sibiayalain")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sitotaltransaksi")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sijmluangmuka")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sijmlbayar")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sibayartunai")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sibayarkkredit")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sibayarkdebit")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sibayarvoucher")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sibayarpoin")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sibayarjmlpoin")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sichargepersen")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sicharge")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sijmlkembali")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sipoinsebelumnya")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sipoindidapat")), "") & "'", ",",
                            "'" & FxDB(dr("sistatuslunas"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitgllunas"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB(dr("sinofakturpajak"), "") & "'", ",",
                            "'" & FxDB(dr("sisdhbayarpajak"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitglbayarpajak"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB(dr("sirekdiskon"), "") & "'", ",",
                            "'" & FxDB(dr("sirekpajak1"), "") & "'", ",",
                            "'" & FxDB(dr("sirekpajak2"), "") & "'", ",",
                            "'" & FxDB(dr("sirekbiayalain"), "") & "'", ",",
                            "'" & FxDB(dr("sirekuangmuka"), "") & "'", ",",
                            "'" & FxDB(dr("sirekbayar"), "") & "'", ",",
                            "'" & FxDB(dr("sirekcharge"), "") & "'", ",",
                            "'" & FxDB(dr("sirekkembali"), "") & "'", ",",
                            "'" & FxDB(dr("siidsq"), "") & "'", ",",
                            "'" & FxDB(dr("siidso"), "") & "'", ",",
                            "'" & FxDB(dr("siidas"), "") & "'", ",",
                            "'" & FxDB(dr("siidpi"), "") & "'", ",",
                            "'" & FxDB(dr("siidpl"), "") & "'", ",",
                            "'" & FxDB(dr("siiddo"), "") & "'", ",",
                            "'" & FxDB(dr("siiddr"), "") & "'", ",",
                            "'" & FxDB(dr("sistatusrnr"), "") & "'", ",",
                            "'" & FxDB(dr("sistatussr"), "") & "'", ",",
                            "'" & FxDB(dr("sistatusrealisasi"), "") & "'", ",",
                            "'" & FxDB(dr("sistatussie"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitglsie"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB(dr("sistatus"), "") & "'", ",",
                            "'" & FxDB(dr("sistatussebelumnya"), "") & "'", ",",
                            "'" & FxDB(dr("sijmlrevisi"), "") & "'", ",",
                            "'" & FxDB(dr("sicetakanke"), "") & "'", ",",
                            "'" & FxDB(dr("siinputuser"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("siinputtgl"), ""), "yyyy-MM-dd hh:mm:ss") & "'", ",",
                            "'" & FxDB(dr("simodifikasiuser"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("simodifikasitgl"), ""), "yyyy-MM-dd hh:mm:ss") & "'", ",",
                            "'" & FxDB(dr("siposting"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sipostingtgl"), ""), "yyyy-MM-dd hh:mm:ss") & "'", ",",
                            "'" & FxDB(dr("situtupperiode"), "") & "'", ",",
                            "'" & FxDB(dr("siisclose"), "") & "'", ",",
                            "'" & FxDB(dr("siuploaded"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomarea"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext1"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext2"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext3"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext4"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext5"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext6"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext7"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext8"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext9"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext10"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint1"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint2"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint3"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint4"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint5"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint6"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint7"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint8"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint9"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint10"), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sicustomdbl1")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sicustomdbl2")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sicustomdbl3")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sicustomdbl4")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sicustomdbl5")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sicustomdbl6")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sicustomdbl7")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sicustomdbl8")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sicustomdbl9")), "") & "'", ",",
                            "'" & FxDB(FixDouble(dr("sicustomdbl10")), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate1"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate2"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate3"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate4"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate5"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate6"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate7"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate8"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate9"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate10"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB(sqlname, "") & "'", ");" & sptRow)

                Next
                utama = utama.Substring(0, utama.Length - sptRow.Length)
                utama = utama.Replace(sptRow, vbCrLf)
                siid = "(" & siid & ")"
                siid = siid.Replace(",)", ")")
                SetProgress(siid)

                'Detail
                SetProgress("Selecting detail transaction")
                sql = "SELECT sid.idsidetail, sid.idsi, sid.idbarang, sid.namabarang, sid.tipebarang, sid.jml, sid.satuan, sid.nilaisatuan, sid.jmlbarang, sid.satuanbarang, sid.matauang, sid.kurs, sid.idhppkhususmasuk, sid.idhppfifomasuk, sid.harga, sid.hargapricelist, sid.hpp, sid.diskon, sid.jmldiskon, sid.pajak1, sid.jmlpajak1, sid.pajak2, sid.jmlpajak2, sid.cabang, sid.lokasi, sid.gudangasal, sid.gudangtransit, sid.gudangtujuan, sid.rekpersediaan, sid.rekhargapokok, sid.rekdiskonpenjualan, sid.rekpenjualan, sid.costcenter, sid.divisi, sid.subdivisi, sid.proyek, sid.catatan, sid.urutan, sid.idsqdetail, sid.idsodetail, sid.idpidetail, sid.idpldetail, sid.iddodetail, sid.iddrdetail, sid.jmlrnr, sid.statusrnr, sid.jmlsr, sid.statussr, sid.jmlrealisasi, sid.statusrealisasi, sid.isbonus, sid.isbonusfrom, sid.isclose, sid.customtext1, sid.customtext2, CONCAT(si.sinotransaksi,'-T') as customtext3, sid.customdbl1, sid.customdbl2, sid.customdbl3, sid.customdate1, sid.customdate2, sid.customdate3 FROM m5_si_detail sid join m5_si si on sid.idsi = si.siid where si.sistatus in (2,3,4,7) AND si.siuploaded = 0 AND si.silokasi = '" & txtlokasi.Text & "'"
                dtDetail = AsDataTableAmbilDariDB(sql, strCon) ' Ambil data ke databases
                If dtDetail.Rows.Count > 0 Then
                    For Each dr As DataRow In dtDetail.Rows

                        detail = String.Concat(detail,
                                               "INSERT INTO `m0_si_detail` VALUES(" &
                                "'" & FxDB(dr("idsidetail"), "") & "'", ",",
                                "'" & FxDB(dr("idsi"), "") & "'", ",",
                                "'" & FxDB(dr("idbarang"), "") & "'", ",",
                                "'" & FxDB(dr("namabarang"), "") & "'", ",",
                                "'" & FxDB(dr("tipebarang"), "") & "'", ",",
                                "'" & FxDB(dr("jml"), "") & "'", ",",
                                "'" & FxDB(dr("satuan"), "") & "'", ",",
                                "'" & FxDB(FixDouble(dr("nilaisatuan")), "") & "'", ",",
                                "'" & FxDB(FixDouble(dr("jmlbarang")), "") & "'", ",",
                                "'" & FxDB(dr("satuanbarang"), "") & "'", ",",
                                "'" & FxDB(dr("matauang"), "") & "'", ",",
                                "'" & FxDB(dr("kurs"), "") & "'", ",",
                                "'" & FxDB(dr("idhppkhususmasuk"), "") & "'", ",",
                                "'" & FxDB(dr("idhppfifomasuk"), "") & "'", ",",
                                "'" & FxDB(FixDouble(dr("harga")), "") & "'", ",",
                                "'" & FxDB(FixDouble(dr("hargapricelist")), "") & "'", ",",
                                "'" & FxDB(FixDouble(dr("hpp")), "") & "'", ",",
                                "'" & FxDB(FixDouble(dr("diskon")), "") & "'", ",",
                                "'" & FxDB(FixDouble(dr("jmldiskon")), "") & "'", ",",
                                "'" & FxDB(dr("pajak1"), "") & "'", ",",
                                "'" & FxDB(FixDouble(dr("jmlpajak1")), "") & "'", ",",
                                "'" & FxDB(dr("pajak2"), "") & "'", ",",
                                "'" & FxDB(FixDouble(dr("jmlpajak2")), "") & "'", ",",
                                "'" & FxDB(dr("cabang"), "") & "'", ",",
                                "'" & FxDB(dr("lokasi"), "") & "'", ",",
                                "'" & FxDB(dr("gudangasal"), "") & "'", ",",
                                "'" & FxDB(dr("gudangtransit"), "") & "'", ",",
                                "'" & FxDB(dr("gudangtujuan"), "") & "'", ",",
                                "'" & FxDB(dr("rekpersediaan"), "") & "'", ",",
                                "'" & FxDB(dr("rekhargapokok"), "") & "'", ",",
                                "'" & FxDB(dr("rekdiskonpenjualan"), "") & "'", ",",
                                "'" & FxDB(dr("rekpenjualan"), "") & "'", ",",
                                "'" & FxDB(dr("costcenter"), "") & "'", ",",
                                "'" & FxDB(dr("divisi"), "") & "'", ",",
                                "'" & FxDB(dr("subdivisi"), "") & "'", ",",
                                "'" & FxDB(dr("proyek"), "") & "'", ",",
                                "'" & FxDB(dr("catatan"), "") & "'", ",",
                                "'" & FxDB(dr("urutan"), "") & "'", ",",
                                "'" & FxDB(dr("idsqdetail"), "") & "'", ",",
                                "'" & FxDB(dr("idsodetail"), "") & "'", ",",
                                "'" & FxDB(dr("idpidetail"), "") & "'", ",",
                                "'" & FxDB(dr("idpldetail"), "") & "'", ",",
                                "'" & FxDB(dr("iddodetail"), "") & "'", ",",
                                "'" & FxDB(dr("iddrdetail"), "") & "'", ",",
                                "'" & FxDB(dr("jmlrnr"), "") & "'", ",",
                                "'" & FxDB(dr("statusrnr"), "") & "'", ",",
                                "'" & FxDB(dr("jmlsr"), "") & "'", ",",
                                "'" & FxDB(dr("statussr"), "") & "'", ",",
                                "'" & FxDB(dr("jmlrealisasi"), "") & "'", ",",
                                "'" & FxDB(dr("statusrealisasi"), "") & "'", ",",
                                "'" & FxDB(dr("isbonus"), "") & "'", ",",
                                "'" & FxDB(dr("isbonusfrom"), "") & "'", ",",
                                "'" & FxDB(dr("isclose"), "") & "'", ",",
                                "'" & FxDB(dr("customtext1"), "") & "'", ",",
                                "'" & FxDB(dr("customtext2"), "") & "'", ",",
                                "'" & FxDB(dr("customtext3"), "") & "'", ",",
                                "'" & FxDB(FixDouble(dr("customdbl1")), "") & "'", ",",
                                "'" & FxDB(FixDouble(dr("customdbl2")), "") & "'", ",",
                                "'" & FxDB(FixDouble(dr("customdbl3")), "") & "'", ",",
                                "'" & AsFormatTanggal(FxDB(dr("customdate1"), ""), "yyyy-MM-dd") & "'", ",",
                                "'" & AsFormatTanggal(FxDB(dr("customdate2"), ""), "yyyy-MM-dd") & "'", ",",
                                "'" & AsFormatTanggal(FxDB(dr("customdate3"), ""), "yyyy-MM-dd") & "'", ",",
                                "'" & FxDB(sqlname, "") & "'", ");" & sptRow)

                    Next

                    detail = detail.Substring(0, detail.Length - sptRow.Length)
                    detail = detail.Replace(sptRow, vbCrLf)
                    detail = detail.Replace("'", "\'")
                    detail = detail.Replace(",\'", ",'")
                    detail = detail.Replace("\',", "',")
                    detail = detail.Replace("(\'", "('")
                    detail = detail.Replace("\')", "')")
                End If

                'pay
                SetProgress("Selecting payment transaction")
                'sql = "SELECT sid.idsicarabayar as idsicarabayar,sid.idsi as idsi,sid.carabayar as carabayar,sid.matauang as matauang,sid.kurs as kurs,sid.jumlah as jumlah,sid.jumlahvalas as jumlahvalas,sid.nogiro as nogiro,sid.tgljt as tgljt,sid.bank as bank,sid.noacbank as noacbank,sid.rekbank as rekbank,CONCAT(si.sinotransaksi,'-T') as rekgiro,CONCAT(si.sicabang,' - ',si.silokasi) as catatan, sid.urutan as urutan, sid.isclose as isclose FROM m5_si si join m5_si_pay sid ON sid.idsi = si.siid where si.sistatus in (2,3,4,7) AND si.siuploaded = 0 AND si.silokasi = '" & txtlokasi.Text & "'"
                sql = "SELECT sid.idsicarabayar as idsicarabayar,sid.idsi as idsi,sid.carabayar as carabayar,sid.matauang as matauang,sid.kurs as kurs,sid.jumlah as jumlah,sid.jumlahvalas as jumlahvalas,sid.nogiro as nogiro,sid.tgljt as tgljt,sid.bank as bank,sid.noacbank as noacbank,sid.rekbank as rekbank,CONCAT(si.sinotransaksi,'-T') as rekgiro,CONCAT(si.sicabang,' - ',si.silokasi) as catatan, sid.urutan as urutan, sid.isclose as isclose FROM m5_si si join m5_si_pay sid ON sid.idsi = si.siid where sid.idsi in " & siid & ""
                dtPay = AsDataTableAmbilDariDB(sql, strCon) ' Ambil data ke databases
                SetProgress(dtPay.Rows.Count.ToString)
                If dtPay.Rows.Count > 0 Then

                    For Each dr As DataRow In dtPay.Rows
                        pay = String.Concat(pay,
                                            "INSERT INTO `m0_si_pay` VALUES(" &
                                "|" & FxDB(dr("idsicarabayar"), "") & "|", ",",
                                "|" & FxDB(dr("idsi"), "") & "|", ",",
                                "|" & FxDB(dr("carabayar"), "") & "|", ",",
                                "|" & FxDB(dr("matauang"), "") & "|", ",",
                                "|" & FxDB(FixDouble(dr("kurs")), "") & "|", ",",
                                "|" & FxDB(FixDouble(dr("jumlah")), "") & "|", ",",
                                "|" & FxDB(FixDouble(dr("jumlahvalas")), "") & "|", ",",
                                "|" & FxDB(dr("nogiro"), "") & "|", ",",
                                "|" & AsFormatTanggal(FxDB(dr("tgljt"), ""), "yyyy-MM-dd") & "|", ",",
                                "|" & FxDB(dr("bank"), "") & "|", ",",
                                "|" & FxDB(dr("noacbank"), "") & "|", ",",
                                "|" & FxDB(dr("rekbank"), "") & "|", ",",
                                "|" & FxDB(dr("rekgiro"), "") & "|", ",",
                                "|" & FxDB(dr("catatan"), "") & "|", ",",
                                "|" & FxDB(dr("urutan"), "") & "|", ",",
                                "|" & FxDB(dr("isclose"), "") & "|", ",",
                                "|" & FxDB(sqlname, "") & "|", ");" & sptRow)
                    Next
                    pay = pay.Substring(0, pay.Length - sptRow.Length)
                    pay = pay.Replace(sptRow, vbCrLf)
                    pay = pay.Replace("|", """")
                End If

                SetProgress("Create Main SQL File...")
                
                Dim Data As String = utama & vbCrLf & detail & vbCrLf & pay
                Dim fileExecute As String = ""
                '        WebsiteAccessKey       ?       M12_SiCreateFile     ?        0      ?         0     ?                   ?                   ?        dd/MM/yyyy         ?        dd/MM/yyyy H:mm:ss         ?        userId             ?       0         ?     sqlname           ?       data
                strWs = "WebsiteAccessKey" & sptParam & "M12_SiCreateFile" & sptParam & 0 & sptSubParam & 0 & sptSubParam & noRef & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "0" & sptParam & sqlname & ".sql" & sptSubParam & Data

                'UPLOAD TRANSAKSI Create SQL file di pusat
                wsResult = WS_Request(strUrlWs, strWs)
                rsWs = wsResult.Split(sptSubParam) 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
                If rsWs.Length > 1 Then
                    rsTarget = rsWs(0)
                    rsSuccess = rsWs(1)
                    rsMessage = rsWs(2)
                    fileExecute = rsWs(2)
                    rsStep = rsWs(3)
                    rsId = rsWs(4)
                Else
                    rsTarget = "Invalid web services result."
                    rsSuccess = 0
                    rsMessage = wsResult
                    rsStep = 0
                    rsId = 0
                    errMessage = rsMessage : GoTo selesai
                End If

                SetProgress("Execute SQL File...")
                '        WebsiteAccessKey       ?       M12_ExecuteDbFile     ?        0      ?         0     ?                   ?                   ?        dd/MM/yyyy         ?        dd/MM/yyyy H:mm:ss         ?        userId             ?       0         ?     sqlname
                strWs = "WebsiteAccessKey" & sptParam & "M12_ExecuteDbFileAuto" & sptParam & 0 & sptSubParam & 0 & sptSubParam & noRef & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "0" & sptParam & fileExecute

                'EXECUTE SQL FILE DI PUSAT
                wsResult = WS_Request(strUrlWs, strWs)
                rsWs = wsResult.Split(sptSubParam) 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
                If rsWs.Length > 1 Then
                    rsTarget = rsWs(0)
                    rsSuccess = rsWs(1)
                    rsMessage = rsWs(2)
                    rsStep = rsWs(3)
                    rsId = rsWs(4)
                Else
                    rsTarget = "Invalid web services result."
                    rsSuccess = 0
                    rsMessage = wsResult
                    rsStep = 0
                    rsId = 0
                    errMessage = rsMessage : GoTo selesai
                End If

                SetProgress("Insert into main transaction table...")
                '        WebsiteAccessKey       ?       M12_InsertSIUtama     ?        0      ?         0     ?                   ?                   ?        dd/MM/yyyy         ?        dd/MM/yyyy H:mm:ss         ?        userId             ?       0         ?     
                strWs = "WebsiteAccessKey" & sptParam & "M12_InsertSIUtama" & sptParam & 0 & sptSubParam & 0 & sptSubParam & siid & " AND si.silokasi = '" & txtlokasi.Text & "' and siidupload = '" & sqlname & "'" & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "0" & sptParam
                SetProgress(strWs)
                'INSERT SI UTAMA
                wsResult = WS_Request(strUrlWs, strWs)
                rsWs = wsResult.Split(sptSubParam) 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
                If rsWs.Length > 1 Then
                    rsTarget = rsWs(0)
                    rsSuccess = rsWs(1)
                    rsMessage = rsWs(2)
                    rsStep = rsWs(3)
                    rsId = rsWs(4)
                Else
                    rsTarget = "Invalid web services result."
                    rsSuccess = 0
                    rsMessage = wsResult
                    rsStep = 0
                    rsId = 0
                    errMessage = rsMessage : GoTo selesai
                End If

                SetProgress("Insert into detail transaction table...")
                '        WebsiteAccessKey       ?       M12_InsertSIDetail     ?        0      ?         0     ?                   ?                   ?        dd/MM/yyyy         ?        dd/MM/yyyy H:mm:ss         ?        userId             ?       0         ?     
                strWs = "WebsiteAccessKey" & sptParam & "M12_InsertSIDetail" & sptParam & 0 & sptSubParam & 0 & sptSubParam & "sid.idupload = '" & sqlname & "'" & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "0" & sptParam
                SetProgress(strWs)
                'UPLOAD TRANSAKSI (PANGGIL WS SIMPAN SERVER PUSAT)
                wsResult = WS_Request(strUrlWs, strWs)
                rsWs = wsResult.Split(sptSubParam) 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
                If rsWs.Length > 1 Then
                    rsTarget = rsWs(0)
                    rsSuccess = rsWs(1)
                    rsMessage = rsWs(2)
                    rsStep = rsWs(3)
                    rsId = rsWs(4)
                Else
                    rsTarget = "Invalid web services result."
                    rsSuccess = 0
                    rsMessage = wsResult
                    rsStep = 0
                    rsId = 0
                    errMessage = rsMessage : GoTo selesai
                End If

                SetProgress("Insert into pay transaction table...")
                '        WebsiteAccessKey       ?       M12_InsertSIPay     ?        0      ?         0     ?                   ?                   ?        dd/MM/yyyy         ?        dd/MM/yyyy H:mm:ss         ?        userId             ?       0         ?     
                strWs = "WebsiteAccessKey" & sptParam & "M12_InsertSIPay" & sptParam & 0 & sptSubParam & 0 & sptSubParam & "sid.idupload = '" & sqlname & "'" & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "0" & sptParam
                'SetProgress(strWs)
                'UPLOAD TRANSAKSI (PANGGIL WS SIMPAN SERVER PUSAT)
                wsResult = WS_Request(strUrlWs, strWs)
                rsWs = wsResult.Split(sptSubParam) 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
                If rsWs.Length > 1 Then
                    rsTarget = rsWs(0)
                    rsSuccess = rsWs(1)
                    rsMessage = rsWs(2)
                    rsStep = rsWs(3)
                    rsId = rsWs(4)
                Else
                    rsTarget = "Invalid web services result."
                    rsSuccess = 0
                    rsMessage = wsResult
                    rsStep = 0
                    rsId = 0
                    errMessage = rsMessage : GoTo selesai
                End If

                'UPDATE STATUS TRANSAKSI
                If rsSuccess = 1 Then
                    'JIKA UPLOAD BERHASIL MAKA UPDATE STATUS TRANSAKSI MENJADI SUDAH UPLOAD

                    'TRANSAKSI KE DATABASE
                    Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
                    Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

                    Dim myConn As MySql.Data.MySqlClient.MySqlConnection
                    myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
                    myConn.Open()

                    'Start Transaction
                    Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

                    Try
                        SetProgress("Update Status Uploaded..")
                        'UPDATE STATUS TERUPLOAD
                        sql = "UPDATE m5_si si SET si.siuploaded = 1 WHERE si.siid in " & siid & ""
                        'errMessage = sql : GoTo selesai
                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                        With objCmd
                            .Connection = myConn
                            .Transaction = Trans
                            .CommandType = CommandType.Text
                            .CommandText = sql
                        End With
                        objCmd.ExecuteNonQuery()

                        'Commit Transaction
                        Trans.Commit()

                    Catch ex As Exception

                        'RollBack Transaction
                        Trans.Rollback()
                        rsMessage &= " | " & ex.Message
                        errMessage = rsMessage : GoTo selesai
                    End Try

                    Try

                        SetProgress("Delete data from temporary table...")
                        '        WebsiteAccessKey       ?       M12_DeleteSIPenampung     ?        0      ?         0     ?                                                                     ?                   ?        dd/MM/yyyy         ?        dd/MM/yyyy H:mm:ss         ?        userId             ?       0         ?     
                        strWs = "WebsiteAccessKey" & sptParam & "M12_DeleteSIPenampung" & sptParam & 0 & sptSubParam & 0 & sptSubParam & siid & " AND si.silokasi = '" & txtlokasi.Text & "'" & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "0" & sptParam
                        'errMessage = strWs : GoTo selesai
                        'INSERT SI UTAMA
                        wsResult = WS_Request(strUrlWs, strWs)
                        rsWs = wsResult.Split(sptSubParam) 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
                        If rsWs.Length > 1 Then
                            rsTarget = rsWs(0)
                            rsSuccess = rsWs(1)
                            rsMessage = rsWs(2)
                            rsStep = rsWs(3)
                            rsId = rsWs(4)
                        Else
                            rsTarget = "Invalid web services result."
                            rsSuccess = 0
                            rsMessage = wsResult
                            rsStep = 0
                            rsId = 0
                            errMessage = rsMessage : GoTo selesai
                        End If
                    Catch ex As Exception
                        rsMessage &= " | " & ex.Message
                        errMessage = rsMessage : GoTo selesai
                    End Try


                    Try

                        SetProgress("Insert Item Transaction...")
                        '        WebsiteAccessKey       ?       M12_InsertItemTransaction     ?        0      ?         0     ?                                                                     ?                   ?        dd/MM/yyyy         ?        dd/MM/yyyy H:mm:ss         ?        userId             ?       0         ?     
                        strWs = "WebsiteAccessKey" & sptParam & "M12_InsertItemTransaction" & sptParam & 0 & sptSubParam & 0 & sptSubParam & siid & " AND si.silokasi = '" & txtlokasi.Text & "'" & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "0" & sptParam
                        'errMessage = strWs : GoTo selesai
                        'INSERT SI UTAMA
                        wsResult = WS_Request(strUrlWs, strWs)
                        rsWs = wsResult.Split(sptSubParam) 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
                        If rsWs.Length > 1 Then
                            rsTarget = rsWs(0)
                            rsSuccess = rsWs(1)
                            rsMessage = rsWs(2)
                            rsStep = rsWs(3)
                            rsId = rsWs(4)
                        Else
                            rsTarget = "Invalid web services result."
                            rsSuccess = 0
                            rsMessage = wsResult
                            rsStep = 0
                            rsId = 0
                            errMessage = rsMessage : GoTo selesai
                        End If
                    Catch ex As Exception
                        rsMessage &= " | " & ex.Message
                        errMessage = rsMessage : GoTo selesai
                    End Try

                    Try

                        SetProgress("Recalculating stock...")
                        '        WebsiteAccessKey       ?       M12_CalculatingStock     ?        0      ?         0     ?                                                                     ?                   ?        dd/MM/yyyy         ?        dd/MM/yyyy H:mm:ss         ?        userId             ?       0         ?     
                        strWs = "WebsiteAccessKey" & sptParam & "M12_CalculatingStock" & sptParam & 0 & sptSubParam & 0 & sptSubParam & siid & " AND si.silokasi = '" & txtlokasi.Text & "'" & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "0" & sptParam
                        'errMessage = strWs : GoTo selesai
                        'INSERT SI UTAMA
                        wsResult = WS_Request(strUrlWs, strWs)
                        rsWs = wsResult.Split(sptSubParam) 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
                        If rsWs.Length > 1 Then
                            rsTarget = rsWs(0)
                            rsSuccess = rsWs(1)
                            rsMessage = rsWs(2)
                            rsStep = rsWs(3)
                            rsId = rsWs(4)
                        Else
                            rsTarget = "Invalid web services result."
                            rsSuccess = 0
                            rsMessage = wsResult
                            rsStep = 0
                            rsId = 0
                            errMessage = rsMessage : GoTo selesai
                        End If
                    Catch ex As Exception
                        rsMessage &= " | " & ex.Message
                        errMessage = rsMessage : GoTo selesai
                    End Try

                    Try

                        SetProgress("Update global stock...")
                        '        WebsiteAccessKey       ?       M12_UpdateGlobalStock     ?        0      ?         0     ?                                                                     ?                   ?        dd/MM/yyyy         ?        dd/MM/yyyy H:mm:ss         ?        userId             ?       0         ?     
                        strWs = "WebsiteAccessKey" & sptParam & "M12_UpdateGlobalStock" & sptParam & 0 & sptSubParam & 0 & sptSubParam & "" & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "0" & sptParam
                        'errMessage = strWs : GoTo selesai
                        'INSERT SI UTAMA
                        wsResult = WS_Request(strUrlWs, strWs)
                        rsWs = wsResult.Split(sptSubParam) 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
                        If rsWs.Length > 1 Then
                            rsTarget = rsWs(0)
                            rsSuccess = rsWs(1)
                            rsMessage = rsWs(2)
                            rsStep = rsWs(3)
                            rsId = rsWs(4)
                        Else
                            rsTarget = "Invalid web services result."
                            rsSuccess = 0
                            rsMessage = wsResult
                            rsStep = 0
                            rsId = 0
                            errMessage = rsMessage : GoTo selesai
                        End If
                    Catch ex As Exception
                        rsMessage &= " | " & ex.Message
                        errMessage = rsMessage : GoTo selesai
                    End Try
                End If
            Else
                errMessage = "Data was uploaded" : GoTo selesai
            End If
        Catch ex As Exception

            errMessage = "Failed : gagal semuanya " & ex.Message
            GoTo selesai

        End Try




        errMessage = "Success"

selesai:
        SetProgress("Finish Upload Sales Invoice (SI) - " & errMessage)

    End Sub

    Public Sub MyUploadContact()

        SetProgress("Start uploading Contact...")

        'PROSESNYA
        '1. AMBIL DATA KONTAK YANG DIEDIT YANG BELUM TERUPLOAD (UTAMA, ATTENTION, PRICE, COMMISION)
        '2. INSERT/UPDATE KE TABEL KONTAK
        '3. UPDATE STATUS UPLOAD KONTAK TOKO

        Dim sql As String = "", idUtama As Double = 0, noTransaksi As String = "", noRef As String = "", userId As Double = 0
        Dim dtUtama As New DataTable, dtAttention As New DataTable, dtPrice As New DataTable, dtCommission As New DataTable
        Dim dtAttentionCurr As New DataTable, dtPriceCurr As New DataTable, dtCommissionCurr As New DataTable
        Dim strWs As String = "", strUtama As String = "", strAttention As String = "", strPrice As String = "", strCommission As String = ""
        Dim errMessage As String = "", wsResult As String = ""
        Dim rsWs() As String 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim rsTarget As String = "", rsSuccess As String = 0, rsMessage As String = "", rsStep As String = 0, rsId As String = 0
        Dim utama As String = "", Attention As String = "", siid As String = ""
        Try

            'UTAMA
            SetProgress("Selecting Contact Main data")
            sql = "SELECT c.* FROM m1_contact c WHERE c.ksinkron = 0 ORDER BY c.kid ASC"
            dtUtama = AsDataTableAmbilDariDB(sql, strCon)
            If dtUtama.Rows.Count > 0 Then

                'AMBIL DATA Attention
                SetProgress("Selecting Contact Attention data")
                sql = "SELECT ca.* FROM m1_contact c JOIN m1_contact_attention ca ON c.kid = ca.kaidkontak AND c.ksinkron = 0 ORDER BY c.kid ASC, ca.kaid ASC"
                dtAttention = AsDataTableAmbilDariDB(sql, strCon)

                'AMBIL DATA Price
                SetProgress("Selecting Contact Price data")
                sql = "SELECT cp.* FROM m1_contact c JOIN m1_contact_price cp ON c.kid = cp.khidkontak AND c.ksinkron = 0 ORDER BY c.kid ASC, cp.khidbarang ASC"
                dtPrice = AsDataTableAmbilDariDB(sql, strCon)

                'AMBIL DATA Commission
                SetProgress("Selecting Contact Commission data")
                sql = "SELECT sc.* FROM m1_contact c JOIN m1_salesman_commission sc ON c.kid = sc.scidkontak AND c.ksinkron = 0 ORDER BY c.kid ASC, sc.scidkontak ASC"
                dtCommission = AsDataTableAmbilDariDB(sql, strCon)


                'PERULANGAN PROSES UPLOAD DAN UPDATE STATUS UPLOADED
                'PROSES UTAMA
                SetProgress("Processing upload data Contact")
                For Each drUtama As DataRow In dtUtama.Rows

                    'RESET VARIABEL
                    strUtama = "" : strAttention = "" : strPrice = "" : strCommission = ""

                    'SET VARIABEL
                    idUtama = FixDouble(drUtama("kid"))

                    'MAPPING UTAMA
                    '            kid,                                   kkode,                                  knama,                                  kkategori,                                  kkategorinama,                                  kcabang,                                    kcabangnama,                                    klokasi,                                    klokasinama,                                    kgudang,                                kgudangnama,                                    kkategorisalesman,                                  kkategorisalesmannama,                                      karea,                                  kareanama,                                  kkategoricustomer,                                  kkategoricustomernama,                                      kdivisi,                                kdivisinama,                                    ksubdivisi,                                     ksubdivisinama,                         ksalesman,                                  ksalesmannama,                                  kkontakperson,                          kterminglobal,                      kaktif,                                                 kaktiftgl,                                      k1alamat1,                                  k1alamat2,                                  k1alamat3,                                      k1alamat4,                                  k1alamat5,                                  k1kota,                                     k1propinsi,                                 k1kodepos,                                  k1negara,                                   k1kontakperson,                                     k1kontaknohp,                                   k1kontakemail,                                  k1notelp1,                                  k1notelp2,                                      k1nofax,                                k1email,                                    k1website,                                  k2alamat1,                                  k2alamat2,                                      k2alamat3,                                  k2alamat4,                                  k2alamat5,                                      k2propinsi,                                 k2kota,                                 k2kodepos,                                      k2negara,                                   k2kontakperson,                                     k2kontaknohp,                                   k2kontakemail,                                  k2notelp1,                                  k2notelp2,                                  k2nofax,                                    k2email,                                    k2website,                                  k3alamat1,                                  k3alamat2,                                  k3alamat3,                                  k3alamat4,                                      k3alamat5,                                  k3kota,                                     k3propinsi,                                 k3kodepos,                                      k3negara,                                   k3kontakperson,                                 k3kontaknohp,                                   k3kontakemail,                                  k3notelp1,                                      k3notelp2,                                  k3nofax,                                k3email,                                    k3website,                                  k4alamat1,                                  k4alamat2,                                      k4alamat3,                                  k4alamat4,                                  k4alamat5,                                      k4kota,                                 k4propinsi,                                     k4kodepos,                                  k4negara,                                   k4kontakperson,                                 k4kontaknohp,                                       k4kontakemail,                                  k4notelp1,                                  k4notelp2,                                  k4nofax,                                    k4email,                                    k4website,                                  knpwp,                          kpkp,                               kbatashutang,                                   kterminbeli,                                    krekhutang,                         kbagpembelian,                                  kfobbeli,                                   kviabeli,                                   kbataspiutang,                                  kterminjual,                                    krekpiutang,                            kbagpenjualan,                      ktingkatjual,                                   kfobjual,                                   kviajual,                                                   ktglkontrak,                                    kbank,                                  knorekening,                        kjeniskelamin,                                  kmatauang,                                                      ktgllahir,                                                  ktglnikah,                                      kkomisipenjualan,                                   kcatatan,                       kinputuser,                                                 kinputtgl,                                      kcustomtext1,                                   kcustomtext2,                                   kcustomtext3,                                   kcustomtext4,                                   kcustomtext5,                                   kcustomtext6,                                   kcustomtext7,                                   kcustomtext8,                                   kcustomtext9,                       kmodifikasiuser,                                                    kmodifikasitgl,                                     kcustomtext10,                      kcustomint1,                        kcustomint2,                        kcustomint3,                                    kcustomdbl1,                                    kcustomdbl2,                                kcustomdbl3,                                                    kcustomdate1,                                                   kcustomdate2,                                                   kcustomdate3,                                       kkategorisupplier,                                  kkategorisuppliernama,                                  kkomisikode,                                    khargacustom
                    strUtama = idUtama & sptField & FixQuotes(drUtama("kkode")) & sptField & FixQuotes(drUtama("knama")) & sptField & FixQuotes(drUtama("kkategori")) & sptField & FixQuotes(drUtama("kkategorinama")) & sptField & FixQuotes(drUtama("kcabang")) & sptField & FixQuotes(drUtama("kcabangnama")) & sptField & FixQuotes(drUtama("klokasi")) & sptField & FixQuotes(drUtama("klokasinama")) & sptField & FixQuotes(drUtama("kgudang")) & sptField & FixQuotes(drUtama("kgudangnama")) & sptField & FixQuotes(drUtama("kkategorisalesman")) & sptField & FixQuotes(drUtama("kkategorisalesmannama")) & sptField & FixQuotes(drUtama("karea")) & sptField & FixQuotes(drUtama("kareanama")) & sptField & FixQuotes(drUtama("kkategoricustomer")) & sptField & FixQuotes(drUtama("kkategoricustomernama")) & sptField & FixQuotes(drUtama("kdivisi")) & sptField & FixQuotes(drUtama("kdivisinama")) & sptField & FixQuotes(drUtama("ksubdivisi")) & sptField & FixQuotes(drUtama("ksubdivisinama")) & sptField & drUtama("ksalesman") & sptField & FixQuotes(drUtama("ksalesmannama")) & sptField & FixQuotes(drUtama("kkontakperson")) & sptField & drUtama("kterminglobal") & sptField & drUtama("kaktif") & sptField & FixQuotes(AsFormatTanggal(drUtama("kaktiftgl"))) & sptField & FixQuotes(drUtama("k1alamat1")) & sptField & FixQuotes(drUtama("k1alamat2")) & sptField & FixQuotes(drUtama("k1alamat3")) & sptField & FixQuotes(drUtama("k1alamat4")) & sptField & FixQuotes(drUtama("k1alamat5")) & sptField & FixQuotes(drUtama("k1kota")) & sptField & FixQuotes(drUtama("k1propinsi")) & sptField & FixQuotes(drUtama("k1kodepos")) & sptField & FixQuotes(drUtama("k1negara")) & sptField & FixQuotes(drUtama("k1kontakperson")) & sptField & FixQuotes(drUtama("k1kontaknohp")) & sptField & FixQuotes(drUtama("k1kontakemail")) & sptField & FixQuotes(drUtama("k1notelp1")) & sptField & FixQuotes(drUtama("k1notelp2")) & sptField & FixQuotes(drUtama("k1nofax")) & sptField & FixQuotes(drUtama("k1email")) & sptField & FixQuotes(drUtama("k1website")) & sptField & FixQuotes(drUtama("k2alamat1")) & sptField & FixQuotes(drUtama("k2alamat2")) & sptField & FixQuotes(drUtama("k2alamat3")) & sptField & FixQuotes(drUtama("k2alamat4")) & sptField & FixQuotes(drUtama("k2alamat5")) & sptField & FixQuotes(drUtama("k2propinsi")) & sptField & FixQuotes(drUtama("k2kota")) & sptField & FixQuotes(drUtama("k2kodepos")) & sptField & FixQuotes(drUtama("k2negara")) & sptField & FixQuotes(drUtama("k2kontakperson")) & sptField & FixQuotes(drUtama("k2kontaknohp")) & sptField & FixQuotes(drUtama("k2kontakemail")) & sptField & FixQuotes(drUtama("k2notelp1")) & sptField & FixQuotes(drUtama("k2notelp2")) & sptField & FixQuotes(drUtama("k2nofax")) & sptField & FixQuotes(drUtama("k2email")) & sptField & FixQuotes(drUtama("k2website")) & sptField & FixQuotes(drUtama("k3alamat1")) & sptField & FixQuotes(drUtama("k3alamat2")) & sptField & FixQuotes(drUtama("k3alamat3")) & sptField & FixQuotes(drUtama("k3alamat4")) & sptField & FixQuotes(drUtama("k3alamat5")) & sptField & FixQuotes(drUtama("k3kota")) & sptField & FixQuotes(drUtama("k3propinsi")) & sptField & FixQuotes(drUtama("k3kodepos")) & sptField & FixQuotes(drUtama("k3negara")) & sptField & FixQuotes(drUtama("k3kontakperson")) & sptField & FixQuotes(drUtama("k3kontaknohp")) & sptField & FixQuotes(drUtama("k3kontakemail")) & sptField & FixQuotes(drUtama("k3notelp1")) & sptField & FixQuotes(drUtama("k3notelp2")) & sptField & FixQuotes(drUtama("k3nofax")) & sptField & FixQuotes(drUtama("k3email")) & sptField & FixQuotes(drUtama("k3website")) & sptField & FixQuotes(drUtama("k4alamat1")) & sptField & FixQuotes(drUtama("k4alamat2")) & sptField & FixQuotes(drUtama("k4alamat3")) & sptField & FixQuotes(drUtama("k4alamat4")) & sptField & FixQuotes(drUtama("k4alamat5")) & sptField & FixQuotes(drUtama("k4kota")) & sptField & FixQuotes(drUtama("k4propinsi")) & sptField & FixQuotes(drUtama("k4kodepos")) & sptField & FixQuotes(drUtama("k4negara")) & sptField & FixQuotes(drUtama("k4kontakperson")) & sptField & FixQuotes(drUtama("k4kontaknohp")) & sptField & FixQuotes(drUtama("k4kontakemail")) & sptField & FixQuotes(drUtama("k4notelp1")) & sptField & FixQuotes(drUtama("k4notelp2")) & sptField & FixQuotes(drUtama("k4nofax")) & sptField & FixQuotes(drUtama("k4email")) & sptField & FixQuotes(drUtama("k4website")) & sptField & FixQuotes(drUtama("knpwp")) & sptField & drUtama("kpkp") & sptField & FixDouble(drUtama("kbatashutang")) & sptField & FixQuotes(drUtama("kterminbeli")) & sptField & FixQuotes(drUtama("krekhutang")) & sptField & drUtama("kbagpembelian") & sptField & FixQuotes(drUtama("kfobbeli")) & sptField & FixQuotes(drUtama("kviabeli")) & sptField & FixDouble(drUtama("kbataspiutang")) & sptField & FixQuotes(drUtama("kterminjual")) & sptField & FixQuotes(drUtama("krekpiutang")) & sptField & drUtama("kbagpenjualan") & sptField & drUtama("ktingkatjual") & sptField & FixQuotes(drUtama("kfobjual")) & sptField & FixQuotes(drUtama("kviajual")) & sptField & FixQuotes(AsFormatTanggal(drUtama("ktglkontrak"))) & sptField & FixQuotes(drUtama("kbank")) & sptField & FixQuotes(drUtama("knorekening")) & sptField & drUtama("kjeniskelamin") & sptField & FixQuotes(drUtama("kmatauang")) & sptField & FixQuotes(AsFormatTanggal(drUtama("ktgllahir"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("ktglnikah"))) & sptField & FixDouble(drUtama("kkomisipenjualan")) & sptField & FixQuotes(drUtama("kcatatan")) & sptField & drUtama("kinputuser") & sptField & FixQuotes(AsFormatTanggal(drUtama("kinputtgl"))) & sptField & FixQuotes(drUtama("kcustomtext1")) & sptField & FixQuotes(drUtama("kcustomtext2")) & sptField & FixQuotes(drUtama("kcustomtext3")) & sptField & FixQuotes(drUtama("kcustomtext4")) & sptField & FixQuotes(drUtama("kcustomtext5")) & sptField & FixQuotes(drUtama("kcustomtext6")) & sptField & FixQuotes(drUtama("kcustomtext7")) & sptField & FixQuotes(drUtama("kcustomtext8")) & sptField & FixQuotes(drUtama("kcustomtext9")) & sptField & drUtama("kmodifikasiuser") & sptField & FixQuotes(AsFormatTanggal(drUtama("kmodifikasitgl"))) & sptField & FixQuotes(drUtama("kcustomtext10")) & sptField & drUtama("kcustomint1") & sptField & drUtama("kcustomint2") & sptField & drUtama("kcustomint3") & sptField & FixDouble(drUtama("kcustomdbl1")) & sptField & FixDouble(drUtama("kcustomdbl2")) & sptField & FixDouble(drUtama("kcustomdbl3")) & sptField & FixQuotes(AsFormatTanggal(drUtama("kcustomdate1"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("kcustomdate2"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("kcustomdate3"))) & sptField & FixQuotes(drUtama("kkategorisupplier")) & sptField & FixQuotes(drUtama("kkategorisuppliernama")) & sptField & FixQuotes(drUtama("kkomisikode")) & sptField & FixDouble(drUtama("khargacustom"))

                    'PROSES Attention
                    dtAttentionCurr = AsDataTableFilterSortDt(dtAttention, "kaidkontak = " & idUtama)
                    For Each dr1 As DataRow In dtAttentionCurr.Rows
                        strAttention = IIf(Len(strAttention) > 0, strAttention & sptRow, strAttention)

                        'MAPPING Attention
                        '                   kaid,                   kaidkontak,                                 kakodekontak,                               kanama,                             kajabatan,                              kanotelp,                                   kanofax,                            kanohp,                                 kaemail,                            kawebsite,                                  kamessenger,                            kaalamat,                                               katgllahir,                                                 katglnikah,                                 kacatatan,                      kadefault,                  kainputuser,                                                kainputtgl,                                             kamodifikasiuser,                                           kamodifikasitgl
                        strAttention &= dr1("kaid") & sptField & dr1("kaidkontak") & sptField & FixQuotes(dr1("kakodekontak")) & sptField & FixQuotes(dr1("kanama")) & sptField & FixQuotes(dr1("kajabatan")) & sptField & FixQuotes(dr1("kanotelp")) & sptField & FixQuotes(dr1("kanofax")) & sptField & FixQuotes(dr1("kanohp")) & sptField & FixQuotes(dr1("kaemail")) & sptField & FixQuotes(dr1("kawebsite")) & sptField & FixQuotes(dr1("kamessenger")) & sptField & FixQuotes(dr1("kaalamat")) & sptField & FixQuotes(AsFormatTanggal(dr1("katgllahir"))) & sptField & FixQuotes(AsFormatTanggal(dr1("katglnikah"))) & sptField & FixQuotes(dr1("kacatatan")) & sptField & dr1("kadefault") & sptField & dr1("kainputuser") & sptField & FixQuotes(AsFormatTanggal(dr1("kainputtgl"), "yyyy-MM-dd H:mm:ss")) & sptField & dr1("kamodifikasiuser") & sptField & FixQuotes(AsFormatTanggal(dr1("kamodifikasitgl"), "yyyy-MM-dd H:mm:ss"))
                    Next

                    'PROSES Price
                    dtPriceCurr = AsDataTableFilterSortDt(dtPrice, "khidkontak = " & idUtama)
                    For Each dr1 As DataRow In dtPriceCurr.Rows
                        strPrice = IIf(Len(strPrice) > 0, strPrice & sptRow, strPrice)

                        'MAPPING Price
                        '               khidkontak,                             khidbarang,                                 khsatuan,                               khkomisi,                               khhargabeli,                                khhargajual,                                            khberlakudari,                                                  khberlakusampai,                                khcatatan,                              khinputuser,                                                khinputtgl,                                                         khmodifikasiuser,                                               khmodifikasitgl,                                                    khcustomtext1,                                  khcustomtext2,                              khcustomtext3,                              khcustomtext4,                              khcustomtext5,                      khcustomint1,                   khcustomint2,                       khcustomint3,                   khcustomint4,                   khcustomint5,                               khcustomdbl1,                               khcustomdbl2,                               khcustomdbl3,                               khcustomdbl4,                               khcustomdbl5,                                               khcustomdate1,                                              khcustomdate2,                                              khcustomdate3,                                                  khcustomdate4,                                                  khcustomdate5
                        strPrice &= dr1("khidkontak") & sptField & FixQuotes(dr1("khidbarang")) & sptField & FixQuotes(dr1("khsatuan")) & sptField & FixDouble(dr1("khkomisi")) & sptField & FixDouble(dr1("khhargabeli")) & sptField & FixDouble(dr1("khhargajual")) & sptField & FixQuotes(AsFormatTanggal(dr1("khberlakudari"))) & sptField & FixQuotes(AsFormatTanggal(dr1("khberlakusampai"))) & sptField & FixQuotes(dr1("khcatatan")) & sptField & FixQuotes(dr1("khinputuser")) & sptField & FixQuotes(AsFormatTanggal(dr1("khinputtgl"), "yyyy-MM-dd HH:mm:ss")) & sptField & FixQuotes(dr1("khmodifikasiuser")) & sptField & FixQuotes(AsFormatTanggal(dr1("khmodifikasitgl"), "yyyy-MM-dd HH:mm:ss")) & sptField & FixQuotes(dr1("khcustomtext1")) & sptField & FixQuotes(dr1("khcustomtext2")) & sptField & FixQuotes(dr1("khcustomtext3")) & sptField & FixQuotes(dr1("khcustomtext4")) & sptField & FixQuotes(dr1("khcustomtext5")) & sptField & dr1("khcustomint1") & sptField & dr1("khcustomint2") & sptField & dr1("khcustomint3") & sptField & dr1("khcustomint4") & sptField & dr1("khcustomint5") & sptField & FixDouble(dr1("khcustomdbl1")) & sptField & FixDouble(dr1("khcustomdbl2")) & sptField & FixDouble(dr1("khcustomdbl3")) & sptField & FixDouble(dr1("khcustomdbl4")) & sptField & FixDouble(dr1("khcustomdbl5")) & sptField & FixQuotes(AsFormatTanggal(dr1("khcustomdate1"))) & sptField & FixQuotes(AsFormatTanggal(dr1("khcustomdate2"))) & sptField & FixQuotes(AsFormatTanggal(dr1("khcustomdate3"))) & sptField & FixQuotes(AsFormatTanggal(dr1("khcustomdate4"))) & sptField & FixQuotes(AsFormatTanggal(dr1("khcustomdate5")))
                    Next

                    'PROSES Commission
                    dtCommissionCurr = AsDataTableFilterSortDt(dtCommission, "scidkontak = " & idUtama)
                    For Each dr1 As DataRow In dtCommissionCurr.Rows
                        strCommission = IIf(Len(strCommission) > 0, strCommission & sptRow, strCommission)

                        'MAPPING Commission
                        '                   scidkontak,                             sckomisi1,                                  sckomisi2,                              sckomisi3,                              sckomisi4,                                  sckomisi5,                              sckomisi6,                              sckomisi7,                              sckomisi8,                              sckomisi9,                                  sckomisi10,                                 sccustomtext1,                              sccustomtext2,                              sccustomtext3,                              sccustomtext4,                                  sccustomtext5,                              sccustomtext6,                              sccustomtext7,                              sccustomtext8,                                  sccustomtext9,                              sccustomtext10,                             sccustomint1,                               sccustomint2,                               sccustomint3,                               sccustomint4,                               sccustomint5,                               sccustomint6,                               sccustomint7,                               sccustomint8,                               sccustomint9,                               sccustomint10,                              sccustomdbl1,                               sccustomdbl2,                               sccustomdbl3,                               sccustomdbl4,                               sccustomdbl5,                               sccustomdbl6,                               sccustomdbl7,                               sccustomdbl8,                               sccustomdbl9,                               sccustomdbl10,                                                  sccustomdate1,                                              sccustomdate2,                                                  sccustomdate3,                                              sccustomdate4,                                                  sccustomdate5,                                              sccustomdate6,                                                  sccustomdate7,                                              sccustomdate8,                                                  sccustomdate9,                                              sccustomdate10
                        strCommission &= dr1("scidkontak") & sptField & FixDouble(dr1("sckomisi1")) & sptField & FixDouble(dr1("sckomisi2")) & sptField & FixDouble(dr1("sckomisi3")) & sptField & FixDouble(dr1("sckomisi4")) & sptField & FixDouble(dr1("sckomisi5")) & sptField & FixDouble(dr1("sckomisi6")) & sptField & FixDouble(dr1("sckomisi7")) & sptField & FixDouble(dr1("sckomisi8")) & sptField & FixDouble(dr1("sckomisi9")) & sptField & FixDouble(dr1("sckomisi10")) & sptField & FixQuotes(dr1("sccustomtext1")) & sptField & FixQuotes(dr1("sccustomtext2")) & sptField & FixQuotes(dr1("sccustomtext3")) & sptField & FixQuotes(dr1("sccustomtext4")) & sptField & FixQuotes(dr1("sccustomtext5")) & sptField & FixQuotes(dr1("sccustomtext6")) & sptField & FixQuotes(dr1("sccustomtext7")) & sptField & FixQuotes(dr1("sccustomtext8")) & sptField & FixQuotes(dr1("sccustomtext9")) & sptField & FixQuotes(dr1("sccustomtext10")) & sptField & FixQuotes(dr1("sccustomint1")) & sptField & FixQuotes(dr1("sccustomint2")) & sptField & FixQuotes(dr1("sccustomint3")) & sptField & FixQuotes(dr1("sccustomint4")) & sptField & FixQuotes(dr1("sccustomint5")) & sptField & FixQuotes(dr1("sccustomint6")) & sptField & FixQuotes(dr1("sccustomint7")) & sptField & FixQuotes(dr1("sccustomint8")) & sptField & FixQuotes(dr1("sccustomint9")) & sptField & FixQuotes(dr1("sccustomint10")) & sptField & FixDouble(dr1("sccustomdbl1")) & sptField & FixDouble(dr1("sccustomdbl2")) & sptField & FixDouble(dr1("sccustomdbl3")) & sptField & FixDouble(dr1("sccustomdbl4")) & sptField & FixDouble(dr1("sccustomdbl5")) & sptField & FixDouble(dr1("sccustomdbl6")) & sptField & FixDouble(dr1("sccustomdbl7")) & sptField & FixDouble(dr1("sccustomdbl8")) & sptField & FixDouble(dr1("sccustomdbl9")) & sptField & FixDouble(dr1("sccustomdbl10")) & sptField & FixQuotes(AsFormatTanggal(dr1("sccustomdate1"))) & sptField & FixQuotes(AsFormatTanggal(dr1("sccustomdate2"))) & sptField & FixQuotes(AsFormatTanggal(dr1("sccustomdate3"))) & sptField & FixQuotes(AsFormatTanggal(dr1("sccustomdate4"))) & sptField & FixQuotes(AsFormatTanggal(dr1("sccustomdate5"))) & sptField & FixQuotes(AsFormatTanggal(dr1("sccustomdate6"))) & sptField & FixQuotes(AsFormatTanggal(dr1("sccustomdate7"))) & sptField & FixQuotes(AsFormatTanggal(dr1("sccustomdate8"))) & sptField & FixQuotes(AsFormatTanggal(dr1("sccustomdate9"))) & sptField & FixQuotes(AsFormatTanggal(dr1("sccustomdate10")))
                    Next


                    'PARAMETER WS
                    '        WebsiteAccessKey       ★       M1_ContactSimpan     ★        0      △         0     △                   △                   △        dd/MM/yyyy         △        dd/MM/yyyy H:mm:ss         ★          userId             ★       1         ★     utama           △       Attention          △        Price          △           Commission
                    strWs = "WebsiteAccessKey" & sptParam & "M1_ContactSimpan" & sptParam & 0 & sptSubParam & 0 & sptSubParam & noRef & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "1" & sptParam & strUtama & sptSubParam & strAttention & sptSubParam & strPrice & sptSubParam & strCommission


                    'UPLOAD TRANSAKSI (PANGGIL WS SIMPAN SERVER PUSAT)
                    wsResult = WS_Request(strUrlWs, strWs)
                    rsWs = wsResult.Split(sptSubParam) 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
                    If rsWs.Length > 1 Then
                        rsTarget = rsWs(0)
                        rsSuccess = rsWs(1)
                        rsMessage = rsWs(2)
                        rsStep = rsWs(3)
                        rsId = rsWs(4)
                    Else
                        rsTarget = "Invalid web services result."
                        rsSuccess = 0
                        rsMessage = wsResult
                        rsStep = 0
                        rsId = 0
                    End If

                    'UPDATE STATUS TRANSAKSI
                    If rsSuccess = 1 Then
                        'JIKA UPLOAD BERHASIL MAKA UPDATE STATUS DATA MENJADI SUDAH UPLOAD

                        'TRANSAKSI KE DATABASE
                        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
                        Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

                        Dim myConn As MySql.Data.MySqlClient.MySqlConnection
                        myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
                        myConn.Open()

                        'Start Transaction
                        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

                        Try

                            'UPDATE STATUS TERUPLOAD
                            sql = "UPDATE m1_contact c SET c.ksinkron = 1 WHERE c.kid = '" & FixDouble(idUtama) & "'"
                            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                            With objCmd
                                .Connection = myConn
                                .Transaction = Trans
                                .CommandType = CommandType.Text
                                .CommandText = sql
                            End With
                            objCmd.ExecuteNonQuery()

                            'Commit Transaction
                            Trans.Commit()

                        Catch ex As Exception

                            'RollBack Transaction
                            Trans.Rollback()
                            rsMessage &= " | " & ex.Message

                        End Try

                    End If

                    'UPDATE DATATABLE
                    AsDataTableTambahData(dtTransaksi, "Trans~IdTrans~NoTrans~Uploaded~Desc", "Contact" & "~" & idUtama & "~" & drUtama("kkode") & "~" & rsSuccess & "~" & rsMessage)
                    'SetDGTransaksi(dtTransaksi)

                    SetProgress(drUtama("kkode") & " : " & IIf(rsSuccess = 1, "Uploaded", "Failed") & " - " & rsMessage)

                    'JEDA 1 DETIK
                    System.Threading.Thread.Sleep(500)

                Next

            End If

            'AMBIL JUMLAH BERHASIL UPLOAD DAN GAGAL UPLOAD
            strResultUpload &= vbCrLf & "Contact : " & AsDataTableDCount(dtTransaksi, "Trans = 'Contact' AND Uploaded = 1") & " Uploaded, "
            strResultUpload &= AsDataTableDCount(dtTransaksi, "Trans = 'Contact' AND Uploaded = 0") & " Failed"

        Catch ex As Exception

            errMessage = "#End, Failed " & ex.Message
            GoTo selesai

        End Try

        errMessage = "Success"

selesai:
        SetProgress("Finish Upload Contact - " & errMessage)

    End Sub

    Public Sub MyUploadM5_SiMain()

        'AMBIL DATA TRANSAKSI YANG BELUM TERUPLOAD
        'PANGGIL FUNGSI PROSES (BUAT PARAMETER DAN KIRIM WS)

        SetProgress("Start uploading Sales Invoice (SI)...")

        'PROSESNYA
        '1. HAPUS DATA TEMPORARY STOK KURANG --> SESUAI DATA GUDANG PADA TRANSAKSI
        '2. AMBIL DATA TRANSAKSI YANG BELUM TERUPLOAD
        '3. SET NOREF = NOTRANSAKSI LOKAL, DIKIRIM DI PARAM FILTER JUGA
        '4. UPLOAD TRANSAKSI
        '5. UPDATE STATUS TRANSAKSI SUDAH TERUPLOAD

        Try

            'CLEAR VARIABLE
            ClearVariable()

            'AMBIL DATA BELUM TERUPLOAD
            'AMBIL DATA UTAMA
            SetProgress("Selecting main transaction")

            'UTAMA
            Dim utama As String = "", detail As String = "", pay As String = ""
            sql = "SELECT si.siid, si.sicabang, si.silokasi, si.sigudang, si.siasalbarang, si.siasalbarangkategori, si.sijenispenjualan, si.sijenispenjualankategori, si.sisaldoawal, si.sicarabayar, si.sisumber, si.siautonotransaksi, si.sinotransaksi, si.sitgl, si.sikodepa, si.sicustomer, si.sicustomerkontak, si.si1alamat1, si.si1alamat2, si.si1alamat3, si.si2alamat1, si.si2alamat2, si.si2alamat3, si.sibagianpenjualan, si.siekspedisi, si.sitglkirim, si.sitermin, si.sitgljatuhtempo, si.siuraian, si.sicatatan, CONCAT('(',si.sinotransaksi,')')sinoref, si.sitglnoref, si.sitglpenutupan, si.simatauang, si.sikurs, si.sihargatermasukpajak, si.sitotal, si.sidiskonpersen, si.sijmldiskon, si.sitotalpajak1detail, si.sitotalpajak2detail, si.sibiayalainpersen, si.sibiayalain, si.sitotaltransaksi, si.sijmluangmuka, si.sijmlbayar, si.sibayartunai, si.sibayarkkredit, si.sibayarkdebit, si.sibayarvoucher, si.sibayarpoin, si.sibayarjmlpoin, si.sichargepersen, si.sicharge, si.sijmlkembali, si.sipoinsebelumnya, si.sipoindidapat, si.sistatuslunas, si.sitgllunas, si.sinofakturpajak, si.sisdhbayarpajak, si.sitglbayarpajak, si.sirekdiskon, si.sirekpajak1, si.sirekpajak2, si.sirekbiayalain, si.sirekuangmuka, si.sirekbayar, si.sirekcharge, si.sirekkembali, si.siidsq, si.siidso, si.siidas, si.siidpi, si.siidpl, si.siiddo, si.siiddr, si.sistatusrnr, si.sistatussr, si.sistatusrealisasi, 0 as sistatussie, '1900-01-01' as sitglsie, si.sistatus, si.sistatussebelumnya, si.sijmlrevisi, si.sicetakanke, si.siinputuser, si.siinputtgl, si.simodifikasiuser, si.simodifikasitgl, si.siposting, si.sipostingtgl, si.situtupperiode, si.siisclose, si.siuploaded, si.sicustomarea, si.sicustomtext1, si.sicustomtext2, si.sicustomtext3, si.sicustomtext4, si.sicustomtext5, si.sicustomtext6, si.sicustomtext7, si.sicustomtext8, si.sicustomtext9, si.sicustomtext10, si.sicustomint1, si.sicustomint2, si.sicustomint3, si.sicustomint4, si.sicustomint5, si.sicustomint6, si.sicustomint7, si.sicustomint8, si.sicustomint9, si.sicustomint10, si.sicustomdbl1, si.sicustomdbl2, si.sicustomdbl3, si.sicustomdbl4, si.sicustomdbl5, si.sicustomdbl6, si.sicustomdbl7, si.sicustomdbl8, si.sicustomdbl9, si.sicustomdbl10, si.sicustomdate1, si.sicustomdate2, si.sicustomdate3, si.sicustomdate4, si.sicustomdate5, si.sicustomdate6, si.sicustomdate7, si.sicustomdate8, si.sicustomdate9, si.sicustomdate10 FROM m5_si si where si.sistatus in (2,3,4,7) AND si.siuploaded = 0"
            dtUtama = AsDataTableAmbilDariDB(sql, strCon) 'Ambil data ke databases
            'result(2) = dt.Rows.Count & "" : GoTo selesai
            If dtUtama.Rows.Count > 0 Then
                For Each dr As DataRow In dtUtama.Rows

                    utama = String.Concat(utama,
                                          "INSERT INTO `m0_si` VALUES(" &
                             FxDB(dr("siid"), ""), ",",
                            "'" & FxDB(dr("sicabang"), "") & "'", ",",
                            "'" & FxDB(dr("silokasi"), "") & "'", ",",
                            "'" & FxDB(dr("sigudang"), "") & "'", ",",
                            "'" & FxDB(dr("siasalbarang") & "'", ""), ",",
                            "'" & FxDB(dr("siasalbarangkategori") & "'", ""), ",",
                            "'" & FxDB(dr("sijenispenjualan"), "") & "'", ",",
                            "'" & FxDB(dr("sijenispenjualankategori") & "'", ""), ",",
                            "'" & FxDB(dr("sisaldoawal"), "") & "'", ",",
                            "'" & FxDB(dr("sicarabayar"), "") & "'", ",",
                            "'" & FxDB(dr("sisumber"), "") & "'", ",",
                            "'" & FxDB(dr("siautonotransaksi") & "'", ""), ",",
                            "'" & FxDB(dr("sinotransaksi"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitgl"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB(dr("sikodepa"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomer"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomerkontak"), "") & "'", ",",
                            "'" & FxDB(dr("si1alamat1"), "") & "'", ",",
                            "'" & FxDB(dr("si1alamat2"), "") & "'", ",",
                            "'" & FxDB(dr("si1alamat3"), "") & "'", ",",
                            "'" & FxDB(dr("si2alamat1"), "") & "'", ",",
                            "'" & FxDB(dr("si2alamat2"), "") & "'", ",",
                            "'" & FxDB(dr("si2alamat3"), "") & "'", ",",
                            "'" & FxDB(dr("sibagianpenjualan"), "") & "'", ",",
                            "'" & FxDB(dr("siekspedisi"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitglkirim"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB(dr("sitermin"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitgljatuhtempo"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB(dr("siuraian"), "") & "'", ",",
                            "'" & FxDB(dr("sicatatan"), "") & "'", ",",
                            "'" & FxDB(dr("sinoref"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitglnoref"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitglpenutupan"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB(dr("simatauang"), "") & "'", ",",
                            "'" & FxDB(dr("sikurs"), "") & "'", ",",
                            "'" & FxDB(dr("sihargatermasukpajak"), "") & "'", ",",
                            "'" & FxDB(dr("sitotal"), "") & "'", ",",
                            "'" & FxDB(dr("sidiskonpersen"), "") & "'", ",",
                            "'" & FxDB(dr("sijmldiskon"), "") & "'", ",",
                            "'" & FxDB(dr("sitotalpajak1detail"), "") & "'", ",",
                            "'" & FxDB(dr("sitotalpajak2detail"), "") & "'", ",",
                            "'" & FxDB(dr("sibiayalainpersen"), "") & "'", ",",
                            "'" & FxDB(dr("sibiayalain"), "") & "'", ",",
                            "'" & FxDB(dr("sitotaltransaksi"), "") & "'", ",",
                            "'" & FxDB(dr("sijmluangmuka"), "") & "'", ",",
                            "'" & FxDB(dr("sijmlbayar"), "") & "'", ",",
                            "'" & FxDB(dr("sibayartunai"), "") & "'", ",",
                            "'" & FxDB(dr("sibayarkkredit"), "") & "'", ",",
                            "'" & FxDB(dr("sibayarkdebit"), "") & "'", ",",
                            "'" & FxDB(dr("sibayarvoucher"), "") & "'", ",",
                            "'" & FxDB(dr("sibayarpoin"), "") & "'", ",",
                            "'" & FxDB(dr("sibayarjmlpoin"), "") & "'", ",",
                            "'" & FxDB(dr("sichargepersen"), "") & "'", ",",
                            "'" & FxDB(dr("sicharge"), "") & "'", ",",
                            "'" & FxDB(dr("sijmlkembali"), "") & "'", ",",
                            "'" & FxDB(dr("sipoinsebelumnya"), "") & "'", ",",
                            "'" & FxDB(dr("sipoindidapat"), "") & "'", ",",
                            "'" & FxDB(dr("sistatuslunas"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitgllunas"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB(dr("sinofakturpajak"), "") & "'", ",",
                            "'" & FxDB(dr("sisdhbayarpajak"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitglbayarpajak"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB(dr("sirekdiskon"), "") & "'", ",",
                            "'" & FxDB(dr("sirekpajak1"), "") & "'", ",",
                            "'" & FxDB(dr("sirekpajak2"), "") & "'", ",",
                            "'" & FxDB(dr("sirekbiayalain"), "") & "'", ",",
                            "'" & FxDB(dr("sirekuangmuka"), "") & "'", ",",
                            "'" & FxDB(dr("sirekbayar"), "") & "'", ",",
                            "'" & FxDB(dr("sirekcharge"), "") & "'", ",",
                            "'" & FxDB(dr("sirekkembali"), "") & "'", ",",
                            "'" & FxDB(dr("siidsq"), "") & "'", ",",
                            "'" & FxDB(dr("siidso"), "") & "'", ",",
                            "'" & FxDB(dr("siidas"), "") & "'", ",",
                            "'" & FxDB(dr("siidpi"), "") & "'", ",",
                            "'" & FxDB(dr("siidpl"), "") & "'", ",",
                            "'" & FxDB(dr("siiddo"), "") & "'", ",",
                            "'" & FxDB(dr("siiddr"), "") & "'", ",",
                            "'" & FxDB(dr("sistatusrnr"), "") & "'", ",",
                            "'" & FxDB(dr("sistatussr"), "") & "'", ",",
                            "'" & FxDB(dr("sistatusrealisasi"), "") & "'", ",",
                            "'" & FxDB(dr("sistatussie"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sitglsie"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & FxDB(dr("sistatus"), "") & "'", ",",
                            "'" & FxDB(dr("sistatussebelumnya"), "") & "'", ",",
                            "'" & FxDB(dr("sijmlrevisi"), "") & "'", ",",
                            "'" & FxDB(dr("sicetakanke"), "") & "'", ",",
                            "'" & FxDB(dr("siinputuser"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("siinputtgl"), ""), "yyyy-MM-dd hh:mm:ss") & "'", ",",
                            "'" & FxDB(dr("simodifikasiuser"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("simodifikasitgl"), ""), "yyyy-MM-dd hh:mm:ss") & "'", ",",
                            "'" & FxDB(dr("siposting"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sipostingtgl"), ""), "yyyy-MM-dd hh:mm:ss") & "'", ",",
                            "'" & FxDB(dr("situtupperiode"), "") & "'", ",",
                            "'" & FxDB(dr("siisclose"), "") & "'", ",",
                            "'" & FxDB(dr("siuploaded"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomarea"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext1"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext2"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext3"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext4"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext5"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext6"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext7"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext8"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext9"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomtext10"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint1"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint2"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint3"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint4"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint5"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint6"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint7"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint8"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint9"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomint10"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomdbl1"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomdbl2"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomdbl3"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomdbl4"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomdbl5"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomdbl6"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomdbl7"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomdbl8"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomdbl9"), "") & "'", ",",
                            "'" & FxDB(dr("sicustomdbl10"), "") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate1"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate2"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate3"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate4"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate5"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate6"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate7"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate8"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate9"), ""), "yyyy-MM-dd") & "'", ",",
                            "'" & AsFormatTanggal(FxDB(dr("sicustomdate10"), ""), "yyyy-MM-dd") & "'", ");" & sptRow)

                Next
                utama = utama.Substring(0, utama.Length - sptRow.Length)
                utama = utama.Replace(sptRow, vbCrLf)

                'Detail
                SetProgress("Selecting detail transaction")
                sql = "SELECT sid.idsidetail, sid.idsi, sid.idbarang, sid.namabarang, sid.tipebarang, sid.jml, sid.satuan, sid.nilaisatuan, sid.jmlbarang, sid.satuanbarang, sid.matauang, sid.kurs, sid.idhppkhususmasuk, sid.idhppfifomasuk, sid.harga, sid.hargapricelist, sid.hpp, sid.diskon, sid.jmldiskon, sid.pajak1, sid.jmlpajak1, sid.pajak2, sid.jmlpajak2, sid.cabang, sid.lokasi, sid.gudangasal, sid.gudangtransit, sid.gudangtujuan, sid.rekpersediaan, sid.rekhargapokok, sid.rekdiskonpenjualan, sid.rekpenjualan, sid.costcenter, sid.divisi, sid.subdivisi, sid.proyek, sid.catatan, sid.urutan, sid.idsqdetail, sid.idsodetail, sid.idpidetail, sid.idpldetail, sid.iddodetail, sid.iddrdetail, sid.jmlrnr, sid.statusrnr, sid.jmlsr, sid.statussr, sid.jmlrealisasi, sid.statusrealisasi, sid.isbonus, sid.isbonusfrom, sid.isclose, sid.customtext1, sid.customtext2, CONCAT(si.sinotransaksi,'-T') as customtext3, sid.customdbl1, sid.customdbl2, sid.customdbl3, sid.customdate1, sid.customdate2, sid.customdate3 FROM m5_si_detail sid join m5_si si on sid.idsi = si.siid where si.sistatus in (2,3,4,7) AND si.siuploaded = 0"
                dtDetail = AsDataTableAmbilDariDB(sql, strCon) ' Ambil data ke databases
                If dtDetail.Rows.Count > 0 Then
                    For Each dr As DataRow In dtDetail.Rows

                        detail = String.Concat(detail,
                                               "INSERT INTO `m0_si_detail` VALUES(" &
                                "'" & FxDB(dr("idsidetail"), "") & "'", ",",
                                "'" & FxDB(dr("idsi"), "") & "'", ",",
                                "'" & FxDB(dr("idbarang"), "") & "'", ",",
                                "'" & FxDB(dr("namabarang"), "") & "'", ",",
                                "'" & FxDB(dr("tipebarang"), "") & "'", ",",
                                "'" & FxDB(dr("jml"), "") & "'", ",",
                                "'" & FxDB(dr("satuan"), "") & "'", ",",
                                "'" & FxDB(dr("nilaisatuan"), "") & "'", ",",
                                "'" & FxDB(dr("jmlbarang"), "") & "'", ",",
                                "'" & FxDB(dr("satuanbarang"), "") & "'", ",",
                                "'" & FxDB(dr("matauang"), "") & "'", ",",
                                "'" & FxDB(dr("kurs"), "") & "'", ",",
                                "'" & FxDB(dr("idhppkhususmasuk"), "") & "'", ",",
                                "'" & FxDB(dr("idhppfifomasuk"), "") & "'", ",",
                                "'" & FxDB(dr("harga"), "") & "'", ",",
                                "'" & FxDB(dr("hargapricelist"), "") & "'", ",",
                                "'" & FxDB(dr("hpp"), "") & "'", ",",
                                "'" & FxDB(dr("diskon"), "") & "'", ",",
                                "'" & FxDB(dr("jmldiskon"), "") & "'", ",",
                                "'" & FxDB(dr("pajak1"), "") & "'", ",",
                                "'" & FxDB(dr("jmlpajak1"), "") & "'", ",",
                                "'" & FxDB(dr("pajak2"), "") & "'", ",",
                                "'" & FxDB(dr("jmlpajak2"), "") & "'", ",",
                                "'" & FxDB(dr("cabang"), "") & "'", ",",
                                "'" & FxDB(dr("lokasi"), "") & "'", ",",
                                "'" & FxDB(dr("gudangasal"), "") & "'", ",",
                                "'" & FxDB(dr("gudangtransit"), "") & "'", ",",
                                "'" & FxDB(dr("gudangtujuan"), "") & "'", ",",
                                "'" & FxDB(dr("rekpersediaan"), "") & "'", ",",
                                "'" & FxDB(dr("rekhargapokok"), "") & "'", ",",
                                "'" & FxDB(dr("rekdiskonpenjualan"), "") & "'", ",",
                                "'" & FxDB(dr("rekpenjualan"), "") & "'", ",",
                                "'" & FxDB(dr("costcenter"), "") & "'", ",",
                                "'" & FxDB(dr("divisi"), "") & "'", ",",
                                "'" & FxDB(dr("subdivisi"), "") & "'", ",",
                                "'" & FxDB(dr("proyek"), "") & "'", ",",
                                "'" & FxDB(dr("catatan"), "") & "'", ",",
                                "'" & FxDB(dr("urutan"), "") & "'", ",",
                                "'" & FxDB(dr("idsqdetail"), "") & "'", ",",
                                "'" & FxDB(dr("idsodetail"), "") & "'", ",",
                                "'" & FxDB(dr("idpidetail"), "") & "'", ",",
                                "'" & FxDB(dr("idpldetail"), "") & "'", ",",
                                "'" & FxDB(dr("iddodetail"), "") & "'", ",",
                                "'" & FxDB(dr("iddrdetail"), "") & "'", ",",
                                "'" & FxDB(dr("jmlrnr"), "") & "'", ",",
                                "'" & FxDB(dr("statusrnr"), "") & "'", ",",
                                "'" & FxDB(dr("jmlsr"), "") & "'", ",",
                                "'" & FxDB(dr("statussr"), "") & "'", ",",
                                "'" & FxDB(dr("jmlrealisasi"), "") & "'", ",",
                                "'" & FxDB(dr("statusrealisasi"), "") & "'", ",",
                                "'" & FxDB(dr("isbonus"), "") & "'", ",",
                                "'" & FxDB(dr("isbonusfrom"), "") & "'", ",",
                                "'" & FxDB(dr("isclose"), "") & "'", ",",
                                "'" & FxDB(dr("customtext1"), "") & "'", ",",
                                "'" & FxDB(dr("customtext2"), "") & "'", ",",
                                "'" & FxDB(dr("customtext3"), "") & "'", ",",
                                "'" & FxDB(dr("customdbl1"), "") & "'", ",",
                                "'" & FxDB(dr("customdbl2"), "") & "'", ",",
                                "'" & FxDB(dr("customdbl3"), "") & "'", ",",
                                "'" & AsFormatTanggal(FxDB(dr("customdate1"), ""), "yyyy-MM-dd") & "'", ",",
                                "'" & AsFormatTanggal(FxDB(dr("customdate2"), ""), "yyyy-MM-dd") & "'", ",",
                                "'" & AsFormatTanggal(FxDB(dr("customdate3"), ""), "yyyy-MM-dd") & "'", ");" & sptRow)

                    Next

                    detail = detail.Substring(0, detail.Length - sptRow.Length)
                    detail = detail.Replace(sptRow, vbCrLf)
                    detail = detail.Replace("'", "\'")
                    detail = detail.Replace(",\'", ",'")
                    detail = detail.Replace("\',", "',")
                    detail = detail.Replace("(\'", "('")
                    detail = detail.Replace("\')", "')")
                End If

                'pay
                SetProgress("Selecting payment transaction")
                sql = "SELECT sid.idsicarabayar as idsicarabayar,sid.idsi as idsi,sid.carabayar as carabayar,sid.matauang as matauang,sid.kurs as kurs,sid.jumlah as jumlah,sid.jumlahvalas as jumlahvalas,sid.nogiro as nogiro,sid.tgljt as tgljt,sid.bank as bank,sid.noacbank as noacbank,sid.rekbank as rekbank,CONCAT(si.sinotransaksi,'-T') as rekgiro,CONCAT(si.sicabang,' - ',si.silokasi) as catatan, sid.urutan as urutan, sid.isclose as isclose FROM m5_si si join m5_si_pay sid ON si.siid = sid.idsi where si.sistatus in (2,3,4,7) AND si.siuploaded = 0"
                dtPay = AsDataTableAmbilDariDB(sql, strCon) ' Ambil data ke databases
                If dtPay.Rows.Count > 0 Then

                    For Each dr As DataRow In dtPay.Rows
                        errMessage = sql : GoTo selesai
                        pay = String.Concat(pay,
                                            "INSERT INTO `m0_si_pay` VALUES(" &
                                "|" & FxDB(dr("idsicarabayar"), "") & "|", ",",
                                "|" & FxDB(dr("idsi"), "") & "|", ",",
                                "|" & FxDB(dr("carabayar"), "") & "|", ",",
                                "|" & FxDB(dr("matauang"), "") & "|", ",",
                                "|" & FxDB(dr("kurs"), "") & "|", ",",
                                "|" & FxDB(dr("jumlah"), "") & "|", ",",
                                "|" & FxDB(dr("jumlahvalas"), "") & "|", ",",
                                "|" & FxDB(dr("nogiro"), "") & "|", ",",
                                "|" & AsFormatTanggal(FxDB(dr("tgljt"), ""), "yyyy-MM-dd") & "|", ",",
                                "|" & FxDB(dr("bank"), "") & "|", ",",
                                "|" & FxDB(dr("noacbank"), "") & "|", ",",
                                "|" & FxDB(dr("rekbank"), "") & "|", ",",
                                "|" & FxDB(dr("rekgiro"), "") & "|", ",",
                                "|" & FxDB(dr("catatan"), "") & "|", ",",
                                "|" & FxDB(dr("urutan"), "") & "|", ",",
                                "|" & FxDB(dr("isclose"), "") & "|", ");" & sptRow)
                    Next
                    pay = pay.Substring(0, pay.Length - sptRow.Length)
                    pay = pay.Replace(sptRow, vbCrLf)
                    pay = pay.Replace("|", """")
                End If


                Dim contents As String = "", myPath As String = ""
                Dim fileName As String = "", folderGlobal As String = ""
                Dim tglupload As DateTime = Now()
                'CREATE SQL FILE =====================================
                SetProgress("Create SQL File")
                myPath = HttpContext.Current.Server.MapPath("~/") & "UploadPenjualan/"
                'myPath = "E:UploadPenjualan/"
                Dim SqlName As String = txtlokasi.Text & "_" & tglupload.Date & "(" & tglupload.Hour & "-" & tglupload.Minute & "-" & tglupload.Second & ").sql"
                'errMessage = SqlName : GoTo selesai
                'CEK FILE EXISTS
                Try
                    'File.Delete(myPath & SqlName)
                    File.WriteAllText(myPath & SqlName, utama & vbCrLf & detail & vbCrLf & pay)

                Catch ex As Exception
                    errMessage = ex.Message
                    contents = "" : GoTo selesai
                End Try
                'END OF CREATE SQL FILE ==============================

            End If


        Catch ex As Exception

            errMessage = "Failed : " & ex.Message
            GoTo selesai

        End Try

        errMessage = "Success"

selesai:
        SetProgress("Get data Sales Invoice (SI) - " & errMessage)

    End Sub

    Public Sub MyUploadM5_SiProcess()

        'BUAT PARAMETER DAN KIRIM WS KE PUSAT
        'SETELAH PANGGIL WS, RESULT WS AKAN DIPROSES PADA FUNGSI f_confirmWS
        'UPLOAD TRANSAKSI

        'PROSESNYA
        '1. HAPUS DATA TEMPORARY STOK KURANG --> SESUAI DATA GUDANG PADA TRANSAKSI
        '2. AMBIL DATA TRANSAKSI YANG BELUM TERUPLOAD
        '3. SET NOREF = NOTRANSAKSI LOKAL, DIKIRIM DI PARAM FILTER JUGA
        '4. UPLOAD TRANSAKSI
        '5. UPDATE STATUS TRANSAKSI SUDAH TERUPLOAD

        Try

            If noPerulangan < dtUtama.Rows.Count Then

                'SET DATA UTAMA
                drUtama = dtUtama.Rows(noPerulangan)

                'RESET VARIABEL
                strUtama = "" : strDetail = "" : strBatch = "" : strSerial = "" : strPay = ""

                'SET VARIABEL
                sumber = FixQuotes(drUtama("sisumber"))
                idUtama = FixDouble(drUtama("siid"))
                noTransaksi = FixQuotes(drUtama("sinotransaksi"))
                noRef = FixQuotes(drUtama("sinoref")) & "(" & noTransaksi & ")"
                userId = FixDouble(drUtama("siinputuser"))

                'MAPPING UTAMA
                '            siid,                                 sicabang,                                   silokasi,                                   sigudang,                                   siasalbarang,                         siasalbarangkategori,                                  sijenispenjualan,                         sijenispenjualankategori,                        sicarabayar,                                  sisumber,                         siautonotransaksi,                       sinotransaksi,                                                   sitgl,                          sikodepa,                        sicustomer,                                  sicustomerkontak,                                     si1alamat1,                                 si1alamat2,                                     si1alamat3,                                 si2alamat1,                                     si2alamat2,                                 si2alamat3,                         sibagianpenjualan,                                  siekspedisi,                                                    sitglkirim,                                     sitermin,                                                   sitgljatuhtempo,                                siuraian,                                   sicatatan,                        sinoref,                                                sitglnoref,                                                     sitglpenutupan,                                     simatauang,                                     sikurs,                         sihargatermasukpajak,                               sitotal,                                    sidiskonpersen,                                     sijmldiskon,                                sitotalpajak1detail,                                    sitotalpajak2detail,                                    sibiayalainpersen,                                  sibiayalain,                                    sitotaltransaksi,                                   sijmlbayar,                         sistatuslunas,                                                  sitgllunas,                                     sinofakturpajak,                        sisdhbayarpajak,                                                sitglbayarpajak,                                    sirekdiskon,                                    sirekpajak1,                                    sirekpajak2,                                    sirekbiayalain,                                 sirekbayar,                         siidsq,                         siidso,                         siidpl,                     siiddo,                         siiddr,                         siidpi,                         sistatusrnr,                    sistatussr,                         sistatus,                       sistatussebelumnya,                         sijmlrevisi,                        sicetakanke,                        siinputuser,                siinputtgl,                                 simodifikasiuser,               simodifikasitgl,            siposting,                      situtupperiode,                         siisclose,                                  sicustomtext1,                                  sicustomtext2,                                  sicustomtext3,                                      sicustomtext4,                                  sicustomtext5,                          sicustomint1,                       sicustomint2,                       sicustomint3,                                   sicustomdbl1,                                   sicustomdbl2,                                   sicustomdbl3,                                                   sicustomdate1,                                                      sicustomdate2,                                                  sicustomdate3,                                      sijmluangmuka,                                  sirekuangmuka,                                  siidas,                                     sibayartunai,                               sibayarkkredit,                                     sibayarkdebit,                                      sibayarvoucher,                                 sibayarpoin,                                sibayarjmlpoin,                                     sichargepersen,                                 sicharge,                                   sipoinsebelumnya,                                       sipoindidapat,                                  sicustomtext6,                                  sicustomtext7,                                  sicustomtext8,                                      sicustomtext9,                                  sicustomtext10,                                 sicustomint4,                                   sicustomint5,                                   sicustomint6,                                   sicustomint7,                                   sicustomint8,                                   sicustomint9,                                   sicustomint10,                                  sicustomdbl4,                                       sicustomdbl5,                                   sicustomdbl6,                               sicustomdbl7,                                       sicustomdbl8,                                   sicustomdbl9,                                   sicustomdbl10,                                                  sicustomdate4,                                                  sicustomdate5,                                                      sicustomdate6,                                                  sicustomdate7,                                                      sicustomdate8,                                                  sicustomdate9,                                                      sicustomdate10,                                 sicustomarea,                                       sirekcharge,                                sijmlkembali,                                   sirekkembali
                strUtama = idUtama & sptField & FixQuotes(drUtama("sicabang")) & sptField & FixQuotes(drUtama("silokasi")) & sptField & FixQuotes(drUtama("sigudang")) & sptField & FixQuotes(drUtama("siasalbarang")) & sptField & drUtama("siasalbarangkategori") & sptField & FixQuotes(drUtama("sijenispenjualan")) & sptField & drUtama("sijenispenjualankategori") & sptField & drUtama("sicarabayar") & sptField & FixQuotes(drUtama("sisumber")) & sptField & drUtama("siautonotransaksi") & sptField & FixQuotes(noTransaksi) & sptField & FixQuotes(AsFormatTanggal(drUtama("sitgl"))) & sptField & drUtama("sikodepa") & sptField & drUtama("sicustomer") & sptField & FixQuotes(drUtama("sicustomerkontak")) & sptField & FixQuotes(drUtama("si1alamat1")) & sptField & FixQuotes(drUtama("si1alamat2")) & sptField & FixQuotes(drUtama("si1alamat3")) & sptField & FixQuotes(drUtama("si2alamat1")) & sptField & FixQuotes(drUtama("si2alamat2")) & sptField & FixQuotes(drUtama("si2alamat3")) & sptField & drUtama("sibagianpenjualan") & sptField & FixQuotes(drUtama("siekspedisi")) & sptField & FixQuotes(AsFormatTanggal(drUtama("sitglkirim"))) & sptField & FixQuotes(drUtama("sitermin")) & sptField & FixQuotes(AsFormatTanggal(drUtama("sitgljatuhtempo"))) & sptField & FixQuotes(drUtama("siuraian")) & sptField & FixQuotes(drUtama("sicatatan")) & sptField & FixQuotes(noRef) & sptField & FixQuotes(AsFormatTanggal(drUtama("sitglnoref"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("sitglpenutupan"))) & sptField & FixQuotes(drUtama("simatauang")) & sptField & FixDouble(drUtama("sikurs")) & sptField & drUtama("sihargatermasukpajak") & sptField & FixDouble(drUtama("sitotal")) & sptField & FixQuotes(drUtama("sidiskonpersen")) & sptField & FixDouble(drUtama("sijmldiskon")) & sptField & FixDouble(drUtama("sitotalpajak1detail")) & sptField & FixDouble(drUtama("sitotalpajak2detail")) & sptField & FixDouble(drUtama("sibiayalainpersen")) & sptField & FixDouble(drUtama("sibiayalain")) & sptField & FixDouble(drUtama("sitotaltransaksi")) & sptField & FixDouble(drUtama("sijmlbayar")) & sptField & drUtama("sistatuslunas") & sptField & FixQuotes(AsFormatTanggal(drUtama("sitgllunas"))) & sptField & FixQuotes(drUtama("sinofakturpajak")) & sptField & drUtama("sisdhbayarpajak") & sptField & FixQuotes(AsFormatTanggal(drUtama("sitglbayarpajak"))) & sptField & FixQuotes(drUtama("sirekdiskon")) & sptField & FixQuotes(drUtama("sirekpajak1")) & sptField & FixQuotes(drUtama("sirekpajak2")) & sptField & FixQuotes(drUtama("sirekbiayalain")) & sptField & FixQuotes(drUtama("sirekbayar")) & sptField & drUtama("siidsq") & sptField & drUtama("siidso") & sptField & drUtama("siidpl") & sptField & drUtama("siiddo") & sptField & drUtama("siiddr") & sptField & drUtama("siidpi") & sptField & drUtama("sistatusrnr") & sptField & drUtama("sistatussr") & sptField & drUtama("sistatus") & sptField & drUtama("sistatussebelumnya") & sptField & drUtama("sijmlrevisi") & sptField & drUtama("sicetakanke") & sptField & drUtama("siinputuser") & sptField & "1971-01-01 00:00:00" & sptField & drUtama("simodifikasiuser") & sptField & "1971-01-01 00:00:00" & sptField & "0" & sptField & drUtama("situtupperiode") & sptField & drUtama("siisclose") & sptField & FixQuotes(drUtama("sicustomtext1")) & sptField & FixQuotes(drUtama("sicustomtext2")) & sptField & FixQuotes(drUtama("sicustomtext3")) & sptField & FixQuotes(drUtama("sicustomtext4")) & sptField & FixQuotes(drUtama("sicustomtext5")) & sptField & drUtama("sicustomint1") & sptField & drUtama("sicustomint2") & sptField & drUtama("sicustomint3") & sptField & FixDouble(drUtama("sicustomdbl1")) & sptField & FixDouble(drUtama("sicustomdbl2")) & sptField & FixDouble(drUtama("sicustomdbl3")) & sptField & FixQuotes(AsFormatTanggal(drUtama("sicustomdate1"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("sicustomdate2"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("sicustomdate3"))) & sptField & FixDouble(drUtama("sijmluangmuka")) & sptField & FixQuotes(drUtama("sirekuangmuka")) & sptField & FixDouble(drUtama("siidas")) & sptField & FixDouble(drUtama("sibayartunai")) & sptField & FixDouble(drUtama("sibayarkkredit")) & sptField & FixDouble(drUtama("sibayarkdebit")) & sptField & FixDouble(drUtama("sibayarvoucher")) & sptField & FixDouble(drUtama("sibayarpoin")) & sptField & FixDouble(drUtama("sibayarjmlpoin")) & sptField & FixQuotes(drUtama("sichargepersen")) & sptField & FixDouble(drUtama("sicharge")) & sptField & FixDouble(drUtama("sipoinsebelumnya")) & sptField & FixDouble(drUtama("sipoindidapat")) & sptField & FixQuotes(drUtama("sicustomtext6")) & sptField & FixQuotes(drUtama("sicustomtext7")) & sptField & FixQuotes(drUtama("sicustomtext8")) & sptField & FixQuotes(drUtama("sicustomtext9")) & sptField & FixQuotes(drUtama("sicustomtext10")) & sptField & FixDouble(drUtama("sicustomint4")) & sptField & FixDouble(drUtama("sicustomint5")) & sptField & FixDouble(drUtama("sicustomint6")) & sptField & FixDouble(drUtama("sicustomint7")) & sptField & FixDouble(drUtama("sicustomint8")) & sptField & FixDouble(drUtama("sicustomint9")) & sptField & FixDouble(drUtama("sicustomint10")) & sptField & FixDouble(drUtama("sicustomdbl4")) & sptField & FixDouble(drUtama("sicustomdbl5")) & sptField & FixDouble(drUtama("sicustomdbl6")) & sptField & FixDouble(drUtama("sicustomdbl7")) & sptField & FixDouble(drUtama("sicustomdbl8")) & sptField & FixDouble(drUtama("sicustomdbl9")) & sptField & FixDouble(drUtama("sicustomdbl10")) & sptField & FixQuotes(AsFormatTanggal(drUtama("sicustomdate4"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("sicustomdate5"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("sicustomdate6"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("sicustomdate7"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("sicustomdate8"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("sicustomdate9"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("sicustomdate10"))) & sptField & FixQuotes(drUtama("sicustomarea")) & sptField & FixQuotes(drUtama("sirekcharge")) & sptField & FixQuotes(drUtama("sijmlkembali")) & sptField & FixQuotes(drUtama("sirekkembali"))

                'PROSES DETAIL
                dtDetailCurr = AsDataTableFilterSortDt(dtDetail, "idsi = " & idUtama)
                For Each dr1 As DataRow In dtDetailCurr.Rows
                    strDetail = IIf(Len(strDetail) > 0, strDetail & sptRow, strDetail)

                    'MAPPING DETAIL
                    '   idsidetail,                     idsi,                       idbarang,                               namabarang,                             tipebarang,                             jml,                                satuan,                                 nilaisatuan,                            jmlbarang,                                  satuanbarang,                           matauang,                                   kurs,                   idhppkhususmasuk,                       idhppfifomasuk,                             harga,                              hargapricelist,                             hpp,                                diskon,                                 jmldiskon,                              pajak1,                             jmlpajak1,                                  pajak2,                             jmlpajak2,                                  cabang,                             lokasi,                             gudangasal,                                 gudangtransit,                              gudangtujuan,                               rekpersediaan,                              rekhargapokok,                                  rekdiskonpenjualan,                             rekpenjualan,                               costcenter,                             divisi,                                 subdivisi,                                  proyek,                             catatan,                    urutan,                     idsqdetail,                     idsodetail,                 idpldetail,                     iddodetail,                     iddrdetail,                     idpidetail,                             jmlrnr,                     statusrnr,                              jmlsr,                      statussr,                   isclose,                            customtext1,                                customtext2,                                customtext3,                                customdbl1,                             customdbl2,                                 customdbl3,                                             customdate1,                                                customdate2,                                                customdate3,                        isbonus,                    isbonusfrom
                    strDetail &= 0 & sptField & FixDouble(idUtama) & sptField & dr1("idbarang") & sptField & FixQuotes(dr1("namabarang")) & sptField & FixQuotes(dr1("tipebarang")) & sptField & FixDouble(dr1("jml")) & sptField & FixQuotes(dr1("satuan")) & sptField & FixDouble(dr1("nilaisatuan")) & sptField & FixDouble(dr1("jmlbarang")) & sptField & FixQuotes(dr1("satuanbarang")) & sptField & FixQuotes(dr1("matauang")) & sptField & FixDouble(dr1("kurs")) & sptField & dr1("idhppkhususmasuk") & sptField & dr1("idhppfifomasuk") & sptField & FixDouble(dr1("harga")) & sptField & FixDouble(dr1("hargapricelist")) & sptField & FixDouble(dr1("hpp")) & sptField & FixQuotes(dr1("diskon")) & sptField & FixQuotes(dr1("jmldiskon")) & sptField & FixQuotes(dr1("pajak1")) & sptField & FixDouble(dr1("jmlpajak1")) & sptField & FixQuotes(dr1("pajak2")) & sptField & FixDouble(dr1("jmlpajak2")) & sptField & FixQuotes(dr1("cabang")) & sptField & FixQuotes(dr1("lokasi")) & sptField & FixQuotes(dr1("gudangasal")) & sptField & FixQuotes(dr1("gudangtransit")) & sptField & FixQuotes(dr1("gudangtujuan")) & sptField & FixQuotes(dr1("rekpersediaan")) & sptField & FixQuotes(dr1("rekhargapokok")) & sptField & FixQuotes(dr1("rekdiskonpenjualan")) & sptField & FixQuotes(dr1("rekpenjualan")) & sptField & FixQuotes(dr1("costcenter")) & sptField & FixQuotes(dr1("divisi")) & sptField & FixQuotes(dr1("subdivisi")) & sptField & FixQuotes(dr1("proyek")) & sptField & FixQuotes(dr1("catatan")) & sptField & dr1("urutan") & sptField & dr1("idsqdetail") & sptField & dr1("idsodetail") & sptField & dr1("idpldetail") & sptField & dr1("iddodetail") & sptField & dr1("iddrdetail") & sptField & dr1("idpidetail") & sptField & FixDouble(dr1("jmlrnr")) & sptField & dr1("statusrnr") & sptField & FixDouble(dr1("jmlsr")) & sptField & dr1("statussr") & sptField & dr1("isclose") & sptField & FixQuotes(dr1("customtext1")) & sptField & FixQuotes(dr1("customtext2")) & sptField & FixQuotes(dr1("customtext3")) & sptField & FixDouble(dr1("customdbl1")) & sptField & FixDouble(dr1("customdbl2")) & sptField & FixDouble(dr1("customdbl3")) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate3"))) & sptField & dr1("isbonus") & sptField & dr1("isbonusfrom")
                Next

                'PROSES BATCH
                dtBatchCurr = AsDataTableFilterSortDt(dtBatch, "nbtidtransaksi = " & idUtama)
                For Each dr1 As DataRow In dtBatchCurr.Rows
                    strBatch = IIf(Len(strBatch) > 0, strBatch & sptRow, strBatch)

                    'MAPPING BATCH
                    '       nbtid,                  nbtjenismutasi,                    nbtidbarang,                                nbtkode,                            nbtsumber,                      nbtidtransaksi,                             nbtsatuan,                              nbtjml,                                 nbtcustomtext1,                             nbtcustomtext2,                             nbtcustomtext3,                                 nbtcustomdbl1,                              nbtcustomdbl2,                                  nbtcustomdbl3,                                              nbtcustomdate1,                                                 nbtcustomdate2,                                             nbtcustomdate3,                              nbtgudang,                      nbtidbatchin
                    strBatch &= 0 & sptField & dr1("nbtjenismutasi") & sptField & dr1("nbtidbarang") & sptField & FixQuotes(dr1("nbtkode")) & sptField & FixQuotes(dr1("nbtsumber")) & sptField & FixDouble(idUtama) & sptField & FixQuotes(dr1("nbtsatuan")) & sptField & FixDouble(dr1("nbtjml")) & sptField & FixQuotes(dr1("nbtcustomtext1")) & sptField & FixQuotes(dr1("nbtcustomtext2")) & sptField & FixQuotes(dr1("nbtcustomtext3")) & sptField & FixDouble(dr1("nbtcustomdbl1")) & sptField & FixDouble(dr1("nbtcustomdbl2")) & sptField & FixDouble(dr1("nbtcustomdbl3")) & sptField & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate1"))) & sptField & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate2"))) & sptField & FixQuotes(AsFormatTanggal(dr1("nbtcustomdate3"))) & sptField & FixQuotes(dr1("nbtgudang")) & sptField & dr1("nbtidbatchin")
                Next

                'PROSES SERIAL
                dtSerialCurr = AsDataTableFilterSortDt(dtSerial, "nstidtransaksi = " & idUtama)
                For Each dr1 As DataRow In dtSerialCurr.Rows
                    strSerial = IIf(Len(strSerial) > 0, strSerial & sptRow, strSerial)

                    'MAPPING SERIAL
                    '        nstid,                 nstjenismutasi,                     nstidbarang,                                nstkode,                                nstsumber,                  nstidtransaksi,                             nstsatuan,                                  nstjml,                             nstcustomtext1,                             nstcustomtext2,                                 nstcustomtext3,                                 nstcustomdbl1,                              nstcustomdbl2,                              nstcustomdbl3,                                              nstcustomdate1,                                                 nstcustomdate2,                                                 nstcustomdate3,                          nstgudang,                     nstidserialin
                    strSerial &= 0 & sptField & dr1("nstjenismutasi") & sptField & dr1("nstidbarang") & sptField & FixQuotes(dr1("nstkode")) & sptField & FixQuotes(dr1("nstsumber")) & sptField & FixDouble(idUtama) & sptField & FixQuotes(dr1("nstsatuan")) & sptField & FixDouble(dr1("nstjml")) & sptField & FixQuotes(dr1("nstcustomtext1")) & sptField & FixQuotes(dr1("nstcustomtext2")) & sptField & FixQuotes(dr1("nstcustomtext3")) & sptField & FixDouble(dr1("nstcustomdbl1")) & sptField & FixDouble(dr1("nstcustomdbl2")) & sptField & FixDouble(dr1("nstcustomdbl3")) & sptField & FixQuotes(AsFormatTanggal(dr1("nstcustomdate1"))) & sptField & FixQuotes(AsFormatTanggal(dr1("nstcustomdate2"))) & sptField & FixQuotes(AsFormatTanggal(dr1("nstcustomdate3"))) & sptField & FixQuotes(dr1("nstgudang")) & sptField & dr1("nstidserialin")
                Next

                'PROSES PAY
                dtPayCurr = AsDataTableFilterSortDt(dtPay, "idsi = " & idUtama)
                For Each dr1 As DataRow In dtPayCurr.Rows
                    strPay = IIf(Len(strPay) > 0, strPay & sptRow, strPay)

                    'MAPPING PAY
                    'idsicarabayar,                     idsi,                   carabayar,                              matauang,                               kurs,                               jumlah,                                 jumlahvalas,                                nogiro,                                             tgljt,                                  bank,                               noacbank,                           rekbank,                                rekgiro,                                catatan,                    urutan,                     isclose
                    strPay &= 0 & sptField & FixDouble(idUtama) & sptField & dr1("carabayar") & sptField & FixQuotes(dr1("matauang")) & sptField & FixDouble(dr1("kurs")) & sptField & FixDouble(dr1("jumlah")) & sptField & FixDouble(dr1("jumlahvalas")) & sptField & FixQuotes(dr1("nogiro")) & sptField & FixQuotes(AsFormatTanggal(dr1("tgljt"))) & sptField & FixQuotes(dr1("bank")) & sptField & FixQuotes(dr1("noacbank")) & sptField & FixQuotes(dr1("rekbank")) & sptField & FixQuotes(dr1("rekgiro")) & sptField & FixQuotes(dr1("catatan")) & sptField & dr1("urutan") & sptField & dr1("isclose")
                Next

                'PARAMETER WS
                '        WebsiteAccessKey       ★       M5_SiSimpan     ★        0      △         0     △                   △                   △        dd/MM/yyyy         △        dd/MM/yyyy H:mm:ss         ★        userId             ★       0         ★     utama           △       detail          △        batch          △           serial      △           pay
                strWs = "WebsiteAccessKey" & sptParam & "M5_SiSimpan" & sptParam & 0 & sptSubParam & 0 & sptSubParam & noRef & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "0" & sptParam & strUtama & sptSubParam & strDetail & sptSubParam & strBatch & sptSubParam & strSerial & sptSubParam & strPay


                'UPLOAD TRANSAKSI (PANGGIL WS SIMPAN SERVER PUSAT)
                F_CallWs(strUrlWs, strWs)

            Else
                'PROSES UPLOAD DATA SELESAI
                errMessage = "Success"
                GoTo selesai

            End If

        Catch ex As Exception

            errMessage = "Failed : " & ex.Message
            GoTo selesai

        End Try

        errMessage = "Success"

selesai:
        SetProgress("Upload data Sales Invoice (SI) - " & errMessage)

    End Sub

    Public Sub MyUploadM5_SiResult(ByVal data As String)

        'HASIL WS, PROSES SETELAH UPLOAD
        'UPDATE STATUS TRANSAKSI SUDAH TERUPLOAD

        'PROSESNYA
        '1. HAPUS DATA TEMPORARY STOK KURANG --> SESUAI DATA GUDANG PADA TRANSAKSI
        '2. AMBIL DATA TRANSAKSI YANG BELUM TERUPLOAD
        '3. SET NOREF = NOTRANSAKSI LOKAL, DIKIRIM DI PARAM FILTER JUGA
        '4. UPLOAD TRANSAKSI
        '5. UPDATE STATUS TRANSAKSI SUDAH TERUPLOAD

        Try

            'AMBIL RESULT WS
            wsResult = data
            rsWs = wsResult.Split(sptSubParam) 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
            If rsWs.Length > 1 Then
                rsTarget = rsWs(0)
                rsSuccess = rsWs(1)
                rsMessage = rsWs(2)
                rsStep = rsWs(3)
                rsId = rsWs(4)
            Else
                rsTarget = "Invalid web services result."
                rsSuccess = 0
                rsMessage = wsResult
                rsStep = 0
                rsId = 0
            End If


            'UPDATE STATUS TRANSAKSI
            If rsSuccess = 1 Then
                'JIKA UPLOAD BERHASIL MAKA UPDATE STATUS TRANSAKSI MENJADI SUDAH UPLOAD

                'TRANSAKSI KE DATABASE
                Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
                Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

                Dim myConn As MySql.Data.MySqlClient.MySqlConnection
                myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
                myConn.Open()

                'Start Transaction
                Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

                Try

                    'UPDATE STATUS TERUPLOAD
                    sql = "UPDATE m5_si si SET si.siuploaded = 1 WHERE si.siid = '" & FixDouble(idUtama) & "'"
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = myConn
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()

                    'Commit Transaction
                    Trans.Commit()

                Catch ex As Exception

                    'RollBack Transaction
                    Trans.Rollback()
                    rsMessage &= " | " & ex.Message

                End Try

            End If

            'UPDATE DATATABLE
            AsDataTableTambahData(dtTransaksi, "IdTrans~NoTrans~Uploaded~Desc", idUtama & "~" & noTransaksi & "~" & rsSuccess & "~" & rsMessage)
            'SetDGTransaksi(dtTransaksi)


        Catch ex As Exception

            errMessage = "Failed : " & ex.Message
            GoTo selesai

        End Try

        errMessage = "Success"

selesai:
        SetProgress("Update data Sales Invoice (SI) - " & errMessage)

        'PROSES BUAT PARAMETER DAN KIRIM WS KE PUSAT, UPLOAD TRANSAKSI DATA SELANJUTNYA
        noPerulangan += 1
        MyUploadM5_SiProcess()

    End Sub

    '=======================================================

    'EXAMPLE
    Public Sub Inti()

        SetProgress("Start uploading Stock Opname (SP)...")

        'PROSESNYA
        '1. HAPUS DATA TEMPORARY STOK KURANG --> SESUAI DATA GUDANG PADA TRANSAKSI
        '2. AMBIL DATA TRANSAKSI YANG BELUM TERUPLOAD
        '3. SET NOREF = NOTRANSAKSI LOKAL, DIKIRIM DI PARAM FILTER JUGA
        '4. UPLOAD TRANSAKSI
        '5. UPDATE STATUS TRANSAKSI SUDAH TERUPLOAD

        Dim sql As String = "", idUtama As Double = 0, noTransaksi As String = "", noRef As String = "", userId As Double = 0
        Dim dtDetailCurr As New DataTable, dtBatchCurr As New DataTable, dtSerialCurr As New DataTable, dtPayCurr As New DataTable
        Dim strWs As String = "", strUtama As String = "", strDetail As String = "", strBatch As String = "", strSerial As String = "", strPay As String = ""
        Dim errMessage As String = "", wsResult As String = ""
        Dim rsTarget As String = "", rsSuccess As String = 0, rsMessage As String = "", rsStep As String = 0, rsId As String = 0

        Try

            'AMBIL DATA BELUM TERUPLOAD
            'AMBIL DATA UTAMA
            SetProgress("Selecting main transaction")
            sql = "SELECT sp.*, '' as pesan FROM m3_sp sp WHERE sp.spuploaded = 0 AND sp.spstatus = 2 ORDER BY sp.spid ASC"
            dtUtama = AsDataTableAmbilDariDB(sql, strCon)
            If dtUtama.Rows.Count > 0 Then
                'AMBIL DATA DETAIL
                SetProgress("Selecting detail transaction")
                sql = "SELECT spd.* FROM m3_sp sp JOIN m3_sp_detail spd ON sp.spid = spd.idsp WHERE sp.spuploaded = 0 AND sp.spstatus = 2 ORDER BY sp.spid ASC, spd.idspdetail ASC"
                dtDetail = AsDataTableAmbilDariDB(sql, strCon)

                'PERULANGAN PROSES UPLOAD DAN UPDATE STATUS UPLOADED
                'PROSES UTAMA
                SetProgress("Processing upload transaction")
                noPerulangan = 0
                pembantu()
            End If


        Catch ex As Exception

            errMessage = "Failed : " & ex.Message
            GoTo selesai

        End Try

        'errMessage = "Success"

selesai:
        'SetProgress("Finish Upload Stock Opname (SP) - " & errMessage)

    End Sub

    Public Sub pembantu()
        'SetProgress("Start uploading Stock Opname (SP)...")

        'PROSESNYA
        '1. HAPUS DATA TEMPORARY STOK KURANG --> SESUAI DATA GUDANG PADA TRANSAKSI
        '2. AMBIL DATA TRANSAKSI YANG BELUM TERUPLOAD
        '3. SET NOREF = NOTRANSAKSI LOKAL, DIKIRIM DI PARAM FILTER JUGA
        '4. UPLOAD TRANSAKSI
        '5. UPDATE STATUS TRANSAKSI SUDAH TERUPLOAD

        Dim sql As String = "", idUtama As Double = 0, noTransaksi As String = "", noRef As String = "", userId As Double = 0
        Dim dtDetailCurr As New DataTable, dtBatchCurr As New DataTable, dtSerialCurr As New DataTable, dtPayCurr As New DataTable
        Dim strWs As String = "", strUtama As String = "", strDetail As String = "", strBatch As String = "", strSerial As String = "", strPay As String = ""
        Dim errMessage As String = "", wsResult As String = ""
        Dim rsTarget As String = "", rsSuccess As String = 0, rsMessage As String = "", rsStep As String = 0, rsId As String = 0


        If noPerulangan < dtUtama.Rows.Count Then
            Dim drUtama As DataRow
            drUtama = dtUtama.Rows(noPerulangan)
            'RESET VARIABEL
            strUtama = "" : strDetail = "" : strBatch = "" : strSerial = "" : strPay = ""

            'SET VARIABEL
            idUtama = FixDouble(drUtama("spid"))
            noTransaksi = FixQuotes(drUtama("spnotransaksi"))
            noRef = FixQuotes(drUtama("spnoref")) & "(" & noTransaksi & ")"
            userId = FixDouble(drUtama("spinputuser"))

            'MAPPING UTAMA
            '            spid,                                  spcabang,                                   splokasi,                               spgudang,                                       spsumber,                       spautonotransaksi,                          spnotransaksi,                                              sptgl,                          spkodepa,                       spbagiansp,                                     spbagianspkontak,                                   spuraian,                                   spcatatan,                      spnoref,                                                    sptglnoref,                         spstatussa,                     spstatus,                           spstatussebelumnya,                     spjmlrevisi,                            spcetakanke,                        spinputuser,                spinputtgl,                             spmodifikasiuser,               spmodifikasitgl,                spposting,                      sptutupperiode,                     spisclose,                                  spcustomtext1,                                      spcustomtext2,                                  spcustomtext3,                                  spcustomtext4,                                  spcustomtext5,                          spcustomint1,                       spcustomint2,                       spcustomint3,                                   spcustomdbl1,                                   spcustomdbl2,                                   spcustomdbl3,                                                   spcustomdate1,                                                      spcustomdate2,                                                  spcustomdate3,                                      spstepke
            strUtama = idUtama & sptField & FixQuotes(drUtama("spcabang")) & sptField & FixQuotes(drUtama("splokasi")) & sptField & FixQuotes(drUtama("spgudang")) & sptField & FixQuotes(drUtama("spsumber")) & sptField & drUtama("spautonotransaksi") & sptField & FixQuotes(noTransaksi) & sptField & FixQuotes(AsFormatTanggal(drUtama("sptgl"))) & sptField & drUtama("spkodepa") & sptField & drUtama("spbagiansp") & sptField & FixQuotes(drUtama("spbagianspkontak")) & sptField & FixQuotes(drUtama("spuraian")) & sptField & FixQuotes(drUtama("spcatatan")) & sptField & FixQuotes(noRef) & sptField & FixQuotes(AsFormatTanggal(drUtama("sptglnoref"))) & sptField & drUtama("spstatussa") & sptField & drUtama("spstatus") & sptField & drUtama("spstatussebelumnya") & sptField & drUtama("spjmlrevisi") & sptField & drUtama("spcetakanke") & sptField & drUtama("spinputuser") & sptField & "1971-01-01 00:00:00" & sptField & drUtama("spmodifikasiuser") & sptField & "1971-01-01 00:00:00" & sptField & "0" & sptField & drUtama("sptutupperiode") & sptField & drUtama("spisclose") & sptField & FixQuotes(drUtama("spcustomtext1")) & sptField & FixQuotes(drUtama("spcustomtext2")) & sptField & FixQuotes(drUtama("spcustomtext3")) & sptField & FixQuotes(drUtama("spcustomtext4")) & sptField & FixQuotes(drUtama("spcustomtext5")) & sptField & drUtama("spcustomint1") & sptField & drUtama("spcustomint2") & sptField & drUtama("spcustomint3") & sptField & FixDouble(drUtama("spcustomdbl1")) & sptField & FixDouble(drUtama("spcustomdbl2")) & sptField & FixDouble(drUtama("spcustomdbl3")) & sptField & FixQuotes(AsFormatTanggal(drUtama("spcustomdate1"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("spcustomdate2"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("spcustomdate3"))) & sptField & FixDouble(drUtama("spstepke"))

            'PROSES DETAIL
            dtDetailCurr = AsDataTableFilterSortDt(dtDetail, "idsp = " & idUtama)
            For Each dr1 As DataRow In dtDetailCurr.Rows
                strDetail = IIf(Len(strDetail) > 0, strDetail & sptRow, strDetail)

                'MAPPING DETAIL
                '   idspdetail,                         idsp,                   idbarang,                               namabarang,                                 tipebarang,                             jmlsistem,                              jmlfisik,                               jmlbagus,                               jmlrusak,                               selisih,                                satuan,                             nilaisatuan,                                jmlbarangsistem,                                jmlbarangfisik,                                 jmlbarangbagus,                             jmlbarangrusak,                                 selisihbarang,                              satuanbarang,                               cabang,                             lokasi,                                 gudang,                             lokasibarang,                       jmlsa,                  statussa,                               costcenter,                                 divisi,                             subdivisi,                              proyek,                                 catatan,                    urutan,                     isclose,                            customtext1,                                customtext2,                                customtext3,                            customdbl1,                                 customdbl2,                                 customdbl3,                                             customdate1,                                                customdate2,                                                customdate3
                strDetail &= 0 & sptField & FixDouble(idUtama) & sptField & dr1("idbarang") & sptField & FixQuotes(dr1("namabarang")) & sptField & FixQuotes(dr1("tipebarang")) & sptField & FixDouble(dr1("jmlsistem")) & sptField & FixDouble(dr1("jmlfisik")) & sptField & FixDouble(dr1("jmlbagus")) & sptField & FixDouble(dr1("jmlrusak")) & sptField & FixDouble(dr1("selisih")) & sptField & FixQuotes(dr1("satuan")) & sptField & FixDouble(dr1("nilaisatuan")) & sptField & FixDouble(dr1("jmlbarangsistem")) & sptField & FixDouble(dr1("jmlbarangfisik")) & sptField & FixDouble(dr1("jmlbarangbagus")) & sptField & FixDouble(dr1("jmlbarangrusak")) & sptField & FixDouble(dr1("selisihbarang")) & sptField & FixQuotes(dr1("satuanbarang")) & sptField & FixQuotes(dr1("cabang")) & sptField & FixQuotes(dr1("lokasi")) & sptField & FixQuotes(dr1("gudang")) & sptField & FixQuotes(dr1("lokasibarang")) & sptField & dr1("jmlsa") & sptField & dr1("statussa") & sptField & FixQuotes(dr1("costcenter")) & sptField & FixQuotes(dr1("divisi")) & sptField & FixQuotes(dr1("subdivisi")) & sptField & FixQuotes(dr1("proyek")) & sptField & FixQuotes(dr1("catatan")) & sptField & dr1("urutan") & sptField & dr1("isclose") & sptField & FixQuotes(dr1("customtext1")) & sptField & FixQuotes(dr1("customtext2")) & sptField & FixQuotes(dr1("customtext3")) & sptField & FixDouble(dr1("customdbl1")) & sptField & FixDouble(dr1("customdbl2")) & sptField & FixDouble(dr1("customdbl3")) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate3")))
            Next

            'PARAMETER WS
            '        WebsiteAccessKey       ★       M3_SpSimpan     ★        0      △         0     △                   △                   △        dd/MM/yyyy         △        dd/MM/yyyy H:mm:ss         ★        userId             ★       0         ★     utama           △       detail
            strWs = "WebsiteAccessKey" & sptParam & "M3_SpSimpan" & sptParam & 0 & sptSubParam & 0 & sptSubParam & noRef & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "0" & sptParam & strUtama & sptSubParam & strDetail


            'UPLOAD TRANSAKSI (PANGGIL WS SIMPAN SERVER PUSAT)
            'F_CallWs(strUrlWs, strWs
            noPerulangan = noPerulangan + 1
            F_CallWs(strUrlWs, strWs)
        Else
            Log("sukses")
        End If

    End Sub

    Public Sub pembantu2(ByVal data As String)
        'PROSESNYA
        '1. HAPUS DATA TEMPORARY STOK KURANG --> SESUAI DATA GUDANG PADA TRANSAKSI
        '2. AMBIL DATA TRANSAKSI YANG BELUM TERUPLOAD
        '3. SET NOREF = NOTRANSAKSI LOKAL, DIKIRIM DI PARAM FILTER JUGA
        '4. UPLOAD TRANSAKSI
        '5. UPDATE STATUS TRANSAKSI SUDAH TERUPLOAD

        Dim sql As String = "", idUtama As Double = 0, noTransaksi As String = "", noRef As String = "", userId As Double = 0
        Dim dtDetailCurr As New DataTable, dtBatchCurr As New DataTable, dtSerialCurr As New DataTable, dtPayCurr As New DataTable
        Dim strWs As String = "", strUtama As String = "", strDetail As String = "", strBatch As String = "", strSerial As String = "", strPay As String = ""
        Dim errMessage As String = "", wsResult As String = ""
        Dim rsTarget As String = "", rsSuccess As String = 0, rsMessage As String = "", rsStep As String = 0, rsId As String = 0

        wsResult = data
        rsWs = wsResult.Split(sptSubParam) 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        If rsWs.Length > 1 Then
            rsTarget = rsWs(0)
            rsSuccess = rsWs(1)
            rsMessage = rsWs(2)
            rsStep = rsWs(3)
            rsId = rsWs(4)
        Else
            rsTarget = "Invalid web services result."
            rsSuccess = 0
            rsMessage = wsResult
            rsStep = 0
            rsId = 0
        End If

        'UPDATE STATUS TRANSAKSI
        If rsSuccess = 1 Then
            'JIKA UPLOAD BERHASIL MAKA UPDATE STATUS TRANSAKSI MENJADI SUDAH UPLOAD

            'TRANSAKSI KE DATABASE
            Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
            Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

            Dim myConn As MySql.Data.MySqlClient.MySqlConnection
            myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
            myConn.Open()

            'Start Transaction
            Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

            Try

                'UPDATE STATUS TERUPLOAD
                sql = "UPDATE m3_sp sp SET sp.spuploaded = 1 WHERE sp.spid = '" & FixDouble(idUtama) & "'"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'Commit Transaction
                Trans.Commit()

            Catch ex As Exception

                'RollBack Transaction
                Trans.Rollback()
                rsMessage &= " | " & ex.Message

            End Try

        End If

        'UPDATE DATATABLE
        AsDataTableTambahData(dtTransaksi, "IdTrans~NoTrans~Uploaded~Desc", idUtama & "~" & noTransaksi & "~" & rsSuccess & "~" & rsMessage)
        'SetDGTransaksi(dtTransaksi)

        pembantu()
    End Sub

#End Region

#Region "Download"

    Public Sub MyDownload()

        Dim stepKe As Double = 0, strParam As String = "", sql As String = ""
        Dim rsDownload() As String, strSqlData As String = "", fileName As String = ""

        Dim IsServer As String = "", UrlWS As String = "", ValidasiStok As String = ""
        Dim ProsesHpp As String = "", Alamat1 As String = "", Alamat2 As String = ""

        SetProgress("Starting download data.")

        'DISABLE BUTTON DAN TIMER
        btnstop.Enabled = False
        btnuploadnow.Enabled = False
        btndownloadnow.Enabled = False
        tmrDurasi.Stop()
        tmrJam.Stop()

        Try

            '1. AMBIL SETTING KHUSUS YANG TIDAK DIREPLACE DARI DATA DOWNLOAD
            '2. DOWNLOAD DB DARI PUSAT (BERBENTUK SQL STRING)
            '3. CREATE FILE SQL (DARI SQL STRING YANG SUDAH DI DOWNLOAD DARI PUSAT)
            '4. EXECUTE FILE SQL YANG SUDAH DIBUAT
            '5. UPDATE SETTING KHUSUS YANG TIDAK DIREPLACE DARI DATA DOWNLOAD

            'PROSES AMBIL SETTING KHUSUS YANG TIDAK DIREPLACE DARI DATA DOWNLOAD (IsServer, UrlWS, ValidasiStok, ProsesHpp, Alamat1, Alamat2)
            stepKe += 1
            SetProgress("Get data setting.")
            'IsServer
            IsServer = F_getSetting(0, "company", "IsServer", strCon)
            If Len(IsServer) = 0 Then
                SetProgress("Get setting IsServer data failed - step :" & stepKe) : GoTo selesai
            End If
            'UrlWS
            UrlWS = F_getSetting(0, "company", "UrlWS", strCon)
            If Len(UrlWS) = 0 Then
                SetProgress("Get setting UrlWS data failed - step :" & stepKe) : GoTo selesai
            End If
            'ValidasiStok
            ValidasiStok = F_getSetting(0, "company", "ValidasiStok", strCon)
            If Len(ValidasiStok) = 0 Then
                SetProgress("Get setting ValidasiStok data failed - step :" & stepKe) : GoTo selesai
            End If
            'ProsesHpp
            ProsesHpp = F_getSetting(0, "accounting", "ProsesHpp", strCon)
            If Len(ProsesHpp) = 0 Then
                SetProgress("Get setting ProsesHpp data failed - step :" & stepKe) : GoTo selesai
            End If
            'Alamat1
            Alamat1 = F_getSetting(0, "company", "Alamat1", strCon)
            If Len(Alamat1) = 0 Then
                SetProgress("Get setting Alamat1 data failed - step :" & stepKe) : GoTo selesai
            End If
            'Alamat2
            Alamat2 = F_getSetting(0, "company", "Alamat2", strCon)
            If Len(Alamat2) = 0 Then
                SetProgress("Get setting Alamat2 data failed - step :" & stepKe) : GoTo selesai
            End If


            'PROSES DOWNLOAD DB DARI PUSAT
            stepKe += 1
            SetProgress("Downloading data.")
            rsDownload = MyDownload_DB() 'isSuccess(0), errMessage(1), strSql(2)
            If rsDownload(0) = 1 Then
                strSqlData = rsDownload(2)
            Else
                SetProgress("Download data failed - step :" & stepKe & ". " & rsDownload(1)) : GoTo selesai
            End If


            'PROSES CREATE FILE SQL
            stepKe += 1
            SetProgress("Creating data.")
            'PARAMETER : WebsiteAccessKey★M0_ExecuteDbFile★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★userId★0★content
            strParam = "WebsiteAccessKey★M0_ExecuteDbFile★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★0★" & strSqlData
            rsDownload = M0_CreateDbFile(strParam) 'isSuccess(0), errMessage(1), fileName(2)
            If rsDownload(0) = 1 Then
                fileName = rsDownload(2)
            Else
                SetProgress("Creating data failed - step :" & stepKe & ". " & rsDownload(1)) : GoTo selesai
            End If


            'PROSES EXECUTE FILE SQL
            stepKe += 1
            SetProgress("Executing data.")
            'PARAMETER : WebsiteAccessKey★M0_ExecuteDbFile★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★userId★0★fileName
            strParam = "WebsiteAccessKey★M0_ExecuteDbFile★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★0★" & fileName
            rsDownload = M0_ExecuteDbFile(strParam) 'isSuccess(0), errMessage(1), fileName(2)
            If rsDownload(0) <> 1 Then
                SetProgress("Executing data failed - step :" & stepKe & ". " & rsDownload(1)) : GoTo selesai
            End If


            'PROSES UPDATE SETTING KHUSUS YANG TIDAK DIREPLACE DARI DATA DOWNLOAD
            SetProgress("Updating data setting.")
            stepKe += 1

            'TRANSAKSI KE DATABASE
            Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
            Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

            Dim myConn As MySql.Data.MySqlClient.MySqlConnection
            myConn = New MySql.Data.MySqlClient.MySqlConnection(strCon)
            myConn.Open()

            'Start Transaction
            Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

            Try

                'UPDATE SETTING KHUSUS
                sql = "  UPDATE m0_setting"
                sql &= " SET snilai = "
                sql &= " (CASE "
                sql &= " WHEN smodule = 0 AND sgrup = 'company' AND skode = 'IsServer' THEN '" & IsServer & "'"
                sql &= " WHEN smodule = 0 AND sgrup = 'company' AND skode = 'UrlWS' THEN '" & UrlWS & "'"
                sql &= " WHEN smodule = 0 AND sgrup = 'company' AND skode = 'ValidasiStok' THEN '" & ValidasiStok & "'"
                sql &= " WHEN smodule = 0 AND sgrup = 'accounting' AND skode = 'ProsesHpp' THEN '" & ProsesHpp & "'"
                sql &= " WHEN smodule = 0 AND sgrup = 'company' AND skode = 'Alamat1' THEN '" & Alamat1 & "'"
                sql &= " WHEN smodule = 0 AND sgrup = 'company' AND skode = 'Alamat2' THEN '" & Alamat2 & "'"
                sql &= " ELSE snilai"
                sql &= " END)"
                sql &= " WHERE"
                sql &= " (smodule = 0 AND sgrup = 'company' AND skode = 'IsServer')"
                sql &= " OR (smodule = 0 AND sgrup = 'company' AND skode = 'UrlWS')"
                sql &= " OR (smodule = 0 AND sgrup = 'company' AND skode = 'ValidasiStok')"
                sql &= " OR (smodule = 0 AND sgrup = 'accounting' AND skode = 'ProsesHpp')"
                sql &= " OR (smodule = 0 AND sgrup = 'company' AND skode = 'Alamat1')"
                sql &= " OR (smodule = 0 AND sgrup = 'company' AND skode = 'Alamat2')"
                objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                With objCmd
                    .Connection = myConn
                    .Transaction = Trans
                    .CommandType = CommandType.Text
                    .CommandText = sql
                End With
                objCmd.ExecuteNonQuery()

                'Commit Transaction
                Trans.Commit()

            Catch ex As Exception

                'RollBack Transaction
                Trans.Rollback()
                SetProgress("Update setting - step :" & stepKe & ". " & ex.Message) : GoTo selesai

            End Try


        Catch ex As Exception
            SetProgress("Download data failed - step :" & stepKe & ". " & ex.Message) : GoTo selesai

        End Try

        SetProgress("Download data finished.")

selesai:

        'ENABLE BUTTON DAN TIMER
        btnstop.Enabled = True
        btnuploadnow.Enabled = True
        btndownloadnow.Enabled = True
        tmrDurasi.Start()
        tmrJam.Start()

    End Sub

    Public Function MyDownload_DB() As String()

        Dim rsDownload(3) As String 'isSuccess(0), errMessage(1), strSql(2)
        Dim strWs As String = "", wsResult As String = "", rsSqlData As String = ""
        Dim rsWs() As String, rsWsResult() As String
        Dim rsTarget As String = "", rsSuccess As String = 0, rsMessage As String = "", rsStep As String = 0, rsId As String = 0

        Try
            'PARAMETER WS
            'WebsiteAccessKey★M0_DownloadDbFile★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★userId★0★
            strWs = "WebsiteAccessKey★M0_DownloadDbFile★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★0★0★"

            'DOWNLOAD DATA (PANGGIL WS DOWNLOAD SERVER PUSAT)
            wsResult = WS_Request(strUrlWs, strWs)
            rsWs = wsResult.Split(sptParam) 'result(0), paging(1), data(2), mapping(3)
            rsWsResult = rsWs(0).Split(sptSubParam) 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
            If rsWsResult.Length > 1 Then
                rsTarget = rsWsResult(0)
                rsSuccess = rsWsResult(1)
                rsMessage = rsWsResult(2)
                rsStep = rsWsResult(3)
                rsId = rsWsResult(4)
            Else
                rsTarget = "Invalid web services result."
                rsSuccess = 0
                rsMessage = wsResult
                rsStep = 0
                rsId = 0
            End If

            rsDownload(0) = rsSuccess 'isSuccess
            rsDownload(1) = wsResult 'rsMessage 'errMessage
            rsDownload(2) = IIf(rsWs.Length > 2, rsWs(2), "") 'data

        Catch ex As Exception
            SetProgress("MyDownload_DB : " & ex.Message)

        End Try

        Return rsDownload

    End Function

    Public Function M0_CreateDbFile(ByVal param As String) As String()

        'RESULT
        'isSuccess(0), errMessage(1), fileName(2)

        'M0_CreateDbFile --------------------------------------------------------
        'content

        Dim rsCreateDB(3) As String

        ''On Error GoTo selesai
        'Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        'Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        'Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        'Dim wsResult As String = ""
        'Dim strResult, strResultPaging As String

        Dim contents As String = "", myPath As String = AppPath + "\files\db\" 'HttpContext.Current.Server.MapPath("~/") & "files\db\"
        Dim fileName As String = ""

        'SET DEFAULT 
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        ''SET DEFAULT PAGING
        'resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0

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

        ''Cek apakah WebsiteAccessKey valid
        'Dim ClsValidKey As New ClsSecurity
        'Dim validKey As RsValidKey
        'validKey = ValidateKey(paramSplit(0))
        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        ''///Validasi Hak akses. Cek ModuleID dan MenuID
        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
        '    result(2) = "Access denied for insert/update data"
        'End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (IsNumeric(paramSplit(3)) = False) Then
            result(2) = "userid required numeric." : GoTo selesai
        End If

        'SET USERID
        userId = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'VALIDASI DAN SET DATA =============================================================
        'SET CONTENTS
        contents = paramSplit(5)

        'SET FILENAME DAN FILEPATH
        Dim Security As New ClsSecurity
        fileName = Security.MD5CalcString(userId & paramSplit(0) & Now & Now.Millisecond) & ".sql"
        fileName = fileName.Replace(" ", "_")
        'END OF VALIDASI DAN SET DATA ======================================================


        'CEK FILE EXISTS
        Try
            File.Delete(myPath & fileName)
            File.WriteAllText(myPath & fileName, contents)
            'contents = fileName & sptSubParam & contents
        Catch ex As Exception
            result(2) = ex.Message
            'contents = "" : GoTo selesai
        End Try

        result(1) = 1

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        'strResult = String.Join(sptSubParam, result)
        'strResultPaging = String.Join(sptSubParam, resultPaging)
        'wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, fileName)

        rsCreateDB(0) = result(1) 'isSuccess
        rsCreateDB(1) = result(2) 'errMessage
        rsCreateDB(2) = fileName 'fileName

        'Return wsResult
        Return rsCreateDB

    End Function

    Public Function M0_ExecuteDbFile(ByVal param As String) As String()

        'RESULT
        'isSuccess(0), errMessage(1), fileName(2)

        'M0_ExecuteDbFile --------------------------------------------------------
        'namaFile

        Dim rsExecuteDB(3) As String

        ''On Error GoTo selesai
        'Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        'Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        'Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        'Dim wsResult As String = ""
        'Dim strResult, strResultPaging As String

        Dim filename As String = "", myPath As String = AppPath + "\files\db\" 'HttpContext.Current.Server.MapPath("~/") & "files\db\"

        Try

            'SET DEFAULT 
            result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
            result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

            ''SET DEFAULT PAGING
            'resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0


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

            ''Cek apakah WebsiteAccessKey valid
            'Dim ClsValidKey As New ClsSecurity
            'Dim validKey As RsValidKey
            'validKey = ValidateKey(paramSplit(0))
            'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

            ''///Validasi Hak akses. Cek ModuleID dan MenuID
            'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            '    result(2) = "Access denied for insert/update data"
            'End If
            'END OF VALIDASI WEBSITEACCESSKEY ==================================================


            'VALIDASI DAN SET USERID ===========================================================
            'CEK USERID
            If (IsNumeric(paramSplit(3)) = False) Then
                result(2) = "userid required numeric." : GoTo selesai
            End If

            'SET USERID
            userId = paramSplit(3)
            'END OF VALIDASI DAN SET USERID ====================================================


            'VALIDASI DAN SET DATA =============================================================
            'SET FILENAME
            filename = paramSplit(5)
            If Len(filename) < 1 Then
                result(2) = "Filename can't be empty." : GoTo selesai
            End If
            'END OF VALIDASI DAN SET DATA ======================================================


            'EXECUTE SQL FILE
            Dim arrExecute() As String = F_ExecuteSQL(paramSplit(0), userId, filename, strCon)
            If arrExecute(0) = 0 Then
                result(2) = arrExecute(1) : GoTo selesai
            End If

            result(1) = 1

selesai:
            If result(1) = 0 Then
                If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
            End If

            'strResult = String.Join(sptSubParam, result)
            'strResultPaging = String.Join(sptSubParam, resultPaging)
            'wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, "")

            'Return wsResult

            rsExecuteDB(0) = result(1) 'isSuccess
            rsExecuteDB(1) = result(2) 'errMessage
            rsExecuteDB(2) = filename 'fileName

        Catch ex As Exception
            SetProgress("M0_ExecuteDbFile : " & ex.Message)

        End Try

        Return rsExecuteDB

    End Function

    '    Public Function M0_DownloadDbFile(ByVal param As String) As String
    '        'M0_DownloadDbFile --------------------------------------------------------

    '        'On Error GoTo selesai
    '        Dim formatTgl As String = "", formatTglWaktu As String = "", search As String = ""

    '        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
    '        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)

    '        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
    '        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

    '        Dim wsResult As String = ""
    '        Dim strResult, strResultPaging As String

    '        Dim filename As String = "", myPath As String = AppPath + "\files\db\" 'HttpContext.Current.Server.MapPath("~/") & "files\db\"

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
    '        'Dim ClsValidKey As New ClsSecurity
    '        'Dim validKey As RsValidKey
    '        'validKey = ValidateKey(paramSplit(0))
    '        'If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

    '        ''///Validasi Hak akses. Cek ModuleID dan MenuID
    '        'If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
    '        '    result(2) = "Access denied for insert/update data"
    '        'End If
    '        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


    '        'VALIDASI DAN SET USERID ===========================================================
    '        'CEK USERID
    '        If (IsNumeric(paramSplit(3)) = False) Then
    '            result(2) = "userid required numeric." : GoTo selesai
    '        End If

    '        'SET USERID
    '        userid = paramSplit(3)
    '        'END OF VALIDASI DAN SET USERID ====================================================


    '        ''VALIDASI DAN SET DATA =============================================================
    '        ''SET FILENAME
    '        'filename = paramSplit(5)
    '        'If Len(filename) < 1 Then
    '        '    result(2) = "Filename can't be empty." : GoTo selesai
    '        'End If
    '        ''END OF VALIDASI DAN SET DATA ======================================================

    '        'EXECUTE SQL FILE
    '        Dim arrDownload() As String = F_DumpSQLAsString(paramSplit(0), userid)
    '        If arrDownload(0) = 0 Then
    '            result(2) = arrDownload(1) : GoTo selesai
    '        Else
    '            search = arrDownload(1)
    '        End If

    '        result(1) = 1

    'selesai:
    '        If result(1) = 0 Then
    '            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
    '        End If

    '        strResult = String.Join(sptSubParam, result)
    '        strResultPaging = String.Join(sptSubParam, resultPaging)
    '        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, search)

    '        Return wsResult
    '    End Function

#End Region

End Class
