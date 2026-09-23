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
                        F_Switch("SimpanConfig")

                    Else
                        Dim dataString = File.ReadAllText(LokasiConfig)
                        Using reader As XmlReader = XmlReader.Create(New StringReader(dataString))
                            While reader.Read()
                                Select Case reader.NodeType
                                    Case XmlNodeType.Element
                                        Select Case reader.Name
                                            Case "durasi" : txtdurasi.Text = reader.ReadElementContentAsString()
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
                   
                    btnstart.PerformClick()

                Case "Stop"
                    F_Switch("SimpanConfig")

                    cbxDurasi.Enabled = True
                    txtdurasi.Enabled = True

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
                F_ProsesUploadDownload("uploaddownload")

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
            stepKe += 1 : MyUploadM3_SP()

            'PROSES UPLOAD SA
            stepKe += 1 : MyUploadM3_SA()

            'PROSES UPLOAD SO
            stepKe += 1 : MyUploadM5_SO()

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

    '=======================================================

    Public Sub MyUploadM3_SP()

        SetProgress("Start uploading Stock Opname (SP)...")

        'PROSESNYA
        '1. HAPUS DATA TEMPORARY STOK KURANG --> SESUAI DATA GUDANG PADA TRANSAKSI
        '2. AMBIL DATA TRANSAKSI YANG BELUM TERUPLOAD
        '3. SET NOREF = NOTRANSAKSI LOKAL, DIKIRIM DI PARAM FILTER JUGA
        '4. UPLOAD TRANSAKSI
        '5. UPDATE STATUS TRANSAKSI SUDAH TERUPLOAD

        Dim sql As String = "", idUtama As Double = 0, noTransaksi As String = "", noRef As String = "", userId As Double = 0
        Dim dtUtama As New DataTable, dtDetail As New DataTable, dtBatch As New DataTable, dtSerial As New DataTable, dtPay As New DataTable
        Dim dtDetailCurr As New DataTable, dtBatchCurr As New DataTable, dtSerialCurr As New DataTable, dtPayCurr As New DataTable
        Dim strWs As String = "", strUtama As String = "", strDetail As String = "", strBatch As String = "", strSerial As String = "", strPay As String = ""
        Dim errMessage As String = "", wsResult As String = ""
        Dim rsWs() As String 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
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
                For Each drUtama As DataRow In dtUtama.Rows

                    'RESET VARIABEL
                    strUtama = "" : strDetail = "" : strBatch = "" : strSerial = "" : strPay = ""

                    'SET VARIABEL
                    sumber = FixQuotes(drUtama("spsumber"))
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
                    AsDataTableTambahData(dtTransaksi, "Trans~IdTrans~NoTrans~Uploaded~Desc", sumber & "~" & idUtama & "~" & noTransaksi & "~" & rsSuccess & "~" & rsMessage)
                    'SetDGTransaksi(dtTransaksi)

                    SetProgress(noTransaksi & " : " & IIf(rsSuccess = 1, "Uploaded", "Failed") & " - " & rsMessage)

                    'JEDA 1 DETIK
                    System.Threading.Thread.Sleep(1000)

                Next

            End If

            'AMBIL JUMLAH BERHASIL UPLOAD DAN GAGAL UPLOAD
            strResultUpload &= vbCrLf & "Stock Opname (SP) : " & AsDataTableDCount(dtTransaksi, "Trans = 'SP' AND Uploaded = 1") & " Uploaded, "
            strResultUpload &= AsDataTableDCount(dtTransaksi, "Trans = 'SP' AND Uploaded = 0") & " Failed"

        Catch ex As Exception

            errMessage = "Failed : " & ex.Message
            GoTo selesai

        End Try

        errMessage = "Success"

selesai:
        SetProgress("Finish Upload Stock Opname (SP) - " & errMessage)

    End Sub

    Public Sub MyUploadM3_SpMain()

        'AMBIL DATA TRANSAKSI YANG BELUM TERUPLOAD
        'PANGGIL FUNGSI PROSES (BUAT PARAMETER DAN KIRIM WS)

        SetProgress("Start uploading Stock Opname (SP)...")

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
            sql = "SELECT sp.*, '' as pesan FROM m3_sp sp WHERE sp.spuploaded = 0 AND sp.spstatus = 2 ORDER BY sp.spid ASC"
            dtUtama = AsDataTableAmbilDariDB(sql, strCon)
            If dtUtama.Rows.Count > 0 Then

                'AMBIL DATA DETAIL
                SetProgress("Selecting detail transaction")
                sql = "SELECT spd.* FROM m3_sp sp JOIN m3_sp_detail spd ON sp.spid = spd.idsp WHERE sp.spuploaded = 0 AND sp.spstatus = 2 ORDER BY sp.spid ASC, spd.idspdetail ASC"
                dtDetail = AsDataTableAmbilDariDB(sql, strCon)

                'PANGGIL FUNGSI PROSES (BUAT PARAMETER DAN KIRIM WS)
                SetProgress("Processing upload transaction")
                noPerulangan = 0
                MyUploadM3_SpProcess()

            End If

        Catch ex As Exception

            errMessage = "Failed : " & ex.Message
            GoTo selesai

        End Try

        errMessage = "Success"

selesai:
        SetProgress("Get data Stock Opname (SP) - " & errMessage)

    End Sub

    Public Sub MyUploadM3_SpProcess()

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
                sumber = FixQuotes(drUtama("spsumber"))
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
        SetProgress("Upload data Stock Opname (SP) - " & errMessage)

    End Sub

    Public Sub MyUploadM3_SpResult(ByVal data As String)

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


        Catch ex As Exception

            errMessage = "Failed : " & ex.Message
            GoTo selesai

        End Try

        errMessage = "Success"

