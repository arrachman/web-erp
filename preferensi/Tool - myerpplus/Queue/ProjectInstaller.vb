Imports System.ComponentModel
Imports System.Configuration.Install
Imports System.ServiceProcess

Public Class ProjectInstaller

    Public Sub New()
        MyBase.New()

        'This call is required by the Component Designer.
        InitializeComponent()

        'Add initialization code after the call to InitializeComponent

    End Sub

    Protected Overrides Sub OnAfterInstall(ByVal savedState As System.Collections.IDictionary)
        MyBase.OnAfterInstall(savedState)
        Using serviceController = New ServiceController(Me.QueueInstaller.ServiceName, Environment.MachineName)
            serviceController.Start()
        End Using
    End Sub
End Class