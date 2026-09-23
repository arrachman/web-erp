<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.txturl = New System.Windows.Forms.TextBox()
        Me.txtnamecompany = New System.Windows.Forms.TextBox()
        Me.txtkodeapp = New System.Windows.Forms.TextBox()
        Me.txtsqlservicename = New System.Windows.Forms.TextBox()
        Me.txtserver = New System.Windows.Forms.TextBox()
        Me.txtdatabase = New System.Windows.Forms.TextBox()
        Me.txtuser = New System.Windows.Forms.TextBox()
        Me.txtpassword = New System.Windows.Forms.TextBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.btngenerate = New System.Windows.Forms.Button()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.txtdirectoryapp = New System.Windows.Forms.TextBox()
        Me.btndirectoryapp = New System.Windows.Forms.Button()
        Me.FolderBrowserApp = New System.Windows.Forms.FolderBrowserDialog()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(11, 44)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(35, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "URL :"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(11, 69)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(88, 13)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Company Name :"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(11, 95)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(60, 13)
        Me.Label3.TabIndex = 2
        Me.Label3.Text = "Kode App :"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(12, 142)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(98, 13)
        Me.Label4.TabIndex = 3
        Me.Label4.Text = "Sql Service Name :"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(12, 169)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(44, 13)
        Me.Label5.TabIndex = 4
        Me.Label5.Text = "Server :"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(12, 195)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(59, 13)
        Me.Label6.TabIndex = 5
        Me.Label6.Text = "Database :"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(12, 221)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(35, 13)
        Me.Label7.TabIndex = 6
        Me.Label7.Text = "User :"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(12, 246)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(59, 13)
        Me.Label8.TabIndex = 7
        Me.Label8.Text = "Password :"
        '
        'txturl
        '
        Me.txturl.Location = New System.Drawing.Point(176, 40)
        Me.txturl.Name = "txturl"
        Me.txturl.Size = New System.Drawing.Size(161, 20)
        Me.txturl.TabIndex = 3
        '
        'txtnamecompany
        '
        Me.txtnamecompany.Location = New System.Drawing.Point(129, 67)
        Me.txtnamecompany.Name = "txtnamecompany"
        Me.txtnamecompany.Size = New System.Drawing.Size(208, 20)
        Me.txtnamecompany.TabIndex = 4
        '
        'txtkodeapp
        '
        Me.txtkodeapp.Location = New System.Drawing.Point(129, 93)
        Me.txtkodeapp.Name = "txtkodeapp"
        Me.txtkodeapp.Size = New System.Drawing.Size(76, 20)
        Me.txtkodeapp.TabIndex = 5
        '
        'txtsqlservicename
        '
        Me.txtsqlservicename.Location = New System.Drawing.Point(130, 141)
        Me.txtsqlservicename.Name = "txtsqlservicename"
        Me.txtsqlservicename.Size = New System.Drawing.Size(38, 20)
        Me.txtsqlservicename.TabIndex = 11
        Me.txtsqlservicename.Text = "mysql"
        '
        'txtserver
        '
        Me.txtserver.Location = New System.Drawing.Point(130, 167)
        Me.txtserver.Name = "txtserver"
        Me.txtserver.Size = New System.Drawing.Size(76, 20)
        Me.txtserver.TabIndex = 6
        Me.txtserver.Text = "127.0.0.1"
        '
        'txtdatabase
        '
        Me.txtdatabase.Location = New System.Drawing.Point(130, 193)
        Me.txtdatabase.Name = "txtdatabase"
        Me.txtdatabase.Size = New System.Drawing.Size(76, 20)
        Me.txtdatabase.TabIndex = 7
        '
        'txtuser
        '
        Me.txtuser.Location = New System.Drawing.Point(130, 219)
        Me.txtuser.Name = "txtuser"
        Me.txtuser.Size = New System.Drawing.Size(76, 20)
        Me.txtuser.TabIndex = 8
        Me.txtuser.Text = "myerpplus"
        '
        'txtpassword
        '
        Me.txtpassword.Location = New System.Drawing.Point(130, 245)
        Me.txtpassword.Name = "txtpassword"
        Me.txtpassword.Size = New System.Drawing.Size(76, 20)
        Me.txtpassword.TabIndex = 9
        Me.txtpassword.Text = "myerpplus"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(129, 44)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(38, 13)
        Me.Label9.TabIndex = 16
        Me.Label9.Text = "http://"
        '
        'btngenerate
        '
        Me.btngenerate.Location = New System.Drawing.Point(227, 219)
        Me.btngenerate.Name = "btngenerate"
        Me.btngenerate.Size = New System.Drawing.Size(110, 46)
        Me.btngenerate.TabIndex = 10
        Me.btngenerate.Text = "Generate"
        Me.btngenerate.UseVisualStyleBackColor = True
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label10.Location = New System.Drawing.Point(9, 118)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(113, 13)
        Me.Label10.TabIndex = 18
        Me.Label10.Text = "Database Setting :"
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(12, 9)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(77, 13)
        Me.Label11.TabIndex = 19
        Me.Label11.Text = "Directory App :"
        '
        'txtdirectoryapp
        '
        Me.txtdirectoryapp.Location = New System.Drawing.Point(130, 9)
        Me.txtdirectoryapp.Name = "txtdirectoryapp"
        Me.txtdirectoryapp.Size = New System.Drawing.Size(146, 20)
        Me.txtdirectoryapp.TabIndex = 2
        '
        'btndirectoryapp
        '
        Me.btndirectoryapp.Location = New System.Drawing.Point(282, 6)
        Me.btndirectoryapp.Name = "btndirectoryapp"
        Me.btndirectoryapp.Size = New System.Drawing.Size(55, 25)
        Me.btndirectoryapp.TabIndex = 1
        Me.btndirectoryapp.Text = "Browse"
        Me.btndirectoryapp.UseVisualStyleBackColor = True
        '
        'Panel1
        '
        Me.Panel1.Controls.Add(Me.Label13)
        Me.Panel1.Controls.Add(Me.Label12)
        Me.Panel1.Controls.Add(Me.TextBox1)
        Me.Panel1.Location = New System.Drawing.Point(6, 6)
        Me.Panel1.Name = "Panel1"
        Me.Panel1.Size = New System.Drawing.Size(331, 259)
        Me.Panel1.TabIndex = 20
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Location = New System.Drawing.Point(107, 37)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(0, 13)
        Me.Label13.TabIndex = 2
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(107, 18)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(109, 13)
        Me.Label12.TabIndex = 1
        Me.Label12.Text = "MyERP Plus Premium"
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(29, 35)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.PasswordChar = Global.Microsoft.VisualBasic.ChrW(42)
        Me.TextBox1.Size = New System.Drawing.Size(275, 20)
        Me.TextBox1.TabIndex = 0
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(348, 277)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.btndirectoryapp)
        Me.Controls.Add(Me.txtdirectoryapp)
        Me.Controls.Add(Me.Label11)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.btngenerate)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.txtpassword)
        Me.Controls.Add(Me.txtuser)
        Me.Controls.Add(Me.txtdatabase)
        Me.Controls.Add(Me.txtserver)
        Me.Controls.Add(Me.txtsqlservicename)
        Me.Controls.Add(Me.txtkodeapp)
        Me.Controls.Add(Me.txtnamecompany)
        Me.Controls.Add(Me.txturl)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Name = "Form1"
        Me.Text = "Connection "
        Me.Panel1.ResumeLayout(False)
        Me.Panel1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents txturl As System.Windows.Forms.TextBox
    Friend WithEvents txtnamecompany As System.Windows.Forms.TextBox
    Friend WithEvents txtkodeapp As System.Windows.Forms.TextBox
    Friend WithEvents txtsqlservicename As System.Windows.Forms.TextBox
    Friend WithEvents txtserver As System.Windows.Forms.TextBox
    Friend WithEvents txtdatabase As System.Windows.Forms.TextBox
    Friend WithEvents txtuser As System.Windows.Forms.TextBox
    Friend WithEvents txtpassword As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents btngenerate As System.Windows.Forms.Button
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents txtdirectoryapp As System.Windows.Forms.TextBox
    Friend WithEvents btndirectoryapp As System.Windows.Forms.Button
    Friend WithEvents FolderBrowserApp As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox

End Class