selesai:
        SetProgress("Update data Stock Opname (SP) - " & errMessage)

        'PROSES BUAT PARAMETER DAN KIRIM WS KE PUSAT, UPLOAD TRANSAKSI DATA SELANJUTNYA
        noPerulangan += 1
        MyUploadM3_SpProcess()

    End Sub

    '=======================================================

    Public Sub MyUploadM3_SA()

        SetProgress("Start uploading Stock Adjusment (SA)...")

        'PROSESNYA
        '1. HAPUS DATA TEMPORARY STOK KURANG --> SESUAI DATA GUDANG PADA TRANSAKSI
        '2. AMBIL DATA TRANSAKSI YANG BELUM TERUPLOAD
        '3. SET NOREF = NOTRANSAKSI LOKAL, DIKIRIM DI PARAM FILTER JUGA
        '4. UPLOAD TRANSAKSI
        '5. UPDATE STATUS TRANSAKSI SUDAH TERUPLOAD

        Dim sql As String = "", idUtama As Double = 0, noTransaksi As String = "", noRef As String = "", userId As Double = 0
        Dim dtUtama As New DataTable, dtDetail As New DataTable, dtBatch As New DataTable, dtSerial As New DataTable, dtPay As New DataTable
        Dim dtDetailCurr As New DataTable, dtBatchCurr As New DataTable, dtSerialCurr As New DataTable, dtPayCurr As New DataTable
        Dim strWs As String = "", strUtama As String = "", strDetail As String = "", strBatch As String = "", strSerial As String = "", strPay As String = ""
        Dim errMessage As String = "", wsResult As String = ""
        Dim rsWs() As String 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim rsTarget As String = "", rsSuccess As String = 0, rsMessage As String = "", rsStep As String = 0, rsId As String = 0

        Try

            'AMBIL DATA BELUM TERUPLOAD
            'AMBIL DATA UTAMA
            SetProgress("Selecting main transaction")
            sql = "SELECT sa.*, '' as pesan FROM m3_sa sa WHERE sa.sauploaded = 0 AND sa.sastatus = 2 ORDER BY sa.said ASC"
            dtUtama = AsDataTableAmbilDariDB(sql, strCon)
            If dtUtama.Rows.Count > 0 Then

                'AMBIL DATA DETAIL
                SetProgress("Selecting detail transaction")
                sql = "SELECT sad.* FROM m3_sa sa JOIN m3_sa_detail sad ON sa.said = sad.idsa WHERE sa.sauploaded = 0 AND sa.sastatus = 2 ORDER BY sa.said ASC, sad.idsadetail ASC"
                dtDetail = AsDataTableAmbilDariDB(sql, strCon)

                'AMBIL DATA BATCH
                SetProgress("Selecting batch transaction")
                sql = "SELECT nbt.* FROM m3_sa sa JOIN m1_no_batch_transaction nbt ON sa.sasumber = nbt.nbtsumber AND sa.said = nbt.nbtidtransaksi WHERE sa.sauploaded = 0 AND sa.sastatus = 2 ORDER BY sa.said ASC, nbt.nbtid ASC"
                dtBatch = AsDataTableAmbilDariDB(sql, strCon)

                'AMBIL DATA SERIAL
                SetProgress("Selecting serial transaction")
                sql = "SELECT nst.* FROM m3_sa sa JOIN m1_no_serial_transaction nst ON sa.sasumber = nst.nstsumber AND sa.said = nst.nstidtransaksi WHERE sa.sauploaded = 0 AND sa.sastatus = 2 ORDER BY sa.said ASC, nst.nstid ASC"
                dtSerial = AsDataTableAmbilDariDB(sql, strCon)


                'PERULANGAN PROSES UPLOAD DAN UPDATE STATUS UPLOADED
                'PROSES UTAMA
                SetProgress("Processing upload transaction")
                For Each drUtama As DataRow In dtUtama.Rows

                    'RESET VARIABEL
                    strUtama = "" : strDetail = "" : strBatch = "" : strSerial = "" : strPay = ""

                    'SET VARIABEL
                    sumber = FixQuotes(drUtama("sasumber"))
                    idUtama = FixDouble(drUtama("said"))
                    noTransaksi = FixQuotes(drUtama("sanotransaksi"))
                    noRef = FixQuotes(drUtama("sanoref")) & "(" & noTransaksi & ")"
                    userId = FixDouble(drUtama("sainputuser"))

                    'MAPPING UTAMA
                    '            said,                                  sacabang,                                   salokasi,                                   sagudang,                                   sasumber,                       sajenis,                        saautonotransaksi,                          sanotransaksi,                                              satgl,                          sakodepa,                       sabagiansa,                                     sabagiansakontak,                                   sauraian,                                   sacatatan,                      sanoref,                                                    satglnoref,                        saidsp,                      sastatus,                           sastatussebelumnya,                     sajmlrevisi,                            sacetakanke,                    sainputuser,                    sainputtgl,                             samodifikasiuser,               samodifikasitgl,                saposting,                  satutupperiode,                         saisclose,                                  sacustomtext1,                                  sacustomtext2,                                      sacustomtext3,                                  sacustomtext4,                                  sacustomtext5,                          sacustomint1,                       sacustomint2,                       sacustomint3,                                   sacustomdbl1,                                   sacustomdbl2,                                   sacustomdbl3,                                                       sacustomdate1,                                                  sacustomdate2,                                                  sacustomdate3
                    strUtama = idUtama & sptField & FixQuotes(drUtama("sacabang")) & sptField & FixQuotes(drUtama("salokasi")) & sptField & FixQuotes(drUtama("sagudang")) & sptField & FixQuotes(drUtama("sasumber")) & sptField & drUtama("sajenis") & sptField & drUtama("saautonotransaksi") & sptField & FixQuotes(noTransaksi) & sptField & FixQuotes(AsFormatTanggal(drUtama("satgl"))) & sptField & drUtama("sakodepa") & sptField & drUtama("sabagiansa") & sptField & FixQuotes(drUtama("sabagiansakontak")) & sptField & FixQuotes(drUtama("sauraian")) & sptField & FixQuotes(drUtama("sacatatan")) & sptField & FixQuotes(noRef) & sptField & FixQuotes(AsFormatTanggal(drUtama("satglnoref"))) & sptField & drUtama("saidsp") & sptField & drUtama("sastatus") & sptField & drUtama("sastatussebelumnya") & sptField & drUtama("sajmlrevisi") & sptField & drUtama("sacetakanke") & sptField & drUtama("sainputuser") & sptField & "1971-01-01 00:00:00" & sptField & drUtama("samodifikasiuser") & sptField & "1971-01-01 00:00:00" & sptField & "0" & sptField & drUtama("satutupperiode") & sptField & drUtama("saisclose") & sptField & FixQuotes(drUtama("sacustomtext1")) & sptField & FixQuotes(drUtama("sacustomtext2")) & sptField & FixQuotes(drUtama("sacustomtext3")) & sptField & FixQuotes(drUtama("sacustomtext4")) & sptField & FixQuotes(drUtama("sacustomtext5")) & sptField & drUtama("sacustomint1") & sptField & drUtama("sacustomint2") & sptField & drUtama("sacustomint3") & sptField & FixDouble(drUtama("sacustomdbl1")) & sptField & FixDouble(drUtama("sacustomdbl2")) & sptField & FixDouble(drUtama("sacustomdbl3")) & sptField & FixQuotes(AsFormatTanggal(drUtama("sacustomdate1"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("sacustomdate2"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("sacustomdate3")))

                    'PROSES DETAIL
                    dtDetailCurr = AsDataTableFilterSortDt(dtDetail, "idsa = " & idUtama)
                    For Each dr1 As DataRow In dtDetailCurr.Rows
                        strDetail = IIf(Len(strDetail) > 0, strDetail & sptRow, strDetail)

                        'MAPPING DETAIL
                        '   idsadetail,                         idsa,                   idbarang,                               namabarang,                             tipebarang,                             jmlmasuk,                                   jmlkeluar,                              satuan,                                 nilaisatuan,                            jmlbarangmasuk,                                 jmlbarangkeluar,                            satuanbarang,                                   idhppkhususmasuk,                               hpplama,                                hpp,                            rekpersediaan,                              reklawan,                               idspdetail,                                 cabang,                             lokasi,                                 gudang,                                 costcenter,                             divisi,                                 subdivisi,                              proyek,                             catatan,                        urutan,                  isclose,                               customtext1,                            customtext2,                                customtext3,                                customdbl1,                             customdbl2,                                 customdbl3,                                                 customdate1,                                                customdate2,                                                customdate3
                        strDetail &= 0 & sptField & FixDouble(idUtama) & sptField & dr1("idbarang") & sptField & FixQuotes(dr1("namabarang")) & sptField & FixQuotes(dr1("tipebarang")) & sptField & FixDouble(dr1("jmlmasuk")) & sptField & FixDouble(dr1("jmlkeluar")) & sptField & FixQuotes(dr1("satuan")) & sptField & FixDouble(dr1("nilaisatuan")) & sptField & FixDouble(dr1("jmlbarangmasuk")) & sptField & FixDouble(dr1("jmlbarangkeluar")) & sptField & FixQuotes(dr1("satuanbarang")) & sptField & FixDouble(dr1("idhppkhususmasuk")) & sptField & FixDouble(dr1("hpplama")) & sptField & FixDouble(dr1("hpp")) & sptField & FixQuotes(dr1("rekpersediaan")) & sptField & FixQuotes(dr1("reklawan")) & sptField & FixDouble(dr1("idspdetail")) & sptField & FixQuotes(dr1("cabang")) & sptField & FixQuotes(dr1("lokasi")) & sptField & FixQuotes(dr1("gudang")) & sptField & FixQuotes(dr1("costcenter")) & sptField & FixQuotes(dr1("divisi")) & sptField & FixQuotes(dr1("subdivisi")) & sptField & FixQuotes(dr1("proyek")) & sptField & FixQuotes(dr1("catatan")) & sptField & dr1("urutan") & sptField & dr1("isclose") & sptField & FixQuotes(dr1("customtext1")) & sptField & FixQuotes(dr1("customtext2")) & sptField & FixQuotes(dr1("customtext3")) & sptField & FixDouble(dr1("customdbl1")) & sptField & FixDouble(dr1("customdbl2")) & sptField & FixDouble(dr1("customdbl3")) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate3")))
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


                    'PARAMETER WS
                    '        WebsiteAccessKey       ★       M3_SaSimpan     ★        0      △         0     △                   △                   △        dd/MM/yyyy         △        dd/MM/yyyy H:mm:ss         ★        userId             ★       0         ★     utama           △       detail          △        batch          △           serial
                    strWs = "WebsiteAccessKey" & sptParam & "M3_SaSimpan" & sptParam & 0 & sptSubParam & 0 & sptSubParam & noRef & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "0" & sptParam & strUtama & sptSubParam & strDetail & sptSubParam & strBatch & sptSubParam & strSerial


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
                            sql = "UPDATE m3_sa sa SET sa.sauploaded = 1 WHERE sa.said = '" & FixDouble(idUtama) & "'"
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
                    AsDataTableTambahData(dtTransaksi, "Trans~IdTrans~NoTrans~Uploaded~Desc", sumber & "~" & idUtama & "~" & noTransaksi & "~" & rsSuccess & "~" & rsMessage)
                    'SetDGTransaksi(dtTransaksi)

                    SetProgress(noTransaksi & " : " & IIf(rsSuccess = 1, "Uploaded", "Failed") & " - " & rsMessage)

                    'JEDA 1 DETIK
                    System.Threading.Thread.Sleep(1000)

                Next

            End If

            'AMBIL JUMLAH BERHASIL UPLOAD DAN GAGAL UPLOAD
            strResultUpload &= vbCrLf & "Stock Adjustment (SA) : " & AsDataTableDCount(dtTransaksi, "Trans = 'SA' AND Uploaded = 1") & " Uploaded, "
            strResultUpload &= AsDataTableDCount(dtTransaksi, "Trans = 'SA' AND Uploaded = 0") & " Failed"

        Catch ex As Exception

            errMessage = "Failed : " & ex.Message
            GoTo selesai

        End Try

        errMessage = "Success"

selesai:
        SetProgress("Finish Upload Stock Adjustment (SA) - " & errMessage)

    End Sub

    Public Sub MyUploadM3_SaMain()

        'AMBIL DATA TRANSAKSI YANG BELUM TERUPLOAD
        'PANGGIL FUNGSI PROSES (BUAT PARAMETER DAN KIRIM WS)

        SetProgress("Start uploading Stock Adjusment (SA)...")

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
            sql = "SELECT sa.*, '' as pesan FROM m3_sa sa WHERE sa.sauploaded = 0 AND sa.sastatus = 2 ORDER BY sa.said ASC"
            dtUtama = AsDataTableAmbilDariDB(sql, strCon)
            If dtUtama.Rows.Count > 0 Then

                'AMBIL DATA DETAIL
                SetProgress("Selecting detail transaction")
                sql = "SELECT sad.* FROM m3_sa sa JOIN m3_sa_detail sad ON sa.said = sad.idsa WHERE sa.sauploaded = 0 AND sa.sastatus = 2 ORDER BY sa.said ASC, sad.idsadetail ASC"
                dtDetail = AsDataTableAmbilDariDB(sql, strCon)

                'AMBIL DATA BATCH
                SetProgress("Selecting batch transaction")
                sql = "SELECT nbt.* FROM m3_sa sa JOIN m1_no_batch_transaction nbt ON sa.sasumber = nbt.nbtsumber AND sa.said = nbt.nbtidtransaksi WHERE sa.sauploaded = 0 AND sa.sastatus = 2 ORDER BY sa.said ASC, nbt.nbtid ASC"
                dtBatch = AsDataTableAmbilDariDB(sql, strCon)

                'AMBIL DATA SERIAL
                SetProgress("Selecting serial transaction")
                sql = "SELECT nst.* FROM m3_sa sa JOIN m1_no_serial_transaction nst ON sa.sasumber = nst.nstsumber AND sa.said = nst.nstidtransaksi WHERE sa.sauploaded = 0 AND sa.sastatus = 2 ORDER BY sa.said ASC, nst.nstid ASC"
                dtSerial = AsDataTableAmbilDariDB(sql, strCon)

                'PANGGIL FUNGSI PROSES (BUAT PARAMETER DAN KIRIM WS)
                SetProgress("Processing upload transaction")
                noPerulangan = 0
                MyUploadM3_SaProcess()

            End If

        Catch ex As Exception

            errMessage = "Failed : " & ex.Message
            GoTo selesai

        End Try

        errMessage = "Success"

selesai:
        SetProgress("Get data Stock Adjustment (SA) - " & errMessage)

    End Sub

    Public Sub MyUploadM3_SaProcess()

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
                sumber = FixQuotes(drUtama("sasumber"))
                idUtama = FixDouble(drUtama("said"))
                noTransaksi = FixQuotes(drUtama("sanotransaksi"))
                noRef = FixQuotes(drUtama("sanoref")) & "(" & noTransaksi & ")"
                userId = FixDouble(drUtama("sainputuser"))

                'MAPPING UTAMA
                '            said,                                  sacabang,                                   salokasi,                                   sagudang,                                   sasumber,                       sajenis,                        saautonotransaksi,                          sanotransaksi,                                              satgl,                          sakodepa,                       sabagiansa,                                     sabagiansakontak,                                   sauraian,                                   sacatatan,                      sanoref,                                                    satglnoref,                        saidsp,                      sastatus,                           sastatussebelumnya,                     sajmlrevisi,                            sacetakanke,                    sainputuser,                    sainputtgl,                             samodifikasiuser,               samodifikasitgl,                saposting,                  satutupperiode,                         saisclose,                                  sacustomtext1,                                  sacustomtext2,                                      sacustomtext3,                                  sacustomtext4,                                  sacustomtext5,                          sacustomint1,                       sacustomint2,                       sacustomint3,                                   sacustomdbl1,                                   sacustomdbl2,                                   sacustomdbl3,                                                       sacustomdate1,                                                  sacustomdate2,                                                  sacustomdate3
                strUtama = idUtama & sptField & FixQuotes(drUtama("sacabang")) & sptField & FixQuotes(drUtama("salokasi")) & sptField & FixQuotes(drUtama("sagudang")) & sptField & FixQuotes(drUtama("sasumber")) & sptField & drUtama("sajenis") & sptField & drUtama("saautonotransaksi") & sptField & FixQuotes(noTransaksi) & sptField & FixQuotes(AsFormatTanggal(drUtama("satgl"))) & sptField & drUtama("sakodepa") & sptField & drUtama("sabagiansa") & sptField & FixQuotes(drUtama("sabagiansakontak")) & sptField & FixQuotes(drUtama("sauraian")) & sptField & FixQuotes(drUtama("sacatatan")) & sptField & FixQuotes(noRef) & sptField & FixQuotes(AsFormatTanggal(drUtama("satglnoref"))) & sptField & drUtama("saidsp") & sptField & drUtama("sastatus") & sptField & drUtama("sastatussebelumnya") & sptField & drUtama("sajmlrevisi") & sptField & drUtama("sacetakanke") & sptField & drUtama("sainputuser") & sptField & "1971-01-01 00:00:00" & sptField & drUtama("samodifikasiuser") & sptField & "1971-01-01 00:00:00" & sptField & "0" & sptField & drUtama("satutupperiode") & sptField & drUtama("saisclose") & sptField & FixQuotes(drUtama("sacustomtext1")) & sptField & FixQuotes(drUtama("sacustomtext2")) & sptField & FixQuotes(drUtama("sacustomtext3")) & sptField & FixQuotes(drUtama("sacustomtext4")) & sptField & FixQuotes(drUtama("sacustomtext5")) & sptField & drUtama("sacustomint1") & sptField & drUtama("sacustomint2") & sptField & drUtama("sacustomint3") & sptField & FixDouble(drUtama("sacustomdbl1")) & sptField & FixDouble(drUtama("sacustomdbl2")) & sptField & FixDouble(drUtama("sacustomdbl3")) & sptField & FixQuotes(AsFormatTanggal(drUtama("sacustomdate1"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("sacustomdate2"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("sacustomdate3")))

                'PROSES DETAIL
                dtDetailCurr = AsDataTableFilterSortDt(dtDetail, "idsa = " & idUtama)
                For Each dr1 As DataRow In dtDetailCurr.Rows
                    strDetail = IIf(Len(strDetail) > 0, strDetail & sptRow, strDetail)

                    'MAPPING DETAIL
                    '   idsadetail,                         idsa,                   idbarang,                               namabarang,                             tipebarang,                             jmlmasuk,                                   jmlkeluar,                              satuan,                                 nilaisatuan,                            jmlbarangmasuk,                                 jmlbarangkeluar,                            satuanbarang,                                   idhppkhususmasuk,                               hpplama,                                hpp,                            rekpersediaan,                              reklawan,                               idspdetail,                                 cabang,                             lokasi,                                 gudang,                                 costcenter,                             divisi,                                 subdivisi,                              proyek,                             catatan,                        urutan,                  isclose,                               customtext1,                            customtext2,                                customtext3,                                customdbl1,                             customdbl2,                                 customdbl3,                                                 customdate1,                                                customdate2,                                                customdate3
                    strDetail &= 0 & sptField & FixDouble(idUtama) & sptField & dr1("idbarang") & sptField & FixQuotes(dr1("namabarang")) & sptField & FixQuotes(dr1("tipebarang")) & sptField & FixDouble(dr1("jmlmasuk")) & sptField & FixDouble(dr1("jmlkeluar")) & sptField & FixQuotes(dr1("satuan")) & sptField & FixDouble(dr1("nilaisatuan")) & sptField & FixDouble(dr1("jmlbarangmasuk")) & sptField & FixDouble(dr1("jmlbarangkeluar")) & sptField & FixQuotes(dr1("satuanbarang")) & sptField & FixDouble(dr1("idhppkhususmasuk")) & sptField & FixDouble(dr1("hpplama")) & sptField & FixDouble(dr1("hpp")) & sptField & FixQuotes(dr1("rekpersediaan")) & sptField & FixQuotes(dr1("reklawan")) & sptField & FixDouble(dr1("idspdetail")) & sptField & FixQuotes(dr1("cabang")) & sptField & FixQuotes(dr1("lokasi")) & sptField & FixQuotes(dr1("gudang")) & sptField & FixQuotes(dr1("costcenter")) & sptField & FixQuotes(dr1("divisi")) & sptField & FixQuotes(dr1("subdivisi")) & sptField & FixQuotes(dr1("proyek")) & sptField & FixQuotes(dr1("catatan")) & sptField & dr1("urutan") & sptField & dr1("isclose") & sptField & FixQuotes(dr1("customtext1")) & sptField & FixQuotes(dr1("customtext2")) & sptField & FixQuotes(dr1("customtext3")) & sptField & FixDouble(dr1("customdbl1")) & sptField & FixDouble(dr1("customdbl2")) & sptField & FixDouble(dr1("customdbl3")) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate3")))
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


                'PARAMETER WS
                '        WebsiteAccessKey       ★       M3_SaSimpan     ★        0      △         0     △                   △                   △        dd/MM/yyyy         △        dd/MM/yyyy H:mm:ss         ★        userId             ★       0         ★     utama           △       detail          △        batch          △           serial
                strWs = "WebsiteAccessKey" & sptParam & "M3_SaSimpan" & sptParam & 0 & sptSubParam & 0 & sptSubParam & noRef & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "0" & sptParam & strUtama & sptSubParam & strDetail & sptSubParam & strBatch & sptSubParam & strSerial


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
        SetProgress("Upload data Stock Adjustment (SA) - " & errMessage)

    End Sub

    Public Sub MyUploadM3_SaResult(ByVal data As String)

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
                    sql = "UPDATE m3_sa sa SET sa.sauploaded = 1 WHERE sa.said = '" & FixDouble(idUtama) & "'"
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
        SetProgress("Update data Stock Adjustment (SA) - " & errMessage)

        'PROSES BUAT PARAMETER DAN KIRIM WS KE PUSAT, UPLOAD TRANSAKSI DATA SELANJUTNYA
        noPerulangan += 1
        MyUploadM3_SaProcess()

    End Sub

    '=======================================================

    Public Sub MyUploadM5_SO()

        SetProgress("Start uploading Sales Order (SO)...")

        'PROSESNYA
        '1. HAPUS DATA TEMPORARY STOK KURANG --> SESUAI DATA GUDANG PADA TRANSAKSI
        '2. AMBIL DATA TRANSAKSI YANG BELUM TERUPLOAD
        '3. SET NOREF = NOTRANSAKSI LOKAL, DIKIRIM DI PARAM FILTER JUGA
        '4. UPLOAD TRANSAKSI
        '5. UPDATE STATUS TRANSAKSI SUDAH TERUPLOAD

        Dim sql As String = "", idUtama As Double = 0, noTransaksi As String = "", noRef As String = "", userId As Double = 0
        Dim dtUtama As New DataTable, dtDetail As New DataTable, dtBatch As New DataTable, dtSerial As New DataTable, dtPay As New DataTable
        Dim dtDetailCurr As New DataTable, dtBatchCurr As New DataTable, dtSerialCurr As New DataTable, dtPayCurr As New DataTable
        Dim strWs As String = "", strUtama As String = "", strDetail As String = "", strBatch As String = "", strSerial As String = "", strPay As String = ""
        Dim errMessage As String = "", wsResult As String = ""
        Dim rsWs() As String 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim rsTarget As String = "", rsSuccess As String = 0, rsMessage As String = "", rsStep As String = 0, rsId As String = 0

        Try

            'AMBIL DATA BELUM TERUPLOAD
            'AMBIL DATA UTAMA
            SetProgress("Selecting main transaction")
            sql = "SELECT so.*, '' as pesan FROM m5_so so WHERE so.souploaded = 0 AND so.sostatus = 2 ORDER BY so.soid ASC"
            dtUtama = AsDataTableAmbilDariDB(sql, strCon)
            If dtUtama.Rows.Count > 0 Then

                'AMBIL DATA DETAIL
                SetProgress("Selecting detail transaction")
                sql = "SELECT sod.* FROM m5_so so JOIN m5_so_detail sod ON so.soid = sod.idso WHERE so.souploaded = 0 AND so.sostatus = 2 ORDER BY so.soid ASC, sod.idsodetail ASC"
                dtDetail = AsDataTableAmbilDariDB(sql, strCon)


                'PERULANGAN PROSES UPLOAD DAN UPDATE STATUS UPLOADED
                'PROSES UTAMA
                SetProgress("Processing upload transaction")
                For Each drUtama As DataRow In dtUtama.Rows

                    'RESET VARIABEL
                    strUtama = "" : strDetail = "" : strBatch = "" : strSerial = "" : strPay = ""

                    'SET VARIABEL
                    sumber = FixQuotes(drUtama("sosumber"))
                    idUtama = FixDouble(drUtama("soid"))
                    noTransaksi = FixQuotes(drUtama("sonotransaksi"))
                    noRef = FixQuotes(drUtama("sonoref")) & "(" & noTransaksi & ")"
                    userId = FixDouble(drUtama("soinputuser"))

                    'MAPPING UTAMA
                    '            soid,                                  socabang,                                   solokasi,                                   sogudang,                                   soasalbarang,                       soasalbarangkategori,                                   sojenispenjualan,                       sojenispenjualankategori,                           socarabayar,                                sosumber,                           soautonotransaksi,                      sonotransaksi,                                                  sotgl,                          sokodepa,                       socustomer,                                 socustomerkontak,                                   so1alamat1,                                     so1alamat2,                                 so1alamat3,                                     so2alamat1,                                 so2alamat2,                                     so2alamat3,                         sobagianpenjualan,                                  soekspedisi,                                                    sotglkirim,                                 sotermin,                                                   sotgljatuhtempo,                                    souraian,                                   socatatan,                          sonoref,                                                sotglnoref,                                                     sotglpenutupan,                                     somatauang,                                 sokurs,                         sohargatermasukpajak,                                   sototal,                                sodiskonpersen,                                     sojmldiskon,                                    sototalpajak1detail,                                sototalpajak2detail,                                    sobiayalainpersen,                                      sobiayalain,                                sototaltransaksi,                                   sojmlbayar,                                  sorekdiskon,                                   sorekpajak1,                                    sorekpajak2,                                    sorekbiayalain,                                     sorekbayar,                     soidsq,             sostatuspl,                         sostatusdo,                         sostatusdr,                         sostatuspi,                         sostatussi,                     sostatusrnr,                        sostatussr,                         sostatus,                       sostatussebelumnya,                         sojmlrevisi,                        socetakanke,                        soinputuser,                soinputtgl,                                 somodifikasiuser,               somodifikasitgl,                            soisclose,                                  socustomtext1,                                  socustomtext2,                                  socustomtext3,                                      socustomtext4,                                  socustomtext5,                      socustomint1,                           socustomint2,                       socustomint3,                                   socustomdbl1,                                   socustomdbl2,                                   socustomdbl3,                                                   socustomdate1,                                                  socustomdate2,                                                      socustomdate3
                    strUtama = idUtama & sptField & FixQuotes(drUtama("socabang")) & sptField & FixQuotes(drUtama("solokasi")) & sptField & FixQuotes(drUtama("sogudang")) & sptField & FixQuotes(drUtama("soasalbarang")) & sptField & drUtama("soasalbarangkategori") & sptField & FixQuotes(drUtama("sojenispenjualan")) & sptField & drUtama("sojenispenjualankategori") & sptField & drUtama("socarabayar") & sptField & FixQuotes(drUtama("sosumber")) & sptField & drUtama("soautonotransaksi") & sptField & FixQuotes(noTransaksi) & sptField & FixQuotes(AsFormatTanggal(drUtama("sotgl"))) & sptField & drUtama("sokodepa") & sptField & drUtama("socustomer") & sptField & FixQuotes(drUtama("socustomerkontak")) & sptField & FixQuotes(drUtama("so1alamat1")) & sptField & FixQuotes(drUtama("so1alamat2")) & sptField & FixQuotes(drUtama("so1alamat3")) & sptField & FixQuotes(drUtama("so2alamat1")) & sptField & FixQuotes(drUtama("so2alamat2")) & sptField & FixQuotes(drUtama("so2alamat3")) & sptField & drUtama("sobagianpenjualan") & sptField & FixQuotes(drUtama("soekspedisi")) & sptField & FixQuotes(AsFormatTanggal(drUtama("sotglkirim"))) & sptField & FixQuotes(drUtama("sotermin")) & sptField & FixQuotes(AsFormatTanggal(drUtama("sotgljatuhtempo"))) & sptField & FixQuotes(drUtama("souraian")) & sptField & FixQuotes(drUtama("socatatan")) & sptField & FixQuotes(noRef) & sptField & FixQuotes(AsFormatTanggal(drUtama("sotglnoref"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("sotglpenutupan"))) & sptField & FixQuotes(drUtama("somatauang")) & sptField & FixDouble(drUtama("sokurs")) & sptField & drUtama("sohargatermasukpajak") & sptField & FixDouble(drUtama("sototal")) & sptField & FixQuotes(drUtama("sodiskonpersen")) & sptField & FixDouble(drUtama("sojmldiskon")) & sptField & FixDouble(drUtama("sototalpajak1detail")) & sptField & FixDouble(drUtama("sototalpajak2detail")) & sptField & FixDouble(drUtama("sobiayalainpersen")) & sptField & FixDouble(drUtama("sobiayalain")) & sptField & FixDouble(drUtama("sototaltransaksi")) & sptField & FixDouble(drUtama("sojmlbayar")) & sptField & FixQuotes(drUtama("sorekdiskon")) & sptField & FixQuotes(drUtama("sorekpajak1")) & sptField & FixQuotes(drUtama("sorekpajak2")) & sptField & FixQuotes(drUtama("sorekbiayalain")) & sptField & FixQuotes(drUtama("sorekbayar")) & sptField & drUtama("soidsq") & drUtama("sostatuspl") & sptField & drUtama("sostatusdo") & sptField & drUtama("sostatusdr") & sptField & drUtama("sostatuspi") & sptField & drUtama("sostatussi") & sptField & drUtama("sostatusrnr") & sptField & drUtama("sostatussr") & sptField & drUtama("sostatus") & sptField & drUtama("sostatussebelumnya") & sptField & drUtama("sojmlrevisi") & sptField & drUtama("socetakanke") & sptField & drUtama("soinputuser") & sptField & "1971-01-01 00:00:00" & sptField & drUtama("somodifikasiuser") & sptField & "1971-01-01 00:00:00" & sptField & drUtama("soisclose") & sptField & FixQuotes(drUtama("socustomtext1")) & sptField & FixQuotes(drUtama("socustomtext2")) & sptField & FixQuotes(drUtama("socustomtext3")) & sptField & FixQuotes(drUtama("socustomtext4")) & sptField & FixQuotes(drUtama("socustomtext5")) & sptField & drUtama("socustomint1") & sptField & drUtama("socustomint2") & sptField & drUtama("socustomint3") & sptField & FixDouble(drUtama("socustomdbl1")) & sptField & FixDouble(drUtama("socustomdbl2")) & sptField & FixDouble(drUtama("socustomdbl3")) & sptField & FixQuotes(AsFormatTanggal(drUtama("socustomdate1"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("socustomdate2"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("socustomdate3")))

                    'PROSES DETAIL
                    dtDetailCurr = AsDataTableFilterSortDt(dtDetail, "idso = " & idUtama)
                    For Each dr1 As DataRow In dtDetailCurr.Rows
                        strDetail = IIf(Len(strDetail) > 0, strDetail & sptRow, strDetail)

                        'MAPPING DETAIL
                        '   idsodetail,                         idso,                   idbarang,                               namabarang,                             tipebarang,                                 jml,                                satuan,                             nilaisatuan,                                jmlbarang,                              satuanbarang,                               matauang,                               kurs,                                 harga,                             diskon,                                jmldiskon,                              pajak1,                                 jmlpajak1,                              pajak2,                             jmlpajak2,                                  cabang,                             lokasi,                                 gudang,                            costcenter,                                  divisi,                             subdivisi,                              proyek,                                 catatan,                    urutan,                     idsqdetail,                     jmlpl,                  statuspl,                       jmldo,                  statusdo,                   jmldr,                              statusdr,                       jmlpi,                              statuspi,                   jmlsi,                      statussi,                   jmlrnr,                     statusrnr,                  jmlsr,                      statussr,                   isclose,                            customtext1,                                customtext2,                                customtext3,                                customdbl1,                             customdbl2,                                 customdbl3,                                             customdate1,                                                customdate2,                                                customdate3
                        strDetail &= 0 & sptField & FixDouble(idUtama) & sptField & dr1("idbarang") & sptField & FixQuotes(dr1("namabarang")) & sptField & FixQuotes(dr1("tipebarang")) & sptField & FixDouble(dr1("jml")) & sptField & FixQuotes(dr1("satuan")) & sptField & FixDouble(dr1("nilaisatuan")) & sptField & FixDouble(dr1("jmlbarang")) & sptField & FixQuotes(dr1("satuanbarang")) & sptField & FixQuotes(dr1("matauang")) & sptField & FixDouble(dr1("kurs")) & sptField & FixDouble(dr1("harga")) & sptField & FixQuotes(dr1("diskon")) & sptField & FixQuotes(dr1("jmldiskon")) & sptField & FixQuotes(dr1("pajak1")) & sptField & FixDouble(dr1("jmlpajak1")) & sptField & FixQuotes(dr1("pajak2")) & sptField & FixDouble(dr1("jmlpajak2")) & sptField & FixQuotes(dr1("cabang")) & sptField & FixQuotes(dr1("lokasi")) & sptField & FixQuotes(dr1("gudang")) & sptField & FixQuotes(dr1("costcenter")) & sptField & FixQuotes(dr1("divisi")) & sptField & FixQuotes(dr1("subdivisi")) & sptField & FixQuotes(dr1("proyek")) & sptField & FixQuotes(dr1("catatan")) & sptField & dr1("urutan") & sptField & dr1("idsqdetail") & sptField & dr1("jmlpl") & sptField & dr1("statuspl") & sptField & dr1("jmldo") & sptField & dr1("statusdo") & sptField & dr1("jmldr") & sptField & FixDouble(dr1("statusdr")) & sptField & dr1("jmlpi") & sptField & FixDouble(dr1("statuspi")) & sptField & dr1("jmlsi") & sptField & dr1("statussi") & sptField & dr1("jmlrnr") & sptField & dr1("statusrnr") & sptField & dr1("jmlsr") & sptField & dr1("statussr") & sptField & dr1("isclose") & sptField & FixQuotes(dr1("customtext1")) & sptField & FixQuotes(dr1("customtext2")) & sptField & FixQuotes(dr1("customtext3")) & sptField & FixDouble(dr1("customdbl1")) & sptField & FixDouble(dr1("customdbl2")) & sptField & FixDouble(dr1("customdbl3")) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate3")))
                    Next


                    'PARAMETER WS
                    '        WebsiteAccessKey       ★       M5_SoSimpan     ★        0      △         0     △                   △                   △        dd/MM/yyyy         △        dd/MM/yyyy H:mm:ss         ★        userId             ★       0         ★     utama           △       detail
                    strWs = "WebsiteAccessKey" & sptParam & "M5_SoSimpan" & sptParam & 0 & sptSubParam & 0 & sptSubParam & noRef & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "0" & sptParam & strUtama & sptSubParam & strDetail


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
                            sql = "UPDATE m5_so so SET so.souploaded = 1 WHERE so.soid = '" & FixDouble(idUtama) & "'"
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
                    AsDataTableTambahData(dtTransaksi, "Trans~IdTrans~NoTrans~Uploaded~Desc", sumber & "~" & idUtama & "~" & noTransaksi & "~" & rsSuccess & "~" & rsMessage)
                    'SetDGTransaksi(dtTransaksi)

                    SetProgress(noTransaksi & " : " & IIf(rsSuccess = 1, "Uploaded", "Failed") & " - " & rsMessage)

                    'JEDA 1 DETIK
                    System.Threading.Thread.Sleep(1000)

                Next

            End If

            'AMBIL JUMLAH BERHASIL UPLOAD DAN GAGAL UPLOAD
            strResultUpload &= vbCrLf & "Sales Order (SO) : " & AsDataTableDCount(dtTransaksi, "Trans = 'SO' AND Uploaded = 1") & " Uploaded, "
            strResultUpload &= AsDataTableDCount(dtTransaksi, "Trans = 'SO' AND Uploaded = 0") & " Failed"

        Catch ex As Exception

            errMessage = "Failed : " & ex.Message
            GoTo selesai

        End Try

        errMessage = "Success"

selesai:
        SetProgress("Finish Upload Sales Order (SO) - " & errMessage)

    End Sub

    Public Sub MyUploadM5_SoMain()

        'AMBIL DATA TRANSAKSI YANG BELUM TERUPLOAD
        'PANGGIL FUNGSI PROSES (BUAT PARAMETER DAN KIRIM WS)

        SetProgress("Start uploading Sales Order (SO)...")

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
            sql = "SELECT so.*, '' as pesan FROM m5_so so WHERE so.souploaded = 0 AND so.sostatus = 2 ORDER BY so.soid ASC"
            dtUtama = AsDataTableAmbilDariDB(sql, strCon)
            If dtUtama.Rows.Count > 0 Then

                'AMBIL DATA DETAIL
                SetProgress("Selecting detail transaction")
                sql = "SELECT sod.* FROM m5_so so JOIN m5_so_detail sod ON so.soid = sod.idso WHERE so.souploaded = 0 AND so.sostatus = 2 ORDER BY so.soid ASC, sod.idsodetail ASC"
                dtDetail = AsDataTableAmbilDariDB(sql, strCon)

                'PANGGIL FUNGSI PROSES (BUAT PARAMETER DAN KIRIM WS)
                SetProgress("Processing upload transaction")
                noPerulangan = 0
                MyUploadM5_SoProcess()

            End If

        Catch ex As Exception

            errMessage = "Failed : " & ex.Message
            GoTo selesai

        End Try

        errMessage = "Success"

selesai:
        SetProgress("Get data Sales Order (SO) - " & errMessage)

    End Sub

    Public Sub MyUploadM5_SoProcess()

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
                sumber = FixQuotes(drUtama("sosumber"))
                idUtama = FixDouble(drUtama("soid"))
                noTransaksi = FixQuotes(drUtama("sonotransaksi"))
                noRef = FixQuotes(drUtama("sonoref")) & "(" & noTransaksi & ")"
                userId = FixDouble(drUtama("soinputuser"))

                'MAPPING UTAMA
                '            soid,                                  socabang,                                   solokasi,                                   sogudang,                                   soasalbarang,                       soasalbarangkategori,                                   sojenispenjualan,                       sojenispenjualankategori,                           socarabayar,                                sosumber,                           soautonotransaksi,                      sonotransaksi,                                                  sotgl,                          sokodepa,                       socustomer,                                 socustomerkontak,                                   so1alamat1,                                     so1alamat2,                                 so1alamat3,                                     so2alamat1,                                 so2alamat2,                                     so2alamat3,                         sobagianpenjualan,                                  soekspedisi,                                                    sotglkirim,                                 sotermin,                                                   sotgljatuhtempo,                                    souraian,                                   socatatan,                          sonoref,                                                sotglnoref,                                                     sotglpenutupan,                                     somatauang,                                 sokurs,                         sohargatermasukpajak,                                   sototal,                                sodiskonpersen,                                     sojmldiskon,                                    sototalpajak1detail,                                sototalpajak2detail,                                    sobiayalainpersen,                                      sobiayalain,                                sototaltransaksi,                                   sojmlbayar,                                  sorekdiskon,                                   sorekpajak1,                                    sorekpajak2,                                    sorekbiayalain,                                     sorekbayar,                     soidsq,             sostatuspl,                         sostatusdo,                         sostatusdr,                         sostatuspi,                         sostatussi,                     sostatusrnr,                        sostatussr,                         sostatus,                       sostatussebelumnya,                         sojmlrevisi,                        socetakanke,                        soinputuser,                soinputtgl,                                 somodifikasiuser,               somodifikasitgl,                            soisclose,                                  socustomtext1,                                  socustomtext2,                                  socustomtext3,                                      socustomtext4,                                  socustomtext5,                      socustomint1,                           socustomint2,                       socustomint3,                                   socustomdbl1,                                   socustomdbl2,                                   socustomdbl3,                                                   socustomdate1,                                                  socustomdate2,                                                      socustomdate3
                strUtama = idUtama & sptField & FixQuotes(drUtama("socabang")) & sptField & FixQuotes(drUtama("solokasi")) & sptField & FixQuotes(drUtama("sogudang")) & sptField & FixQuotes(drUtama("soasalbarang")) & sptField & drUtama("soasalbarangkategori") & sptField & FixQuotes(drUtama("sojenispenjualan")) & sptField & drUtama("sojenispenjualankategori") & sptField & drUtama("socarabayar") & sptField & FixQuotes(drUtama("sosumber")) & sptField & drUtama("soautonotransaksi") & sptField & FixQuotes(noTransaksi) & sptField & FixQuotes(AsFormatTanggal(drUtama("sotgl"))) & sptField & drUtama("sokodepa") & sptField & drUtama("socustomer") & sptField & FixQuotes(drUtama("socustomerkontak")) & sptField & FixQuotes(drUtama("so1alamat1")) & sptField & FixQuotes(drUtama("so1alamat2")) & sptField & FixQuotes(drUtama("so1alamat3")) & sptField & FixQuotes(drUtama("so2alamat1")) & sptField & FixQuotes(drUtama("so2alamat2")) & sptField & FixQuotes(drUtama("so2alamat3")) & sptField & drUtama("sobagianpenjualan") & sptField & FixQuotes(drUtama("soekspedisi")) & sptField & FixQuotes(AsFormatTanggal(drUtama("sotglkirim"))) & sptField & FixQuotes(drUtama("sotermin")) & sptField & FixQuotes(AsFormatTanggal(drUtama("sotgljatuhtempo"))) & sptField & FixQuotes(drUtama("souraian")) & sptField & FixQuotes(drUtama("socatatan")) & sptField & FixQuotes(noRef) & sptField & FixQuotes(AsFormatTanggal(drUtama("sotglnoref"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("sotglpenutupan"))) & sptField & FixQuotes(drUtama("somatauang")) & sptField & FixDouble(drUtama("sokurs")) & sptField & drUtama("sohargatermasukpajak") & sptField & FixDouble(drUtama("sototal")) & sptField & FixQuotes(drUtama("sodiskonpersen")) & sptField & FixDouble(drUtama("sojmldiskon")) & sptField & FixDouble(drUtama("sototalpajak1detail")) & sptField & FixDouble(drUtama("sototalpajak2detail")) & sptField & FixDouble(drUtama("sobiayalainpersen")) & sptField & FixDouble(drUtama("sobiayalain")) & sptField & FixDouble(drUtama("sototaltransaksi")) & sptField & FixDouble(drUtama("sojmlbayar")) & sptField & FixQuotes(drUtama("sorekdiskon")) & sptField & FixQuotes(drUtama("sorekpajak1")) & sptField & FixQuotes(drUtama("sorekpajak2")) & sptField & FixQuotes(drUtama("sorekbiayalain")) & sptField & FixQuotes(drUtama("sorekbayar")) & sptField & drUtama("soidsq") & drUtama("sostatuspl") & sptField & drUtama("sostatusdo") & sptField & drUtama("sostatusdr") & sptField & drUtama("sostatuspi") & sptField & drUtama("sostatussi") & sptField & drUtama("sostatusrnr") & sptField & drUtama("sostatussr") & sptField & drUtama("sostatus") & sptField & drUtama("sostatussebelumnya") & sptField & drUtama("sojmlrevisi") & sptField & drUtama("socetakanke") & sptField & drUtama("soinputuser") & sptField & "1971-01-01 00:00:00" & sptField & drUtama("somodifikasiuser") & sptField & "1971-01-01 00:00:00" & sptField & drUtama("soisclose") & sptField & FixQuotes(drUtama("socustomtext1")) & sptField & FixQuotes(drUtama("socustomtext2")) & sptField & FixQuotes(drUtama("socustomtext3")) & sptField & FixQuotes(drUtama("socustomtext4")) & sptField & FixQuotes(drUtama("socustomtext5")) & sptField & drUtama("socustomint1") & sptField & drUtama("socustomint2") & sptField & drUtama("socustomint3") & sptField & FixDouble(drUtama("socustomdbl1")) & sptField & FixDouble(drUtama("socustomdbl2")) & sptField & FixDouble(drUtama("socustomdbl3")) & sptField & FixQuotes(AsFormatTanggal(drUtama("socustomdate1"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("socustomdate2"))) & sptField & FixQuotes(AsFormatTanggal(drUtama("socustomdate3")))

                'PROSES DETAIL
                dtDetailCurr = AsDataTableFilterSortDt(dtDetail, "idso = " & idUtama)
                For Each dr1 As DataRow In dtDetailCurr.Rows
                    strDetail = IIf(Len(strDetail) > 0, strDetail & sptRow, strDetail)

                    'MAPPING DETAIL
                    '   idsodetail,                         idso,                   idbarang,                               namabarang,                             tipebarang,                                 jml,                                satuan,                             nilaisatuan,                                jmlbarang,                              satuanbarang,                               matauang,                               kurs,                                 harga,                             diskon,                                jmldiskon,                              pajak1,                                 jmlpajak1,                              pajak2,                             jmlpajak2,                                  cabang,                             lokasi,                                 gudang,                            costcenter,                                  divisi,                             subdivisi,                              proyek,                                 catatan,                    urutan,                     idsqdetail,                     jmlpl,                  statuspl,                       jmldo,                  statusdo,                   jmldr,                              statusdr,                       jmlpi,                              statuspi,                   jmlsi,                      statussi,                   jmlrnr,                     statusrnr,                  jmlsr,                      statussr,                   isclose,                            customtext1,                                customtext2,                                customtext3,                                customdbl1,                             customdbl2,                                 customdbl3,                                             customdate1,                                                customdate2,                                                customdate3
                    strDetail &= 0 & sptField & FixDouble(idUtama) & sptField & dr1("idbarang") & sptField & FixQuotes(dr1("namabarang")) & sptField & FixQuotes(dr1("tipebarang")) & sptField & FixDouble(dr1("jml")) & sptField & FixQuotes(dr1("satuan")) & sptField & FixDouble(dr1("nilaisatuan")) & sptField & FixDouble(dr1("jmlbarang")) & sptField & FixQuotes(dr1("satuanbarang")) & sptField & FixQuotes(dr1("matauang")) & sptField & FixDouble(dr1("kurs")) & sptField & FixDouble(dr1("harga")) & sptField & FixQuotes(dr1("diskon")) & sptField & FixQuotes(dr1("jmldiskon")) & sptField & FixQuotes(dr1("pajak1")) & sptField & FixDouble(dr1("jmlpajak1")) & sptField & FixQuotes(dr1("pajak2")) & sptField & FixDouble(dr1("jmlpajak2")) & sptField & FixQuotes(dr1("cabang")) & sptField & FixQuotes(dr1("lokasi")) & sptField & FixQuotes(dr1("gudang")) & sptField & FixQuotes(dr1("costcenter")) & sptField & FixQuotes(dr1("divisi")) & sptField & FixQuotes(dr1("subdivisi")) & sptField & FixQuotes(dr1("proyek")) & sptField & FixQuotes(dr1("catatan")) & sptField & dr1("urutan") & sptField & dr1("idsqdetail") & sptField & dr1("jmlpl") & sptField & dr1("statuspl") & sptField & dr1("jmldo") & sptField & dr1("statusdo") & sptField & dr1("jmldr") & sptField & FixDouble(dr1("statusdr")) & sptField & dr1("jmlpi") & sptField & FixDouble(dr1("statuspi")) & sptField & dr1("jmlsi") & sptField & dr1("statussi") & sptField & dr1("jmlrnr") & sptField & dr1("statusrnr") & sptField & dr1("jmlsr") & sptField & dr1("statussr") & sptField & dr1("isclose") & sptField & FixQuotes(dr1("customtext1")) & sptField & FixQuotes(dr1("customtext2")) & sptField & FixQuotes(dr1("customtext3")) & sptField & FixDouble(dr1("customdbl1")) & sptField & FixDouble(dr1("customdbl2")) & sptField & FixDouble(dr1("customdbl3")) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate1"))) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate2"))) & sptField & FixQuotes(AsFormatTanggal(dr1("customdate3")))
                Next


                'PARAMETER WS
                '        WebsiteAccessKey       ★       M5_SoSimpan     ★        0      △         0     △                   △                   △        dd/MM/yyyy         △        dd/MM/yyyy H:mm:ss         ★        userId             ★       0         ★     utama           △       detail
                strWs = "WebsiteAccessKey" & sptParam & "M5_SoSimpan" & sptParam & 0 & sptSubParam & 0 & sptSubParam & noRef & sptSubParam & "" & sptSubParam & "dd/MM/yyyy" & sptSubParam & "dd/MM/yyyy H:mm:ss" & sptParam & FixDouble(userId) & sptParam & "0" & sptParam & strUtama & sptSubParam & strDetail


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
        SetProgress("Upload data Sales Order (SO) - " & errMessage)

    End Sub

    Public Sub MyUploadM5_SoResult(ByVal data As String)

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
                    sql = "UPDATE m5_so so SET so.souploaded = 1 WHERE so.soid = '" & FixDouble(idUtama) & "'"
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
        SetProgress("Update data Sales Order (SO) - " & errMessage)

        'PROSES BUAT PARAMETER DAN KIRIM WS KE PUSAT, UPLOAD TRANSAKSI DATA SELANJUTNYA
        noPerulangan += 1
        MyUploadM3_SpProcess()

    End Sub

    '=======================================================

    Public Sub MyUploadM5_SI()

        SetProgress("Start uploading Sales Invoice (SI)...")

        'PROSESNYA
        '1. HAPUS DATA TEMPORARY STOK KURANG --> SESUAI DATA GUDANG PADA TRANSAKSI
        '2. AMBIL DATA TRANSAKSI YANG BELUM TERUPLOAD
        '3. SET NOREF = NOTRANSAKSI LOKAL, DIKIRIM DI PARAM FILTER JUGA
        '4. UPLOAD TRANSAKSI
        '5. UPDATE STATUS TRANSAKSI SUDAH TERUPLOAD

        Dim sql As String = "", idUtama As Double = 0, noTransaksi As String = "", noRef As String = "", userId As Double = 0
        Dim dtUtama As New DataTable, dtDetail As New DataTable, dtBatch As New DataTable, dtSerial As New DataTable, dtPay As New DataTable
        Dim dtDetailCurr As New DataTable, dtBatchCurr As New DataTable, dtSerialCurr As New DataTable, dtPayCurr As New DataTable
        Dim strWs As String = "", strUtama As String = "", strDetail As String = "", strBatch As String = "", strSerial As String = "", strPay As String = ""
        Dim errMessage As String = "", wsResult As String = ""
        Dim rsWs() As String 'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim rsTarget As String = "", rsSuccess As String = 0, rsMessage As String = "", rsStep As String = 0, rsId As String = 0

        Try

            'AMBIL DATA BELUM TERUPLOAD
            'AMBIL DATA UTAMA
            SetProgress("Selecting main transaction")
            sql = "SELECT si.*, '' as pesan FROM m5_si si WHERE si.siuploaded = 0 AND si.sistatus = 2 ORDER BY si.siid ASC"
            dtUtama = AsDataTableAmbilDariDB(sql, strCon)
            If dtUtama.Rows.Count > 0 Then

                'AMBIL DATA DETAIL
                SetProgress("Selecting detail transaction")
                sql = "SELECT sid.* FROM m5_si si JOIN m5_si_detail sid ON si.siid = sid.idsi WHERE si.siuploaded = 0 AND si.sistatus = 2 ORDER BY si.siid ASC, sid.idsidetail ASC"
                dtDetail = AsDataTableAmbilDariDB(sql, strCon)

                'AMBIL DATA BATCH
                SetProgress("Selecting batch transaction")
                sql = "SELECT nbt.* FROM m5_si si JOIN m1_no_batch_transaction nbt ON si.sisumber = nbt.nbtsumber AND si.siid = nbt.nbtidtransaksi WHERE si.siuploaded = 0 AND si.sistatus = 2 ORDER BY si.siid ASC, nbt.nbtid ASC"
                dtBatch = AsDataTableAmbilDariDB(sql, strCon)

                'AMBIL DATA SERIAL
                SetProgress("Selecting serial transaction")
                sql = "SELECT nst.* FROM m5_si si JOIN m1_no_serial_transaction nst ON si.sisumber = nst.nstsumber AND si.siid = nst.nstidtransaksi WHERE si.siuploaded = 0 AND si.sistatus = 2 ORDER BY si.siid ASC, nst.nstid ASC"
                dtSerial = AsDataTableAmbilDariDB(sql, strCon)

                'AMBIL DATA PAY
                SetProgress("Selecting payment transaction")
                sql = "SELECT sip.* FROM m5_si si JOIN m5_si_pay sip ON si.siid = sip.idsi WHERE si.siuploaded = 0 AND si.sistatus = 2 ORDER BY si.siid ASC, sip.idsicarabayar ASC"
                dtPay = AsDataTableAmbilDariDB(sql, strCon)

                'PERULANGAN PROSES UPLOAD DAN UPDATE STATUS UPLOADED
                'PROSES UTAMA
                SetProgress("Processing upload transaction")
                For Each drUtama As DataRow In dtUtama.Rows

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
                    AsDataTableTambahData(dtTransaksi, "Trans~IdTrans~NoTrans~Uploaded~Desc", sumber & "~" & idUtama & "~" & noTransaksi & "~" & rsSuccess & "~" & rsMessage)
                    'SetDGTransaksi(dtTransaksi)

                    SetProgress(noTransaksi & " : " & IIf(rsSuccess = 1, "Uploaded", "Failed") & " - " & rsMessage)

                    'JEDA 1 DETIK
                    System.Threading.Thread.Sleep(1000)

                Next

            End If

            'AMBIL JUMLAH BERHASIL UPLOAD DAN GAGAL UPLOAD
            strResultUpload &= vbCrLf & "Sales Invoice (SI) : " & AsDataTableDCount(dtTransaksi, "Trans = 'SI' AND Uploaded = 1") & " Uploaded, "
            strResultUpload &= AsDataTableDCount(dtTransaksi, "Trans = 'SI' AND Uploaded = 0") & " Failed"

        Catch ex As Exception

            errMessage = "Failed : " & ex.Message
            GoTo selesai

        End Try

        errMessage = "Success"

selesai:
        SetProgress("Finish Upload Sales Invoice (SI) - " & errMessage)

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
            sql = "SELECT si.*, '' as pesan FROM m5_si si WHERE si.siuploaded = 0 AND si.sistatus = 2 ORDER BY si.siid ASC"
            dtUtama = AsDataTableAmbilDariDB(sql, strCon)
            If dtUtama.Rows.Count > 0 Then

                'AMBIL DATA DETAIL
                SetProgress("Selecting detail transaction")
                sql = "SELECT sid.* FROM m5_si si JOIN m5_si_detail sid ON si.siid = sid.idsi WHERE si.siuploaded = 0 AND si.sistatus = 2 ORDER BY si.siid ASC, sid.idsidetail ASC"
                dtDetail = AsDataTableAmbilDariDB(sql, strCon)

                'AMBIL DATA BATCH
                SetProgress("Selecting batch transaction")
                sql = "SELECT nbt.* FROM m5_si si JOIN m1_no_batch_transaction nbt ON si.sisumber = nbt.nbtsumber AND si.siid = nbt.nbtidtransaksi WHERE si.siuploaded = 0 AND si.sistatus = 2 ORDER BY si.siid ASC, nbt.nbtid ASC"
                dtBatch = AsDataTableAmbilDariDB(sql, strCon)

                'AMBIL DATA SERIAL
                SetProgress("Selecting serial transaction")
                sql = "SELECT nst.* FROM m5_si si JOIN m1_no_serial_transaction nst ON si.sisumber = nst.nstsumber AND si.siid = nst.nstidtransaksi WHERE si.siuploaded = 0 AND si.sistatus = 2 ORDER BY si.siid ASC, nst.nstid ASC"
                dtSerial = AsDataTableAmbilDariDB(sql, strCon)

                'AMBIL DATA PAY
                SetProgress("Selecting payment transaction")
                sql = "SELECT sip.* FROM m5_si si JOIN m5_si_pay sip ON si.siid = sip.idsi WHERE si.siuploaded = 0 AND si.sistatus = 2 ORDER BY si.siid ASC, sip.idsicarabayar ASC"
                dtPay = AsDataTableAmbilDariDB(sql, strCon)

                'PANGGIL FUNGSI PROSES (BUAT PARAMETER DAN KIRIM WS)
                SetProgress("Processing upload transaction")
                noPerulangan = 0
                MyUploadM5_SiProcess()

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
