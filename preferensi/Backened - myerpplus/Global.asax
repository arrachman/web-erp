<%@ Application Language="VB" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Text" %>

<script runat="server">

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        AsModuleMySQL.KataKunci = "Source code ini punya Alfasoft"
        Dim fa As String = File.OpenText(HttpContext.Current.Server.MapPath("~/") + "app\app.xml").ReadToEnd
        Application("AppCode") = "myerpplus_pci"
        Application("As_ConStr1") = Encoding.UTF8.GetString(Convert.FromBase64String("U2VydmVyPTEyNy4wLjAuMTtQb3J0PTMzMDY7RGF0YWJhc2U9bXllcnBwbHVzX3BjaTtVaWQ9bXllcnBwbHVzO1B3ZD1teWVycHBsdXM7TWF4IFBvb2wgU2l6ZT0xMDAwMDtBbGxvdyBVc2VyIFZhcmlhYmxlcz1UcnVlOw=="))
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

</script>