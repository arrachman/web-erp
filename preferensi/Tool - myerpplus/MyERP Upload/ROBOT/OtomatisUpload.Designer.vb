<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class formOtomatisUpload
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(formOtomatisUpload))
        Me.txtlog = New System.Windows.Forms.TextBox()
        Me.btnstart = New System.Windows.Forms.Button()
        Me.btnstop = New System.Windows.Forms.Button()
        Me.btnuploadnow = New System.Windows.Forms.Button()
        Me.txtdurasi = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtjam = New System.Windows.Forms.TextBox()
        Me.cbxDurasi = New System.Windows.Forms.CheckBox()
        Me.cbxjam = New System.Windows.Forms.CheckBox()
        Me.lblclearlog = New System.Windows.Forms.Label()
        Me.ntfOtomatisUpload = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.dgTransaksi = New System.Windows.Forms.DataGridView()
        Me.btndownloadnow = New System.Windows.Forms.Button()
        CType(Me.dgTransaksi, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'txtlog
        '
        Me.txtlog.AcceptsReturn = True
        Me.txtlog.Location = New System.Drawing.Point(12, 87)
        Me.txtlog.Multiline = True
        Me.txtlog.Name = "txtlog"
        Me.txtlog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtlog.Size = New System.Drawing.Size(431, 487)
        Me.txtlog.TabIndex = 4
        '
        'btnstart
        '
        Me.btnstart.Location = New System.Drawing.Point(252, 10)
        Me.btnstart.Name = "btnstart"
        Me.btnstart.Size = New System.Drawing.Size(91, 23)
        Me.btnstart.TabIndex = 5
        Me.btnstart.Text = "Start"
        Me.btnstart.UseVisualStyleBackColor = True
        '
        'btnstop
        '
        Me.btnstop.Location = New System.Drawing.Point(349, 10)
        Me.btnstop.Name = "btnstop"
        Me.btnstop.Size = New System.Drawing.Size(91, 23)
        Me.btnstop.TabIndex = 6
        Me.btnstop.Text = "Stop"
        Me.btnstop.UseVisualStyleBackColor = True
        '
        'btnuploadnow
        '
        Me.btnuploadnow.Location = New System.Drawing.Point(252, 39)
        Me.btnuploadnow.Name = "btnuploadnow"
        Me.btnuploadnow.Size = New System.Drawing.Size(91, 23)
        Me.btnuploadnow.TabIndex = 8
        Me.btnuploadnow.Text = "Upload Now"
        Me.btnuploadnow.UseVisualStyleBackColor = True
        '
        'txtdurasi
        '
        Me.txtdurasi.Location = New System.Drawing.Point(76, 10)
        Me.txtdurasi.Name = "txtdurasi"
        Me.txtdurasi.Size = New System.Drawing.Size(100, 20)
        Me.txtdurasi.TabIndex = 9
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(182, 13)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(33, 13)
        Me.Label3.TabIndex = 10
        Me.Label3.Text = "Menit"
        '
        'txtjam
        '
        Me.txtjam.Location = New System.Drawing.Point(76, 36)
        Me.txtjam.Name = "txtjam"
        Me.txtjam.Size = New System.Drawing.Size(100, 20)
        Me.txtjam.TabIndex = 12
        '
        'cbxDurasi
        '
        Me.cbxDurasi.AutoSize = True
        Me.cbxDurasi.Location = New System.Drawing.Point(12, 11)
        Me.cbxDurasi.Name = "cbxDurasi"
        Me.cbxDurasi.Size = New System.Drawing.Size(62, 17)
        Me.cbxDurasi.TabIndex = 14
        Me.cbxDurasi.Text = "Durasi :"
        Me.cbxDurasi.UseVisualStyleBackColor = True
        '
        'cbxjam
        '
        Me.cbxjam.AutoSize = True
        Me.cbxjam.Location = New System.Drawing.Point(12, 38)
        Me.cbxjam.Name = "cbxjam"
        Me.cbxjam.Size = New System.Drawing.Size(51, 17)
        Me.cbxjam.TabIndex = 15
        Me.cbxjam.Text = "Jam :"
        Me.cbxjam.UseVisualStyleBackColor = True
        '
        'lblclearlog
        '
        Me.lblclearlog.AutoSize = True
        Me.lblclearlog.Location = New System.Drawing.Point(388, 70)
        Me.lblclearlog.Name = "lblclearlog"
        Me.lblclearlog.Size = New System.Drawing.Size(52, 13)
        Me.lblclearlog.TabIndex = 16
        Me.lblclearlog.Text = "Clear Log"
        '
        'ntfOtomatisUpload
        '
        Me.ntfOtomatisUpload.BalloonTipText = "Application Minimized."
        Me.ntfOtomatisUpload.BalloonTipTitle = "MyERP - Otomatis Upload"
        Me.ntfOtomatisUpload.Icon = CType(resources.GetObject("ntfOtomatisUpload.Icon"), System.Drawing.Icon)
        Me.ntfOtomatisUpload.Text = "MyERP - Otomatis Upload"
        Me.ntfOtomatisUpload.Visible = True
        '
        'dgTransaksi
        '
        Me.dgTransaksi.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgTransaksi.Location = New System.Drawing.Point(12, 580)
        Me.dgTransaksi.Name = "dgTransaksi"
        Me.dgTransaksi.ReadOnly = True
        Me.dgTransaksi.Size = New System.Drawing.Size(428, 20)
        Me.dgTransaksi.TabIndex = 17
        '
        'btndownloadnow
        '
        Me.btndownloadnow.Location = New System.Drawing.Point(349, 39)
        Me.btndownloadnow.Name = "btndownloadnow"
        Me.btndownloadnow.Size = New System.Drawing.Size(91, 23)
        Me.btndownloadnow.TabIndex = 18
        Me.btndownloadnow.Text = "Download Now"
        Me.btndownloadnow.UseVisualStyleBackColor = True
        '
        'formOtomatisUpload
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(455, 612)
        Me.Controls.Add(Me.btndownloadnow)
        Me.Controls.Add(Me.dgTransaksi)
        Me.Controls.Add(Me.lblclearlog)
        Me.Controls.Add(Me.cbxjam)
        Me.Controls.Add(Me.cbxDurasi)
        Me.Controls.Add(Me.txtjam)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtdurasi)
        Me.Controls.Add(Me.btnuploadnow)
        Me.Controls.Add(Me.btnstop)
        Me.Controls.Add(Me.btnstart)
        Me.Controls.Add(Me.txtlog)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "formOtomatisUpload"
        Me.Text = "MyERP - Otomatis Upload"
        CType(Me.dgTransaksi, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtlog As System.Windows.Forms.TextBox
    Friend WithEvents btnstart As System.Windows.Forms.Button
    Friend WithEvents btnstop As System.Windows.Forms.Button
    Friend WithEvents btnuploadnow As System.Windows.Forms.Button
    Friend WithEvents txtdurasi As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents txtjam As System.Windows.Forms.TextBox
    Friend WithEvents cbxDurasi As System.Windows.Forms.CheckBox
    Friend WithEvents cbxjam As System.Windows.Forms.CheckBox
    Friend WithEvents lblclearlog As System.Windows.Forms.Label
    Friend WithEvents ntfOtomatisUpload As System.Windows.Forms.NotifyIcon
    Friend WithEvents dgTransaksi As System.Windows.Forms.DataGridView
    Friend WithEvents btndownloadnow As System.Windows.Forms.Button

End Class
