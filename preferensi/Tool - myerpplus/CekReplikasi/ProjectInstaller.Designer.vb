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
        Me.QueueInstaller = New System.ServiceProcess.ServiceInstaller()
        Me.ServiceProcessInstaller = New System.ServiceProcess.ServiceProcessInstaller()
        '
        'QueueInstaller
        '
        Me.QueueInstaller.DelayedAutoStart = True
        Me.QueueInstaller.Description = "Cek Replikasi"
        Me.QueueInstaller.DisplayName = "Cek Replikasi"
        Me.QueueInstaller.ServiceName = "Cek Replikasi"
        Me.QueueInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic
        '
        'ServiceProcessInstaller
        '
        Me.ServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem
        Me.ServiceProcessInstaller.Password = Nothing
        Me.ServiceProcessInstaller.Username = Nothing
        '
        'ProjectInstaller
        '
        Me.Installers.AddRange(New System.Configuration.Install.Installer() {Me.QueueInstaller, Me.ServiceProcessInstaller})

    End Sub
    Friend WithEvents QueueInstaller As System.ServiceProcess.ServiceInstaller
    Friend WithEvents ServiceProcessInstaller As System.ServiceProcess.ServiceProcessInstaller

End Class
