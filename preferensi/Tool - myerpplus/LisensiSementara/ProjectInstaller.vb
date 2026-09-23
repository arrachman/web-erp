Imports System.ComponentModel
Imports System.Configuration.Install
Imports System.ServiceProcess

Public Class ProjectInstaller

    Public Sub New()
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()
        Timer1.Start()
        'Add initialization code after the call to InitializeComponent

    End Sub

    Protected Overrides Sub OnAfterInstall(ByVal savedState As System.Collections.IDictionary)
        MyBase.OnAfterInstall(savedState)
        Using serviceController = New ServiceController(Me.LisensiSementaraInstaller.ServiceName, Environment.MachineName)
            serviceController.Start()
        End Using
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        'SimpanLogToFile("test")
    End Sub

    Private Sub LisensiSementaraInstaller_AfterInstall(ByVal sender As System.Object, ByVal e As System.Configuration.Install.InstallEventArgs) Handles LisensiSementaraInstaller.AfterInstall

    End Sub
End Class