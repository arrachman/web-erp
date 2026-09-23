<System.ComponentModel.RunInstaller(True)> Partial Class ProjectInstaller
    Inherits System.Configuration.Install.Installer

    'Installer overrides dispose to clean up the component list.
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

    'Required by the Component Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Component Designer
    'It can be modified using the Component Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.LisensiSementaraInstaller = New System.ServiceProcess.ServiceInstaller()
        Me.ServiceProcessInstaller = New System.ServiceProcess.ServiceProcessInstaller()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        '
        'LisensiSementaraInstaller
        '
        Me.LisensiSementaraInstaller.DelayedAutoStart = True
        Me.LisensiSementaraInstaller.Description = "LisensiSementara Tool Manager"
        Me.LisensiSementaraInstaller.DisplayName = "UserControlDistributor - 1.0.0"
        Me.LisensiSementaraInstaller.ServiceName = "UserControlMMM2014 - 1.0.0"
        Me.LisensiSementaraInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic
        '
        'ServiceProcessInstaller
        '
        Me.ServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem
        Me.ServiceProcessInstaller.Password = Nothing
        Me.ServiceProcessInstaller.Username = Nothing
        '
        'Timer1
        '
        '
        'ProjectInstaller
        '
        Me.Installers.AddRange(New System.Configuration.Install.Installer() {Me.ServiceProcessInstaller, Me.LisensiSementaraInstaller})

    End Sub
    Friend WithEvents LisensiSementaraInstaller As System.ServiceProcess.ServiceInstaller
    Friend WithEvents ServiceProcessInstaller As System.ServiceProcess.ServiceProcessInstaller
    Friend WithEvents Timer1 As System.Windows.Forms.Timer

End Class
