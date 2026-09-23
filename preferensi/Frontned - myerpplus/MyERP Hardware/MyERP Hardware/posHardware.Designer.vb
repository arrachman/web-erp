<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class POS_Hardware
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(POS_Hardware))
        Me.ntfHardware = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.btstart = New System.Windows.Forms.Button()
        Me.btnstop = New System.Windows.Forms.Button()
        Me.dgAntri = New System.Windows.Forms.DataGridView()
        Me.dgPrinted = New System.Windows.Forms.DataGridView()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtport = New System.Windows.Forms.TextBox()
        Me.btnsave = New System.Windows.Forms.Button()
        CType(Me.dgAntri, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.dgPrinted, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'ntfHardware
        '
        Me.ntfHardware.BalloonTipText = "Application Minimized."
        Me.ntfHardware.BalloonTipTitle = "MyERP Hardware"
        Me.ntfHardware.Icon = CType(resources.GetObject("ntfHardware.Icon"), System.Drawing.Icon)
        Me.ntfHardware.Text = "POS Hardware"
        Me.ntfHardware.Visible = True
        '
        'btstart
        '
        Me.btstart.Location = New System.Drawing.Point(183, 8)
        Me.btstart.Name = "btstart"
        Me.btstart.Size = New System.Drawing.Size(75, 23)
        Me.btstart.TabIndex = 3
        Me.btstart.Text = "Start"
        Me.btstart.UseVisualStyleBackColor = True
        '
        'btnstop
        '
        Me.btnstop.Location = New System.Drawing.Point(257, 8)
        Me.btnstop.Name = "btnstop"
        Me.btnstop.Size = New System.Drawing.Size(75, 23)
        Me.btnstop.TabIndex = 4
        Me.btnstop.Text = "Stop"
        Me.btnstop.UseVisualStyleBackColor = True
        '
        'dgAntri
        '
        Me.dgAntri.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgAntri.Location = New System.Drawing.Point(9, 42)
        Me.dgAntri.Name = "dgAntri"
        Me.dgAntri.Size = New System.Drawing.Size(666, 153)
        Me.dgAntri.TabIndex = 5
        '
        'dgPrinted
        '
        Me.dgPrinted.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgPrinted.Location = New System.Drawing.Point(9, 201)
        Me.dgPrinted.Name = "dgPrinted"
        Me.dgPrinted.Size = New System.Drawing.Size(666, 267)
        Me.dgPrinted.TabIndex = 6
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(13, 13)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(32, 13)
        Me.Label1.TabIndex = 7
        Me.Label1.Text = "Port :"
        '
        'txtport
        '
        Me.txtport.Location = New System.Drawing.Point(46, 10)
        Me.txtport.Name = "txtport"
        Me.txtport.Size = New System.Drawing.Size(50, 20)
        Me.txtport.TabIndex = 1
        '
        'btnsave
        '
        Me.btnsave.Location = New System.Drawing.Point(102, 8)
        Me.btnsave.Name = "btnsave"
        Me.btnsave.Size = New System.Drawing.Size(75, 23)
        Me.btnsave.TabIndex = 2
        Me.btnsave.Text = "Save"
        Me.btnsave.UseVisualStyleBackColor = True
        '
        'POS_Hardware
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(687, 480)
        Me.Controls.Add(Me.btnsave)
        Me.Controls.Add(Me.txtport)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.dgPrinted)
        Me.Controls.Add(Me.dgAntri)
        Me.Controls.Add(Me.btnstop)
        Me.Controls.Add(Me.btstart)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "POS_Hardware"
        Me.ShowInTaskbar = False
        Me.Text = "POS Hardware"
        Me.WindowState = System.Windows.Forms.FormWindowState.Minimized
        CType(Me.dgAntri, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.dgPrinted, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ntfHardware As System.Windows.Forms.NotifyIcon
    Friend WithEvents btstart As System.Windows.Forms.Button
    Friend WithEvents btnstop As System.Windows.Forms.Button
    Friend WithEvents dgAntri As System.Windows.Forms.DataGridView
    Friend WithEvents dgPrinted As System.Windows.Forms.DataGridView
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtport As System.Windows.Forms.TextBox
    Friend WithEvents btnsave As System.Windows.Forms.Button

End Class
