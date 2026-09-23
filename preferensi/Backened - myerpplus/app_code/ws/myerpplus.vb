Imports System.Web
Imports System.Web.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.IO
Imports MySql.Data.MySqlClient
Imports System.Data.OleDb

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
' <System.Web.Script.Services.ScriptService()> _
<WebService(Namespace:="http://tempuri.org/")> _
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Public Class myerpplus
    Inherits System.Web.Services.WebService

    <WebMethod>
    Public Function Ws(ByVal param As String) As String
        'On Error GoTo selesai
        Dim isDemo As Boolean = False
        Dim paket As String = ""
        Dim hasil As String = ""
        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)

        Dim hasilDemo As String = paket & sptSubParam & "0" & sptSubParam & "This action can't accessed by Demo Account." & sptSubParam & "0" & sptSubParam & sptParam & "0" & _
                  sptSubParam & "0" & sptSubParam & "0" & sptSubParam & "0" & sptParam


        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
        param = param.Replace("&lt;", "<")
        param = param.Replace("&gt;", ">")		
        paramSplit = param.Split(sptParam)

        'CEK PANJANG ARRAY PARAM
        'If (paramSplit.Length <> 6) Then
        '    hasil = "Target" & sptSubParam & "0" & sptSubParam & "Invalid parameter." & sptSubParam & "0" & sptSubParam & sptParam & "0" & _
        '        sptSubParam & "0" & sptSubParam & "0" & sptSubParam & "0" & sptParam : GoTo selesai
        'End If

        'CEK PAKET
        If (Len(paramSplit(1)) = 0) Then
            hasil = "Target" & sptSubParam & "0" & sptSubParam & "Packet can't be empty." & sptSubParam & "0" & sptSubParam & sptParam & "0" & _
                sptSubParam & "0" & sptSubParam & "0" & sptSubParam & "0" & sptParam : GoTo selesai
        Else
            paket = paramSplit(1)
        End If
		
        'CEK KARAKTER ~
        'If param.Contains("~") Then
        '    hasil = "Target" & sptSubParam & "0" & sptSubParam & "Please don't use ' ~ ' character." & sptSubParam & "0" & sptSubParam & sptParam & "0" & _
        '        sptSubParam & "0" & sptSubParam & "0" & sptSubParam & "0" & sptParam : GoTo selesai
        'End If		
        'END OF VALIDASI PARAMETER GLOBAL ==================================================


        'TAMBAHKAN MSMQ ====================================================================
        If paket <> "M0_Login" And paket <> "MobM0_Login" And paket <> "M0_GetLibraryGrid" And paket <> "M0_GetLibraryReport" And paket <> "M0_GetLibraryStatistic" Then
            'tipe = login/check/logout
            Dim tipeMsmq As String = "check"
            Dim hasilMsmq As String = SendMsmqLogin(dirMsmqUserLogin, tipeMsmq, paramSplit(0), paramSplit(3), Application("AppCode"))
            If Len(hasilMsmq) > 0 Then
                hasil = paket & sptSubParam & "0" & sptSubParam & hasilMsmq & sptSubParam & "0" & sptSubParam & sptParam & "0" & _
                        sptSubParam & "0" & sptSubParam & "0" & sptSubParam & "0" & sptParam : GoTo selesai
            End If
        End If
        'END OF TAMBAHKAN MSMQ =============================================================


        Select Case paket

            '    *********************************** CD ***********************************
            Case "m2r_laba_pertahun"
                Dim wsM0_Report_Progress As New m2r_laba_pertahun
                hasil = wsM0_Report_Progress.m2r_laba_pertahun(param)

            Case "CdM0_Status_Rq"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM0_Status_Rq(param)

            Case "CdM0_Import_Target"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM0_Import_Target(param)

            Case "CdM0_Selling_Rate"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM0_Selling_Rate(param)

            Case "CdM0_Pd_Process"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM0_Pd_Process(param)
				
            Case "CdM0_Carabayar"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM0_Carabayar(param)

            Case "CdM0_Nomor"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM0_Nomor(param)

            Case "CdM0_Module"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM0_Module(param)

            Case "CdM0_User"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM0_User(param)

			Case "CdM0_Status"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM0_Status(param)
				
            Case "CdM0_Status_Giro"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM0_Status_Giro(param)

            Case "CdM0_StatisticPacket"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM0_StatisticPacket(param)

            Case "CdM0_Realization_Category"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM0_Realization_Category(param)

            Case "M0_MenuSearch"
                Dim wsM0_menu As New wsM0_Menu
                hasil = wsM0_menu.M0_MenuSearch(param)

            Case "M0_SetAplikasiSearch"
                Dim wsM0_menu As New m0_setting_company
                hasil = wsM0_menu.M0_SetAplikasiSearch(param)

            Case "CdM0_ReportPacket"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM0_ReportPacket(param)

            Case "M0_GetProfilCompany"
                Dim wsm As New m0_setting_company
                hasil = wsm.M0_GetProfilCompany(param)

            Case "M0_SimpanProfilCompany"
                Dim wsm As New m0_setting_company
                hasil = wsm.M0_SimpanProfilCompany(param)

            Case "CdM0_Userlog"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM0_Userlog(param)

            Case "CdM0_UserlogCategory"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM0_UserlogCategory(param)

            Case "CdM1_Cogs_Special_In"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Cogs_Special_In(param)

            Case "CdM1_Cogs_Special_Out"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Cogs_Special_Out(param)

            Case "CdM1_Item_Location"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Item_Location(param)

            Case "CdM1_Item_Type"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Item_Type(param)

            Case "CdM1_Type_Sa"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Type_Sa(param)

            Case "CdM1_Bank"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Bank(param)

            Case "CdM1_City"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_City(param)

            Case "CdM1_Province"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Province(param)

            Case "CdM1_Reference"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Reference(param)

            Case "CdM1_Colleague"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Colleague(param)

            Case "CdM1_Insurer"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Insurer(param)

            Case "CdM1_Country"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Country(param)

            Case "CdM1_Diagnosis"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Diagnosis(param)

            Case "CdM1_Village"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Village(param)

            Case "CdM1_Subdistrict"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_SubDistrict(param)

            Case "CdM1_Patient"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Patient(param)

            Case "CdM1_Area"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Area(param)

			Case "CdM1_Upline"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Upline(param)
				
            Case "CdM1_Layanan"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Layanan(param)

            Case "CdM1_Item_Category"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Item_Category(param)

            Case "CdM1_Contact_Category"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Contact_Category(param)

            Case "CdM1_Salesman_Category"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Salesman_Category(param)

            Case "CdM1_Customer_Category"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Customer_Category(param)

            Case "CdM1_Patient_Category"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Patient_Category(param)

            Case "CdM1_Supplier_Category"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Supplier_Category(param)

            Case "CdM1_Contact"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Contact(param)

            Case "CdM1_Contact_Attention"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Contact_Attention(param)

            Case "CdM1_ItemLookup"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_ItemLookup(param)

            Case "CdM1_Item"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Item(param)
            Case "CdM1_ItemInput"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_ItemInput(param)
            Case "CdM1_Item_PA"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Item_PA(param)
            Case "CdM1_ItemPergudang"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_ItemPergudang(param)

            Case "CdM1_ItemPickingList"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_ItemPickingList(param)

            Case "CdM1_ItemPickingListPR"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_ItemPickingListPR(param)

            Case "CdM1_Item_Assembly"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Item_Assembly(param)

            Case "CdM1_Branch"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Branch(param)

            Case "CdM1_BranchAll"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_BranchAll(param)

            Case "CdM1_Location"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Location(param)

            Case "CdM1_LocationAll"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_LocationAll(param)

            Case "CdM1_Cost_Center"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Cost_Center(param)

            Case "CdM1_Division"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Division(param)

            Case "CdM1_Expedition"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Expedition(param)

            Case "CdM1_Subdivision"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Subdivision(param)

            Case "CdM1_Project"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Project(param)

            Case "CdM1_Unit"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Unit(param)

            Case "CdM1_Transaction_Note"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Transaction_Note(param)

            Case "CdM1_Transaction_Note_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Transaction_Note_Detail(param)

            Case "CdM1_Currency"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Currency(param)

            Case "CdM1_Coa"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Coa(param)

            Case "CdM1_Tax"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Tax(param)

            Case "CdM1_Terms"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Terms(param)

            Case "CdM1_Warehouse"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Warehouse(param)

            Case "CdM1_WarehouseAll"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_WarehouseAll(param)

            Case "CdM1_Working_Estimate"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Working_Estimate(param)

            Case "CdM1_Production_Category"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Production_Category(param)

            Case "CdM1_No_Batch_In"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_No_Batch_In(param)

            Case "CdM1_No_Serial_In"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_No_Serial_In(param)

            Case "CdM1_No_BatchSerial_In"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_No_BatchSerial_In(param)

            Case "CdM1_No_BatchSerial_In_Group"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_No_BatchSerial_In_Group(param)

            Case "CdM1_No_BatchSerial_In_Getdata"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_No_BatchSerial_In_Getdata(param)

            Case "CdM1_NoBatchGroup"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_NoBatchGroup(param)

            Case "CdM1_NoSerialGroup"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_NoSerialGroup(param)

            Case "CdM1_NoBatchGetdata"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_NoBatchGetdata(param)

            Case "CdM1_NoSerialGetdata"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_NoSerialGetdata(param)

            Case "CdM1_Item_Hauling"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Item_Hauling(param)

            Case "CdM1_Checking_Category"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Checking_Category(param)

            Case "CdM1_Selling_Point"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Selling_Point(param)

            Case "CdM1_Room"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Room(param)

            Case "CdM1_Bed"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Bed(param)

            Case "CdM1_Other_Cost"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Other_Cost(param)

            Case "CdM1_Class_Product"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Class_Product(param)

            Case "CdM1_Index_Price"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Index_Price(param)

            Case "CdM1_Department"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Department(param)

            Case "CdM1_SubDepartment"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_SubDepartment(param)

            Case "CdM1_Commission"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Commission(param)

            Case "CdM1_Accident"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Accident(param)

            Case "CdM1_Icd"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Icd(param)

            Case "CdM1_Trm"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Trm(param)

            Case "CdM1_Lab_Result"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Lab_Result(param)

            Case "CdM1_Item_Permission"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Item_Permission(param)

            Case "CdM1_Labour"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Labour(param)

            Case "CdM1_Machine"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Machine(param)

            Case "CdM1_Class"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Class(param)

            Case "CdM2_Giro_List"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM2_Giro_List(param)

            Case "CdM2_Realization"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM2_Realization(param)

            Case "CdM2_Realization_Branch"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM2_Realization_Branch(param)

            Case "CdM2_Realization_Location"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM2_Realization_Location(param)

            Case "CdM2_Realization_Costcenter"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM2_Realization_Costcenter(param)

            Case "CdM2_Realization_Division"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM2_Realization_Division(param)

            Case "CdM2_Realization_Subdivision"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM2_Realization_Subdivision(param)

            Case "CdM2_Realization_Project"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM2_Realization_Project(param)

            Case "CdM3_Mr"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM3_Mr(param)

            Case "CdM3_Mr_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM3_Mr_Detail(param)

            Case "CdM3_Ts"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM3_Ts(param)
            Case "CdM3_Ts_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM3_Ts_Detail(param)

            Case "CdM3_Sp"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM3_Sp(param)

            Case "CdM3_Sp_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM3_Sp_Detail(param)

            Case "CdM3_Sp_DetailSelisihPenjualan"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM3_Sp_DetailSelisihPenjualan(param)

            Case "CdM4_Pr"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM4_Pr(param)

            Case "CdM4_Pr_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM4_Pr_Detail(param)

            Case "CdM4_Po"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM4_Po(param)

            Case "CdM4_Po_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM4_Po_Detail(param)

            Case "CdM4_Grn"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM4_Grn(param)

            Case "CdM4_Rq"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM4_Rq(param)
            Case "CdM4_Rq_Nogrup"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM4_Rq_Nogrup(param)
            Case "CdM4_Rq_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM4_Rq_Detail(param)

            Case "CdM4_Grn_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM4_Grn_Detail(param)

            Case "CdM4_Ri"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM4_Ri(param)

            Case "CdM4_Ri_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM4_Ri_Detail(param)

            Case "CdM4_Dnr_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM4_Dnr_Detail(param)

            Case "CdM4_Vpp"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM4_Vpp(param)

            Case "CdM5_Sq"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_Sq(param)

            Case "CdM5_Sq_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_Sq_Detail(param)

            Case "CdM5_So"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_So(param)

            Case "CdM5_So_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_So_Detail(param)

            Case "CdM5_Ip"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_Ip(param)

            Case "CdM5_As"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_As(param)

            Case "CdM5_AsambilSi"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_AsambilSi(param)

            Case "CdM4_Ap"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM4_Ap(param)

            Case "CdM5_Pl_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_Pl_Detail(param)

            Case "CdM5_Do"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_Do(param)

            Case "CdM5_Do_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_Do_Detail(param)

            Case "CdM5_Dr"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_Dr(param)

            Case "CdM5_Dr_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_Dr_Detail(param)

            Case "CdM5_Pi"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_Pi(param)

            Case "CdM5_Pi_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_Pi_Detail(param)

            Case "CdM5_Si"

                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_Si(param)

            Case "CdM5_Si_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_Si_Detail(param)

            Case "CdM5_Rnr"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_Rnr(param)

            Case "CdM5_Rnr_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_Rnr_Detail(param)

            Case "CdM5_Ic"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM5_Ic(param)

            Case "CdM6_Bom"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM6_Bom(param)

            Case "CdM6_Pdr"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM6_Pdr(param)

            Case "CdM6_Wo"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM6_Wo(param)

            Case "CdM6_Mrs"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM6_Mrs(param)

            Case "CdM6_Mrs_Out"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM6_Mrs_Out(param)

            Case "CdM6_Mrn"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM6_Mrn(param)

            Case "CdM6_Pd"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM6_Pd(param)

            Case "CdM7_Depreciation_Category"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM7_Depreciation_Category(param)

            Case "CdM7_Asset_Category_Tax"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM7_Asset_Category_Tax(param)

            Case "CdM7_Asset_Category"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM7_Asset_Category(param)

            Case "CdM7_Asset"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM7_Asset(param)

            Case "CdM7_Ar_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM7_Ar_Detail(param)

            Case "CdM7_Aq_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM7_Aq_Detail(param)

            Case "CdM7_Ao_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM7_Ao_Detail(param)

            Case "CdM11_Kj"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM11_Kj(param)

            Case "CdM11_Km"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM11_Km(param)

            Case "CdM12_Promo"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM12_Promo(param)

            Case "CdM1_Production_Activity"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Production_Activity(param)

            Case "CdM1_Production_Route"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Production_Route(param)


                '*********************************** M0 ***********************************

            Case "M0_ImportdataSimpan"
                Dim wsresult As New m0_importdata
                hasil = wsresult.M0_ImportdataSimpan(param)
            Case "M0_ImportdataGetdataById"
                Dim wsresult As New m0_importdata
                hasil = wsresult.M0_ImportdataGetdataById(param)

            Case "M0_StatisticSearch"
                Dim wsresult As New m0_statistic
                hasil = wsresult.M0_StatisticSearch(param)


                'Case "M0_ImportDataSimpan"
                '    Dim wsresult As New m0_importdata
                '    hasil = wsresult.ImportData(param)

            Case "M0_Journal"
                Dim wsM0_Journal As New m0_journal
                hasil = wsM0_Journal.M0_Journal(param)

            Case "M0_JournalUlang"
                Dim wsM0_Journal As New m0_journal
                hasil = wsM0_Journal.M0_JournalUlang(param)

            Case "M0_JournalData"
                Dim wsM0_Journal As New m0_journal
                hasil = wsM0_Journal.M0_JournalData(param)

            Case "M0_ReqJournalUlang"
                Dim wsM0_Journal As New m0_journal
                hasil = wsM0_Journal.M0_ReqJournalUlang(param)

            Case "M0_CogsJournalUlang"
                Dim wsM0_Journal As New m0_cogs_journal
                hasil = wsM0_Journal.M0_CogsJournalUlang(param)

            Case "M0_CogsHitungUlang_Fifo"
                Dim wsM0_Cogs As New m0_cogs
                hasil = wsM0_Cogs.M0_CogsHitungUlang_Fifo(param)

            Case "M0_CogsHitungUlang_MasukAverage"
                Dim wsM0_Cogs As New m0_cogs
                hasil = wsM0_Cogs.M0_CogsHitungUlang_MasukAverage(param)

            Case "M0_CogsHitungUlang_Average"
                Dim wsM0_Cogs As New m0_cogs
                hasil = wsM0_Cogs.M0_CogsHitungUlang_Average(param)

            Case "M0_CogsHitungUlang_AveragePerBarang"
                Dim wsM0_Cogs As New m0_cogs
                hasil = wsM0_Cogs.M0_CogsHitungUlang_AveragePerBarang(param)

            Case "M0_CogsHitungUlang_Saldo"
                Dim wsM0_Cogs As New m0_cogs
                hasil = wsM0_Cogs.M0_CogsHitungUlang_Saldo(param)

            Case "M0_CogsHitungUlang_SaldoFifo"
                Dim wsM0_Cogs As New m0_cogs
                hasil = wsM0_Cogs.M0_CogsHitungUlang_SaldoFifo(param)

            Case "GetDirectoryContent"
                Dim wsM0_File_Manager As New m0_file_manager
                hasil = wsM0_File_Manager.GetDirectoryContent(param)

            Case "M0_MsmqSimpan"
                Dim m0_msmq As New m0_msmq
                hasil = m0_msmq.M0_MsmqSimpan(param)

            Case "M0_MsmqGetdataById"
                Dim m0_msmq As New m0_msmq
                hasil = m0_msmq.M0_MsmqGetdataById(param)

            Case "M0_AppRegister"
                Dim wsM0_App As New m0_app
                hasil = wsM0_App.M0_AppRegister(param)

            Case "M0_GetFileLibrary"
                Dim wsM0_Library As New m0_library
                hasil = wsM0_Library.M0_GetFileLibrary(param)

            Case "M0_SetFileLibrary"
                Dim wsM0_Library As New m0_library
                hasil = wsM0_Library.M0_SetFileLibrary(param)

            Case "M0_SetLangFileLibrary"
                Dim wsM0_Library As New m0_library
                hasil = wsM0_Library.M0_SetLangFileLibrary(param)

            Case "M0_SetFormFileLibrary"
                Dim wsM0_Library As New m0_library
                hasil = wsM0_Library.M0_SetFormFileLibrary(param)

            Case "M0_GetLibraryGrid"
                Dim wsM0_Library As New m0_library
                hasil = wsM0_Library.M0_GetLibraryGrid(param)

            Case "M0_GetLibraryReport"
                Dim wsM0_Library As New m0_library
                hasil = wsM0_Library.M0_GetLibraryReport(param)

            Case "M0_GetLibraryStatistic"
                Dim wsM0_Library As New m0_library
                hasil = wsM0_Library.M0_GetLibraryStatistic(param)

                'M0_NOTES
            Case "M0_Search_PacketSimpan"
                Dim wsM0_Search_Packet As New m0_search_packet
                hasil = wsM0_Search_Packet.M0_Search_PacketSimpan(param)
            Case "M0_Search_PacketSearch"
                Dim wsM0_Search_Packet As New m0_search_packet
                hasil = wsM0_Search_Packet.M0_Search_PacketSearch(param)
            Case "M0_Search_PacketDelete"
                Dim wsM0_Search_Packet As New m0_search_packet
                hasil = wsM0_Search_Packet.M0_Search_PacketDelete(param)

                'M0_NOTES
            Case "M0_NotesSimpan"
                Dim wsM0_Notes As New m0_notes
                hasil = wsM0_Notes.M0_NotesSimpan(param)
            Case "M0_NotesSearch"
                Dim wsM0_Notes As New m0_notes
                hasil = wsM0_Notes.M0_NotesSearch(param)
            Case "M0_NotesDelete"
                Dim wsM0_Notes As New m0_notes
                hasil = wsM0_Notes.M0_NotesDelete(param)

                'M0_FILES
            Case "M0_FilesSimpan_S"
                Dim wsM0_Files As New m0_files
                hasil = wsM0_Files.M0_FilesSimpan_S(param)
            Case "M0_FilesSimpan"
                Dim wsM0_Files As New m0_files
                hasil = wsM0_Files.M0_FilesSimpan(param)
            Case "M0_FilesSearch"
                Dim wsM0_Files As New m0_files
                hasil = wsM0_Files.M0_FilesSearch(param)
            Case "M0_Files_SSearch"
                Dim wsM0_Files As New m0_files
                hasil = wsM0_Files.M0_Files_SSearch(param)
            Case "M0_FilesDelete"
                Dim wsM0_Files As New m0_files
                hasil = wsM0_Files.M0_FilesDelete(param)
            Case "M0_DownloadDbFile"
                Dim wsM0_Files As New m0_files
                hasil = wsM0_Files.M0_DownloadDbFile(param)
            Case "M0_DownloadDb"
                Dim wsM0_Files As New m0_files
                hasil = wsM0_Files.M0_DownloadDb(param)
            Case "M0_DownloadDbPOS"
                Dim wsM0_Files As New m0_files
                hasil = wsM0_Files.M0_DownloadDbPOS(param)
            Case "M0_DownloadDbSql"
                Dim wsM0_Files As New m0_files
                hasil = wsM0_Files.M0_DownloadDbSql(param)
            Case "M0_CreateDbFile"
                Dim wsM0_Files As New m0_files
                hasil = wsM0_Files.M0_CreateDbFile(param)
            Case "M0_ExecuteDbFile"
                Dim wsM0_Files As New m0_files
                hasil = wsM0_Files.M0_ExecuteDbFile(param)
            Case "M0_ExtractFile"
                Dim wsM0_Files As New m0_files
                hasil = wsM0_Files.M0_ExtractFile(param)

                'M0_ROLE_S
            Case "M0_Role_SDelete"
                Dim wsM0_Role As New m0_role_s
                hasil = wsM0_Role.M0_Role_SDelete(param)
            Case "M0_Role_SSearch"
                Dim wsM0_Role As New m0_role_s
                hasil = wsM0_Role.M0_Role_SSearch(param)
            Case "M0_Role_customSearch"
                Dim wsM0_Role As New m0_role_s
                hasil = wsM0_Role.M0_Role_customSearch(param)
            Case "M0_Role_Custom_SSimpan"
                Dim wsM0_Role As New m0_role_s
                hasil = wsM0_Role.M0_Role_Custom_SSimpan(param)
            Case "M0_Role_Report_SSearch"
                Dim wsM0_Role As New m0_role_s
                hasil = wsM0_Role.M0_Role_Report_SSearch(param)
            Case "M0_Role_Report_SSimpan"
                Dim wsM0_Role As New m0_role_s
                hasil = wsM0_Role.M0_Role_Report_SSimpan(param)
            Case "M0_Role_Menu_SSearch"
                Dim wsM0_Role As New m0_role_s
                hasil = wsM0_Role.M0_Role_Menu_SSearch(param)
            Case "M0_Role_Menu_SSimpan"
                Dim wsM0_Role As New m0_role_s
                hasil = wsM0_Role.M0_Role_Menu_SSimpan(param)

                'm0_role_s
            Case "M0_Role_SSimpan"
                Dim wsM0_Role As New m0_role_s
                hasil = wsM0_Role.M0_Role_SSimpan(param)

                'M0_ROLE
            Case "M0_RoleSimpan"
                Dim wsM0_Role As New m0_role
                hasil = wsM0_Role.M0_RoleSimpan(param)
            Case "M0_RoleSearch"
                Dim wsM0_Role As New m0_role
                hasil = wsM0_Role.M0_RoleSearch(param)
            Case "M0_RoleDelete"
                Dim wsM0_Role As New m0_role
                hasil = wsM0_Role.M0_RoleDelete(param)
            Case "M0_RoleGetdataById"
                Dim wsM0_Role As New m0_role
                hasil = wsM0_Role.M0_RoleGetdataById(param)
            Case "M0_RoleGetSetting"
                Dim wsM0_Role As New m0_role
                hasil = wsM0_Role.M0_RoleGetSetting(param)

                'M0_PERMISSIONS_CUSTOM
            Case "M0_Permissions_CustomSearch"
                Dim wsM0_Permissions_Custom As New m0_permissions_custom
                hasil = wsM0_Permissions_Custom.M0_Permissions_CustomSearch(param)

                'M0_COA_TREE
            Case "M0_Coa_Tree"
                Dim wsM0_Coa_Tree As New m0_coa_tree
                hasil = wsM0_Coa_Tree.M0_Coa_Tree(param)

                'M0_VERSI_DB
            Case "M0_Versi_DbSearch"
                Dim wsM0_Versi_Db As New m0_versi_db
                hasil = wsM0_Versi_Db.M0_Versi_DbSearch(param)

                'M0_LOGIN
            Case "M0_Login"
                Dim wsM0_Login As New m0_login
                hasil = wsM0_Login.M0_Login(param)
            Case "M0_Login2"
                Dim wsM0_Login As New m0_login
                hasil = wsM0_Login.M0_Login2(param)
            Case "M0_Logout"
                Dim wsM0_Login As New m0_login
                hasil = wsM0_Login.M0_Logout(param)


                'M0_USER
            Case "M0_User_VSearch"
                Dim wsM0_User As New m0_user
                hasil = wsM0_User.M0_User_VSearch(param)
            Case "M0_UserSearch"
                Dim wsM0_User As New m0_user
                hasil = wsM0_User.M0_UserSearch(param)
            Case "M0_UserSimpan"
                If (isDemo = False) Then
                    Dim wsM0_User As New m0_user
                    hasil = wsM0_User.M0_UserSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_UserGetdataById"
                Dim wsM0_User As New m0_user
                hasil = wsM0_User.M0_UserGetdataById(param)
            Case "M0_User_SGetdataById"
                Dim wsM0_User As New m0_user
                hasil = wsM0_User.M0_User_SGetdataById(param)
            Case "M0_UserResetPassword_S"
                Dim wsM0_User As New m0_user
                hasil = wsM0_User.M0_UserResetPassword_S(param)
            Case "M0_UserUpdatePassword_S"
                Dim wsM0_User As New m0_user
                hasil = wsM0_User.M0_UserUpdatePassword_S(param)
            Case "M0_UserUpdatePassword"
                If (isDemo = False) Then
                    Dim wsM0_User As New m0_user
                    hasil = wsM0_User.M0_UserUpdatePassword(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_UserDelete"
                If (isDemo = False) Then
                    Dim wsM0_User As New m0_user
                    hasil = wsM0_User.M0_UserDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If

                'M0_USERLOGIN
            Case "M0_UserloginGetdataById"
                Dim wsM0_UserLogin As New m0_userlogin
                hasil = wsM0_UserLogin.M0_UserloginGetdataById(param)
                'M0_USERLOGIN
            Case "M0_UserloginSendMsmq"
                Dim wsM0_UserLogin As New m0_userlogin
                hasil = wsM0_UserLogin.M0_UserloginSendMsmq(param)
                'M0_USERLOGIN
            Case "M0_UserloginSearch"
                Dim wsM0_UserLogin As New m0_userlogin
                hasil = wsM0_UserLogin.M0_UserloginSearch(param)

                'M0_USERMODULE
            Case "M0_Usermodule_VSearch"
                Dim wsM0_Usermodule As New m0_usermodule
                hasil = wsM0_Usermodule.M0_Usermodule_VSearch(param)
            Case "M0_UsermoduleSearch"
                Dim wsM0_Usermodule As New m0_usermodule
                hasil = wsM0_Usermodule.M0_UsermoduleSearch(param)

                'M0_USERMENU
            Case "M0_UsermenuSimpan"
                If (isDemo = False) Then
                    Dim wsM0_Usermenu As New Wsm0_usermenu
                    hasil = wsM0_Usermenu.M0_UsermenuSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_Usermenu_VSearch"
                Dim wsM0_Usermenu As New Wsm0_usermenu
                hasil = wsM0_Usermenu.M0_Usermenu_VSearch(param)
            Case "M0_UsermenuDelete"
                If (isDemo = False) Then
                    Dim wsM0_Usermenu As New Wsm0_usermenu
                    hasil = wsM0_Usermenu.M0_UsermenuDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_UsermenuSearch"
                Dim wsM0_Usermenu As New Wsm0_usermenu
                hasil = wsM0_Usermenu.M0_UsermenuSearch(param)

                'M0_MENU
            Case "M0_MenuSimpan"
                If (isDemo = False) Then
                    Dim wsM0_Menu As New wsM0_Menu
                    hasil = wsM0_Menu.M0_MenuSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_MenuDelete"
                If (isDemo = False) Then
                    Dim wsM0_Menu As New wsM0_Menu
                    hasil = wsM0_Menu.M0_MenuDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_MenuSearch"
                Dim wsM0_Menu As New wsM0_Menu
                hasil = wsM0_Menu.M0_MenuSearch(param)
            Case "M0_MenuByLanguage"
                Dim wsM0_Menu As New wsM0_Menu
                hasil = wsM0_Menu.M0_MenuByLanguage(param)
            Case "M0_MenuManagerByLanguage"
                Dim wsM0_Menu As New wsM0_Menu
                hasil = wsM0_Menu.M0_MenuManagerByLanguage(param)

                'M0_MENU_lANG
            Case "M0_Menu_LangSearch"
                Dim wsM0_Menu_Lang As New m0_menu_lang
                hasil = wsM0_Menu_Lang.M0_Menu_LangSearch(param)
            Case "M0_Menu_LangSimpan"
                If (isDemo = False) Then
                    Dim wsM0_Menu_Lang As New m0_menu_lang
                    hasil = wsM0_Menu_Lang.M0_Menu_LangSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_Menu_Lang_SSearch"
                Dim wsM0_Menu_Lang As New m0_menu_lang
                hasil = wsM0_Menu_Lang.M0_Menu_Lang_SSearch(param)
            Case "M0_Menu_Lang_SSimpan"
                Dim wsM0_Menu_Lang As New m0_menu_lang
                hasil = wsM0_Menu_Lang.M0_Menu_Lang_SSimpan(param)

                'M0_REPORT_LANG
            Case "M0_Report_LangSearch"
                Dim wsM0_Report_Lang As New m0_report_lang
                hasil = wsM0_Report_Lang.M0_Report_LangSearch(param)
            Case "M0_Report_LangSimpan"
                If (isDemo = False) Then
                    Dim wsM0_Report_Lang As New m0_report_lang
                    hasil = wsM0_Report_Lang.M0_Report_LangSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_Report_Lang_SSearch"
                Dim wsM0_Report_Lang As New m0_report_lang
                hasil = wsM0_Report_Lang.M0_Report_Lang_SSearch(param)
            Case "M0_Report_Lang_SSimpan"
                Dim wsM0_Report_Lang As New m0_report_lang
                hasil = wsM0_Report_Lang.M0_Report_Lang_SSimpan(param)

                'M0_SETTING_LANG
            Case "M0_Setting_LangSearch"
                Dim wsM0_Setting_Lang As New m0_setting_lang
                hasil = wsM0_Setting_Lang.M0_Setting_LangSearch(param)
            Case "M0_Setting_LangSimpan"
                If (isDemo = False) Then
                    Dim wsM0_Setting_Lang As New m0_setting_lang
                    hasil = wsM0_Setting_Lang.M0_Setting_LangSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If

                'M0_HAK_AKSES
            Case "M0_HakAkses"
                Dim wsM0_HakAkses As New m0_hakAkses
                hasil = wsM0_HakAkses.M0_HakAkses(param)
            Case "M0_HakAkses2"
                Dim wsM0_HakAkses As New m0_hakAkses
                hasil = wsM0_HakAkses.M0_HakAkses2(param)
            Case "M0_MenuTree"
                Dim wsM0_HakAkses As New m0_hakAkses
                hasil = wsM0_HakAkses.M0_MenuTree(param)

				'M0_Userlogerror
            Case "M0_UserlogerrorSimpan"
                Dim wsM0_Setting As New m0_setting
                hasil = wsM0_Setting.M0_UserlogerrorSimpan(param)
				
                'M0_SETTING
            Case "M0_SettingSimpan"
                If (isDemo = False) Then
                    Dim wsM0_Setting As New m0_setting
                    hasil = wsM0_Setting.M0_SettingSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_SettingSearch"
                Dim wsM0_Setting As New m0_setting
                hasil = wsM0_Setting.M0_SettingSearch(param)
            Case "M0_SettingDelete"
                If (isDemo = False) Then
                    Dim wsM0_Setting As New m0_setting
                    hasil = wsM0_Setting.M0_SettingDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_SettingByLanguage"
                Dim wsM0_Setting As New m0_setting
                hasil = wsM0_Setting.M0_SettingByLanguage(param)

            'M0_APPROVAL
            Case "M0_ApprovalSimpan"
                Dim wsM0_Approval As New m0_approval
                hasil = wsM0_Approval.M0_ApprovalSimpan(param)
            Case "M0_ApprovalSearch"
                Dim wsM0_Approval As New m0_approval
                hasil = wsM0_Approval.M0_ApprovalSearch(param)
            Case "M0_ApprovalDelete"
                Dim wsM0_Approval As New m0_approval
                hasil = wsM0_Approval.M0_ApprovalDelete(param)


                'M0_USERCUSTOM
            Case "M0_UsercustomSimpan"
                If (isDemo = False) Then
                    Dim wsM0_Usercustom As New wsm0_usercustom
                    hasil = wsM0_Usercustom.M0_UsercustomSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_Usercustom_VSearch"
                Dim wsM0_Usercustom As New wsm0_usercustom
                hasil = wsM0_Usercustom.M0_Usercustom_VSearch(param)
            Case "M0_UsercustomSearch"
                Dim wsM0_Usercustom As New wsm0_usercustom
                hasil = wsM0_Usercustom.M0_UsercustomSearch(param)
            Case "M0_UsercustomDelete"
                If (isDemo = False) Then
                    Dim wsM0_Usercustom As New wsm0_usercustom
                    hasil = wsM0_Usercustom.M0_UsercustomDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If

                'M0_USERREPORT
            Case "M0_UserreportSimpan"
                If (isDemo = False) Then
                    Dim wsM0_Userreport As New wsm0_userreport
                    hasil = wsM0_Userreport.M0_UserreportSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_Userreport_VSearch"
                Dim wsM0_Userreport As New wsm0_userreport
                hasil = wsM0_Userreport.M0_Userreport_VSearch(param)
            Case "M0_UserreportSearch"
                Dim wsM0_Userreport As New wsm0_userreport
                hasil = wsM0_Userreport.M0_UserreportSearch(param)
            Case "M0_UserreportDelete"
                If (isDemo = False) Then
                    Dim wsM0_Userreport As New wsm0_userreport
                    hasil = wsM0_Userreport.M0_UserreportDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If

                'M0_NOMOR
            Case "M0_NomorSimpan"
                If (isDemo = False) Then
                    Dim wsM0_Nomor As New m0_nomor
                    hasil = wsM0_Nomor.M0_NomorSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_Nomor_SSimpan"
                Dim wsM0_Nomor As New m0_nomor
                hasil = wsM0_Nomor.M0_Nomor_SSimpan(param)
            Case "M0_NomorSearch"
                Dim wsM0_Nomor As New m0_nomor
                hasil = wsM0_Nomor.M0_NomorSearch(param)
            Case "M0_NomorDelete"
                If (isDemo = False) Then
                    Dim wsM0_Nomor As New m0_nomor
                    hasil = wsM0_Nomor.M0_NomorDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_GenerateNoSerial"
                Dim wsM0_Nomor As New m0_nomor
                hasil = wsM0_Nomor.M0_GenerateNoSerial(param)
            Case "M0_GenerateBarcode"
                Dim wsM0_Nomor As New m0_nomor
                hasil = wsM0_Nomor.M0_GenerateBarcode(param)

                'M0_REPORT
            Case "M0_Report_FilterSearch"
                Dim wsM0_Report As New m0_report_filter
                hasil = wsM0_Report.M0_Report_FilterSearch(param)
            Case "M0_ReportSimpan"
                If (isDemo = False) Then
                    Dim wsM0_Report As New m0_report
                    hasil = wsM0_Report.M0_ReportSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_ReportSearch"
                Dim wsM0_Report As New m0_report
                hasil = wsM0_Report.M0_ReportSearch(param)
			Case "M0_Report_FilterSearch"
                Dim wsM0_Report As New m0_report
                hasil = wsM0_Report.M0_Report_FilterSearch(param)
            Case "M0_Report_VSearch"
                Dim wsM0_Report As New m0_report
                hasil = wsM0_Report.M0_Report_VSearch(param)
            Case "M0_ReportDelete"
                If (isDemo = False) Then
                    Dim wsM0_Report As New m0_report
                    hasil = wsM0_Report.M0_ReportDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_ReportByLanguage"
                Dim wsM0_Report As New m0_report
                hasil = wsM0_Report.M0_ReportByLanguage(param)
            Case "M0_ReportManagerByLanguage"
                Dim wsM0_Report As New m0_report
                hasil = wsM0_Report.M0_ReportManagerByLanguage(param)
            Case "M0_ReportSetMemcached"
                If (isDemo = False) Then
                    Dim wsM0_Report As New m0_report
                    hasil = wsM0_Report.M0_ReportSetMemcached(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_ReportGetMemcached"
                If (isDemo = False) Then
                    Dim wsM0_Report As New m0_report
                    hasil = wsM0_Report.M0_ReportGetMemcached(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_ReportDeleteMemcached"
                If (isDemo = False) Then
                    Dim wsM0_Report As New m0_report
                    hasil = wsM0_Report.M0_ReportDeleteMemcached(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_Report_Default_SSearch"
                Dim wsM0_Report As New m0_report
                hasil = wsM0_Report.M0_Report_Default_SSearch(param)
            Case "M0_Report_Default_SSimpan"
                Dim wsM0_Report As New m0_report
                hasil = wsM0_Report.M0_Report_Default_SSimpan(param)
            Case "M0_Report_SimpanAll"
                Dim wsM0_Report As New m0_report
                hasil = wsM0_Report.M0_Report_SimpanAll(param)
            Case "M0_Report_FilterGetdataById"
                Dim wsM0_Report As New m0_report
                hasil = wsM0_Report.M0_Report_FilterGetdataById(param)
            Case "M0_Report_FilterSimpan"
                Dim wsM0_Report As New m0_report
                hasil = wsM0_Report.M0_Report_FilterSimpan(param)
            Case "M0_Report_Label_TranslateSearch"
                Dim wsM0_Report As New m0_report
                hasil = wsM0_Report.M0_Report_Label_TranslateSearch(param)
            Case "M0_Report_Label_TranslateSimpan"
                Dim wsM0_Report As New m0_report
                hasil = wsM0_Report.M0_Report_Label_TranslateSimpan(param)

                'M0_SERVICE
            Case "M0_Service_Report"
                Dim wsM0_Service As New m0_service
                hasil = wsM0_Service.M0_Service_Report(param)
            Case "M0_Service_ReportSearch"
                Dim wsM0_Service As New m0_service
                hasil = wsM0_Service.M0_Service_ReportSearch(param)
            Case "M0_Service_ReportGetSetting"
                Dim wsM0_Service As New m0_service
                hasil = wsM0_Service.M0_Service_ReportGetSetting(param)
            Case "M0_Service_UpdateRelationTable"
                Dim wsM0_Service As New m0_service
                hasil = wsM0_Service.M0_Service_UpdateRelationTable(param)

                'M0_FORM_SETTING_GLOBAL
            Case "M0_Form_Setting_GlobalSimpan"
                Dim wsM0_Form_Setting_Global As New m0_form_setting_global
                hasil = wsM0_Form_Setting_Global.M0_Form_Setting_GlobalSimpan(param)
            Case "M0_Form_Setting_GlobalSearch"
                Dim wsM0_Form_Setting_Global As New m0_form_setting_global
                hasil = wsM0_Form_Setting_Global.M0_Form_Setting_GlobalSearch(param)
            Case "M0_Form_Setting_GlobalDelete"
                Dim wsM0_Form_Setting_Global As New m0_form_setting_global
                hasil = wsM0_Form_Setting_Global.M0_Form_Setting_GlobalDelete(param)

                'M0_FORM_SETTING_USER
            Case "M0_Form_Setting_UserSimpan"
                If (isDemo = False) Then
                    Dim wsM0_Form_Setting_User As New m0_form_setting_user
                    hasil = wsM0_Form_Setting_User.M0_Form_Setting_UserSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_Form_Setting_UserSearch"
                Dim wsM0_Form_Setting_User As New m0_form_setting_user
                hasil = wsM0_Form_Setting_User.M0_Form_Setting_UserSearch(param)
            Case "M0_Form_Setting_UserDelete"
                If (isDemo = False) Then
                    Dim wsM0_Form_Setting_User As New m0_form_setting_user
                    hasil = wsM0_Form_Setting_User.M0_Form_Setting_UserDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If

                'M0_FORM_CUSTOM_TEXT
            Case "M0_Form_Custom_TextSimpan"
                Dim wsM0_Form_Custom_Text As New m0_form_custom_text
                hasil = wsM0_Form_Custom_Text.M0_Form_Custom_TextSimpan(param)
            Case "M0_Form_Custom_TextSearch"
                Dim wsM0_Form_Custom_Text As New m0_form_custom_text
                hasil = wsM0_Form_Custom_Text.M0_Form_Custom_TextSearch(param)
            Case "M0_Form_Custom_TextDelete"
                Dim wsM0_Form_Custom_Text As New m0_form_custom_text
                hasil = wsM0_Form_Custom_Text.M0_Form_Custom_TextDelete(param)

                'M0_FORM_SETTING_SEARCH
            Case "M0_Form_Setting_SearchSimpan"
                Dim wsM0_Form_Setting_Search As New m0_form_setting_search
                hasil = wsM0_Form_Setting_Search.M0_Form_Setting_SearchSimpan(param)
            Case "M0_Form_Setting_SearchSearch"
                Dim wsM0_Form_Setting_Search As New m0_form_setting_search
                hasil = wsM0_Form_Setting_Search.M0_Form_Setting_SearchSearch(param)
            Case "M0_Form_Setting_SearchDelete"
                Dim wsM0_Form_Setting_Search As New m0_form_setting_search
                hasil = wsM0_Form_Setting_Search.M0_Form_Setting_SearchDelete(param)

                'M0_USERGRUP
            Case "M0_UsergrupSimpan"
                If (isDemo = False) Then
                    Dim WsM0_Usergrup As New WsM0_Usergrup
                    hasil = WsM0_Usergrup.M0_UsergrupSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_UsergrupSearch"
                Dim WsM0_Usergrup As New WsM0_Usergrup
                hasil = WsM0_Usergrup.M0_UsergrupSearch(param)
            Case "M0_UsergrupGetdataById"
                Dim WsM0_Usergrup As New WsM0_Usergrup
                hasil = WsM0_Usergrup.M0_UsergrupGetdataById(param)
            Case "M0_UsergrupDelete"
                If (isDemo = False) Then
                    Dim WsM0_Usergrup As New WsM0_Usergrup
                    hasil = WsM0_Usergrup.M0_UsergrupDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If

                'M0_LANGUAGE
            Case "M0_LanguageSimpan"
                If (isDemo = False) Then
                    Dim WsM0_Language As New m0_language
                    hasil = WsM0_Language.M0_LanguageSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_LanguageSearch"
                Dim WsM0_Language As New m0_language
                hasil = WsM0_Language.M0_LanguageSearch(param)
            Case "M0_LanguageTerkait"
                Dim WsM0_Language As New m0_language
                hasil = WsM0_Language.M0_LanguageTerkait(param)
            Case "M0_LanguageDelete"
                If (isDemo = False) Then
                    Dim WsM0_Language As New m0_language
                    hasil = WsM0_Language.M0_LanguageDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If

                'M0_SENTENCE
            Case "M0_SentenceSimpan"
                If (isDemo = False) Then
                    Dim wsM0_sentence As New m0_sentence
                    hasil = wsM0_sentence.M0_SentenceSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_SentenceSearch"
                Dim wsM0_sentence As New m0_sentence
                hasil = wsM0_sentence.M0_SentenceSearch(param)
            Case "M0_SentenceDetailSearch"
                Dim wsM0_sentence As New m0_sentence
                hasil = wsM0_sentence.M0_SentenceDetailSearch(param)
            Case "M0_SentenceDelete"
                If (isDemo = False) Then
                    Dim wsM0_sentence As New m0_sentence
                    hasil = wsM0_sentence.M0_SentenceDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If

                'M0_TRANSLATE
            Case "M0_TranslateSimpan"
                If (isDemo = False) Then
                    Dim wsM0_translate As New m0_translate
                    hasil = wsM0_translate.M0_TranslateSimpan(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M0_TranslateSearch"
                If (isDemo = False) Then
                    Dim wsM0_translate As New m0_translate
                    hasil = wsM0_translate.M0_TranslateSearch(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If


                'M0_SETTING
            Case "M0_SettingDownload"
                Dim wsM0_Setting As New m0_setting
                hasil = wsM0_Setting.M0_SettingDownload(param)
            Case "M0_SettingImport"
                Dim wsM0_Setting As New m0_setting
                hasil = wsM0_Setting.M0_SettingImport(param)

                'M1_BRANCH
            Case "M1_BranchDownload"
                Dim wsM1_Branch As New m1_branch
                hasil = wsM1_Branch.M1_BranchDownload(param)
            Case "M1_BranchImport"
                Dim wsM1_Branch As New m1_branch
                hasil = wsM1_Branch.M1_BranchImport(param)

                'M1_LOCATION
            Case "M1_LocationDownload"
                Dim wsM1_Location As New m1_location
                hasil = wsM1_Location.M1_LocationDownload(param)
            Case "M1_LocationImport"
                Dim wsM1_Location As New m1_location
                hasil = wsM1_Location.M1_LocationImport(param)

                'M1_WAREHOUSE
            Case "M1_WarehouseDownload"
                Dim wsM1_Warehouse As New m1_warehouse
                hasil = wsM1_Warehouse.M1_WarehouseDownload(param)
            Case "M1_WarehouseImport"
                Dim wsM1_Warehouse As New m1_warehouse
                hasil = wsM1_Warehouse.M1_WarehouseImport(param)

                'M0_ROLE
            Case "M0_RoleDownload"
                Dim wsM0_Role As New m0_role
                hasil = wsM0_Role.M0_RoleDownload(param)
            Case "M0_RoleImport"
                Dim wsM0_Role As New m0_role
                hasil = wsM0_Role.M0_RoleImport(param)

                'M0_USER
            Case "M0_UserDownload"
                Dim wsM0_User As New m0_user
                hasil = wsM0_User.M0_UserDownload(param)
            Case "M0_UserImport"
                Dim wsM0_User As New m0_user
                hasil = wsM0_User.M0_UserImport(param)

                'M1_CONTACT_CATEGORY
            Case "M1_Contact_CategoryDownload"
                Dim wsM1_Contact_Category As New m1_contact_category
                hasil = wsM1_Contact_Category.M1_Contact_CategoryDownload(param)
            Case "M1_Contact_CategoryImport"
                Dim wsM1_Contact_Category As New m1_contact_category
                hasil = wsM1_Contact_Category.M1_Contact_CategoryImport(param)

            Case "CdM1_Contact_Price"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Contact_Price(param)

                'M1_SALESMAN_CATEGORY
            Case "M1_Salesman_CategoryDownload"
                Dim wsM1_Salesman_Category As New m1_salesman_category
                hasil = wsM1_Salesman_Category.M1_Salesman_CategoryDownload(param)
            Case "M1_Salesman_CategoryImport"
                Dim wsM1_Salesman_Category As New m1_salesman_category
                hasil = wsM1_Salesman_Category.M1_Salesman_CategoryImport(param)

                'M1_CUSTOMER_CATEGORY
            Case "M1_Customer_CategoryDownload"
                Dim wsM1_Customer_Category As New m1_customer_category
                hasil = wsM1_Customer_Category.M1_Customer_CategoryDownload(param)
            Case "M1_Customer_CategoryImport"
                Dim wsM1_Customer_Category As New m1_customer_category
                hasil = wsM1_Customer_Category.M1_Customer_CategoryImport(param)

                'M1_SUPPLIER_CATEGORY
            Case "M1_Supplier_CategoryDownload"
                Dim wsM1_Supplier_Category As New m1_supplier_category
                hasil = wsM1_Supplier_Category.M1_Supplier_CategoryDownload(param)
            Case "M1_Supplier_CategoryImport"
                Dim wsM1_Supplier_Category As New m1_supplier_category
                hasil = wsM1_Supplier_Category.M1_Supplier_CategoryImport(param)

                'M1_AREA
            Case "M1_AreaDownload"
                Dim wsM1_Area As New m1_area
                hasil = wsM1_Area.M1_AreaDownload(param)
            Case "M1_AreaImport"
                Dim wsM1_Area As New m1_area
                hasil = wsM1_Area.M1_AreaImport(param)

                'M1_AREA
            Case "M1_BankDownload"
                Dim wsM1_Bank As New m1_bank
                hasil = wsM1_Bank.M1_BankDownload(param)
            Case "M1_BankImport"
                Dim wsM1_Bank As New m1_bank
                hasil = wsM1_Bank.M1_BankImport(param)

                'M1_COA
            Case "M1_CoaDownload"
                Dim wsM1_Coa As New m1_coa
                hasil = wsM1_Coa.M1_CoaDownload(param)
            Case "M1_CoaImport"
                Dim wsM1_Coa As New m1_coa
                hasil = wsM1_Coa.M1_CoaImport(param)

                'M1_DIVISION
            Case "M1_DivisionDownload"
                Dim wsM1_Division As New m1_division
                hasil = wsM1_Division.M1_DivisionDownload(param)
            Case "M1_DivisionImport"
                Dim wsM1_Division As New m1_division
                hasil = wsM1_Division.M1_DivisionImport(param)

                'M1_SUBDIVISION
            Case "M1_SubdivisionDownload"
                Dim wsM1_Subdivision As New m1_subdivision
                hasil = wsM1_Subdivision.M1_SubdivisionDownload(param)
            Case "M1_SubdivisionImport"
                Dim wsM1_Subdivision As New m1_subdivision
                hasil = wsM1_Subdivision.M1_SubdivisionImport(param)

                'M0_SELLING_RATE
            Case "M0_Selling_RateSearch"
                Dim wsM0_Selling_Rate As New m0_selling_rate
                hasil = wsM0_Selling_Rate.M0_Selling_RateSearch(param)
            Case "M0_Selling_RateDownload"
                Dim wsM0_Selling_Rate As New m0_selling_rate
                hasil = wsM0_Selling_Rate.M0_Selling_RateDownload(param)
            Case "M0_Selling_RateImport"
                Dim wsM0_Selling_Rate As New m0_selling_rate
                hasil = wsM0_Selling_Rate.M0_Selling_RateImport(param)

                'M1_CONTACT
            Case "M1_ContactDownload"
                Dim wsM1_Contact As New m1_contact
                hasil = wsM1_Contact.M1_ContactDownload(param)
            Case "M1_ContactImport"
                Dim wsM1_Contact As New m1_contact
                hasil = wsM1_Contact.M1_ContactImport(param)

                'M1_ITEM_CATEGORY
            Case "M1_Item_CategoryDownload"
                Dim wsM1_Item_Category As New m1_item_category
                hasil = wsM1_Item_Category.M1_Item_CategoryDownload(param)
            Case "M1_Item_CategoryImport"
                Dim wsM1_Item_Category As New m1_item_category
                hasil = wsM1_Item_Category.M1_Item_CategoryImport(param)

                'M1_ITEM_TYPE
            Case "M1_Item_TypeDownload"
                Dim wsM1_Item_Type As New m1_item_type
                hasil = wsM1_Item_Type.M1_Item_TypeDownload(param)
            Case "M1_Item_TypeImport"
                Dim wsM1_Item_Type As New m1_item_type
                hasil = wsM1_Item_Type.M1_Item_TypeImport(param)

                'M1_UNIT
            Case "M1_UnitDownload"
                Dim wsM1_Unit As New m1_unit
                hasil = wsM1_Unit.M1_UnitDownload(param)
            Case "M1_UnitImport"
                Dim wsM1_Unit As New m1_unit
                hasil = wsM1_Unit.M1_UnitImport(param)

                'M1_PROJECT
            Case "M1_ProjectDownload"
                Dim wsM1_Project As New m1_project
                hasil = wsM1_Project.M1_ProjectDownload(param)
            Case "M1_ProjectImport"
                Dim wsM1_Project As New m1_project
                hasil = wsM1_Project.M1_ProjectImport(param)

                'M1_TAX
            Case "M1_TaxDownload"
                Dim wsM1_Tax As New m1_tax
                hasil = wsM1_Tax.M1_TaxDownload(param)
            Case "M1_TaxImport"
                Dim wsM1_Tax As New m1_tax
                hasil = wsM1_Tax.M1_TaxImport(param)

                'M1_SELLING_POINT
            Case "M1_Selling_PointDownload"
                Dim wsM1_Selling_Point As New m1_selling_point
                hasil = wsM1_Selling_Point.M1_Selling_PointDownload(param)
            Case "M1_Selling_PointImport"
                Dim wsM1_Selling_Point As New m1_selling_point
                hasil = wsM1_Selling_Point.M1_Selling_PointImport(param)

                'M1_ACCIDENT
            Case "M1_AccidentDownload"
                Dim wsM1_Accident As New m1_accident
                hasil = wsM1_Accident.M1_AccidentDownload(param)
            Case "M1_AccidentImport"
                Dim wsM1_Accident As New m1_accident
                hasil = wsM1_Accident.M1_AccidentImport(param)

                'M1_ICD
            Case "M1_IcdDownload"
                Dim wsM1_Icd As New m1_icd
                hasil = wsM1_Icd.M1_IcdDownload(param)
            Case "M1_IcdImport"
                Dim wsM1_Icd As New m1_icd
                hasil = wsM1_Icd.M1_IcdImport(param)


                '*********************************** M1 '***********************************
                'M1_NOTES
            Case "M1_NotesSimpan"
                Dim wsM1_Notes As New m1_notes
                hasil = wsM1_Notes.M1_NotesSimpan(param)
            Case "M1_NotesSearch"
                Dim wsM1_Notes As New m1_notes
                hasil = wsM1_Notes.M1_NotesSearch(param)
            Case "M1_NotesDelete"
                If (isDemo = False) Then
                    Dim wsM1_Notes As New m1_notes
                    hasil = wsM1_Notes.M1_NotesDelete(param)
                Else
                    hasil = "this is demo" : GoTo selesai
                End If

                'M1_FILES
            Case "M1_FilesSimpan"
                Dim wsM1_Files As New m1_files
                hasil = wsM1_Files.M1_FilesSimpan(param)
            Case "M1_FilesSearch"
                Dim wsM1_Files As New m1_files
                hasil = wsM1_Files.M1_FilesSearch(param)
            Case "M1_FilesDelete"
                Dim wsM1_Files As New m1_files
                hasil = wsM1_Files.M1_FilesDelete(param)

                'M1_AREA
            Case "M1_AreaSimpan"
                Dim wsM1_Area As New m1_area
                hasil = wsM1_Area.M1_AreaSimpan(param)
            Case "M1_AreaSearch"
                Dim wsM1_Area As New m1_area
                hasil = wsM1_Area.M1_AreaSearch(param)
            Case "M1_AreaDelete"
                If (isDemo = False) Then
                    Dim wsM1_Area As New m1_area
                    hasil = wsM1_Area.M1_AreaDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_AreaCekId"
                Dim wsM1_Area As New m1_area
                hasil = wsM1_Area.M1_AreaCekId(param)
            Case "M1_AreaTerkait"
                Dim wsM1_Area As New m1_area
                hasil = wsM1_Area.M1_AreaTerkait(param)
            Case "M1_Area_HistorySimpan"
                Dim wsM1_Area As New m1_area_history
                hasil = wsM1_Area.M1_Area_HistorySimpan(param)
            Case "M1_Area_HistorySearch"
                Dim wsM1_Area As New m1_area_history
                hasil = wsM1_Area.M1_Area_HistorySearch(param)

                'M1_LAYANAN
            Case "M1_LayananSimpan"
                Dim wsM1_Layanan As New m1_layanan
                hasil = wsM1_Layanan.M1_LayananSimpan(param)
            Case "M1_LayananSearch"
                Dim wsM1_Layanan As New m1_layanan
                hasil = wsM1_Layanan.M1_LayananSearch(param)
            Case "M1_LayananDelete"
                If (isDemo = False) Then
                    Dim wsM1_Layanan As New m1_layanan
                    hasil = wsM1_Layanan.M1_LayananDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_LayananCekId"
                Dim wsM1_Layanan As New m1_layanan
                hasil = wsM1_Layanan.M1_LayananCekId(param)
            Case "M1_LayananTerkait"
                Dim wsM1_Layanan As New m1_layanan
                hasil = wsM1_Layanan.M1_LayananTerkait(param)

                'M1_BANK
            Case "M1_BankSimpan"
                Dim wsM1_Bank As New m1_bank
                hasil = wsM1_Bank.M1_BankSimpan(param)
            Case "M1_BankSearch"
                Dim wsM1_Bank As New m1_bank
                hasil = wsM1_Bank.M1_BankSearch(param)
            Case "M1_BankDelete"
                If (isDemo = False) Then
                    Dim wsM1_Bank As New m1_bank
                    hasil = wsM1_Bank.M1_BankDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_BankCekId"
                Dim wsM1_Bank As New m1_bank
                hasil = wsM1_Bank.M1_BankCekId(param)
            Case "M1_BankTerkait"
                Dim wsM1_Bank As New m1_bank
                hasil = wsM1_Bank.M1_BankTerkait(param)
            Case "M1_Bank_HistorySimpan"
                Dim wsM1_Bank As New m1_bank_history
                hasil = wsM1_Bank.M1_Bank_HistorySimpan(param)
            Case "M1_Bank_HistorySearch"
                Dim wsM1_Bank As New m1_bank_history
                hasil = wsM1_Bank.M1_Bank_HistorySearch(param)

                'M1_BRANCH
            Case "M1_BranchSimpan"
                Dim wsM1_Branch As New m1_branch
                hasil = wsM1_Branch.M1_BranchSimpan(param)
            Case "M1_BranchSearch"
                Dim wsM1_Branch As New m1_branch
                hasil = wsM1_Branch.M1_BranchSearch(param)
            Case "M1_BranchDelete"
                If (isDemo = False) Then
                    Dim wsM1_Branch As New m1_branch
                    hasil = wsM1_Branch.M1_BranchDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_BranchCekId"
                Dim wsM1_Branch As New m1_branch
                hasil = wsM1_Branch.M1_BranchCekId(param)
            Case "M1_BranchTerkait"
                Dim wsM1_Branch As New m1_branch
                hasil = wsM1_Branch.M1_BranchTerkait(param)
            Case "M1_Branch_HistorySimpan"
                Dim wsM1_Branch As New m1_branch_history
                hasil = wsM1_Branch.M1_Branch_HistorySimpan(param)
            Case "M1_Branch_HistorySearch"
                Dim wsM1_Branch As New m1_branch_history
                hasil = wsM1_Branch.M1_Branch_HistorySearch(param)

                'M1_BED
            Case "M1_BedSimpan"
                Dim wsM1_Bed As New m1_bed
                hasil = wsM1_Bed.M1_BedSimpan(param)
            Case "M1_BedSearch"
                Dim wsM1_Bed As New m1_bed
                hasil = wsM1_Bed.M1_BedSearch(param)
            Case "M1_BedDelete"
                If (isDemo = False) Then
                    Dim wsM1_Bed As New m1_bed
                    hasil = wsM1_Bed.M1_BedDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_BedCekId"
                Dim wsM1_Bed As New m1_bed
                hasil = wsM1_Bed.M1_BedCekId(param)
            Case "M1_BedTerkait"
                Dim wsM1_Bed As New m1_bed
                hasil = wsM1_Bed.M1_BedTerkait(param)
            Case "M1_Bed_HistorySimpan"
                Dim wsM1_Bed As New m1_bed_history
                hasil = wsM1_Bed.M1_Bed_HistorySimpan(param)
            Case "M1_Bed_HistorySearch"
                Dim wsM1_Bed As New m1_bed_history
                hasil = wsM1_Bed.M1_Bed_HistorySearch(param)

                'M1_CITY
            Case "M1_CitySimpan"
                Dim wsM1_City As New m1_city
                hasil = wsM1_City.M1_CitySimpan(param)
            Case "M1_CitySearch"
                Dim wsM1_City As New m1_city
                hasil = wsM1_City.M1_CitySearch(param)
            Case "M1_CityDelete"
                If (isDemo = False) Then
                    Dim wsM1_City As New m1_city
                    hasil = wsM1_City.M1_CityDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_CityCekId"
                Dim wsM1_City As New m1_city
                hasil = wsM1_City.M1_CityCekId(param)
            Case "M1_CityTerkait"
                Dim wsM1_City As New m1_city
                hasil = wsM1_City.M1_CityTerkait(param)
            Case "M1_City_HistorySimpan"
                Dim wsM1_city As New m1_city_history
                hasil = wsM1_city.M1_City_HistorySimpan(param)
            Case "M1_City_HistorySearch"
                Dim wsM1_city As New m1_city_history
                hasil = wsM1_city.M1_City_HistorySearch(param)

                'M1_COA
            Case "M1_CoaSimpan"
                Dim wsM1_Coa As New m1_coa
                hasil = wsM1_Coa.M1_CoaSimpan(param)
            Case "M1_CoaSearch"
                Dim wsM1_Coa As New m1_coa
                hasil = wsM1_Coa.M1_CoaSearch(param)
            Case "M1_CoaDelete"
                If (isDemo = False) Then
                    Dim wsM1_Coa As New m1_coa
                    hasil = wsM1_Coa.M1_CoaDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_CoaCekId"
                Dim wsM1_Coa As New m1_coa
                hasil = wsM1_Coa.M1_CoaCekId(param)
            Case "M1_CoaTerkait"
                Dim wsM1_Coa As New m1_coa
                hasil = wsM1_Coa.M1_CoaTerkait(param)
            Case "M1_Coa_HistorySimpan"
                Dim wsM1_Coa As New m1_coa_history
                hasil = wsM1_Coa.M1_Coa_HistorySimpan(param)
            Case "M1_Coa_HistorySearch"
                Dim wsM1_Coa As New m1_coa_history
                hasil = wsM1_Coa.M1_Coa_HistorySearch(param)

                'M1_COMMISSION
            Case "M1_CommissionSimpan"
                Dim wsM1_Commission As New m1_commission
                hasil = wsM1_Commission.M1_CommissionSimpan(param)
            Case "M1_CommissionSearch"
                Dim wsM1_Commission As New m1_commission
                hasil = wsM1_Commission.M1_CommissionSearch(param)
            Case "M1_CommissionGetdataById"
                Dim wsM1_Commission As New m1_commission
                hasil = wsM1_Commission.M1_CommissionGetdataById(param)
            Case "M1_CommissionDelete"
                If (isDemo = False) Then
                    Dim wsM1_Commission As New m1_commission
                    hasil = wsM1_Commission.M1_CommissionDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_CommissionCekId"
                Dim wsM1_Commission As New m1_commission
                hasil = wsM1_Commission.M1_CommissionCekId(param)
            Case "M1_CommissionTerkait"
                Dim wsM1_Commission As New m1_commission
                hasil = wsM1_Commission.M1_CommissionTerkait(param)
            Case "M1_Commission_HistorySimpan"
                Dim wsM1_Commission As New m1_commission_history
                hasil = wsM1_Commission.M1_Commission_HistorySimpan(param)
            Case "M1_Commission_HistorySearch"
                Dim wsM1_Commission As New m1_commission_history
                hasil = wsM1_Commission.M1_Commission_HistorySearch(param)
                'M1_CONTACT
            Case "M1_ContactSimpan"
                Dim wsM1_Contact As New m1_contact
                hasil = wsM1_Contact.M1_ContactSimpan(param)
            Case "M1_ContactGetdataById"
                Dim wsM1_Contact As New m1_contact
                hasil = wsM1_Contact.M1_ContactGetdataById(param)
            Case "M1_ContactSearch"
                Dim wsM1_Contact As New m1_contact
                hasil = wsM1_Contact.M1_ContactSearch(param)
            Case "M1_ContactDelete"
                If (isDemo = False) Then
                    Dim wsM1_Contact As New m1_contact
                    hasil = wsM1_Contact.M1_ContactDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_ContactTerkait"
                Dim wsM1_Contact As New m1_contact
                hasil = wsM1_Contact.M1_ContactTerkait(param)
            Case "M1_ContactCekId"
                Dim wsM1_Contact As New m1_contact
                hasil = wsM1_Contact.M1_ContactCekId(param)
            Case "M1_Contact_HistorySimpan"
                Dim wsM1_Contact As New m1_Contact_History
                hasil = wsM1_Contact.M1_Contact_HistorySimpan(param)
            Case "M1_Contact_HistorySearch"
                Dim wsM1_Contact As New m1_Contact_History
                hasil = wsM1_Contact.M1_Contact_HistorySearch(param)
            Case "M1_Contact_HistoryGetdataById"
                Dim wsM1_Contact As New m1_Contact_History
                hasil = wsM1_Contact.M1_Contact_HistoryGetdataById(param)

                'M1_CONTACT_ATTENTION
            Case "M1_Contact_AttentionSimpan"
                Dim wsM1_Contact_Attention As New m1_contact_attention
                hasil = wsM1_Contact_Attention.M1_Contact_AttentionSimpan(param)
            Case "M1_Contact_AttentionSearch"
                Dim wsM1_Contact_Attention As New m1_contact_attention
                hasil = wsM1_Contact_Attention.M1_Contact_AttentionSearch(param)
            Case "M1_Contact_AttentionDelete"
                If (isDemo = False) Then
                    Dim wsM1_Contact_Attention As New m1_contact_attention
                    hasil = wsM1_Contact_Attention.M1_Contact_AttentionDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Contact_AttentionTerkait"
                Dim wsM1_Contact_Attention As New m1_contact_attention
                hasil = wsM1_Contact_Attention.M1_Contact_AttentionTerkait(param)
            Case "M1_Contact_Attention_HistorySimpan"
                Dim wsM1_Contact_Attention As New m1_contact_attention_history
                hasil = wsM1_Contact_Attention.M1_Contact_Attention_HistorySimpan(param)
            Case "M1_Contact_Attention_HistorySearch"
                Dim wsM1_Contact_Attention As New m1_contact_attention_history
                hasil = wsM1_Contact_Attention.M1_Contact_Attention_HistorySearch(param)

                'M1_CONTACT_CATEGORY
            Case "M1_Contact_CategorySimpan"
                Dim wsM1_Contact_Category As New m1_contact_category
                hasil = wsM1_Contact_Category.M1_Contact_CategorySimpan(param)
            Case "M1_Contact_CategorySearch"
                Dim wsM1_Contact_Category As New m1_contact_category
                hasil = wsM1_Contact_Category.M1_Contact_CategorySearch(param)
            Case "M1_Contact_CategoryDelete"
                If (isDemo = False) Then
                    Dim wsM1_Contact_Category As New m1_contact_category
                    hasil = wsM1_Contact_Category.M1_Contact_CategoryDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Contact_CategoryCekId"
                Dim wsM1_Contact_Category As New m1_contact_category
                hasil = wsM1_Contact_Category.M1_Contact_CategoryCekId(param)
            Case "M1_Contact_CategoryTerkait"
                Dim wsM1_Contact_Category As New m1_contact_category
                hasil = wsM1_Contact_Category.M1_Contact_CategoryTerkait(param)
            Case "M1_Contact_Category_HistorySimpan"
                Dim wsM1_Contact_Category As New m1_contact_category_history
                hasil = wsM1_Contact_Category.M1_Contact_Category_HistorySimpan(param)
            Case "M1_Contact_Category_HistorySearch"
                Dim wsM1_Contact_Category As New m1_contact_category_history
                hasil = wsM1_Contact_Category.M1_Contact_Category_HistorySearch(param)

                'M1_CONTACT_COMMENT
            Case "M1_Contact_CommentSimpan"
                Dim wsM1_Contact_Comment As New m1_contact_comment
                hasil = wsM1_Contact_Comment.M1_Contact_CommentSimpan(param)
            Case "M1_Contact_CommentSearch"
                Dim wsM1_Contact_Comment As New m1_contact_comment
                hasil = wsM1_Contact_Comment.M1_Contact_CommentSearch(param)
            Case "M1_Contact_CommentDelete"
                If (isDemo = False) Then
                    Dim wsM1_Contact_Comment As New m1_contact_comment
                    hasil = wsM1_Contact_Comment.M1_Contact_CommentDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Contact_CommentTerkait"
                Dim wsM1_Contact_Comment As New m1_contact_comment
                hasil = wsM1_Contact_Comment.M1_Contact_CommentTerkait(param)
            Case "M1_Contact_Comment_HistorySimpan"
                Dim wsM1_Contact_Comment As New m1_contact_comment_history
                hasil = wsM1_Contact_Comment.M1_Contact_Comment_HistorySimpan(param)
            Case "M1_Contact_Comment_HistorySearch"
                Dim wsM1_Contact_Comment As New m1_contact_comment_history
                hasil = wsM1_Contact_Comment.M1_Contact_Comment_HistorySearch(param)

                'M1_COST_CENTER
            Case "M1_Cost_CenterSimpan"
                Dim wsM1_Cost_Center As New m1_cost_center
                hasil = wsM1_Cost_Center.M1_Cost_CenterSimpan(param)
            Case "M1_Cost_CenterSearch"
                Dim wsM1_Cost_Center As New m1_cost_center
                hasil = wsM1_Cost_Center.M1_Cost_CenterSearch(param)
            Case "M1_Cost_CenterDelete"
                If (isDemo = False) Then
                    Dim wsM1_Cost_Center As New m1_cost_center
                    hasil = wsM1_Cost_Center.M1_Cost_CenterDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Cost_CenterCekId"
                Dim wsM1_Cost_Center As New m1_cost_center
                hasil = wsM1_Cost_Center.M1_Cost_CenterCekId(param)
            Case "M1_Cost_CenterTerkait"
                Dim wsM1_Cost_Center As New m1_cost_center
                hasil = wsM1_Cost_Center.M1_Cost_CenterTerkait(param)
            Case "M1_Cost_Center_HistorySimpan"
                Dim wsM1_Cost_Center As New m1_cost_center_history
                hasil = wsM1_Cost_Center.M1_Cost_Center_HistorySimpan(param)
            Case "M1_Cost_Center_HistorySearch"
                Dim wsM1_Cost_Center As New m1_cost_center_history
                hasil = wsM1_Cost_Center.M1_Cost_Center_HistorySearch(param)

                'M1_COUNTRY
            Case "M1_CountrySimpan"
                Dim wsM1_Country As New m1_country
                hasil = wsM1_Country.M1_CountrySimpan(param)
            Case "M1_CountrySearch"
                Dim wsM1_Country As New m1_country
                hasil = wsM1_Country.M1_CountrySearch(param)
            Case "M1_CountryDelete"
                If (isDemo = False) Then
                    Dim wsM1_Country As New m1_country
                    hasil = wsM1_Country.M1_CountryDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_CountryCekId"
                Dim wsM1_Country As New m1_country
                hasil = wsM1_Country.M1_CountryCekId(param)
            Case "M1_CountryTerkait"
                Dim wsM1_Country As New m1_country
                hasil = wsM1_Country.M1_CountryTerkait(param)
            Case "M1_Country_HistorySimpan"
                Dim wsM1_Country As New m1_country_history
                hasil = wsM1_Country.M1_Country_HistorySimpan(param)
            Case "M1_Country_HistorySearch"
                Dim wsM1_Country As New m1_country_history
                hasil = wsM1_Country.M1_Country_HistorySearch(param)

                'M1_Diagnosis
            Case "M1_DiagnosisSimpan"
                Dim wsM1_Diagnosis As New m1_diagnosis
                hasil = wsM1_Diagnosis.M1_DiagnosisSimpan(param)
            Case "M1_DiagnosisSearch"
                Dim wsM1_Diagnosis As New m1_diagnosis
                hasil = wsM1_Diagnosis.M1_DiagnosisSearch(param)
            Case "M1_DiagnosisDelete"
                If (isDemo = False) Then
                    Dim wsM1_Diagnosis As New m1_diagnosis
                    hasil = wsM1_Diagnosis.M1_DiagnosisDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_DiagnosisCekId"
                Dim wsM1_Diagnosis As New m1_diagnosis
                hasil = wsM1_Diagnosis.M1_DiagnosisCekId(param)
            Case "M1_DiagnosisTerkait"
                Dim wsM1_Diagnosis As New m1_diagnosis
                hasil = wsM1_Diagnosis.M1_DiagnosisTerkait(param)

                'M1_VILLAGE
            Case "M1_VillageSimpan"
                Dim wsM1_Village As New m1_village
                hasil = wsM1_Village.M1_VillageSimpan(param)
            Case "M1_VillageSearch"
                Dim wsM1_Village As New m1_village
                hasil = wsM1_Village.M1_VillageSearch(param)
            Case "M1_VillageDelete"
                If (isDemo = False) Then
                    Dim wsM1_Village As New m1_village
                    hasil = wsM1_Village.M1_VillageDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_VillageCekId"
                Dim wsM1_Village As New m1_village
                hasil = wsM1_Village.M1_VillageCekId(param)
            Case "M1_VillageTerkait"
                Dim wsM1_Village As New m1_village
                hasil = wsM1_Village.M1_VillageTerkait(param)

                'M1_SUBDISTRICT
            Case "M1_SubdistrictSimpan"
                Dim wsM1_Subdistrict As New m1_subdistrict
                hasil = wsM1_Subdistrict.M1_SubdistrictSimpan(param)
            Case "M1_SubdistrictSearch"
                Dim wsM1_Subdistrict As New m1_subdistrict
                hasil = wsM1_Subdistrict.M1_SubdistrictSearch(param)
            Case "M1_SubdistrictDelete"
                If (isDemo = False) Then
                    Dim wsM1_Subdistrict As New m1_subdistrict
                    hasil = wsM1_Subdistrict.M1_SubdistrictDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_SubdistrictCekId"
                Dim wsM1_Subdistrict As New m1_subdistrict
                hasil = wsM1_Subdistrict.M1_SubdistrictCekId(param)
            Case "M1_SubdistrictTerkait"
                Dim wsM1_Subdistrict As New m1_subdistrict
                hasil = wsM1_Subdistrict.M1_SubdistrictTerkait(param)

                'M1_CURRENCY
            Case "M1_CurrencySimpan"
                Dim wsM1_Currency As New m1_currency
                hasil = wsM1_Currency.M1_CurrencySimpan(param)
            Case "M1_CurrencySearch"
                Dim wsM1_Currency As New m1_currency
                hasil = wsM1_Currency.M1_CurrencySearch(param)
            Case "M1_CurrencyDelete"
                If (isDemo = False) Then
                    Dim wsM1_Currency As New m1_currency
                    hasil = wsM1_Currency.M1_CurrencyDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_CurrencyCekId"
                Dim wsM1_Currency As New m1_currency
                hasil = wsM1_Currency.M1_CurrencyCekId(param)
            Case "M1_CurrencyTerkait"
                Dim wsM1_Currency As New m1_currency
                hasil = wsM1_Currency.M1_CurrencyTerkait(param)
            Case "M1_Currency_HistorySimpan"
                Dim wsM1_Currency As New m1_currency_history
                hasil = wsM1_Currency.M1_Currency_HistorySimpan(param)
            Case "M1_Currency_HistorySearch"
                Dim wsM1_Currency As New m1_currency_history
                hasil = wsM1_Currency.M1_Currency_HistorySearch(param)

                'M1_CUTOMER_CATEGORY
            Case "M1_Customer_CategorySimpan"
                Dim wsM1_Customer_Category As New m1_customer_category
                hasil = wsM1_Customer_Category.M1_Customer_CategorySimpan(param)
            Case "M1_Customer_CategorySearch"
                Dim wsM1_Customer_Category As New m1_customer_category
                hasil = wsM1_Customer_Category.M1_Customer_CategorySearch(param)
            Case "M1_Customer_CategoryDelete"
                If (isDemo = False) Then
                    Dim wsM1_Customer_Category As New m1_customer_category
                    hasil = wsM1_Customer_Category.M1_Customer_CategoryDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Customer_CategoryCekId"
                Dim wsM1_Customer_Category As New m1_customer_category
                hasil = wsM1_Customer_Category.M1_Customer_CategoryCekId(param)
            Case "M1_Customer_CategoryTerkait"
                Dim wsM1_Customer_Category As New m1_customer_category
                hasil = wsM1_Customer_Category.M1_Customer_CategoryTerkait(param)
            Case "M1_Customer_Category_HistorySimpan"
                Dim wsM1_Customer_Category As New m1_customer_category_history
                hasil = wsM1_Customer_Category.M1_Customer_Category_HistorySimpan(param)
            Case "M1_Customer_Category_HistorySearch"
                Dim wsM1_Customer_Category As New m1_customer_category_history
                hasil = wsM1_Customer_Category.M1_Customer_Category_HistorySearch(param)

                'M1_CUTOMER_CATEGORY
            Case "M1_Patient_CategorySimpan"
                Dim wsM1_Patient_Category As New m1_patient_category
                hasil = wsM1_Patient_Category.M1_Patient_CategorySimpan(param)
            Case "M1_Patient_CategorySearch"
                Dim wsM1_Patient_Category As New m1_patient_category
                hasil = wsM1_Patient_Category.M1_Patient_CategorySearch(param)
            Case "M1_Patient_CategoryDelete"
                If (isDemo = False) Then
                    Dim wsM1_Patient_Category As New m1_patient_category
                    hasil = wsM1_Patient_Category.M1_Patient_CategoryDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Patient_CategoryCekId"
                Dim wsM1_Patient_Category As New m1_patient_category
                hasil = wsM1_Patient_Category.M1_Patient_CategoryCekId(param)
            Case "M1_Patient_CategoryTerkait"
                Dim wsM1_Patient_Category As New m1_patient_category
                hasil = wsM1_Patient_Category.M1_Patient_CategoryTerkait(param)

                'M1_DIVISION
            Case "M1_DivisionSimpan"
                Dim wsM1_Division As New m1_division
                hasil = wsM1_Division.M1_DivisionSimpan(param)
            Case "M1_DivisionSearch"
                Dim wsM1_Division As New m1_division
                hasil = wsM1_Division.M1_DivisionSearch(param)
            Case "M1_DivisionDelete"
                If (isDemo = False) Then
                    Dim wsM1_Division As New m1_division
                    hasil = wsM1_Division.M1_DivisionDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_DivisionCekId"
                Dim wsM1_Division As New m1_division
                hasil = wsM1_Division.M1_DivisionCekId(param)
            Case "M1_DivisionTerkait"
                Dim wsM1_Division As New m1_division
                hasil = wsM1_Division.M1_DivisionTerkait(param)
            Case "M1_Division_HistorySimpan"
                Dim wsM1_Division As New m1_division_history
                hasil = wsM1_Division.M1_Division_HistorySimpan(param)
            Case "M1_Division_HistorySearch"
                Dim wsM1_Division As New m1_division_history
                hasil = wsM1_Division.M1_Division_HistorySearch(param)

                'M1_EXPEDITION
            Case "M1_ExpeditionSimpan"
                Dim wsM1_Expedition As New m1_expedition
                hasil = wsM1_Expedition.M1_ExpeditionSimpan(param)
            Case "M1_ExpeditionSearch"
                Dim wsM1_Expedition As New m1_expedition
                hasil = wsM1_Expedition.M1_ExpeditionSearch(param)
            Case "M1_ExpeditionDelete"
                If (isDemo = False) Then
                    Dim wsM1_Expedition As New m1_expedition
                    hasil = wsM1_Expedition.M1_ExpeditionDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_ExpeditionCekId"
                Dim wsM1_Expedition As New m1_expedition
                hasil = wsM1_Expedition.M1_ExpeditionCekId(param)
            Case "M1_ExpeditionTerkait"
                Dim wsM1_Expedition As New m1_expedition
                hasil = wsM1_Expedition.M1_ExpeditionTerkait(param)
            Case "M1_Expedition_HistorySimpan"
                Dim wsM1_Expedition As New m1_expedition_history
                hasil = wsM1_Expedition.M1_Expedition_HistorySimpan(param)
            Case "M1_Expedition_HistorySearch"
                Dim wsM1_Expedition As New m1_expedition_history
                hasil = wsM1_Expedition.M1_Expedition_HistorySearch(param)

                'M1_ITEM
            Case "M1_ItemSimpan"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_ItemSimpan(param)
            Case "M1_ItemSearch"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_ItemSearch(param)
            Case "M1_ItemDelete"
                If (isDemo = False) Then
                    Dim wsM1_Item As New m1_item
                    hasil = wsM1_Item.M1_ItemDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_ItemCekId"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_ItemCekId(param)
            Case "M1_ItemTerkait"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_ItemTerkait(param)
            Case "M1_ItemSimpan2"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_ItemSimpan2(param)
            Case "M1_ItemGetdataById"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_ItemGetdataById(param)
            Case "M1_Item_DataSearch"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_Item_DataSearch(param)
            Case "M1_ItemTransaksiTerakhir"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_ItemTransaksiTerakhir(param)
            Case "M1_Item_HistoryPoSearch"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_Item_HistoryPoSearch(param)
            Case "M1_Item_HistoryRiSearch"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_Item_HistoryRiSearch(param)
            Case "M1_Item_HistorySoSearch"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_Item_HistorySoSearch(param)
            Case "M1_Item_HistorySiSearch"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_Item_HistorySiSearch(param)
            Case "M1_Item_Mutation_StokGetDataAll"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_Item_Mutation_StokGetDataAll(param)
            Case "M1_Item_InformationSearch"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_Item_InformationSearch(param)
            Case "M1_Item_SpecialInSearch"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_Item_SpecialInSearch(param)
            Case "M1_Item_StockMutation"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_Item_StockMutation(param)
            Case "M1_Item_HistoryTransactionSearch"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_Item_HistoryTransactionSearch(param)
            Case "M1_Item_HistorySimpan"
                Dim wsM1_Item As New m1_item_History
                hasil = wsM1_Item.M1_Item_HistorySimpan(param)
            Case "M1_Item_HistorySearch"
                Dim wsM1_Item As New m1_item_History
                hasil = wsM1_Item.M1_Item_HistorySearch(param)
            Case "M1_Item_HistoryGetdataById"
                Dim wsM1_Item As New m1_item_History
                hasil = wsM1_Item.M1_Item_HistoryGetdataById(param)
            Case "M1_ItemImport"
                Dim wsM1_Item As New m1_item
                hasil = wsM1_Item.M1_ItemImport(param)

                'M1_ITEM_ASSEMBLY
            Case "M1_Item_AssemblySimpan"
                Dim wsM1_Item_Assembly As New m1_item_assembly
                hasil = wsM1_Item_Assembly.M1_Item_AssemblySimpan(param)
            Case "M1_Item_AssemblySearch"
                Dim wsM1_Item_Assembly As New m1_item_assembly
                hasil = wsM1_Item_Assembly.M1_Item_AssemblySearch(param)
            Case "M1_Item_AssemblyDelete"
                If (isDemo = False) Then
                    Dim wsM1_Item_Assembly As New m1_item_assembly
                    hasil = wsM1_Item_Assembly.M1_Item_AssemblyDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Item_AssemblyCekId"
                Dim wsM1_Item_Assembly As New m1_item_assembly
                hasil = wsM1_Item_Assembly.M1_Item_AssemblyCekId(param)
            Case "M1_Item_Assembly_HistorySimpan"
                Dim wsM1_Item_Assembly As New m1_item_assembly_history
                hasil = wsM1_Item_Assembly.M1_Item_Assembly_HistorySimpan(param)
            Case "M1_Item_Assembly_HistorySearch"
                Dim wsM1_Item_Assembly As New m1_item_assembly_history
                hasil = wsM1_Item_Assembly.M1_Item_Assembly_HistorySearch(param)

                'M1_ITEM_CATEGORY
            Case "M1_Item_CategorySimpan"
                Dim wsM1_Item_Category As New m1_item_category
                hasil = wsM1_Item_Category.M1_Item_CategorySimpan(param)
            Case "M1_Item_CategorySearch"
                Dim wsM1_Item_Category As New m1_item_category
                hasil = wsM1_Item_Category.M1_Item_CategorySearch(param)
            Case "M1_Item_CategoryDelete"
                If (isDemo = False) Then
                    Dim wsM1_Item_Category As New m1_item_category
                    hasil = wsM1_Item_Category.M1_Item_CategoryDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Item_CategoryCekId"
                Dim wsM1_Item_Category As New m1_item_category
                hasil = wsM1_Item_Category.M1_Item_CategoryCekId(param)
            Case "M1_Item_CategoryTerkait"
                Dim wsM1_Item_Category As New m1_item_category
                hasil = wsM1_Item_Category.M1_Item_CategoryTerkait(param)
            Case "M1_Item_Category_HistorySimpan"
                Dim wsM1_Item_Category As New m1_item_category_history
                hasil = wsM1_Item_Category.M1_Item_Category_HistorySimpan(param)
            Case "M1_Item_Category_HistorySearch"
                Dim wsM1_Item_Category As New m1_item_category_history
                hasil = wsM1_Item_Category.M1_Item_Category_HistorySearch(param)

                'M1_ITEM_LOCATION
            Case "M1_Item_LocationSimpan"
                Dim wsM1_Item_Location As New m1_item_location
                hasil = wsM1_Item_Location.M1_Item_LocationSimpan(param)
            Case "M1_Item_LocationSearch"
                Dim wsM1_Item_Location As New m1_item_location
                hasil = wsM1_Item_Location.M1_Item_LocationSearch(param)
            Case "M1_Item_LocationDelete"
                If (isDemo = False) Then
                    Dim wsM1_Item_Location As New m1_item_location
                    hasil = wsM1_Item_Location.M1_Item_LocationDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Item_LocationCekId"
                Dim wsM1_Item_Location As New m1_item_location
                hasil = wsM1_Item_Location.M1_Item_LocationCekId(param)
            Case "M1_Item_LocationTerkait"
                Dim wsM1_Item_Location As New m1_item_location
                hasil = wsM1_Item_Location.M1_Item_LocationTerkait(param)
            Case "M1_Item_Location_HistorySimpan"
                Dim wsM1_Item_Location As New m1_item_location_history
                hasil = wsM1_Item_Location.M1_Item_Location_HistorySimpan(param)
            Case "M1_Item_Location_HistorySearch"
                Dim wsM1_Item_Location As New m1_item_location_history
                hasil = wsM1_Item_Location.M1_Item_Location_HistorySearch(param)

                'M1_ITEM_LOCATION_WAREHOUSE
            Case "M1_Item_Location_WarehouseSimpan"
                Dim wsM1_Item_Location_Warehouse As New m1_item_location_warehouse
                hasil = wsM1_Item_Location_Warehouse.M1_Item_Location_WarehouseSimpan(param)
            Case "M1_Item_Location_WarehouseSearch"
                Dim wsM1_Item_Location_Warehouse As New m1_item_location_warehouse
                hasil = wsM1_Item_Location_Warehouse.M1_Item_Location_WarehouseSearch(param)
            Case "M1_Item_Location_WarehouseDelete"
                If (isDemo = False) Then
                    Dim wsM1_Item_Location_Warehouse As New m1_item_location_warehouse
                    hasil = wsM1_Item_Location_Warehouse.M1_Item_Location_WarehouseDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Item_Location_WarehouseCekId"
                Dim wsM1_Item_Location_Warehouse As New m1_item_location_warehouse
                hasil = wsM1_Item_Location_Warehouse.M1_Item_Location_WarehouseCekId(param)
            Case "M1_Item_Stock_WarehouseSearch"
                Dim wsM1_Item_Location_Warehouse As New m1_item_location_warehouse
                hasil = wsM1_Item_Location_Warehouse.M1_Item_Stock_WarehouseSearch(param)
            Case "M1_Item_Location_Warehouse_HistorySimpan"
                Dim wsM1_Item_Location_Warehouse As New m1_item_location_warehouse_history
                hasil = wsM1_Item_Location_Warehouse.M1_Item_Location_Warehouse_HistorySimpan(param)
            Case "M1_Item_Location_Warehouse_HistorySearch"
                Dim wsM1_Item_Location_Warehouse As New m1_item_location_warehouse_history
                hasil = wsM1_Item_Location_Warehouse.M1_Item_Location_Warehouse_HistorySearch(param)

                'M1_ITEM_TYPE
            Case "M1_Item_TypeSimpan"
                Dim wsM1_Item_Type As New m1_item_type
                hasil = wsM1_Item_Type.M1_Item_TypeSimpan(param)
            Case "M1_Item_TypeSearch"
                Dim wsM1_Item_Type As New m1_item_type
                hasil = wsM1_Item_Type.M1_Item_TypeSearch(param)
            Case "M1_Item_TypeDelete"
                If (isDemo = False) Then
                    Dim wsM1_Item_Type As New m1_item_type
                    hasil = wsM1_Item_Type.M1_Item_TypeDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Item_TypeCekId"
                Dim wsM1_Item_Type As New m1_item_type
                hasil = wsM1_Item_Type.M1_Item_TypeCekId(param)
            Case "M1_Item_TypeTerkait"
                Dim wsM1_Item_Type As New m1_item_type
                hasil = wsM1_Item_Type.M1_Item_TypeTerkait(param)
            Case "M1_Item_Type_HistorySimpan"
                Dim wsM1_Item_Type As New m1_item_type_history
                hasil = wsM1_Item_Type.M1_Item_Type_HistorySimpan(param)
            Case "M1_Item_Type_HistorySearch"
                Dim wsM1_Item_Type As New m1_item_type_history
                hasil = wsM1_Item_Type.M1_Item_Type_HistorySearch(param)

                'M1_LOCATION    
            Case "M1_LocationSimpan"
                Dim wsM1_Location As New m1_location
                hasil = wsM1_Location.M1_LocationSimpan(param)
            Case "M1_LocationSearch"
                Dim wsM1_Location As New m1_location
                hasil = wsM1_Location.M1_LocationSearch(param)
            Case "M1_LocationDelete"
                If (isDemo = False) Then
                    Dim wsM1_Location As New m1_location
                    hasil = wsM1_Location.M1_LocationDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_LocationCekId"
                Dim wsM1_Location As New m1_location
                hasil = wsM1_Location.M1_LocationCekId(param)
            Case "M1_LocationTerkait"
                Dim wsM1_Location As New m1_location
                hasil = wsM1_Location.M1_LocationTerkait(param)
            Case "M1_LocationSimpanKategoriPOS"
                Dim wsM1_Location As New m1_location
                hasil = wsM1_Location.M1_LocationSimpanKategoriPOS(param)
            Case "M1_Location_HistorySimpan"
                Dim wsM1_Location As New m1_location_history
                hasil = wsM1_Location.M1_Location_HistorySimpan(param)
            Case "M1_Location_HistorySearch"
                Dim wsM1_Location As New m1_location_history
                hasil = wsM1_Location.M1_Location_HistorySearch(param)


                'M1_OTHER
            Case "M1_OtherSimpan"
                Dim wsM1_Other As New m1_other
                hasil = wsM1_Other.M1_OtherSimpan(param)
            Case "M1_OtherSearch"
                Dim wsM1_Other As New m1_other
                hasil = wsM1_Other.M1_OtherSearch(param)
            Case "M1_OtherDelete"
                If (isDemo = False) Then
                    Dim wsM1_Other As New m1_other
                    hasil = wsM1_Other.M1_OtherDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_OtherCekId"
                Dim wsM1_Other As New m1_other
                hasil = wsM1_Other.M1_OtherCekId(param)
            Case "M1_OtherTerkait"
                Dim wsM1_Other As New m1_other
                hasil = wsM1_Other.M1_OtherTerkait(param)
            Case "M1_Other_HistorySimpan"
                Dim wsM1_Other As New m1_other_history
                hasil = wsM1_Other.M1_Other_HistorySimpan(param)
            Case "M1_Other_HistorySearch"
                Dim wsM1_Other As New m1_other_history
                hasil = wsM1_Other.M1_Other_HistorySearch(param)

                'M1_OTHER_COST
            Case "M1_Other_CostSimpan"
                Dim wsM1_Other_Cost As New m1_other_cost
                hasil = wsM1_Other_Cost.M1_Other_CostSimpan(param)
            Case "M1_Other_CostSearch"
                Dim wsM1_Other_Cost As New m1_other_cost
                hasil = wsM1_Other_Cost.M1_Other_CostSearch(param)
            Case "M1_Other_CostDelete"
                If (isDemo = False) Then
                    Dim wsM1_Other_Cost As New m1_other_cost
                    hasil = wsM1_Other_Cost.M1_Other_CostDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Other_CostCekId"
                Dim wsM1_Other_Cost As New m1_other_cost
                hasil = wsM1_Other_Cost.M1_Other_CostCekId(param)
            Case "M1_Other_CostTerkait"
                Dim wsM1_Other_Cost As New m1_other_cost
                hasil = wsM1_Other_Cost.M1_Other_CostTerkait(param)
            Case "M1_Other_Cost_HistorySimpan"
                Dim wsM1_Other_Cost As New m1_other_cost_history
                hasil = wsM1_Other_Cost.M1_Other_Cost_HistorySimpan(param)
            Case "M1_Other_Cost_HistorySearch"
                Dim wsM1_Other_Cost As New m1_other_cost_history
                hasil = wsM1_Other_Cost.M1_Other_Cost_HistorySearch(param)

                'M1_PROJECT
            Case "M1_ProjectSimpan"
                Dim wsM1_Project As New m1_project
                hasil = wsM1_Project.M1_ProjectSimpan(param)
            Case "M1_ProjectSearch"
                Dim wsM1_Project As New m1_project
                hasil = wsM1_Project.M1_ProjectSearch(param)
            Case "M1_ProjectDelete"
                If (isDemo = False) Then
                    Dim wsM1_Project As New m1_project
                    hasil = wsM1_Project.M1_ProjectDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_ProjectCekId"
                Dim wsM1_Project As New m1_project
                hasil = wsM1_Project.M1_ProjectCekId(param)
            Case "M1_ProjectTerkait"
                Dim wsM1_Project As New m1_project
                hasil = wsM1_Project.M1_ProjectTerkait(param)
            Case "M1_Project_HistorySimpan"
                Dim wsM1_Project As New m1_project_history
                hasil = wsM1_Project.M1_Project_HistorySimpan(param)
            Case "M1_Project_HistorySearch"
                Dim wsM1_Project As New m1_project_history
                hasil = wsM1_Project.M1_Project_HistorySearch(param)

                'M1_PATIENT
            Case "M1_PatientSimpan"
                Dim wsM1_Patient As New m1_patient
                hasil = wsM1_Patient.M1_PatientSimpan(param)
            Case "M1_PatientSearch"
                Dim wsM1_Patient As New m1_patient
                hasil = wsM1_Patient.M1_PatientSearch(param)
            Case "M1_PatientDelete"
                If (isDemo = False) Then
                    Dim wsM1_Patient As New m1_patient
                    hasil = wsM1_Patient.M1_PatientDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_PatientCekId"
                Dim wsM1_Patient As New m1_patient
                hasil = wsM1_Patient.M1_PatientCekId(param)
            Case "M1_PatientTerkait"
                Dim wsM1_Patient As New m1_patient
                hasil = wsM1_Patient.M1_PatientTerkait(param)

                'M1_COLLEAGUE
            Case "M1_ColleagueSimpan"
                Dim wsM1_Colleague As New m1_colleague
                hasil = wsM1_Colleague.M1_ColleagueSimpan(param)
            Case "M1_ColleagueSearch"
                Dim wsM1_Colleague As New m1_colleague
                hasil = wsM1_Colleague.M1_ColleagueSearch(param)
            Case "M1_ColleagueDelete"
                If (isDemo = False) Then
                    Dim wsM1_Colleague As New m1_colleague
                    hasil = wsM1_Colleague.M1_ColleagueDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_ColleagueCekId"
                Dim wsM1_Colleague As New m1_colleague
                hasil = wsM1_Colleague.M1_ColleagueCekId(param)
            Case "M1_ColleagueTerkait"
                Dim wsM1_Colleague As New m1_colleague
                hasil = wsM1_Colleague.M1_ColleagueTerkait(param)

                'M1_PROVINCE
            Case "M1_ProvinceSimpan"
                Dim wsM1_Province As New m1_province
                hasil = wsM1_Province.M1_ProvinceSimpan(param)
            Case "M1_ProvinceSearch"
                Dim wsM1_Province As New m1_province
                hasil = wsM1_Province.M1_ProvinceSearch(param)
            Case "M1_ProvinceDelete"
                If (isDemo = False) Then
                    Dim wsM1_Province As New m1_province
                    hasil = wsM1_Province.M1_ProvinceDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_ProvinceCekId"
                Dim wsM1_Province As New m1_province
                hasil = wsM1_Province.M1_ProvinceCekId(param)
            Case "M1_ProvinceTerkait"
                Dim wsM1_Province As New m1_province
                hasil = wsM1_Province.M1_ProvinceTerkait(param)
            Case "M1_Province_HistorySimpan"
                Dim wsM1_Province As New m1_province_history
                hasil = wsM1_Province.M1_Province_HistorySimpan(param)
            Case "M1_Province_HistorySearch"
                Dim wsM1_Province As New m1_province_history
                hasil = wsM1_Province.M1_Province_HistorySearch(param)

                'M1_REFERENCE
            Case "M1_ReferenceSimpan"
                Dim wsM1_Reference As New m1_reference
                hasil = wsM1_Reference.M1_ReferenceSimpan(param)
            Case "M1_ReferenceSearch"
                Dim wsM1_Reference As New m1_reference
                hasil = wsM1_Reference.M1_ReferenceSearch(param)
            Case "M1_ReferenceDelete"
                If (isDemo = False) Then
                    Dim wsM1_Reference As New m1_reference
                    hasil = wsM1_Reference.M1_ReferenceDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_ReferenceCekId"
                Dim wsM1_Reference As New m1_reference
                hasil = wsM1_Reference.M1_ReferenceCekId(param)
            Case "M1_ReferenceTerkait"
                Dim wsM1_Reference As New m1_reference
                hasil = wsM1_Reference.M1_ReferenceTerkait(param)

                'M1_INSURER
            Case "M1_InsurerSimpan"
                Dim wsM1_Insurer As New m1_insurer
                hasil = wsM1_Insurer.M1_InsurerSimpan(param)
            Case "M1_InsurerSearch"
                Dim wsM1_Insurer As New m1_insurer
                hasil = wsM1_Insurer.M1_InsurerSearch(param)
            Case "M1_InsurerDelete"
                If (isDemo = False) Then
                    Dim wsM1_Insurer As New m1_insurer
                    hasil = wsM1_Insurer.M1_InsurerDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_InsurerCekId"
                Dim wsM1_Insurer As New m1_insurer
                hasil = wsM1_Insurer.M1_InsurerCekId(param)
            Case "M1_InsurerTerkait"
                Dim wsM1_Insurer As New m1_insurer
                hasil = wsM1_Insurer.M1_InsurerTerkait(param)

                'M1_REGION
            Case "M1_RegionSimpan"
                Dim wsM1_Region As New m1_region
                hasil = wsM1_Region.M1_RegionSimpan(param)
            Case "M1_RegionSearch"
                Dim wsM1_Region As New m1_region
                hasil = wsM1_Region.M1_RegionSearch(param)
            Case "M1_RegionDelete"
                If (isDemo = False) Then
                    Dim wsM1_Region As New m1_region
                    hasil = wsM1_Region.M1_RegionDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_RegionTerkait"
                Dim wsM1_Region As New m1_region
                hasil = wsM1_Region.M1_RegionTerkait(param)
            Case "M1_Region_HistorySimpan"
                Dim wsM1_Region As New m1_region_history
                hasil = wsM1_Region.M1_Region_HistorySimpan(param)
            Case "M1_Region_HistorySearch"
                Dim wsM1_Region As New m1_region_history
                hasil = wsM1_Region.M1_Region_HistorySearch(param)

                'M1_ROOM
            Case "M1_RoomSimpan"
                Dim wsM1_Room As New m1_room
                hasil = wsM1_Room.M1_RoomSimpan(param)
            Case "M1_RoomSearch"
                Dim wsM1_Room As New m1_room
                hasil = wsM1_Room.M1_RoomSearch(param)
            Case "M1_RoomDelete"
                If (isDemo = False) Then
                    Dim wsM1_Room As New m1_room
                    hasil = wsM1_Room.M1_RoomDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_RoomTerkait"
                Dim wsM1_Room As New m1_room
                hasil = wsM1_Room.M1_RoomTerkait(param)
            Case "M1_RoomCekId"
                Dim wsM1_Room As New m1_room
                hasil = wsM1_Room.M1_RoomCekId(param)
            Case "M1_Room_HistorySimpan"
                Dim wsM1_Room As New m1_room_history
                hasil = wsM1_Room.M1_Room_HistorySimpan(param)
            Case "M1_Room_HistorySearch"
                Dim wsM1_Room As New m1_room_history
                hasil = wsM1_Room.M1_Room_HistorySearch(param)

                'M1_SALESMAN_CATEGORY
            Case "M1_Salesman_CategorySimpan"
                Dim wsM1_Salesman_Category As New m1_salesman_category
                hasil = wsM1_Salesman_Category.M1_Salesman_CategorySimpan(param)
            Case "M1_Salesman_CategorySearch"
                Dim wsM1_Salesman_Category As New m1_salesman_category
                hasil = wsM1_Salesman_Category.M1_Salesman_CategorySearch(param)
            Case "M1_Salesman_CategoryDelete"
                If (isDemo = False) Then
                    Dim wsM1_Salesman_Category As New m1_salesman_category
                    hasil = wsM1_Salesman_Category.M1_Salesman_CategoryDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Salesman_CategoryCekId"
                Dim wsM1_Salesman_Category As New m1_salesman_category
                hasil = wsM1_Salesman_Category.M1_Salesman_CategoryCekId(param)
            Case "M1_Salesman_CategoryTerkait"
                Dim wsM1_Salesman_Category As New m1_salesman_category
                hasil = wsM1_Salesman_Category.M1_Salesman_CategoryTerkait(param)
            Case "M1_Salesman_Category_HistorySimpan"
                Dim wsM1_Salesman_Category As New m1_salesman_category_history
                hasil = wsM1_Salesman_Category.M1_Salesman_Category_HistorySimpan(param)
            Case "M1_Salesman_Category_HistorySearch"
                Dim wsM1_Salesman_Category As New m1_salesman_category_history
                hasil = wsM1_Salesman_Category.M1_Salesman_Category_HistorySearch(param)

                'M1_SUBDIVISION
            Case "M1_SubdivisionSimpan"
                Dim wsM1_Subdivision As New m1_subdivision
                hasil = wsM1_Subdivision.M1_SubdivisionSimpan(param)
            Case "M1_SubdivisionSearch"
                Dim wsM1_Subdivision As New m1_subdivision
                hasil = wsM1_Subdivision.M1_SubdivisionSearch(param)
            Case "M1_SubdivisionDelete"
                If (isDemo = False) Then
                    Dim wsM1_Subdivision As New m1_subdivision
                    hasil = wsM1_Subdivision.M1_SubdivisionDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_SubdivisionCekId"
                Dim wsM1_Subdivision As New m1_subdivision
                hasil = wsM1_Subdivision.M1_SubdivisionCekId(param)
            Case "M1_SubdivisionTerkait"
                Dim wsM1_Subdivision As New m1_subdivision
                hasil = wsM1_Subdivision.M1_SubdivisionTerkait(param)
            Case "M1_Subdivision_HistorySimpan"
                Dim wsM1_Subdivision As New m1_subdivision_history
                hasil = wsM1_Subdivision.M1_Subdivision_HistorySimpan(param)
            Case "M1_Subdivision_HistorySearch"
                Dim wsM1_Subdivision As New m1_subdivision_history
                hasil = wsM1_Subdivision.M1_Subdivision_HistorySearch(param)


                'M1_SUPPLIER_CATEGORY
            Case "M1_Supplier_CategorySimpan"
                Dim wsM1_Supplier_Category As New m1_supplier_category
                hasil = wsM1_Supplier_Category.M1_Supplier_CategorySimpan(param)
            Case "M1_Supplier_CategorySearch"
                Dim wsM1_Supplier_Category As New m1_supplier_category
                hasil = wsM1_Supplier_Category.M1_Supplier_CategorySearch(param)
            Case "M1_Supplier_CategoryDelete"
                If (isDemo = False) Then
                    Dim wsM1_Supplier_Category As New m1_supplier_category
                    hasil = wsM1_Supplier_Category.M1_Supplier_CategoryDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Supplier_CategoryCekId"
                Dim wsM1_Supplier_Category As New m1_supplier_category
                hasil = wsM1_Supplier_Category.M1_Supplier_CategoryCekId(param)
            Case "M1_Supplier_CategoryTerkait"
                Dim wsM1_Supplier_Category As New m1_supplier_category
                hasil = wsM1_Supplier_Category.M1_Supplier_CategoryTerkait(param)
            Case "M1_Supplier_Category_HistorySimpan"
                Dim wsM1_Supplier_Category As New m1_supplier_category_history
                hasil = wsM1_Supplier_Category.M1_Supplier_Category_HistorySimpan(param)
            Case "M1_Supplier_Category_HistorySearch"
                Dim wsM1_Supplier_Category As New m1_supplier_category_history
                hasil = wsM1_Supplier_Category.M1_Supplier_Category_HistorySearch(param)


                'M1_TAX
            Case "M1_TaxSimpan"
                Dim wsM1_Tax As New m1_tax
                hasil = wsM1_Tax.M1_TaxSimpan(param)
            Case "M1_TaxSearch"
                Dim wsM1_Tax As New m1_tax
                hasil = wsM1_Tax.M1_TaxSearch(param)
            Case "M1_TaxDelete"
                If (isDemo = False) Then
                    Dim wsM1_Tax As New m1_tax
                    hasil = wsM1_Tax.M1_TaxDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_TaxCekId"
                Dim wsM1_Tax As New m1_tax
                hasil = wsM1_Tax.M1_TaxCekId(param)
            Case "M1_TaxTerkait"
                Dim wsM1_Tax As New m1_tax
                hasil = wsM1_Tax.M1_TaxTerkait(param)
            Case "M1_Tax_HistorySimpan"
                Dim wsM1_Tax As New m1_tax_history
                hasil = wsM1_Tax.M1_Tax_HistorySimpan(param)
            Case "M1_Tax_HistorySearch"
                Dim wsM1_Tax As New m1_tax_history
                hasil = wsM1_Tax.M1_Tax_HistorySearch(param)

                'M1_TERMS
            Case "M1_TermsSimpan"
                Dim wsM1_Terms As New m1_terms
                hasil = wsM1_Terms.M1_TermsSimpan(param)
            Case "M1_TermsSearch"
                Dim wsM1_Terms As New m1_terms
                hasil = wsM1_Terms.M1_TermsSearch(param)
            Case "M1_TermsDelete"
                If (isDemo = False) Then
                    Dim wsM1_Terms As New m1_terms
                    hasil = wsM1_Terms.M1_TermsDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_TermsCekId"
                Dim wsM1_Terms As New m1_terms
                hasil = wsM1_Terms.M1_TermsCekId(param)
            Case "M1_TermsTerkait"
                Dim wsM1_Terms As New m1_terms
                hasil = wsM1_Terms.M1_TermsTerkait(param)
            Case "M1_Terms_HistorySimpan"
                Dim wsM1_Terms As New m1_terms_history
                hasil = wsM1_Terms.M1_Terms_HistorySimpan(param)
            Case "M1_Terms_HistorySearch"
                Dim wsM1_Terms As New m1_terms_history
                hasil = wsM1_Terms.M1_Terms_HistorySearch(param)

                'M1_TRANSACTION_NOTE
            Case "M1_Transaction_NoteSimpan"
                Dim wsM1_Transaction_Note As New m1_transaction_note
                hasil = wsM1_Transaction_Note.M1_Transaction_NoteSimpan(param)
            Case "M1_Transaction_NoteSearch"
                Dim wsM1_Transaction_Note As New m1_transaction_note
                hasil = wsM1_Transaction_Note.M1_Transaction_NoteSearch(param)
            Case "M1_Transaction_NoteDelete"
                If (isDemo = False) Then
                    Dim wsM1_Transaction_Note As New m1_transaction_note
                    hasil = wsM1_Transaction_Note.M1_Transaction_NoteDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Transaction_NoteCekId"
                Dim wsM1_Transaction_Note As New m1_transaction_note
                hasil = wsM1_Transaction_Note.M1_Transaction_NoteCekId(param)
            Case "M1_Transaction_NoteTerkait"
                Dim wsM1_Transaction_Note As New m1_transaction_note
                hasil = wsM1_Transaction_Note.M1_Transaction_NoteTerkait(param)
            Case "M1_Transaction_Note_HistorySimpan"
                Dim wsM1_Transaction_Note As New m1_transaction_note_history
                hasil = wsM1_Transaction_Note.M1_Transaction_Note_HistorySimpan(param)
            Case "M1_Transaction_Note_HistorySearch"
                Dim wsM1_Transaction_Note As New m1_transaction_note_history
                hasil = wsM1_Transaction_Note.M1_Transaction_Note_HistorySearch(param)

                'M1_TRANSACTION_NOTE_DETAIL
            Case "M1_Transaction_Note_DetailSimpan"
                Dim wsM1_Transaction_Note_Detail As New m1_transaction_note_detail
                hasil = wsM1_Transaction_Note_Detail.M1_Transaction_Note_DetailSimpan(param)
            Case "M1_Transaction_Note_DetailSearch"
                Dim wsM1_Transaction_Note_Detail As New m1_transaction_note_detail
                hasil = wsM1_Transaction_Note_Detail.M1_Transaction_Note_DetailSearch(param)
            Case "M1_Transaction_Note_DetailDelete"
                If (isDemo = False) Then
                    Dim wsM1_Transaction_Note_Detail As New m1_transaction_note_detail
                    hasil = wsM1_Transaction_Note_Detail.M1_Transaction_Note_DetailDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Transaction_Note_DetailCekId"
                Dim wsM1_Transaction_Note_Detail As New m1_transaction_note_detail
                hasil = wsM1_Transaction_Note_Detail.M1_Transaction_Note_DetailCekId(param)
            Case "M1_Transaction_Note_DetailTerkait"
                Dim wsM1_Transaction_Note_Detail As New m1_transaction_note_detail
                hasil = wsM1_Transaction_Note_Detail.M1_Transaction_Note_DetailTerkait(param)
            Case "M1_Transaction_Note_Detail_HistorySimpan"
                Dim wsM1_Transaction_Note_Detail As New m1_transaction_note_detail_history
                hasil = wsM1_Transaction_Note_Detail.M1_Transaction_Note_Detail_HistorySimpan(param)
            Case "M1_Transaction_Note_Detail_HistorySearch"
                Dim wsM1_Transaction_Note_Detail As New m1_transaction_note_detail_history
                hasil = wsM1_Transaction_Note_Detail.M1_Transaction_Note_Detail_HistorySearch(param)

                'M1_TYPE_SA
            Case "M1_Type_SaSimpan"
                Dim wsM1_Type_Sa As New m1_type_sa
                hasil = wsM1_Type_Sa.M1_Type_SaSimpan(param)
            Case "M1_Type_SaSearch"
                Dim wsM1_Type_Sa As New m1_type_sa
                hasil = wsM1_Type_Sa.M1_Type_SaSearch(param)
            Case "M1_Type_SaDelete"
                If (isDemo = False) Then
                    Dim wsM1_Type_Sa As New m1_type_sa
                    hasil = wsM1_Type_Sa.M1_Type_SaDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Type_SaCekId"
                Dim wsM1_Type_Sa As New m1_type_sa
                hasil = wsM1_Type_Sa.M1_Type_SaCekId(param)
            Case "M1_Type_SaTerkait"
                Dim wsM1_Type_Sa As New m1_type_sa
                hasil = wsM1_Type_Sa.M1_Type_SaTerkait(param)
            Case "M1_Type_Sa_HistorySimpan"
                Dim wsM1_Type_Sa As New m1_type_sa_history
                hasil = wsM1_Type_Sa.M1_Type_Sa_HistorySimpan(param)
            Case "M1_Type_Sa_HistorySearch"
                Dim wsM1_Type_Sa As New m1_type_sa_history
                hasil = wsM1_Type_Sa.M1_Type_Sa_HistorySearch(param)

                'M1_UNIT
            Case "M1_UnitSimpan"
                Dim wsM1_Unit As New m1_unit
                hasil = wsM1_Unit.M1_UnitSimpan(param)
            Case "M1_UnitSearch"
                Dim wsM1_Unit As New m1_unit
                hasil = wsM1_Unit.M1_UnitSearch(param)
            Case "M1_UnitDelete"
                If (isDemo = False) Then
                    Dim wsM1_Unit As New m1_unit
                    hasil = wsM1_Unit.M1_UnitDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_UnitCekId"
                Dim wsM1_Unit As New m1_unit
                hasil = wsM1_Unit.M1_UnitCekId(param)
            Case "M1_UnitTerkait"
                Dim wsM1_Unit As New m1_unit
                hasil = wsM1_Unit.M1_UnitTerkait(param)
            Case "M1_Unit_HistorySimpan"
                Dim wsM1_Unit As New m1_unit_history
                hasil = wsM1_Unit.M1_Unit_HistorySimpan(param)
            Case "M1_Unit_HistorySearch"
                Dim wsM1_Unit As New m1_unit_history
                hasil = wsM1_Unit.M1_Unit_HistorySearch(param)

                'M1_WAREHOUSE
            Case "M1_WarehouseSimpan"
                Dim wsM1_Warehouse As New m1_warehouse
                hasil = wsM1_Warehouse.M1_WarehouseSimpan(param)
            Case "M1_WarehouseSearch"
                Dim wsM1_Warehouse As New m1_warehouse
                hasil = wsM1_Warehouse.M1_WarehouseSearch(param)
            Case "M1_WarehouseDelete"
                If (isDemo = False) Then
                    Dim wsM1_Warehouse As New m1_warehouse
                    hasil = wsM1_Warehouse.M1_WarehouseDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_WarehouseCekId"
                Dim wsM1_Warehouse As New m1_warehouse
                hasil = wsM1_Warehouse.M1_WarehouseCekId(param)
            Case "M1_WarehouseTerkait"
                Dim wsM1_Warehouse As New m1_warehouse
                hasil = wsM1_Warehouse.M1_WarehouseTerkait(param)
            Case "M1_Warehouse_HistorySimpan"
                Dim wsM1_Warehouse As New m1_warehouse_history
                hasil = wsM1_Warehouse.M1_Warehouse_HistorySimpan(param)
            Case "M1_Warehouse_HistorySearch"
                Dim wsM1_Warehouse As New m1_warehouse_history
                hasil = wsM1_Warehouse.M1_Warehouse_HistorySearch(param)

                'M1_WORKING_ESTIMATE
            Case "M1_Working_EstimateSimpan"
                Dim wsM1_Working_Estimate As New m1_working_estimate
                hasil = wsM1_Working_Estimate.M1_Working_EstimateSimpan(param)
            Case "M1_Working_EstimateSearch"
                Dim wsM1_Working_Estimate As New m1_working_estimate
                hasil = wsM1_Working_Estimate.M1_Working_EstimateSearch(param)
            Case "M1_Working_EstimateDelete"
                If (isDemo = False) Then
                    Dim wsM1_Working_Estimate As New m1_working_estimate
                    hasil = wsM1_Working_Estimate.M1_Working_EstimateDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Working_EstimateCekId"
                Dim wsM1_Working_Estimate As New m1_working_estimate
                hasil = wsM1_Working_Estimate.M1_Working_EstimateCekId(param)
            Case "M1_Working_EstimateTerkait"
                Dim wsM1_Working_Estimate As New m1_working_estimate
                hasil = wsM1_Working_Estimate.M1_Working_EstimateTerkait(param)
            Case "M1_Working_Estimate_HistorySimpan"
                Dim wsM1_Working_Estimate As New m1_working_estimate_history
                hasil = wsM1_Working_Estimate.M1_Working_Estimate_HistorySimpan(param)
            Case "M1_Working_Estimate_HistorySearch"
                Dim wsM1_Working_Estimate As New m1_working_estimate_history
                hasil = wsM1_Working_Estimate.M1_Working_Estimate_HistorySearch(param)

                'M1_PRODUCTION_CATEGORY
            Case "M1_Production_CategorySimpan"
                Dim wsM1_Production_Category As New m1_production_category
                hasil = wsM1_Production_Category.M1_Production_CategorySimpan(param)
            Case "M1_Production_CategorySearch"
                Dim wsM1_Production_Category As New m1_production_category
                hasil = wsM1_Production_Category.M1_Production_CategorySearch(param)
            Case "M1_Production_CategoryDelete"
                If (isDemo = False) Then
                    Dim wsM1_Production_Category As New m1_production_category
                    hasil = wsM1_Production_Category.M1_Production_CategoryDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Production_CategoryCekId"
                Dim wsM1_Production_Category As New m1_production_category
                hasil = wsM1_Production_Category.M1_Production_CategoryCekId(param)
            Case "M1_Working_EstimateTerkait"
                Dim wsM1_Production_Category As New m1_production_category
                hasil = wsM1_Production_Category.M1_Production_CategoryTerkait(param)
            Case "M1_Production_Category_HistorySimpan"
                Dim wsM1_Production_Category As New m1_production_category_history
                hasil = wsM1_Production_Category.M1_Production_Category_HistorySimpan(param)
            Case "M1_Production_Category_HistorySearch"
                Dim wsM1_Production_Category As New m1_production_category_history
                hasil = wsM1_Production_Category.M1_Production_Category_HistorySearch(param)


                'M1_ITEM_HAULING
            Case "M1_Item_HaulingSimpan"
                Dim wsM1_Item_Hauling As New m1_item_hauling
                hasil = wsM1_Item_Hauling.M1_Item_HaulingSimpan(param)
            Case "M1_Item_HaulingGetdataAll"
                Dim wsM1_Item_Hauling As New m1_item_hauling
                hasil = wsM1_Item_Hauling.M1_Item_HaulingGetdataAll(param)
            Case "M1_Item_HaulingSearch"
                Dim wsM1_Item_Hauling As New m1_item_hauling
                hasil = wsM1_Item_Hauling.M1_Item_HaulingSearch(param)
            Case "M1_Item_HaulingDelete"
                If (isDemo = False) Then
                    Dim wsM1_Item_Hauling As New m1_item_hauling
                    hasil = wsM1_Item_Hauling.M1_Item_HaulingDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Item_HaulingCekId"
                Dim wsM1_Item_Hauling As New m1_item_hauling
                hasil = wsM1_Item_Hauling.M1_Item_HaulingCekId(param)
            Case "M1_Item_HaulingTerkait"
                Dim wsM1_Item_Hauling As New m1_item_hauling
                hasil = wsM1_Item_Hauling.M1_Item_HaulingTerkait(param)
            Case "M1_Item_Hauling_HistorySimpan"
                Dim wsM1_Item_Hauling As New m1_item_hauling_history
                hasil = wsM1_Item_Hauling.M1_Item_Hauling_HistorySimpan(param)
            Case "M1_Item_Hauling_HistorySearch"
                Dim wsM1_Item_Hauling As New m1_item_hauling_history
                hasil = wsM1_Item_Hauling.M1_Item_Hauling_HistorySearch(param)
            Case "M1_Item_Hauling_HistoryGetdataAll"
                Dim wsM1_Item_Hauling As New m1_item_hauling_history
                hasil = wsM1_Item_Hauling.M1_Item_Hauling_HistoryGetdataAll(param)


                'M1_CHECKING_CATEGORY
            Case "M1_Checking_CategorySimpan"
                Dim wsM1_Checking_Category As New m1_checking_category
                hasil = wsM1_Checking_Category.M1_Checking_CategorySimpan(param)
            Case "M1_Checking_CategorySearch"
                Dim wsM1_Checking_Category As New m1_checking_category
                hasil = wsM1_Checking_Category.M1_Checking_CategorySearch(param)
            Case "M1_Checking_CategoryDelete"
                If (isDemo = False) Then
                    Dim wsM1_Checking_Category As New m1_checking_category
                    hasil = wsM1_Checking_Category.M1_Checking_CategoryDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Checking_CategoryCekId"
                Dim wsM1_Checking_Category As New m1_checking_category
                hasil = wsM1_Checking_Category.M1_Checking_CategoryCekId(param)
            Case "M1_Checking_CategoryTerkait"
                Dim wsM1_Checking_Category As New m1_checking_category
                hasil = wsM1_Checking_Category.M1_Checking_CategoryTerkait(param)
            Case "M1_Checking_Category_HistorySimpan"
                Dim wsM1_Checking_Category As New m1_checking_category_history
                hasil = wsM1_Checking_Category.M1_Checking_Category_HistorySimpan(param)
            Case "M1_Checking_Category_HistorySearch"
                Dim wsM1_Checking_Category As New m1_checking_category_history
                hasil = wsM1_Checking_Category.M1_Checking_Category_HistorySearch(param)


                'M1_SELLING_POINT
            Case "M1_Selling_PointSimpan"
                Dim wsM1_Selling_Point As New m1_selling_point
                hasil = wsM1_Selling_Point.M1_Selling_PointSimpan(param)
            Case "M1_Selling_PointSearch"
                Dim wsM1_Selling_Point As New m1_selling_point
                hasil = wsM1_Selling_Point.M1_Selling_PointSearch(param)
            Case "M1_Selling_PointDelete"
                If (isDemo = False) Then
                    Dim wsM1_Selling_Point As New m1_selling_point
                    hasil = wsM1_Selling_Point.M1_Selling_PointDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Selling_PointCekId"
                Dim wsM1_Selling_Point As New m1_selling_point
                hasil = wsM1_Selling_Point.M1_Selling_PointCekId(param)
            Case "M1_Selling_PointTerkait"
                Dim wsM1_Selling_Point As New m1_selling_point
                hasil = wsM1_Selling_Point.M1_Selling_PointTerkait(param)
            Case "M1_Selling_Point_HistorySimpan"
                Dim wsM1_Selling_Point As New m1_selling_point_history
                hasil = wsM1_Selling_Point.M1_Selling_Point_HistorySimpan(param)
            Case "M1_Selling_Point_HistorySearch"
                Dim wsM1_Selling_Point As New m1_selling_point_history
                hasil = wsM1_Selling_Point.M1_Selling_Point_HistorySearch(param)


                'M1_CLASS_PRODUCT
            Case "M1_Class_ProductSimpan"
                Dim wsM1_ClassProduct As New m1_class_product
                hasil = wsM1_ClassProduct.M1_Class_ProductSimpan(param)
            Case "M1_Class_ProductSearch"
                Dim wsM1_ClassProduct As New m1_class_product
                hasil = wsM1_ClassProduct.M1_Class_ProductSearch(param)
            Case "M1_Class_ProductDelete"
                Dim wsM1_ClassProduct As New m1_class_product
                hasil = wsM1_ClassProduct.M1_Class_ProductDelete(param)
            Case "M1_Class_ProductCekId"
                Dim wsM1_ClassProduct As New m1_class_product
                hasil = wsM1_ClassProduct.M1_Class_ProductCekId(param)
            Case "M1_Class_ProductTerkait"
                Dim wsM1_ClassProduct As New m1_class_product
                hasil = wsM1_ClassProduct.M1_Class_ProductTerkait(param)
            Case "M1_Class_ProductHistorySimpan"
                Dim wsM1_ClassProductHistory As New m1_class_product_history
                hasil = wsM1_ClassProductHistory.M1_Class_ProductHistorySimpan(param)
            Case "M1_Class_ProductHistorySearch"
                Dim wsM1_ClassProductHistory As New m1_class_product_history
                hasil = wsM1_ClassProductHistory.M1_Class_ProductHistorySearch(param)


                'M1_INDEX_PRICE
            Case "M1_Index_PriceSimpan"
                Dim wsM1_IndexPrice As New m1_index_price
                hasil = wsM1_IndexPrice.M1_Index_PriceSimpan(param)
            Case "M1_Index_PriceSearch"
                Dim wsM1_IndexPrice As New m1_index_price
                hasil = wsM1_IndexPrice.M1_Index_PriceSearch(param)
            Case "M1_Index_PriceDelete"
                Dim wsM1_IndexPrice As New m1_index_price
                hasil = wsM1_IndexPrice.M1_Index_PriceDelete(param)
            Case "M1_Index_PriceCekId"
                Dim wsM1_IndexPrice As New m1_index_price
                hasil = wsM1_IndexPrice.M1_Index_PriceCekId(param)
            Case "M1_Index_PriceTerkait"
                Dim wsM1_IndexPrice As New m1_index_price
                hasil = wsM1_IndexPrice.M1_Index_PriceTerkait(param)
            Case "M1_Index_PriceHistorySimpan"
                Dim wsM1_IndexPriceHistory As New m1_index_price_history
                hasil = wsM1_IndexPriceHistory.M1_Index_PriceHistorySimpan(param)
            Case "M1_Index_PriceHistorySearch"
                Dim wsM1_IndexPriceHistory As New m1_index_price_history
                hasil = wsM1_IndexPriceHistory.M1_Index_PriceHistorySearch(param)


                'M1_DEPARTMENT
            Case "M1_DepartmentSimpan"
                Dim wsM1_Department As New m1_department
                hasil = wsM1_Department.M1_DepartmentSimpan(param)
            Case "M1_DepartmentSearch"
                Dim wsM1_Department As New m1_department
                hasil = wsM1_Department.M1_DepartmentSearch(param)
            Case "M1_DepartmentDelete"
                Dim wsM1_Department As New m1_department
                hasil = wsM1_Department.M1_DepartmentDelete(param)
            Case "M1_DepartmentCekId"
                Dim wsM1_Department As New m1_department
                hasil = wsM1_Department.M1_DepartmentCekId(param)
            Case "M1_DepartmentTerkait"
                Dim wsM1_Department As New m1_department
                hasil = wsM1_Department.M1_DepartmentTerkait(param)
            Case "M1_DepartmentHistorySimpan"
                Dim wsM1_DepartmentHistory As New m1_department_history
                hasil = wsM1_DepartmentHistory.M1_DepartmentHistorySimpan(param)
            Case "M1_DepartmentHistorySearch"
                Dim wsM1_DepartmentHistory As New m1_department_history
                hasil = wsM1_DepartmentHistory.M1_DepartmentHistorySearch(param)


                'M1_SUBDEPARTMENT
            Case "M1_SubdepartmentSimpan"
                Dim wsM1_Subdepartment As New m1_subdepartment
                hasil = wsM1_Subdepartment.M1_SubdepartmentSimpan(param)
            Case "M1_SubdepartmentSearch"
                Dim wsM1_Subdepartment As New m1_subdepartment
                hasil = wsM1_Subdepartment.M1_SubdepartmentSearch(param)
            Case "M1_SubdepartmentDelete"
                Dim wsM1_Subdepartment As New m1_subdepartment
                hasil = wsM1_Subdepartment.M1_SubdepartmentDelete(param)
            Case "M1_SubdepartmentCekId"
                Dim wsM1_Subdepartment As New m1_subdepartment
                hasil = wsM1_Subdepartment.M1_SubdepartmentCekId(param)
            Case "M1_SubdepartmentTerkait"
                Dim wsM1_Subdepartment As New m1_subdepartment
                hasil = wsM1_Subdepartment.M1_SubdepartmentTerkait(param)
            Case "M1_SubdepartmentHistorySimpan"
                Dim wsM1_SubdepartmentHistory As New m1_subdepartment_history
                hasil = wsM1_SubdepartmentHistory.M1_SubdepartmentHistorySimpan(param)
            Case "M1_SubdepartmentHistorySearch"
                Dim wsM1_SubdepartmentHistory As New m1_subdepartment_history
                hasil = wsM1_SubdepartmentHistory.M1_SubdepartmentHistorySearch(param)


                'M1_PRICE_CATEGORY
            Case "M1_Price_CategorySimpan"
                Dim wsM1_PriceCategory As New m1_price_category
                hasil = wsM1_PriceCategory.M1_Price_CategorySimpan(param)
            Case "M1_Price_CategorySearch"
                Dim wsM1_PriceCategory As New m1_price_category
                hasil = wsM1_PriceCategory.M1_Price_CategorySearch(param)
            Case "M1_Price_CategoryDelete"
                Dim wsM1_PriceCategory As New m1_price_category
                hasil = wsM1_PriceCategory.M1_Price_CategoryDelete(param)
            Case "M1_Price_CategoryCekId"
                Dim wsM1_PriceCategory As New m1_price_category
                hasil = wsM1_PriceCategory.M1_Price_CategoryCekId(param)
            Case "M1_Price_CategoryTerkait"
                Dim wsM1_PriceCategory As New m1_price_category
                hasil = wsM1_PriceCategory.M1_Price_CategoryTerkait(param)
            Case "M1_Price_CategoryDownload"
                Dim wsM1_PriceCategory As New m1_price_category
                hasil = wsM1_PriceCategory.M1_Price_CategoryDownload(param)
            Case "M1_Price_CategoryImport"
                Dim wsM1_PriceCategory As New m1_price_category
                hasil = wsM1_PriceCategory.M1_Price_CategoryImport(param)
            Case "M1_Price_CategoryHistorySimpan"
                Dim wsM1_PriceCategoryHistory As New m1_price_category_history
                hasil = wsM1_PriceCategoryHistory.M1_Price_CategoryHistorySimpan(param)
            Case "M1_Price_CategoryHistorySearch"
                Dim wsM1_PriceCategoryHistory As New m1_price_category_history
                hasil = wsM1_PriceCategoryHistory.M1_Price_CategoryHistorySearch(param)
            Case "CdM1_Price_Category"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM1_Price_Category(param)


                'M1_ACCIDENT
            Case "M1_AccidentSimpan"
                Dim wsM1_Accident As New m1_accident
                hasil = wsM1_Accident.M1_AccidentSimpan(param)
            Case "M1_AccidentSearch"
                Dim wsM1_Accident As New m1_accident
                hasil = wsM1_Accident.M1_AccidentSearch(param)
            Case "M1_AccidentDelete"
                If (isDemo = False) Then
                    Dim wsM1_Accident As New m1_accident
                    hasil = wsM1_Accident.M1_AccidentDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_AccidentCekId"
                Dim wsM1_Accident As New m1_accident
                hasil = wsM1_Accident.M1_AccidentCekId(param)
            Case "M1_AccidentTerkait"
                Dim wsM1_Accident As New m1_accident
                hasil = wsM1_Accident.M1_AccidentTerkait(param)
            Case "M1_Accident_HistorySimpan"
                Dim wsM1_Accident As New m1_accident_history
                hasil = wsM1_Accident.M1_Accident_HistorySimpan(param)
            Case "M1_Accident_HistorySearch"
                Dim wsM1_Accident As New m1_accident_history
                hasil = wsM1_Accident.M1_Accident_HistorySearch(param)


                'M1_ICD
            Case "M1_IcdSimpan"
                Dim wsM1_Icd As New m1_icd
                hasil = wsM1_Icd.M1_IcdSimpan(param)
            Case "M1_IcdSearch"
                Dim wsM1_Icd As New m1_icd
                hasil = wsM1_Icd.M1_IcdSearch(param)
            Case "M1_IcdDelete"
                If (isDemo = False) Then
                    Dim wsM1_Icd As New m1_icd
                    hasil = wsM1_Icd.M1_IcdDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_IcdCekId"
                Dim wsM1_Icd As New m1_icd
                hasil = wsM1_Icd.M1_IcdCekId(param)
            Case "M1_IcdTerkait"
                Dim wsM1_Icd As New m1_icd
                hasil = wsM1_Icd.M1_IcdTerkait(param)
            Case "M1_Icd_HistorySimpan"
                Dim wsM1_Icd As New m1_icd_history
                hasil = wsM1_Icd.M1_Icd_HistorySimpan(param)
            Case "M1_Icd_HistorySearch"
                Dim wsM1_Icd As New m1_icd_history
                hasil = wsM1_Icd.M1_Icd_HistorySearch(param)

                'M1_TRM
            Case "M1_TrmSimpan"
                Dim wsM1_Trm As New m1_trm
                hasil = wsM1_Trm.M1_TrmSimpan(param)
            Case "M1_TrmSearch"
                Dim wsM1_Trm As New m1_trm
                hasil = wsM1_Trm.M1_TrmSearch(param)
            Case "M1_TrmDelete"
                If (isDemo = False) Then
                    Dim wsM1_Trm As New m1_trm
                    hasil = wsM1_Trm.M1_TrmDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_TrmCekId"
                Dim wsM1_Trm As New m1_trm
                hasil = wsM1_Trm.M1_TrmCekId(param)
            Case "M1_TrmTerkait"
                Dim wsM1_Trm As New m1_trm
                hasil = wsM1_Trm.M1_TrmTerkait(param)

                'M1_LAB_RESULT
            Case "M1_LabResultSimpan"
                Dim wsM1_LabResult As New m1_lab_result
                hasil = wsM1_LabResult.M1_LabResultSimpan(param)
            Case "M1_LabResultSearch"
                Dim wsM1_LabResult As New m1_lab_result
                hasil = wsM1_LabResult.M1_LabResultSearch(param)
            Case "M1_LabResultDelete"
                If (isDemo = False) Then
                    Dim wsM1_LabResult As New m1_lab_result
                    hasil = wsM1_LabResult.M1_LabResultDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_LabResultCekId"
                Dim wsM1_LabResult As New m1_lab_result
                hasil = wsM1_LabResult.M1_LabResultCekId(param)
            Case "M1_LabResultTerkait"
                Dim wsM1_LabResult As New m1_lab_result
                hasil = wsM1_LabResult.M1_LabResultTerkait(param)

                'M1_ITEM_PERMISSION
            Case "M1_Item_PermissionSimpan"
                Dim wsM1_ItemPermission As New m1_item_permission
                hasil = wsM1_ItemPermission.M1_Item_PermissionSimpan(param)

            Case "M1_Item_PermissionDelete"
                Dim wsM1_ItemPermission As New m1_item_permission
                hasil = wsM1_ItemPermission.M1_Item_PermissionDelete(param)

            Case "M1_Item_PermissionSearch"
                Dim wsM1_ItemPermission As New m1_item_permission
                hasil = wsM1_ItemPermission.M1_Item_PermissionSearch(param)

            Case "M1_Item_PermissionCekId"
                Dim wsM1_ItemPermission As New m1_item_permission
                hasil = wsM1_ItemPermission.M1_Item_PermissionCekId(param)

            Case "M1_Item_PermissionTerkait"
                Dim wsM1_ItemPermission As New m1_item_permission
                hasil = wsM1_ItemPermission.M1_Item_PermissionTerkait(param)

            Case "M1_Item_Permission_HistorySimpan"
                Dim wsM1_ItemPermission As New m1_item_permission_history
                hasil = wsM1_ItemPermission.M1_Item_Permission_HistorySimpan(param)

            Case "M1_Item_Permission_HistorySearch"
                Dim wsM1_ItemPermission As New m1_item_permission_history
                hasil = wsM1_ItemPermission.M1_Item_Permission_HistorySearch(param)

                'M1_LABOUR_COST
            Case "M1_Labour_CostSimpan"
                Dim wsM1_Labour_Cost As New m1_labour_cost
                hasil = wsM1_Labour_Cost.M1_Labour_CostSimpan(param)
            Case "M1_Labour_CostSearch"
                Dim wsM1_Labour_Cost As New m1_labour_cost
                hasil = wsM1_Labour_Cost.M1_Labour_CostSearch(param)
            Case "M1_Labour_CostDelete"
                If (isDemo = False) Then
                    Dim wsM1_Labour_Cost As New m1_labour_cost
                    hasil = wsM1_Labour_Cost.M1_Labour_CostDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_Labour_CostCekId"
                Dim wsM1_Labour_Cost As New m1_labour_cost
                hasil = wsM1_Labour_Cost.M1_Labour_CostCekId(param)
            Case "M1_Labour_CostTerkait"
                Dim wsM1_Labour_Cost As New m1_labour_cost
                hasil = wsM1_Labour_Cost.M1_Labour_CostTerkait(param)

                'M1_MACHINE
            Case "M1_MachineSimpan"
                Dim wsM1_Machine As New m1_machine
                hasil = wsM1_Machine.M1_MachineSimpan(param)
            Case "M1_MachineSearch"
                Dim wsM1_Machine As New m1_machine
                hasil = wsM1_Machine.M1_MachineSearch(param)
            Case "M1_MachineDelete"
                If (isDemo = False) Then
                    Dim wsM1_Machine As New m1_machine
                    hasil = wsM1_Machine.M1_MachineDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M1_MachineCekId"
                Dim wsM1_Machine As New m1_machine
                hasil = wsM1_Machine.M1_MachineCekId(param)
            Case "M1_MachineTerkait"
                Dim wsM1_Machine As New m1_machine
                hasil = wsM1_Machine.M1_MachineTerkait(param)

            Case "M1_Machine_HistorySimpan"
                Dim wsM1_Machine As New m1_machine_history
                hasil = wsM1_Machine.M1_Machine_HistorySimpan(param)
            Case "M1_Machine_HistorySearch"
                Dim wsM1_Machine As New m1_machine_history
                hasil = wsM1_Machine.M1_Machine_HistorySearch(param)

                'M1_CLASS
            Case "M1_ClassSimpan"
                Dim wsM1_ClassProduct As New m1_class
                hasil = wsM1_ClassProduct.M1_ClassSimpan(param)
            Case "M1_ClassSearch"
                Dim wsM1_ClassProduct As New m1_class
                hasil = wsM1_ClassProduct.M1_ClassSearch(param)
            Case "M1_ClassDelete"
                Dim wsM1_ClassProduct As New m1_class
                hasil = wsM1_ClassProduct.M1_ClassDelete(param)
            Case "M1_ClassCekId"
                Dim wsM1_ClassProduct As New m1_class
                hasil = wsM1_ClassProduct.M1_ClassCekId(param)
            Case "M1_ClassTerkait"
                Dim wsM1_ClassProduct As New m1_class
                hasil = wsM1_ClassProduct.M1_ClassTerkait(param)
            Case "M1_ClassHistorySimpan"
                Dim wsM1_ClassProductHistory As New m1_class_history
                hasil = wsM1_ClassProductHistory.M1_ClassHistorySimpan(param)
            Case "M1_ClassHistorySearch"
                Dim wsM1_ClassProductHistory As New m1_class_history
                hasil = wsM1_ClassProductHistory.M1_ClassHistorySearch(param)

                'M1_MODEL
            Case "M1_ModelSimpan"
                Dim wsM1_ModelProduct As New m1_model
                hasil = wsM1_ModelProduct.M1_ModelSimpan(param)
            Case "M1_ModelSearch"
                Dim wsM1_ModelProduct As New m1_model
                hasil = wsM1_ModelProduct.M1_ModelSearch(param)
            Case "M1_ModelDelete"
                Dim wsM1_ModelProduct As New m1_model
                hasil = wsM1_ModelProduct.M1_ModelDelete(param)
            Case "M1_ModelCekId"
                Dim wsM1_ModelProduct As New m1_model
                hasil = wsM1_ModelProduct.M1_ModelCekId(param)
            Case "M1_ModelTerkait"
                Dim wsM1_ModelProduct As New m1_model
                hasil = wsM1_ModelProduct.M1_ModelTerkait(param)
            Case "M1_ModelHistorySimpan"
                Dim wsM1_ModelProductHistory As New m1_model_history
                hasil = wsM1_ModelProductHistory.M1_ModelHistorySimpan(param)
            Case "M1_ModelHistorySearch"
                Dim wsM1_ModelProductHistory As New m1_model_history
                hasil = wsM1_ModelProductHistory.M1_ModelHistorySearch(param)

                'M1_SIZE
            Case "M1_SizeSimpan"
                Dim wsM1_SizeProduct As New m1_size
                hasil = wsM1_SizeProduct.M1_SizeSimpan(param)
            Case "M1_SizeSearch"
                Dim wsM1_SizeProduct As New m1_size
                hasil = wsM1_SizeProduct.M1_SizeSearch(param)
            Case "M1_SizeDelete"
                Dim wsM1_SizeProduct As New m1_size
                hasil = wsM1_SizeProduct.M1_SizeDelete(param)
            Case "M1_SizeCekId"
                Dim wsM1_SizeProduct As New m1_size
                hasil = wsM1_SizeProduct.M1_SizeCekId(param)
            Case "M1_SizeTerkait"
                Dim wsM1_SizeProduct As New m1_size
                hasil = wsM1_SizeProduct.M1_SizeTerkait(param)
            Case "M1_SizeHistorySimpan"
                Dim wsM1_SizeProductHistory As New m1_size_history
                hasil = wsM1_SizeProductHistory.M1_SizeHistorySimpan(param)
            Case "M1_SizeHistorySearch"
                Dim wsM1_SizeProductHistory As New m1_size_history
                hasil = wsM1_SizeProductHistory.M1_SizeHistorySearch(param)

                'M1_COLOR
            Case "M1_ColorSimpan"
                Dim wsM1_ColorHistory As New m1_color
                hasil = wsM1_ColorHistory.M1_ColorSimpan(param)
            Case "M1_ColorDelete"
                Dim wsM1_ColorHistory As New m1_color
                hasil = wsM1_ColorHistory.M1_ColorDelete(param)
            Case "M1_ColorSearch"
                Dim wsM1_ColorHistory As New m1_color
                hasil = wsM1_ColorHistory.M1_ColorSearch(param)
            Case "M1_ColorCekId"
                Dim wsM1_ColorHistory As New m1_color
                hasil = wsM1_ColorHistory.M1_ColorCekId(param)
            Case "M1_ColorTerkait"
                Dim wsM1_ColorHistory As New m1_color
                hasil = wsM1_ColorHistory.M1_ColorTerkait(param)
            Case "M1_ColorHistorySimpan"
                Dim wsM1_ColorHistory As New m1_color_history
                hasil = wsM1_ColorHistory.M1_ColorHistorySimpan(param)
            Case "M1_ColorHistorySearch"
                Dim wsM1_ColorHistory As New m1_color_history
                hasil = wsM1_ColorHistory.M1_ColorHistorySearch(param)

                'M1_OEM
            Case "M1_OemSimpan"
                Dim wsM1_OemHistory As New m1_oem
                hasil = wsM1_OemHistory.M1_OemSimpan(param)
            Case "M1_OemDelete"
                Dim wsM1_OemHistory As New m1_oem
                hasil = wsM1_OemHistory.M1_OemDelete(param)
            Case "M1_OemSearch"
                Dim wsM1_OemHistory As New m1_oem
                hasil = wsM1_OemHistory.M1_OemSearch(param)
            Case "M1_OemCekId"
                Dim wsM1_OemHistory As New m1_oem
                hasil = wsM1_OemHistory.M1_OemCekId(param)
            Case "M1_OemTerkait"
                Dim wsM1_OemHistory As New m1_oem
                hasil = wsM1_OemHistory.M1_OemTerkait(param)
            Case "M1_OemHistorySimpan"
                Dim wsM1_OemHistory As New m1_oem_history
                hasil = wsM1_OemHistory.M1_OemHistorySimpan(param)
            Case "M1_OemHistorySearch"
                Dim wsM1_OemHistory As New m1_oem_history
                hasil = wsM1_OemHistory.M1_OemHistorySearch(param)

                'M1_MERK
            Case "M1_MerkSimpan"
                Dim wsM1_MerkHistory As New m1_merk
                hasil = wsM1_MerkHistory.M1_MerkSimpan(param)
            Case "M1_MerkDelete"
                Dim wsM1_MerkHistory As New m1_merk
                hasil = wsM1_MerkHistory.M1_MerkDelete(param)
            Case "M1_MerkSearch"
                Dim wsM1_MerkHistory As New m1_merk
                hasil = wsM1_MerkHistory.M1_MerkSearch(param)
            Case "M1_MerkCekId"
                Dim wsM1_MerkHistory As New m1_merk
                hasil = wsM1_MerkHistory.M1_MerkCekId(param)
            Case "M1_MerkTerkait"
                Dim wsM1_MerkHistory As New m1_merk
                hasil = wsM1_MerkHistory.M1_MerkTerkait(param)
            Case "M1_MerkHistorySimpan"
                Dim wsM1_MerkHistory As New m1_merk_history
                hasil = wsM1_MerkHistory.M1_MerkHistorySimpan(param)
            Case "M1_MerkHistorySearch"
                Dim wsM1_MerkHistory As New m1_merk_history
                hasil = wsM1_MerkHistory.M1_MerkHistorySearch(param)

                'M1_MATERIAL
            Case "M1_MaterialSimpan"
                Dim wsM1_MaterialHistory As New m1_material
                hasil = wsM1_MaterialHistory.M1_MaterialSimpan(param)
            Case "M1_MaterialDelete"
                Dim wsM1_MaterialHistory As New m1_material
                hasil = wsM1_MaterialHistory.M1_MaterialDelete(param)
            Case "M1_MaterialSearch"
                Dim wsM1_MaterialHistory As New m1_material
                hasil = wsM1_MaterialHistory.M1_MaterialSearch(param)
            Case "M1_MaterialCekId"
                Dim wsM1_MaterialHistory As New m1_material
                hasil = wsM1_MaterialHistory.M1_MaterialCekId(param)
            Case "M1_MaterialTerkait"
                Dim wsM1_MaterialHistory As New m1_material
                hasil = wsM1_MaterialHistory.M1_MaterialTerkait(param)
            Case "M1_MaterialHistorySimpan"
                Dim wsM1_MaterialHistory As New m1_material_history
                hasil = wsM1_MaterialHistory.M1_MaterialHistorySimpan(param)
            Case "M1_MaterialHistorySearch"
                Dim wsM1_MaterialHistory As New m1_material_history
                hasil = wsM1_MaterialHistory.M1_MaterialHistorySearch(param)

                'M1_SECTION
            Case "M1_SectionSimpan"
                Dim wsM1_SectionHistory As New m1_section
                hasil = wsM1_SectionHistory.M1_SectionSimpan(param)
            Case "M1_SectionDelete"
                Dim wsM1_SectionHistory As New m1_section
                hasil = wsM1_SectionHistory.M1_SectionDelete(param)
            Case "M1_SectionSearch"
                Dim wsM1_SectionHistory As New m1_section
                hasil = wsM1_SectionHistory.M1_SectionSearch(param)
            Case "M1_SectionCekId"
                Dim wsM1_SectionHistory As New m1_section
                hasil = wsM1_SectionHistory.M1_SectionCekId(param)
            Case "M1_SectionTerkait"
                Dim wsM1_SectionHistory As New m1_section
                hasil = wsM1_SectionHistory.M1_SectionTerkait(param)
            Case "M1_SectionHistorySimpan"
                Dim wsM1_SectionHistory As New m1_section_history
                hasil = wsM1_SectionHistory.M1_SectionHistorySimpan(param)
            Case "M1_SectionHistorySearch"
                Dim wsM1_SectionHistory As New m1_section_history
                hasil = wsM1_SectionHistory.M1_SectionHistorySearch(param)

                'M1_VENDOR
            Case "M1_VendorSimpan"
                Dim wsM1_VendorHistory As New m1_vendor
                hasil = wsM1_VendorHistory.M1_VendorSimpan(param)
            Case "M1_VendorDelete"
                Dim wsM1_VendorHistory As New m1_vendor
                hasil = wsM1_VendorHistory.M1_VendorDelete(param)
            Case "M1_VendorSearch"
                Dim wsM1_VendorHistory As New m1_vendor
                hasil = wsM1_VendorHistory.M1_VendorSearch(param)
            Case "M1_VendorCekId"
                Dim wsM1_VendorHistory As New m1_vendor
                hasil = wsM1_VendorHistory.M1_VendorCekId(param)
            Case "M1_VendorTerkait"
                Dim wsM1_VendorHistory As New m1_vendor
                hasil = wsM1_VendorHistory.M1_VendorTerkait(param)
            Case "M1_VendorHistorySimpan"
                Dim wsM1_VendorHistory As New m1_vendor_history
                hasil = wsM1_VendorHistory.M1_VendorHistorySimpan(param)
            Case "M1_VendorHistorySearch"
                Dim wsM1_VendorHistory As New m1_vendor_history
                hasil = wsM1_VendorHistory.M1_VendorHistorySearch(param)

                'M1_DESIGNER
            Case "M1_DesignerSimpan"
                Dim wsM1_DesignerHistory As New m1_designer
                hasil = wsM1_DesignerHistory.M1_DesignerSimpan(param)
            Case "M1_DesignerDelete"
                Dim wsM1_DesignerHistory As New m1_designer
                hasil = wsM1_DesignerHistory.M1_DesignerDelete(param)
            Case "M1_DesignerSearch"
                Dim wsM1_DesignerHistory As New m1_designer
                hasil = wsM1_DesignerHistory.M1_DesignerSearch(param)
            Case "M1_DesignerCekId"
                Dim wsM1_DesignerHistory As New m1_designer
                hasil = wsM1_DesignerHistory.M1_DesignerCekId(param)
            Case "M1_DesignerTerkait"
                Dim wsM1_DesignerHistory As New m1_designer
                hasil = wsM1_DesignerHistory.M1_DesignerTerkait(param)
            Case "M1_DesignerHistorySimpan"
                Dim wsM1_DesignerHistory As New m1_designer_history
                hasil = wsM1_DesignerHistory.M1_DesignerHistorySimpan(param)
            Case "M1_DesignerHistorySearch"
                Dim wsM1_DesignerHistory As New m1_designer_history
                hasil = wsM1_DesignerHistory.M1_DesignerHistorySearch(param)

                'M1_SUBCLASS
            Case "M1_SubclassSimpan"
                Dim wsM1_SubclassHistory As New m1_subclass
                hasil = wsM1_SubclassHistory.M1_SubclassSimpan(param)
            Case "M1_SubclassDelete"
                Dim wsM1_SubclassHistory As New m1_subclass
                hasil = wsM1_SubclassHistory.M1_SubclassDelete(param)
            Case "M1_SubclassSearch"
                Dim wsM1_SubclassHistory As New m1_subclass
                hasil = wsM1_SubclassHistory.M1_SubclassSearch(param)
            Case "M1_SubclassCekId"
                Dim wsM1_SubclassHistory As New m1_subclass
                hasil = wsM1_SubclassHistory.M1_SubclassCekId(param)
            Case "M1_SubclassTerkait"
                Dim wsM1_SubclassHistory As New m1_subclass
                hasil = wsM1_SubclassHistory.M1_SubclassTerkait(param)
            Case "M1_SubclassHistorySimpan"
                Dim wsM1_SubclassHistory As New m1_subclass_history
                hasil = wsM1_SubclassHistory.M1_SubclassHistorySimpan(param)
            Case "M1_SubclassHistorySearch"
                Dim wsM1_SubclassHistory As New m1_subclass_history
                hasil = wsM1_SubclassHistory.M1_SubclassHistorySearch(param)

                'M1_PRODUCTION_ACTIVITY
            Case "M1_Production_ActivitySimpan"
                Dim wsM1_Production_Activity As New m1_production_activity
                hasil = wsM1_Production_Activity.M1_Production_ActivitySimpan(param)
            Case "M1_Production_ActivityDelete"
                Dim wsM1_Production_Activity As New m1_production_activity
                hasil = wsM1_Production_Activity.M1_Production_ActivityDelete(param)
            Case "M1_Production_ActivitySearch"
                Dim wsM1_Production_Activity As New m1_production_activity
                hasil = wsM1_Production_Activity.M1_Production_ActivitySearch(param)
            Case "M1_Production_ActivityCekId"
                Dim wsM1_Production_Activity As New m1_production_activity
                hasil = wsM1_Production_Activity.M1_Production_ActivityCekId(param)
            Case "M1_Production_ActivityTerkait"
                Dim wsM1_Production_Activity As New m1_production_activity
                hasil = wsM1_Production_Activity.M1_Production_ActivityTerkait(param)
            Case "M1_Production_ActivityGetdataById"
                Dim wsM1_Production_Activity As New m1_production_activity
                hasil = wsM1_Production_Activity.M1_Production_ActivityGetdataById(param)
            Case "M1_Production_Activity_HistorySimpan"
                Dim wsM1_Production_ActivityHistory As New m1_production_activity_history
                hasil = wsM1_Production_ActivityHistory.M1_Production_Activity_HistorySimpan(param)
            Case "M1_Production_Activity_HistorySearch"
                Dim wsM1_Production_ActivityHistory As New m1_production_activity_history
                hasil = wsM1_Production_ActivityHistory.M1_Production_Activity_HistorySearch(param)
            Case "M1_Production_Activity_HistoryGetdataById"
                Dim wsM1_Production_ActivityHistory As New m1_production_activity_history
                hasil = wsM1_Production_ActivityHistory.M1_Production_Activity_HistoryGetdataById(param)

                'M1_PRODUCTION_ROUTE
            Case "M1_Production_RouteSimpan"
                Dim wsM1_Production_Route As New m1_production_route
                hasil = wsM1_Production_Route.M1_Production_RouteSimpan(param)
            Case "M1_Production_RouteDelete"
                Dim wsM1_Production_Route As New m1_production_route
                hasil = wsM1_Production_Route.M1_Production_RouteDelete(param)
            Case "M1_Production_RouteSearch"
                Dim wsM1_Production_Route As New m1_production_route
                hasil = wsM1_Production_Route.M1_Production_RouteSearch(param)
            Case "M1_Production_RouteCekId"
                Dim wsM1_Production_Route As New m1_production_route
                hasil = wsM1_Production_Route.M1_Production_RouteCekId(param)
            Case "M1_Production_RouteTerkait"
                Dim wsM1_Production_Route As New m1_production_route
                hasil = wsM1_Production_Route.M1_Production_RouteTerkait(param)
            Case "M1_Production_RouteGetdataById"
                Dim wsM1_Production_Route As New m1_production_route
                hasil = wsM1_Production_Route.M1_Production_RouteGetdataById(param)
            Case "M1_Production_Route_HistorySimpan"
                Dim wsM1_Production_RouteHistory As New m1_production_route_history
                hasil = wsM1_Production_RouteHistory.M1_Production_Route_HistorySimpan(param)
            Case "M1_Production_Route_HistorySearch"
                Dim wsM1_Production_RouteHistory As New m1_production_route_history
                hasil = wsM1_Production_RouteHistory.M1_Production_Route_HistorySearch(param)
            Case "M1_Production_Route_HistoryGetdataById"
                Dim wsM1_Production_RouteHistory As New m1_production_route_history
                hasil = wsM1_Production_RouteHistory.M1_Production_Route_HistoryGetdataById(param)

                '*********************************** M2 '***********************************

                'M2_PRINT
            Case "M2_Print"
                Dim wsM2_Print As New m2_print
                hasil = wsM2_Print.M2_Print(param)

                'M2_NOTES
            Case "M2_NotesSimpan"
                Dim wsM2_Notes As New m2_notes
                hasil = wsM2_Notes.M2_NotesSimpan(param)
            Case "M2_NotesSearch"
                Dim wsM2_Notes As New m2_notes
                hasil = wsM2_Notes.M2_NotesSearch(param)
            Case "M2_NotesDelete"
                Dim wsM2_Notes As New m2_notes
                hasil = wsM2_Notes.M2_NotesDelete(param)

                'M2_FILES
            Case "M2_FilesSimpan"
                Dim wsM2_Files As New m2_files
                hasil = wsM2_Files.M2_FilesSimpan(param)
            Case "M2_FilesSearch"
                Dim wsM2_Files As New m2_files
                hasil = wsM2_Files.M2_FilesSearch(param)
            Case "M2_FilesDelete"
                Dim wsM2_Files As New m2_files
                hasil = wsM2_Files.M2_FilesDelete(param)

                'M2_ACCOUNTING_PERIOD
            Case "M2_Accounting_PeriodSimpan"
                Dim wsM2_Accounting_Period As New m2_accounting_period
                hasil = wsM2_Accounting_Period.M2_Accounting_PeriodSimpan(param)
            Case "M2_Accounting_PeriodSearch"
                Dim wsM2_Accounting_Period As New m2_accounting_period
                hasil = wsM2_Accounting_Period.M2_Accounting_PeriodSearch(param)
            Case "M2_Accounting_PeriodDelete"
                Dim wsM2_Accounting_Period As New m2_accounting_period
                hasil = wsM2_Accounting_Period.M2_Accounting_PeriodDelete(param)

                'm0_backup_data
            Case "m0_backup_datasearch"
                Using wsm As New m0_backup_data
                    hasil = wsm.m0_backup_dataSearch(param)
                End Using
            Case "m0_backup_dataSimpan"
                Using wsm As New m0_backup_data
                    hasil = wsm.m0_backup_dataSimpan(param)
                End Using
                'Case "m0_backup_dataRestore"
                '    Using wsm As New m0_backup_data
                '        hasil = wsm.m0_backup_dataRestore(param)
                '    End Using

                'M0_HAPUS_DATA
            Case "m0_hapus_dataSimpan"
                Using wsm As New m0_hapus_data
                    hasil = wsm.m0_hapus_dataSimpan(param)
                End Using
            Case "m0_hapus_datasearch"
                Using wsm As New m0_hapus_data
                    hasil = wsm.m0_hapus_datasearch(param)
                End Using
            Case "m0_hapus_dataGetMapTransaksi"
                Using wsm As New m0_hapus_data
                    hasil = wsm.m0_hapus_dataGetMapTransaksi(param)
                End Using

                'M2_AJ
            Case "M2_AjSimpan"
                Dim wsM2_Aj As New m2_aj
                hasil = wsM2_Aj.M2_AjSimpan(param)
            Case "M2_AjSearch"
                Dim wsM2_Aj As New m2_aj
                hasil = wsM2_Aj.M2_AjSearch(param)
            Case "M2_AjDelete"
                If (isDemo = False) Then
                    Dim wsM2_Aj As New m2_aj
                    hasil = wsM2_Aj.M2_AjDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M2_AjGetdataById"
                Dim wsM2_Aj As New m2_aj
                hasil = wsM2_Aj.M2_AjGetdataById(param)
            Case "M2_AjUpdateStatus"
                Dim wsM2_Aj As New m2_aj
                hasil = wsM2_Aj.M2_AjUpdateStatus(param)
            Case "M2_AjTerkait"
                Dim wsM2_Aj As New m2_aj
                hasil = wsM2_Aj.M2_AjTerkait(param)
            Case "M2_Aj_HistorySimpan"
                Dim wsM2_Aj As New m2_aj_history
                hasil = wsM2_Aj.M2_Aj_HistorySimpan(param)
            Case "M2_Aj_HistorySearch"
                Dim wsM2_Aj As New m2_aj_history
                hasil = wsM2_Aj.M2_Aj_HistorySearch(param)
            Case "M2_AjHistoryGetdataById"
                Dim wsM2_Aj As New m2_aj_history
                hasil = wsM2_Aj.M2_AjHistoryGetdataById(param)

                'M2_CD
            Case "M2_CdSimpan"
                Dim wsM2_Cd As New m2_cd
                hasil = wsM2_Cd.M2_CdSimpan(param)
            Case "M2_CdSearch"
                Dim wsM2_Cd As New m2_cd
                hasil = wsM2_Cd.M2_CdSearch(param)
            Case "M2_CdDelete"
                If (isDemo = False) Then
                    Dim wsM2_Cd As New m2_cd
                    hasil = wsM2_Cd.M2_CdDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M2_CdGetdataById"
                Dim wsM2_Cd As New m2_cd
                hasil = wsM2_Cd.M2_CdGetdataById(param)
            Case "M2_CdUpdateStatus"
                Dim wsM2_Cd As New m2_cd
                hasil = wsM2_Cd.M2_CdUpdateStatus(param)
            Case "M2_CdTerkait"
                Dim wsM2_Cd As New m2_cd
                hasil = wsM2_Cd.M2_CdTerkait(param)
            Case "M2_Cd_HistorySimpan"
                Dim wsM2_Cd As New m2_cd_history
                hasil = wsM2_Cd.M2_Cd_HistorySimpan(param)
            Case "M2_Cd_HistorySearch"
                Dim wsM2_Cd As New m2_cd_history
                hasil = wsM2_Cd.M2_Cd_HistorySearch(param)
            Case "M2_CdHistoryGetdataById"
                Dim wsM2_Cd As New m2_cd_history
                hasil = wsM2_Cd.M2_CdHistoryGetdataById(param)

                'M2_CR
            Case "M2_CrSimpan"
                Dim wsM2_Cr As New m2_cr
                hasil = wsM2_Cr.M2_CrSimpan(param)
            Case "M2_CrSearch"
                Dim wsM2_Cr As New m2_cr
                hasil = wsM2_Cr.M2_CrSearch(param)
            Case "M2_CrDelete"
                If (isDemo = False) Then
                    Dim wsM2_Cr As New m2_cr
                    hasil = wsM2_Cr.M2_CrDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If

            Case "M2_CrGetdataById"
                Dim wsM2_Cr As New m2_cr
                hasil = wsM2_Cr.M2_CrGetdataById(param)
            Case "M2_CrUpdateStatus"
                Dim wsM2_Cr As New m2_cr
                hasil = wsM2_Cr.M2_CrUpdateStatus(param)
            Case "M2_CrTerkait"
                Dim wsM2_Cr As New m2_cr
                hasil = wsM2_Cr.M2_CrTerkait(param)
            Case "M2_Cr_HistorySimpan"
                Dim wsM2_Cr As New m2_cr_history
                hasil = wsM2_Cr.M2_Cr_HistorySimpan(param)
            Case "M2_Cr_HistorySearch"
                Dim wsM2_Cr As New m2_cr_history
                hasil = wsM2_Cr.M2_Cr_HistorySearch(param)
            Case "M2_CrHistoryGetdataById"
                Dim wsM2_Cr As New m2_cr_history
                hasil = wsM2_Cr.M2_CrHistoryGetdataById(param)

                'M2_GJ
            Case "M2_GjSimpan"
                Dim wsM2_Gj As New m2_gj
                hasil = wsM2_Gj.M2_GjSimpan(param)
            Case "M2_GjSearch"
                Dim wsM2_Gj As New m2_gj
                hasil = wsM2_Gj.M2_GjSearch(param)
            Case "M2_GjDelete"
                If (isDemo = False) Then
                    Dim wsM2_Gj As New m2_gj
                    hasil = wsM2_Gj.M2_GjDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M2_GjGetdataById"
                Dim wsM2_Gj As New m2_gj
                hasil = wsM2_Gj.M2_GjGetdataById(param)
            Case "M2_GjUpdateStatus"
                Dim wsM2_Gj As New m2_gj
                hasil = wsM2_Gj.M2_GjUpdateStatus(param)
            Case "M2_GjTerkait"
                Dim wsM2_Gj As New m2_gj
                hasil = wsM2_Gj.M2_GjTerkait(param)
            Case "M2_Gj_HistorySimpan"
                Dim wsM2_Gj As New m2_gj_history
                hasil = wsM2_Gj.M2_Gj_HistorySimpan(param)
            Case "M2_Gj_HistorySearch"
                Dim wsM2_Gj As New m2_gj_history
                hasil = wsM2_Gj.M2_Gj_HistorySearch(param)
            Case "M2_GjHistoryGetdataById"
                Dim wsM2_Gj As New m2_gj_history
                hasil = wsM2_Gj.M2_GjHistoryGetdataById(param)

                'M2_RG
            Case "M2_RgSimpan"
                Dim wsM2_Rg As New m2_rg
                hasil = wsM2_Rg.M2_RgSimpan(param)
            Case "M2_RgSearch"
                Dim wsM2_Rg As New m2_rg
                hasil = wsM2_Rg.M2_RgSearch(param)
            Case "M2_RgDelete"
                If (isDemo = False) Then
                    Dim wsM2_Rg As New m2_rg
                    hasil = wsM2_Rg.M2_RgDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M2_RgGetdataById"
                Dim wsM2_Rg As New m2_rg
                hasil = wsM2_Rg.M2_RgGetdataById(param)
            Case "M2_RgUpdateStatus"
                Dim wsM2_Rg As New m2_rg
                hasil = wsM2_Rg.M2_RgUpdateStatus(param)
            Case "M2_Rg_DetailSearch"
                Dim wsM2_Rg As New m2_rg
                hasil = wsM2_Rg.M2_Rg_DetailSearch(param)
            Case "M2_RgTerkait"
                Dim wsM2_Rg As New m2_rg
                hasil = wsM2_Rg.M2_RgTerkait(param)
            Case "M2_Rg_HistorySimpan"
                Dim wsM2_Rg As New m2_rg_history
                hasil = wsM2_Rg.M2_Rg_HistorySimpan(param)
            Case "M2_Rg_HistorySearch"
                Dim wsM2_Rg As New m2_rg_history
                hasil = wsM2_Rg.M2_Rg_HistorySearch(param)
            Case "M2_RgHistoryGetdataById"
                Dim wsM2_Rg As New m2_rg_history
                hasil = wsM2_Rg.M2_RgHistoryGetdataById(param)

                'M2_RM
            Case "M2_RmSimpan"
                Dim wsM2_Rm As New m2_rm
                hasil = wsM2_Rm.M2_RmSimpan(param)
            Case "M2_RmSearch"
                Dim wsM2_Rm As New m2_rm
                hasil = wsM2_Rm.M2_RmSearch(param)
            Case "M2_RmDelete"
                If (isDemo = False) Then
                    Dim wsM2_Rm As New m2_rm
                    hasil = wsM2_Rm.M2_RmDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M2_RmGetdataById"
                Dim wsM2_Rm As New m2_rm
                hasil = wsM2_Rm.M2_RmGetdataById(param)
            Case "M2_RmUpdateStatus"
                Dim wsM2_Rm As New m2_rm
                hasil = wsM2_Rm.M2_RmUpdateStatus(param)
            Case "M2_RmTerkait"
                Dim wsM2_Rm As New m2_rm
                hasil = wsM2_Rm.M2_RmTerkait(param)
            Case "M2_Rm_HistorySimpan"
                Dim wsM2_Rm As New m2_rm_history
                hasil = wsM2_Rm.M2_Rm_HistorySimpan(param)
            Case "M2_Rm_HistorySearch"
                Dim wsM2_Rm As New m2_rm_history
                hasil = wsM2_Rm.M2_Rm_HistorySearch(param)
            Case "M2_RmHistoryGetdataById"
                Dim wsM2_Rm As New m2_rm_history
                hasil = wsM2_Rm.M2_RmHistoryGetdataById(param)

                'M2_SG
            Case "M2_SgSimpan"
                Dim wsM2_Sg As New m2_sg
                hasil = wsM2_Sg.M2_SgSimpan(param)
            Case "M2_SgSearch"
                Dim wsM2_Sg As New m2_sg
                hasil = wsM2_Sg.M2_SgSearch(param)
            Case "M2_SgDelete"
                If (isDemo = False) Then
                    Dim wsM2_Sg As New m2_sg
                    hasil = wsM2_Sg.M2_SgDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M2_SgGetdataById"
                Dim wsM2_Sg As New m2_sg
                hasil = wsM2_Sg.M2_SgGetdataById(param)
            Case "M2_SgUpdateStatus"
                Dim wsM2_Sg As New m2_sg
                hasil = wsM2_Sg.M2_SgUpdateStatus(param)
            Case "M2_Sg_DetailSearch"
                Dim wsM2_Sg As New m2_sg
                hasil = wsM2_Sg.M2_Sg_DetailSearch(param)
            Case "M2_SgTerkait"
                Dim wsM2_Sg As New m2_sg
                hasil = wsM2_Sg.M2_SgTerkait(param)
            Case "M2_Sg_HistorySimpan"
                Dim wsM2_Sg As New m2_sg_history
                hasil = wsM2_Sg.M2_Sg_HistorySimpan(param)
            Case "M2_Sg_HistorySearch"
                Dim wsM2_Sg As New m2_sg_history
                hasil = wsM2_Sg.M2_Sg_HistorySearch(param)
            Case "M2_SgHistoryGetdataById"
                Dim wsM2_Sg As New m2_sg_history
                hasil = wsM2_Sg.M2_SgHistoryGetdataById(param)

                'M2_SM
            Case "M2_SmSimpan"
                Dim wsM2_Sm As New m2_sm
                hasil = wsM2_Sm.M2_SmSimpan(param)
            Case "M2_SmSearch"
                Dim wsM2_Sm As New m2_sm
                hasil = wsM2_Sm.M2_SmSearch(param)
            Case "M2_SmDelete"
                If (isDemo = False) Then
                    Dim wsM2_Sm As New m2_sm
                    hasil = wsM2_Sm.M2_SmDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M2_SmGetdataById"
                Dim wsM2_Sm As New m2_sm
                hasil = wsM2_Sm.M2_SmGetdataById(param)
            Case "M2_SmUpdateStatus"
                Dim wsM2_Sm As New m2_sm
                hasil = wsM2_Sm.M2_SmUpdateStatus(param)
            Case "M2_SmTerkait"
                Dim wsM2_Sm As New m2_sm
                hasil = wsM2_Sm.M2_SmTerkait(param)
            Case "M2_Sm_HistorySimpan"
                Dim wsM2_Sm As New m2_sm_history
                hasil = wsM2_Sm.M2_Sm_HistorySimpan(param)
            Case "M2_Sm_HistorySearch"
                Dim wsM2_Sm As New m2_sm_history
                hasil = wsM2_Sm.M2_Sm_HistorySearch(param)
            Case "M2_SmHistoryGetdataById"
                Dim wsM2_Sm As New m2_sm_history
                hasil = wsM2_Sm.M2_SmHistoryGetdataById(param)

                'M2_RGC
            Case "M2_RgcSimpan"
                Dim wsM2_Rgc As New m2_rgc
                hasil = wsM2_Rgc.M2_RgcSimpan(param)
            Case "M2_RgcSearch"
                Dim wsM2_Rgc As New m2_rgc
                hasil = wsM2_Rgc.M2_RgcSearch(param)
            Case "M2_RgcDelete"
                If (isDemo = False) Then
                    Dim wsM2_Rgc As New m2_rgc
                    hasil = wsM2_Rgc.M2_RgcDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M2_RgcGetdataById"
                Dim wsM2_Rgc As New m2_rgc
                hasil = wsM2_Rgc.M2_RgcGetdataById(param)
            Case "M2_RgcUpdateStatus"
                Dim wsM2_Rgc As New m2_rgc
                hasil = wsM2_Rgc.M2_RgcUpdateStatus(param)
            Case "M2_RgcTerkait"
                Dim wsM2_Rgc As New m2_rgc
                hasil = wsM2_Rgc.M2_RgcTerkait(param)
            Case "M2_Rgc_HistorySimpan"
                Dim wsM2_Rgc As New m2_rgc_history
                hasil = wsM2_Rgc.M2_Rgc_HistorySimpan(param)
            Case "M2_Rgc_HistorySearch"
                Dim wsM2_Rgc As New m2_rgc_history
                hasil = wsM2_Rgc.M2_Rgc_HistorySearch(param)
            Case "M2_RgcHistoryGetdataById"
                Dim wsM2_Rgc As New m2_rgc_history
                hasil = wsM2_Rgc.M2_RgcHistoryGetdataById(param)

                'M2_SGC
            Case "M2_SgcSimpan"
                Dim wsM2_Sgc As New m2_sgc
                hasil = wsM2_Sgc.M2_SgcSimpan(param)
            Case "M2_SgcSearch"
                Dim wsM2_Sgc As New m2_sgc
                hasil = wsM2_Sgc.M2_SgcSearch(param)
            Case "M2_SgcDelete"
                If (isDemo = False) Then
                    Dim wsM2_Sgc As New m2_sgc
                    hasil = wsM2_Sgc.M2_SgcDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M2_SgcGetdataById"
                Dim wsM2_Sgc As New m2_sgc
                hasil = wsM2_Sgc.M2_SgcGetdataById(param)
            Case "M2_SgcUpdateStatus"
                Dim wsM2_Sgc As New m2_sgc
                hasil = wsM2_Sgc.M2_SgcUpdateStatus(param)
            Case "M2_SgcTerkait"
                Dim wsM2_Sgc As New m2_sgc
                hasil = wsM2_Sgc.M2_SgcTerkait(param)
            Case "M2_Sgc_HistorySimpan"
                Dim wsM2_Sgc As New m2_sgc_history
                hasil = wsM2_Sgc.M2_Sgc_HistorySimpan(param)
            Case "M2_Sgc_HistorySearch"
                Dim wsM2_Sgc As New m2_sgc_history
                hasil = wsM2_Sgc.M2_Sgc_HistorySearch(param)
            Case "M2_SgcHistoryGetdataById"
                Dim wsM2_Sgc As New m2_sgc_history
                hasil = wsM2_Sgc.M2_SgcHistoryGetdataById(param)


                'M2_CB
            Case "M2_CbSimpan"
                Dim wsM2_Cb As New m2_cb
                hasil = wsM2_Cb.M2_CbSimpan(param)
            Case "M2_CbSearch"
                Dim wsM2_Cb As New m2_cb
                hasil = wsM2_Cb.M2_CbSearch(param)
            Case "M2_CbDelete"
                If (isDemo = False) Then
                    Dim wsM2_Cb As New m2_cb
                    hasil = wsM2_Cb.M2_CbDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M2_CbGetdataById"
                Dim wsM2_Cb As New m2_cb
                hasil = wsM2_Cb.M2_CbGetdataById(param)
            Case "M2_CbUpdateStatus"
                Dim wsM2_Cb As New m2_cb
                hasil = wsM2_Cb.M2_CbUpdateStatus(param)
            Case "M2_CbTerkait"
                Dim wsM2_Cb As New m2_cb
                hasil = wsM2_Cb.M2_CbTerkait(param)
            Case "M2_Cb_HistorySimpan"
                Dim wsM2_Cb As New m2_cb_history
                hasil = wsM2_Cb.M2_Cb_HistorySimpan(param)
            Case "M2_Cb_HistorySearch"
                Dim wsM2_Cb As New m2_cb_history
                hasil = wsM2_Cb.M2_Cb_HistorySearch(param)
            Case "M2_CbHistoryGetdataById"
                Dim wsM2_Cb As New m2_cb_history
                hasil = wsM2_Cb.M2_CbHistoryGetdataById(param)


                'M2_BD
            Case "M2_BdSimpan"
                Dim wsM2_Bd As New m2_bd
                hasil = wsM2_Bd.M2_BdSimpan(param)
            Case "M2_BdSearch"
                Dim wsM2_Bd As New m2_bd
                hasil = wsM2_Bd.M2_BdSearch(param)
            Case "M2_BdDelete"
                If (isDemo = False) Then
                    Dim wsM2_Bd As New m2_bd
                    hasil = wsM2_Bd.M2_BdDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M2_BdGetdataById"
                Dim wsM2_Bd As New m2_bd
                hasil = wsM2_Bd.M2_BdGetdataById(param)
            Case "M2_BdUpdateStatus"
                Dim wsM2_Bd As New m2_bd
                hasil = wsM2_Bd.M2_BdUpdateStatus(param)
            Case "M2_BdTerkait"
                Dim wsM2_Bd As New m2_bd
                hasil = wsM2_Bd.M2_BdTerkait(param)
            Case "M2_Bd_HistorySimpan"
                Dim wsM2_Bd As New m2_bd_history
                hasil = wsM2_Bd.M2_Bd_HistorySimpan(param)
            Case "M2_Bd_HistorySearch"
                Dim wsM2_Bd As New m2_bd_history
                hasil = wsM2_Bd.M2_Bd_HistorySearch(param)
            Case "M2_BdHistoryGetdataById"
                Dim wsM2_Bd As New m2_bd_history
                hasil = wsM2_Bd.M2_BdHistoryGetdataById(param)


                'M2_GIRO_LIST
            Case "M2_Giro_ListSearch"
                Dim wsM2_Giro_List As New m2_giro_list
                hasil = wsM2_Giro_List.M2_Giro_ListSearch(param)

                'M2_TRANSACTION_JOURNAL
            Case "M2_Transaction_Journal_VoucherSearch"
                Dim wsM2_Transaction_Journal As New m2_transaction_journal
                hasil = wsM2_Transaction_Journal.M2_Transaction_Journal_VoucherSearch(param)

                'M2_GENERAL_LEDGER
            Case "M2_GeneralLedger"
                Dim wsM2_Transaction_Journal As New m2_transaction_journal
                hasil = wsM2_Transaction_Journal.M2_GeneralLedger(param)

                'M2_KARTUSERIAL
            Case "M2_KartuSerial"
                Dim wsM2_Transaction_Journal As New m2_transaction_journal
                hasil = wsM2_Transaction_Journal.M2_KartuSerial(param)

                'M2_DATA_JOURNAL
            Case "M2_Data_Journal_VoucherSearch"
                Dim wsM2_Transaction_Journal As New m2_transaction_journal
                hasil = wsM2_Transaction_Journal.M2_Data_Journal_VoucherSearch(param)

                'M2_DATA_ITEM_TRANSACTION
            Case "M2_Data_Item_TransactionSearch"
                Dim wsM2_Transaction_Journal As New m2_transaction_journal
                hasil = wsM2_Transaction_Journal.M2_Data_Item_TransactionSearch(param)

                'M2_STATISTIK_KASBANK
            Case "M2_Statistik_KasBankSearch"
                Dim wsM2_Statistik_KasBank As New m2_statistik
                hasil = wsM2_Statistik_KasBank.M2_Statistik_KasBankSearch(param)

            Case "M2_Statistik_GiroSearch"
                Dim wsM2_Statistik_Giro As New m2_statistik
                hasil = wsM2_Statistik_Giro.M2_Statistik_GiroSearch(param)


                'M2_STATISTIK_KASBANK
            Case "M2S_CashBank"
                Dim wsM2_Statistik As New m2_statistik
                hasil = wsM2_Statistik.M2S_CashBank(param)

                'M2_STATISTIK_GIRO
            Case "M2S_Giro"
                Dim wsM2_Statistik As New m2_statistik
                hasil = wsM2_Statistik.M2S_Giro(param)

                'M2_STATISTIK_HUTANGPIUTANG
            Case "M2S_HutangPiutang"
                Dim wsM2_Statistik As New m2_statistik
                hasil = wsM2_Statistik.M2S_HutangPiutang(param)

                'M2_JM
            Case "M2_JmSimpan"
                Dim wsM2_Jm As New m2_jm
                hasil = wsM2_Jm.M2_JmSimpan(param)
            Case "M2_JmSearch"
                Dim wsM2_Jm As New m2_jm
                hasil = wsM2_Jm.M2_JmSearch(param)
            Case "M2_JmDelete"
                If (isDemo = False) Then
                    Dim wsM2_Jm As New m2_jm
                    hasil = wsM2_Jm.M2_JmDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M2_JmGetdataById"
                Dim wsM2_Jm As New m2_jm
                hasil = wsM2_Jm.M2_JmGetdataById(param)
            Case "M2_JmUpdateStatus"
                Dim wsM2_Jm As New m2_jm
                hasil = wsM2_Jm.M2_JmUpdateStatus(param)
            Case "M2_JmTerkait"
                Dim wsM2_Jm As New m2_jm
                hasil = wsM2_Jm.M2_JmTerkait(param)
            Case "M2_Jm_HistorySimpan"
                Dim wsM2_Jm As New m2_jm_history
                hasil = wsM2_Jm.M2_Jm_HistorySimpan(param)
            Case "M2_Jm_HistorySearch"
                Dim wsM2_Jm As New m2_jm_history
                hasil = wsM2_Jm.M2_Jm_HistorySearch(param)
            Case "M2_JmHistoryGetdataById"
                Dim wsM2_Jm As New m2_jm_history
                hasil = wsM2_Jm.M2_JmHistoryGetdataById(param)

                '*********************************** M3 '***********************************

                'M3_PRINT
            Case "M3_Print"
                Dim wsM3_Print As New m3_print
                hasil = wsM3_Print.M3_Print(param)

                'M3_NOTES
            Case "M3_NotesSimpan"
                Dim wsM3_Notes As New m3_notes
                hasil = wsM3_Notes.M3_NotesSimpan(param)
            Case "M3_NotesSearch"
                Dim wsM3_Notes As New m3_notes
                hasil = wsM3_Notes.M3_NotesSearch(param)
            Case "M3_NotesDelete"
                Dim wsM3_Notes As New m3_notes
                hasil = wsM3_Notes.M3_NotesDelete(param)

                'M3_FILES
            Case "M3_FilesSimpan"
                Dim wsM3_Files As New m3_files
                hasil = wsM3_Files.M3_FilesSimpan(param)
            Case "M3_FilesSearch"
                Dim wsM3_Files As New m3_files
                hasil = wsM3_Files.M3_FilesSearch(param)
            Case "M3_FilesDelete"
                Dim wsM3_Files As New m3_files
                hasil = wsM3_Files.M3_FilesDelete(param)

                'M3_MR
            Case "M3_MrSimpan"
                Dim wsM3_Mr As New m3_mr
                hasil = wsM3_Mr.M3_MrSimpan(param)
            Case "M3_MrSearch"
                Dim wsM3_Mr As New m3_mr
                hasil = wsM3_Mr.M3_MrSearch(param)
            Case "M3_MrDelete"
                If (isDemo = False) Then
                    Dim wsM3_Mr As New m3_mr
                    hasil = wsM3_Mr.M3_MrDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M3_MrGetdataById"
                Dim wsM3_Mr As New m3_mr
                hasil = wsM3_Mr.M3_MrGetdataById(param)
            Case "M3_MrUpdateStatus"
                Dim wsM3_Mr As New m3_mr
                hasil = wsM3_Mr.M3_MrUpdateStatus(param)
            Case "M3_Mr_Detail_VSearch"
                Dim wsM3_Mr As New m3_mr
                hasil = wsM3_Mr.M3_Mr_Detail_VSearch(param)
            Case "M3_MrTerkait"
                Dim wsM3_Mr As New m3_mr
                hasil = wsM3_Mr.M3_MrTerkait(param)
            Case "M3_Mr_HistorySimpan"
                Dim wsM3_Mr As New m3_mr_history
                hasil = wsM3_Mr.M3_Mr_HistorySimpan(param)
            Case "M3_Mr_HistorySearch"
                Dim wsM3_Mr As New m3_mr_history
                hasil = wsM3_Mr.M3_Mr_HistorySearch(param)
            Case "M3_MrHistoryGetdataById"
                Dim wsM3_Mr As New m3_mr_history
                hasil = wsM3_Mr.M3_MrHistoryGetdataById(param)

                'M3_PA
            Case "M3_PaSimpan"
                Dim wsM3_Pa As New m3_pa
                hasil = wsM3_Pa.M3_PaSimpan(param)
            Case "M3_PaSearch"
                Dim wsM3_Pa As New m3_pa
                hasil = wsM3_Pa.M3_PaSearch(param)
            Case "M3_PaDelete"
                If (isDemo = False) Then
                    Dim wsM3_Pa As New m3_pa
                    hasil = wsM3_Pa.M3_PaDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M3_PaGetdataById"
                Dim wsM3_Pa As New m3_pa
                hasil = wsM3_Pa.M3_PaGetdataById(param)
            Case "M3_PaUpdateStatus"
                Dim wsM3_Pa As New m3_pa
                hasil = wsM3_Pa.M3_PaUpdateStatus(param)
            Case "M3_PaTerkait"
                Dim wsM3_Pa As New m3_pa
                hasil = wsM3_Pa.M3_PaTerkait(param)
            Case "M3_Pa_HistorySimpan"
                Dim wsM3_Pa As New m3_pa_history
                hasil = wsM3_Pa.M3_Pa_HistorySimpan(param)
            Case "M3_Pa_HistorySearch"
                Dim wsM3_Pa As New m3_pa_history
                hasil = wsM3_Pa.M3_Pa_HistorySearch(param)
            Case "M3_PaHistoryGetdataById"
                Dim wsM3_Pa As New m3_pa_history
                hasil = wsM3_Pa.M3_PaHistoryGetdataById(param)

                'M3_RS
            Case "M3_RsSimpan"
                Dim wsM3_Rs As New m3_rs
                hasil = wsM3_Rs.M3_RsSimpan(param)
            Case "M3_RsSearch"
                Dim wsM3_Rs As New m3_rs
                hasil = wsM3_Rs.M3_RsSearch(param)
            Case "M3_RsDelete"
                If (isDemo = False) Then
                    Dim wsM3_Rs As New m3_rs
                    hasil = wsM3_Rs.M3_RsDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M3_RsGetdataById"
                Dim wsM3_Rs As New m3_rs
                hasil = wsM3_Rs.M3_RsGetdataById(param)
            Case "M3_RsUpdateStatus"
                Dim wsM3_Rs As New m3_rs
                hasil = wsM3_Rs.M3_RsUpdateStatus(param)
            Case "M3_RsTerkait"
                Dim wsM3_Rs As New m3_rs
                hasil = wsM3_Rs.M3_RsTerkait(param)
            Case "M3_Rs_HistorySimpan"
                Dim wsM3_Ts As New m3_rs_history
                hasil = wsM3_Ts.M3_Rs_HistorySimpan(param)
            Case "M3_Rs_HistorySearch"
                Dim wsM3_Ts As New m3_rs_history
                hasil = wsM3_Ts.M3_Rs_HistorySearch(param)
            Case "M3_RsHistoryGetdataById"
                Dim wsM3_Ts As New m3_rs_history
                hasil = wsM3_Ts.M3_RsHistoryGetdataById(param)

                'M3_RW
            Case "M3_RwSearch"
                Dim wsM3_Rw As New m3_rw
                hasil = wsM3_Rw.M3_RwSearch(param)
            Case "M3_RwSimpan"
                Dim wsM3_Rw As New m3_rw
                hasil = wsM3_Rw.M3_RwSimpan(param)
            Case "M3_RwDelete"
                If (isDemo = False) Then
                    Dim wsM3_Rw As New m3_rw
                    hasil = wsM3_Rw.M3_RwDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M3_RwGetdataById"
                Dim wsM3_Rw As New m3_rw
                hasil = wsM3_Rw.M3_RwGetdataById(param)
            Case "M3_RwUpdateStatus"
                Dim wsM3_Rw As New m3_rw
                hasil = wsM3_Rw.M3_RwUpdateStatus(param)

                'M3_SA
            Case "M3_SaSimpan"
                Dim wsM3_Sa As New wsm3_sa
                hasil = wsM3_Sa.M3_SaSimpan(param)
            Case "M3_SaSearch"
                Dim wsM3_Sa As New wsm3_sa
                hasil = wsM3_Sa.M3_SaSearch(param)
            Case "M3_SaDelete"
                If (isDemo = False) Then
                    Dim wsM3_Sa As New wsm3_sa
                    hasil = wsM3_Sa.M3_SaDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M3_SaGetdataById"
                Dim wsM3_Sa As New wsm3_sa
                hasil = wsM3_Sa.M3_SaGetdataById(param)
            Case "M3_SaUpdateStatus"
                Dim wsM3_Sa As New wsm3_sa
                hasil = wsM3_Sa.M3_SaUpdateStatus(param)
            Case "M3_SaTerkait"
                Dim wsM3_Sa As New wsm3_sa
                hasil = wsM3_Sa.M3_SaTerkait(param)
            Case "M3_Sa_HistorySimpan"
                Dim wsM3_Sa As New m3_sa_history
                hasil = wsM3_Sa.M3_Sa_HistorySimpan(param)
            Case "M3_Sa_HistorySearch"
                Dim wsM3_Sa As New m3_sa_history
                hasil = wsM3_Sa.M3_Sa_HistorySearch(param)
            Case "M3_SaHistoryGetdataById"
                Dim wsM3_Sa As New m3_sa_history
                hasil = wsM3_Sa.M3_SaHistoryGetdataById(param)

                'M3_SP
            Case "M3_SpSimpan"
                Dim wsM3_Sp As New wsm3_sp
                hasil = wsM3_Sp.M3_SpSimpan(param)
            Case "M3_SpSearch"
                Dim wsM3_Sp As New wsm3_sp
                hasil = wsM3_Sp.M3_SpSearch(param)
            Case "M3_SpDelete"
                If (isDemo = False) Then
                    Dim wsM3_Sp As New wsm3_sp
                    hasil = wsM3_Sp.M3_SpDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M3_SpGetdataById"
                Dim wsM3_Sp As New wsm3_sp
                hasil = wsM3_Sp.M3_SpGetdataById(param)
            Case "M3_SpUpdateStatus"
                Dim wsM3_Sp As New wsm3_sp
                hasil = wsM3_Sp.M3_SpUpdateStatus(param)
            Case "M3_Sp_Detail_VSearch"
                Dim wsM3_Sp As New wsm3_sp
                hasil = wsM3_Sp.M3_Sp_Detail_VSearch(param)
            Case "M3_Sp_Detail_VSearchPenjualan"
                Dim wsM3_Sp As New wsm3_sp
                hasil = wsM3_Sp.M3_Sp_Detail_VSearchPenjualan(param)
            Case "M3_Sp_TakedataSearch"
                Dim wsM3_Sp As New wsm3_sp
                hasil = wsM3_Sp.M3_Sp_TakedataSearch(param)
            Case "M3_SpTerkait"
                Dim wsM3_Sp As New wsm3_sp
                hasil = wsM3_Sp.M3_SpTerkait(param)
            Case "M3_Sp_HistorySimpan"
                Dim wsM3_Sp As New m3_sp_history
                hasil = wsM3_Sp.M3_Sp_HistorySimpan(param)
            Case "M3_Sp_HistorySearch"
                Dim wsM3_Sp As New m3_sp_history
                hasil = wsM3_Sp.M3_Sp_HistorySearch(param)
            Case "M3_SpHistoryGetdataById"
                Dim wsM3_Sp As New m3_sp_history
                hasil = wsM3_Sp.M3_SpHistoryGetdataById(param)

                'M3_TS
            Case "M3_TsSimpan"
                Dim wsM3_Ts As New m3_ts
                hasil = wsM3_Ts.M3_TsSimpan(param)
            Case "M3_TsSearch"
                Dim wsM3_Ts As New m3_ts
                hasil = wsM3_Ts.M3_TsSearch(param)
            Case "M3_TsDelete"
                If (isDemo = False) Then
                    Dim wsM3_Ts As New m3_ts
                    hasil = wsM3_Ts.M3_TsDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M3_TsGetdataById"
                Dim wsM3_Ts As New m3_ts
                hasil = wsM3_Ts.M3_TsGetdataById(param)
            Case "M3_TsUpdateStatus"
                Dim wsM3_Ts As New m3_ts
                hasil = wsM3_Ts.M3_TsUpdateStatus(param)
            Case "M3_Ts_Detail_VSearch"
                Dim wsM3_Ts As New m3_ts
                hasil = wsM3_Ts.M3_Ts_Detail_VSearch(param)
            Case "M3_TsTerkait"
                Dim wsM3_Ts As New m3_ts
                hasil = wsM3_Ts.M3_TsTerkait(param)
            Case "M3_Ts_HistorySimpan"
                Dim wsM3_Ts As New m3_ts_history
                hasil = wsM3_Ts.M3_Ts_HistorySimpan(param)
            Case "M3_Ts_HistorySearch"
                Dim wsM3_Ts As New m3_ts_history
                hasil = wsM3_Ts.M3_Ts_HistorySearch(param)
            Case "M3_TsHistoryGetdataById"
                Dim wsM3_Ts As New m3_ts_history
                hasil = wsM3_Ts.M3_TsHistoryGetdataById(param)

                'M3_TS
            Case "M3_IbSimpan"
                Dim wsM3_Ib As New m3_ib
                hasil = wsM3_Ib.M3_IbSimpan(param)
            Case "M3_IbSearch"
                Dim wsM3_Ib As New m3_ib
                hasil = wsM3_Ib.M3_IbSearch(param)
            Case "M3_IbDelete"
                If (isDemo = False) Then
                    Dim wsM3_Ib As New m3_ib
                    hasil = wsM3_Ib.M3_IbDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M3_IbGetdataById"
                Dim wsM3_Ib As New m3_ib
                hasil = wsM3_Ib.M3_IbGetdataById(param)
            Case "M3_IbUpdateStatus"
                Dim wsM3_Ib As New m3_ib
                hasil = wsM3_Ib.M3_IbUpdateStatus(param)
            Case "M3_IbTerkait"
                Dim wsM3_Ib As New m3_ib
                hasil = wsM3_Ib.M3_IbTerkait(param)
            Case "M3_Ib_HistorySimpan"
                Dim wsM3_Ib As New m3_ib_history
                hasil = wsM3_Ib.M3_Ib_HistorySimpan(param)
            Case "M3_Ib_HistorySearch"
                Dim wsM3_Ib As New m3_ib_history
                hasil = wsM3_Ib.M3_Ib_HistorySearch(param)
            Case "M3_IbHistoryGetdataById"
                Dim wsM3_Ib As New m3_ib_history
                hasil = wsM3_Ib.M3_IbHistoryGetdataById(param)


                'M3_RF
            Case "M3_RfSimpan"
                Dim wsM3_Rf As New m3_rf
                hasil = wsM3_Rf.M3_RfSimpan(param)
            Case "M3_RfSearch"
                Dim wsM3_Rf As New m3_rf
                hasil = wsM3_Rf.M3_RfSearch(param)
            Case "M3_RfDelete"
                If (isDemo = False) Then
                    Dim wsM3_Rf As New m3_rf
                    hasil = wsM3_Rf.M3_RfDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M3_RfGetdataById"
                Dim wsM3_Rf As New m3_rf
                hasil = wsM3_Rf.M3_RfGetdataById(param)
            Case "M3_RfUpdateStatus"
                Dim wsM3_Rf As New m3_rf
                hasil = wsM3_Rf.M3_RfUpdateStatus(param)
            Case "M3_Rf_Detail_VSearch"
                Dim wsM3_Rf As New m3_rf
                hasil = wsM3_Rf.M3_Rf_Detail_VSearch(param)
            Case "M3_RfTerkait"
                Dim wsM3_Rf As New m3_rf
                hasil = wsM3_Rf.M3_RfTerkait(param)
            Case "M3_Rf_HistorySimpan"
                Dim wsM3_Rf As New m3_rf_history
                hasil = wsM3_Rf.M3_Rf_HistorySimpan(param)
            Case "M3_Rf_HistorySearch"
                Dim wsM3_Rf As New m3_rf_history
                hasil = wsM3_Rf.M3_Rf_HistorySearch(param)
            Case "M3_RfHistoryGetdataById"
                Dim wsM3_Rf As New m3_rf_history
                hasil = wsM3_Rf.M3_RfHistoryGetdataById(param)


                'M3_DC
            Case "M3_DcSimpan"
                Dim wsM3_Dc As New m3_dc
                hasil = wsM3_Dc.M3_DcSimpan(param)
            Case "M3_DcSearch"
                Dim wsM3_Dc As New m3_dc
                hasil = wsM3_Dc.M3_DcSearch(param)
            Case "M3_DcDelete"
                If (isDemo = False) Then
                    Dim wsM3_Dc As New m3_dc
                    hasil = wsM3_Dc.M3_DcDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M3_DcGetdataById"
                Dim wsM3_Dc As New m3_dc
                hasil = wsM3_Dc.M3_DcGetdataById(param)
            Case "M3_DcUpdateStatus"
                Dim wsM3_Dc As New m3_dc
                hasil = wsM3_Dc.M3_DcUpdateStatus(param)
            Case "M3_DcTerkait"
                Dim wsM3_Dc As New m3_dc
                hasil = wsM3_Dc.M3_DcTerkait(param)
            Case "M3_Dc_HistorySimpan"
                Dim wsM3_Dc As New m3_dc_history
                hasil = wsM3_Dc.M3_Dc_HistorySimpan(param)
            Case "M3_Dc_HistorySearch"
                Dim wsM3_Dc As New m3_dc_history
                hasil = wsM3_Dc.M3_Dc_HistorySearch(param)
            Case "M3_DcHistoryGetdataById"
                Dim wsM3_Dc As New m3_dc_history
                hasil = wsM3_Dc.M3_DcHistoryGetdataById(param)


                'M3_STATISTIK
            Case "M3S_ProdukOmzet"
                Dim wsM3_Statistik As New m3_statistik
                hasil = wsM3_Statistik.M3S_ProdukOmzet(param)
            Case "M3S_ProdukProfit"
                Dim wsM3_Statistik As New m3_statistik
                hasil = wsM3_Statistik.M3S_ProdukProfit(param)
            Case "M3S_ProdukLaris"
                Dim wsM3_Statistik As New m3_statistik
                hasil = wsM3_Statistik.M3S_ProdukLaris(param)
            Case "M3S_ProdukStokMinim"
                Dim wsM3_Statistik As New m3_statistik
                hasil = wsM3_Statistik.M3S_ProdukStokMinim(param)

                '*********************************** M4 '***********************************

                'M4_PRINT
            Case "M4_Print"
                Dim wsM4_Print As New m4_print
                hasil = wsM4_Print.M4_Print(param)

                'M4_NOTES
            Case "M4_NotesSimpan"
                Dim wsM4_Notes As New m4_notes
                hasil = wsM4_Notes.M4_NotesSimpan(param)
            Case "M4_NotesSearch"
                Dim wsM4_Notes As New m4_notes
                hasil = wsM4_Notes.M4_NotesSearch(param)
            Case "M4_NotesDelete"
                Dim wsM4_Notes As New m4_notes
                hasil = wsM4_Notes.M4_NotesDelete(param)

                'M4_FILES
            Case "M4_FilesSimpan"
                Dim wsM4_Files As New m4_files
                hasil = wsM4_Files.M4_FilesSimpan(param)
            Case "M4_FilesSearch"
                Dim wsM4_Files As New m4_files
                hasil = wsM4_Files.M4_FilesSearch(param)
            Case "M4_FilesDelete"
                Dim wsM4_Files As New m4_files
                hasil = wsM4_Files.M4_FilesDelete(param)

                'M4_PR
            Case "M4_PrSimpan"
                Dim wsM4_Pr As New m4_pr
                hasil = wsM4_Pr.M4_PrSimpan(param)
            Case "M4_PrSearch"
                Dim wsM4_Pr As New m4_pr
                hasil = wsM4_Pr.M4_PrSearch(param)
            Case "M4_PrDelete"
                If (isDemo = False) Then
                    Dim wsM4_Pr As New m4_pr
                    hasil = wsM4_Pr.M4_PrDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M4_PrGetdataById"
                Dim wsM4_Pr As New m4_pr
                hasil = wsM4_Pr.M4_PrGetdataById(param)
            Case "M4_PrUpdateStatus"
                Dim wsM4_Pr As New m4_pr
                hasil = wsM4_Pr.M4_PrUpdateStatus(param)
            Case "M4_Pr_Detail_VSearch"
                Dim wsM4_Pr As New m4_pr
                hasil = wsM4_Pr.M4_Pr_Detail_VSearch(param)
            Case "M4_PrTerkait"
                Dim wsM4_Pr As New m4_pr
                hasil = wsM4_Pr.M4_PrTerkait(param)
            Case "M4_Pr_HistorySimpan"
                Dim wsM4_Pr As New m4_pr_history
                hasil = wsM4_Pr.M4_Pr_HistorySimpan(param)
            Case "M4_Pr_HistorySearch"
                Dim wsM4_Pr As New m4_pr_history
                hasil = wsM4_Pr.M4_Pr_HistorySearch(param)
            Case "M4_PrHistoryGetdataById"
                Dim wsM4_Pr As New m4_pr_history
                hasil = wsM4_Pr.M4_PrHistoryGetdataById(param)

                'M4_RQ
            Case "M4_RqSimpan"
                Dim wsM4_Rq As New m4_rq
                hasil = wsM4_Rq.M4_RqSimpan(param)
            Case "M4_RqSearch"
                Dim wsM4_Rq As New m4_rq
                hasil = wsM4_Rq.M4_RqSearch(param)
            Case "M4_RqDelete"
                If (isDemo = False) Then
                    Dim wsM4_Rq As New m4_rq
                    hasil = wsM4_Rq.M4_RqDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M4_RqGetdataById"
                Dim wsM4_Rq As New m4_rq
                hasil = wsM4_Rq.M4_RqGetdataById(param)
            Case "M4_Rq_Detail_VSearch"
                Dim wsM4_Rq As New m4_rq
                hasil = wsM4_Rq.M4_Rq_Detail_VSearch(param)
            Case "M4_RqUpdateStatus"
                Dim wsM4_Rq As New m4_rq
                hasil = wsM4_Rq.M4_RqUpdateStatus(param)
            Case "M4_RqTerkait"
                Dim wsM4_Rq As New m4_rq
                hasil = wsM4_Rq.M4_RqTerkait(param)
            Case "M4_Rq_HistorySimpan"
                Dim wsM4_Rq As New m4_rq_history
                hasil = wsM4_Rq.M4_Rq_HistorySimpan(param)
            Case "M4_Rq_HistorySearch"
                Dim wsM4_Rq As New m4_rq_history
                hasil = wsM4_Rq.M4_Rq_HistorySearch(param)
            Case "M4_Rq_HistorySearch"
                Dim wsM4_Rq As New m4_rq_history
                hasil = wsM4_Rq.M4_Rq_HistorySearch(param)
            Case "M4_RqHistoryGetdataById"
                Dim wsM4_Rq As New m4_rq_history
                hasil = wsM4_Rq.M4_RqHistoryGetdataById(param)

                'M4_BS
            Case "M4_BsSimpan"
                Dim wsM4_Bs As New m4_bs
                hasil = wsM4_Bs.M4_BsSimpan(param)
            Case "M4_BsSearch"
                Dim wsM4_Bs As New m4_bs
                hasil = wsM4_Bs.M4_BsSearch(param)
            Case "M4_BsDelete"
                If (isDemo = False) Then
                    Dim wsM4_Bs As New m4_bs
                    hasil = wsM4_Bs.M4_BsDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M4_BsGetdataById"
                Dim wsM4_Bs As New m4_bs
                hasil = wsM4_Bs.M4_BsGetdataById(param)
            Case "M4_BsUpdateStatus"
                Dim wsM4_Bs As New m4_bs
                hasil = wsM4_Bs.M4_BsUpdateStatus(param)
            Case "M4_BsTerkait"
                Dim wsM4_Bs As New m4_bs
                hasil = wsM4_Bs.M4_BsTerkait(param)
            Case "M4_Bs_HistorySimpan"
                Dim wsM4_Bs As New m4_bs_history
                hasil = wsM4_Bs.M4_Bs_HistorySimpan(param)
            Case "M4_Bs_HistorySearch"
                Dim wsM4_Bs As New m4_bs_history
                hasil = wsM4_Bs.M4_Bs_HistorySearch(param)
            Case "M4_BsHistoryGetdataById"
                Dim wsM4_Bs As New m4_bs_history
                hasil = wsM4_Bs.M4_BsHistoryGetdataById(param)

                'M4_PO
            Case "M4_PoSimpan"
                Dim wsM4_Po As New m4_po
                hasil = wsM4_Po.M4_PoSimpan(param)
            Case "M4_PoSearch"
                Dim wsM4_Po As New m4_po
                hasil = wsM4_Po.M4_PoSearch(param)
            Case "M4_PoDelete"
                If (isDemo = False) Then
                    Dim wsM4_Po As New m4_po
                    hasil = wsM4_Po.M4_PoDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M4_PoGetdataById"
                Dim wsM4_Po As New m4_po
                hasil = wsM4_Po.M4_PoGetdataById(param)
            Case "M4_PoUpdateStatus"
                Dim wsM4_Po As New m4_po
                hasil = wsM4_Po.M4_PoUpdateStatus(param)
            Case "M4_Po_Detail_VSearch"
                Dim wsM4_Po As New m4_po
                hasil = wsM4_Po.M4_Po_Detail_VSearch(param)
            Case "M4_Po_Detail_Cost"
                Dim wsM4_Po As New m4_po
                hasil = wsM4_Po.M4_Po_Detail_Cost(param)
            Case "M4_PoTerkait"
                Dim wsM4_Po As New m4_po
                hasil = wsM4_Po.M4_PoTerkait(param)
            Case "M4_Po_HistorySimpan"
                Dim wsM4_Po As New m4_po_history
                hasil = wsM4_Po.M4_Po_HistorySimpan(param)
            Case "M4_Po_HistorySearch"
                Dim wsM4_Po As New m4_po_history
                hasil = wsM4_Po.M4_Po_HistorySearch(param)
            Case "M4_PoHistoryGetdataById"
                Dim wsM4_Po As New m4_po_history
                hasil = wsM4_Po.M4_PoHistoryGetdataById(param)

                'M4_PF
            Case "M4_PfSimpan"
                Dim wsM4_Pf As New m4_pf
                hasil = wsM4_Pf.M4_PfSimpan(param)
            Case "M4_PfCloseItem"
                Dim wsM4_Pf As New m4_pf
                hasil = wsM4_Pf.M4_PfCloseItem(param)
            Case "M4_PfSearch"
                Dim wsM4_Pf As New m4_pf
                hasil = wsM4_Pf.M4_PfSearch(param)
            Case "M4_PfDelete"
                If (isDemo = False) Then
                    Dim wsM4_Pf As New m4_pf
                    hasil = wsM4_Pf.M4_PfDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M4_PfGetdataById"
                Dim wsM4_Pf As New m4_pf
                hasil = wsM4_Pf.M4_PfGetdataById(param)
            Case "M4_PfUpdateStatus"
                Dim wsM4_Pf As New m4_pf
                hasil = wsM4_Pf.M4_PfUpdateStatus(param)
            Case "M4_Pf_Detail_VSearch"
                Dim wsM4_Pf As New m4_pf
                hasil = wsM4_Pf.M4_Pf_Detail_VSearch(param)
            Case "M4_Pf_Detail_Cost"
                Dim wsM4_Pf As New m4_pf
                hasil = wsM4_Pf.M4_Pf_Detail_Cost(param)
            Case "M4_PfTerkait"
                Dim wsM4_Pf As New m4_pf
                hasil = wsM4_Pf.M4_PfTerkait(param)
            Case "M4_Pf_HistorySimpan"
                Dim wsM4_Pf As New m4_pf_history
                hasil = wsM4_Pf.M4_Pf_HistorySimpan(param)
            Case "M4_Pf_HistorySearch"
                Dim wsM4_Pf As New m4_pf_history
                hasil = wsM4_Pf.M4_Pf_HistorySearch(param)
            Case "M4_PfHistoryGetdataById"
                Dim wsM4_Pf As New m4_pf_history
                hasil = wsM4_Pf.M4_PfHistoryGetdataById(param)

                'M4_AP
            Case "M4_ApSimpan"
                Dim wsM4_Ap As New m4_ap
                hasil = wsM4_Ap.M4_ApSimpan(param)
            Case "M4_ApSearch"
                Dim wsM4_Ap As New m4_ap
                hasil = wsM4_Ap.M4_ApSearch(param)
            Case "M4_ApDelete"
                If (isDemo = False) Then
                    Dim wsM4_Ap As New m4_ap
                    hasil = wsM4_Ap.M4_ApDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M4_ApGetdataById"
                Dim wsM4_Ap As New m4_ap
                hasil = wsM4_Ap.M4_ApGetdataById(param)
            Case "M4_ApUpdateStatus"
                Dim wsM4_Ap As New m4_ap
                hasil = wsM4_Ap.M4_ApUpdateStatus(param)
            Case "M4_ApTerkait"
                Dim wsM4_Ap As New m4_ap
                hasil = wsM4_Ap.M4_ApTerkait(param)
            Case "M4_Ap_HistorySimpan"
                Dim wsM4_Ap As New m4_ap_history
                hasil = wsM4_Ap.M4_Ap_HistorySimpan(param)
            Case "M4_Ap_HistorySearch"
                Dim wsM4_Ap As New m4_ap_history
                hasil = wsM4_Ap.M4_Ap_HistorySearch(param)
            Case "M4_ApHistoryGetdataById"
                Dim wsM4_Ap As New m4_ap_history
                hasil = wsM4_Ap.M4_ApHistoryGetdataById(param)

                'M4_GRN
            Case "M4_GrnSimpan"
                Dim wsM4_Grn As New m4_grn
                hasil = wsM4_Grn.M4_GrnSimpan(param)
            Case "M4_GrnSearch"
                Dim wsM4_Grn As New m4_grn
                hasil = wsM4_Grn.M4_GrnSearch(param)
            Case "M4_GrnDelete"
                If (isDemo = False) Then
                    Dim wsM4_Grn As New m4_grn
                    hasil = wsM4_Grn.M4_GrnDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M4_GrnGetdataById"
                Dim wsM4_Grn As New m4_grn
                hasil = wsM4_Grn.M4_GrnGetdataById(param)
            Case "M4_GrnUpdateStatus"
                Dim wsM4_Grn As New m4_grn
                hasil = wsM4_Grn.M4_GrnUpdateStatus(param)
            Case "M4_GrnTerkait"
                Dim wsM4_Grn As New m4_grn
                hasil = wsM4_Grn.M4_GrnTerkait(param)
            Case "M4_Grn_Detail_VSearch"
                Dim wsM4_Grn As New m4_grn
                hasil = wsM4_Grn.M4_Grn_Detail_VSearch(param)
            Case "M4_Grn_Detail_Cost"
                Dim wsM4_Grn As New m4_grn
                hasil = wsM4_Grn.M4_Grn_Detail_Cost(param)
            Case "M4_Grn_HistorySimpan"
                Dim wsM4_Grn As New m4_grn_history
                hasil = wsM4_Grn.m4_Grn_HistorySimpan(param)
            Case "M4_Grn_HistorySearch"
                Dim wsM4_Grn As New m4_grn_history
                hasil = wsM4_Grn.M4_Grn_HistorySearch(param)
            Case "M4_GrnHistoryGetdataById"
                Dim wsM4_Grn As New m4_grn_history
                hasil = wsM4_Grn.M4_GrnHistoryGetdataById(param)

                'M4_RI
            Case "M4_RiSimpan"
                Dim wsM4_Ri As New m4_ri
                hasil = wsM4_Ri.M4_RiSimpan(param)
            Case "M4_RiSearch"
                Dim wsM4_Ri As New m4_ri
                hasil = wsM4_Ri.M4_RiSearch(param)
            Case "M4_RiDelete"
                If (isDemo = False) Then
                    Dim wsM4_Ri As New m4_ri
                    hasil = wsM4_Ri.M4_RiDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M4_RiGetdataById"
                Dim wsM4_Ri As New m4_ri
                hasil = wsM4_Ri.M4_RiGetdataById(param)
            Case "M4_RiUpdateStatus"
                Dim wsM4_Ri As New m4_ri
                hasil = wsM4_Ri.M4_RiUpdateStatus(param)
            Case "M4_RiTerkait"
                Dim wsM4_Ri As New m4_ri
                hasil = wsM4_Ri.M4_RiTerkait(param)
            Case "M4_Ri_Detail_VSearch"
                Dim wsM4_Ri As New m4_ri
                hasil = wsM4_Ri.M4_Ri_Detail_VSearch(param)
            Case "M4_Ri_HistorySimpan"
                Dim wsM4_Ri As New m4_ri_history
                hasil = wsM4_Ri.M4_Ri_HistorySimpan(param)
            Case "M4_Ri_HistorySearch"
                Dim wsM4_Ri As New m4_ri_history
                hasil = wsM4_Ri.M4_Ri_HistorySearch(param)
            Case "M4_RiHistoryGetdataById"
                Dim wsM4_Ri As New m4_ri_history
                hasil = wsM4_Ri.M4_RiHistoryGetdataById(param)
            Case "M4_RiBalance"
                Dim wsM4_Ri As New m4_ri
                hasil = wsM4_Ri.M4_RiBalance(param)
            Case "M4_RiBSearch"
                Dim wsM4_Ri As New m4_ri
                hasil = wsM4_Ri.M4_RiBSearch(param)
            Case "M4_RiBUpdateStatus"
                Dim wsM4_Ri As New m4_ri
                hasil = wsM4_Ri.M4_RiBUpdateStatus(param)
            Case "M4_RiBDelete"
                If (isDemo = False) Then
                    Dim wsM4_Ri As New m4_ri
                    hasil = wsM4_Ri.M4_RiBDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If

            Case "M4_RiUpdateUraian"
                Dim wsM4_Ri As New m4_ri
                hasil = wsM4_Ri.M4_RiUpdateUraian(param)
            Case "M4_RiImport"
                Dim wsM4_Ri As New m4_ri
                hasil = wsM4_Ri.M4_RiImport(param)

                'M4_PP
            Case "M4_PpSimpan"
                Dim wsM4_Pp As New m4_pp
                hasil = wsM4_Pp.M4_PpSimpan(param)
            Case "M4_PpSearch"
                Dim wsM4_Pp As New m4_pp
                hasil = wsM4_Pp.M4_PpSearch(param)
            Case "M4_PpDelete"
                Dim wsM4_Pp As New m4_pp
                hasil = wsM4_Pp.M4_PpDelete(param)
            Case "M4_PpGetdataById"
                Dim wsM4_Pp As New m4_pp
                hasil = wsM4_Pp.M4_PpGetdataById(param)
            Case "M4_PpUpdateStatus"
                Dim wsM4_Pp As New m4_pp
                hasil = wsM4_Pp.M4_PpUpdateStatus(param)
            Case "M4_PpTerkait"
                Dim wsM4_Pp As New m4_pp
                hasil = wsM4_Pp.M4_PpTerkait(param)

                'M4_DNR
            Case "M4_DnrSimpan"
                Dim wsM4_Dnr As New m4_dnr
                hasil = wsM4_Dnr.M4_DnrSimpan(param)
            Case "M4_DnrSearch"
                Dim wsM4_Dnr As New m4_dnr
                hasil = wsM4_Dnr.M4_DnrSearch(param)
            Case "M4_DnrDelete"
                If (isDemo = False) Then
                    Dim wsM4_Dnr As New m4_dnr
                    hasil = wsM4_Dnr.M4_DnrDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M4_DnrGetdataById"
                Dim wsM4_Dnr As New m4_dnr
                hasil = wsM4_Dnr.M4_DnrGetdataById(param)
            Case "M4_DnrUpdateStatus"
                Dim wsM4_Dnr As New m4_dnr
                hasil = wsM4_Dnr.M4_DnrUpdateStatus(param)
            Case "M4_DnrTerkait"
                Dim wsM4_Dnr As New m4_dnr
                hasil = wsM4_Dnr.M4_DnrTerkait(param)
            Case "M4_Dnr_Detail_VSearch"
                Dim wsM4_Dnr As New m4_dnr
                hasil = wsM4_Dnr.M4_Dnr_Detail_VSearch(param)
            Case "M4_Dnr_HistorySimpan"
                Dim wsM4_Dnr As New m4_dnr_history
                hasil = wsM4_Dnr.m4_Dnr_HistorySimpan(param)
            Case "M4_Dnr_HistorySearch"
                Dim wsM4_Dnr As New m4_dnr_history
                hasil = wsM4_Dnr.M4_Dnr_HistorySearch(param)
            Case "M4_DnrHistoryGetdataById"
                Dim wsM4_Dnr As New m4_dnr_history
                hasil = wsM4_Dnr.M4_DnrHistoryGetdataById(param)

                'M4_PRT
            Case "M4_PrtSimpan"
                Dim wsM4_Prt As New m4_prt
                hasil = wsM4_Prt.M4_PrtSimpan(param)
            Case "M4_PrtSearch"
                Dim wsM4_Prt As New m4_prt
                hasil = wsM4_Prt.M4_PrtSearch(param)
            Case "M4_PrtDelete"
                If (isDemo = False) Then
                    Dim wsM4_Prt As New m4_prt
                    hasil = wsM4_Prt.M4_PrtDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M4_PrtGetdataById"
                Dim wsM4_Prt As New m4_prt
                hasil = wsM4_Prt.M4_PrtGetdataById(param)
            Case "M4_PrtUpdateStatus"
                Dim wsM4_Prt As New m4_prt
                hasil = wsM4_Prt.M4_PrtUpdateStatus(param)
            Case "M4_PrtTerkait"
                Dim wsM4_Prt As New m4_prt
                hasil = wsM4_Prt.M4_PrtTerkait(param)
            Case "M4_Prt_HistorySimpan"
                Dim wsM4_Prt As New m4_prt_history
                hasil = wsM4_Prt.m4_Prt_HistorySimpan(param)
            Case "M4_Prt_HistorySearch"
                Dim wsM4_Prt As New m4_prt_history
                hasil = wsM4_Prt.M4_Prt_HistorySearch(param)
            Case "M4_PrtHistoryGetdataById"
                Dim wsM4_Prt As New m4_prt_history
                hasil = wsM4_Prt.M4_PrtHistoryGetdataById(param)
            Case "M4_PrtBalance"
                Dim wsM4_Prt As New m4_prt
                hasil = wsM4_Prt.M4_PrtBalance(param)
            Case "M4_PrtBSearch"
                Dim wsM4_Prt As New m4_prt
                hasil = wsM4_Prt.M4_PrtBSearch(param)
            Case "M4_PrtBUpdateStatus"
                Dim wsM4_Prt As New m4_prt
                hasil = wsM4_Prt.M4_PrtBUpdateStatus(param)
            Case "M4_PrtBDelete"
                If (isDemo = False) Then
                    Dim wsM4_Prt As New m4_prt
                    hasil = wsM4_Prt.M4_PrtBDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If

                'M4_VPP
            Case "M4_VppSimpan"
                Dim wsM4_Vpp As New m4_vpp
                hasil = wsM4_Vpp.M4_VppSimpan(param)
            Case "M4_VppSearch"
                Dim wsM4_Vpp As New m4_vpp
                hasil = wsM4_Vpp.M4_VppSearch(param)
            Case "M4_VppDelete"
                If (isDemo = False) Then
                    Dim wsM4_Vpp As New m4_vpp
                    hasil = wsM4_Vpp.M4_VppDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M4_VppGetdataById"
                Dim wsM4_Vpp As New m4_vpp
                hasil = wsM4_Vpp.M4_VppGetdataById(param)
            Case "M4_VppGetdataByIdSerenity"
                Dim wsM4_Vpp As New m4_vpp
                hasil = wsM4_Vpp.M4_VppGetdataByIdSerenity(param)
            Case "M4_VppUpdateStatus"
                Dim wsM4_Vpp As New m4_vpp
                hasil = wsM4_Vpp.M4_VppUpdateStatus(param)
            Case "M4_VppTerkait"
                Dim wsM4_Vpp As New m4_vpp
                hasil = wsM4_Vpp.M4_VppTerkait(param)
            Case "M4_VppTakedataSearch"
                Dim wsM4_Vpp As New m4_vpp
                hasil = wsM4_Vpp.M4_VppTakedataSearch(param)
            Case "M4_Vpp_HistorySimpan"
                Dim wsM4_Vpp As New m4_vpp_history
                hasil = wsM4_Vpp.M4_Vpp_HistorySimpan(param)
            Case "M4_Vpp_HistorySearch"
                Dim wsM4_Vpp As New m4_vpp_history
                hasil = wsM4_Vpp.M4_Vpp_HistorySearch(param)
            Case "M4_VppHistoryGetdataById"
                Dim wsM4_Vpp As New m4_vpp_history
                hasil = wsM4_Vpp.M4_VppHistoryGetdataById(param)

                'M4_VP
            Case "M4_VpSimpan"
                Dim wsM4_Vp As New m4_vp
                hasil = wsM4_Vp.M4_VpSimpan(param)
            Case "M4_VpSearch"
                Dim wsM4_Vp As New m4_vp
                hasil = wsM4_Vp.M4_VpSearch(param)
            Case "M4_VpDelete"
                If (isDemo = False) Then
                    Dim wsM4_Vp As New m4_vp
                    hasil = wsM4_Vp.M4_VpDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M4_VpGetdataById"
                Dim wsM4_Vp As New m4_vp
                hasil = wsM4_Vp.M4_VpGetdataById(param)
            Case "M4_VpGetdataByIdSerenity"
                Dim wsM4_Vp As New m4_vp
                hasil = wsM4_Vp.M4_VpGetdataByIdSerenity(param)
            Case "M4_VpUpdateStatus"
                Dim wsM4_Vp As New m4_vp
                hasil = wsM4_Vp.M4_VpUpdateStatus(param)
            Case "M4_VpTerkait"
                Dim wsM4_Vp As New m4_vp
                hasil = wsM4_Vp.M4_VpTerkait(param)
            Case "M4_Vp_HistorySimpan"
                Dim wsM4_Vpp As New m4_vp_history
                hasil = wsM4_Vpp.M4_Vp_HistorySimpan(param)
            Case "M4_Vp_HistorySearch"
                Dim wsM4_Vpp As New m4_vp_history
                hasil = wsM4_Vpp.M4_Vp_HistorySearch(param)
            Case "M4_VpHistoryGetdataById"
                Dim wsM4_Vpp As New m4_vp_history
                hasil = wsM4_Vpp.M4_VpHistoryGetdataById(param)


                'M4_PIE
            Case "M4_PieSimpan"
                Dim wsM4_Pie As New m4_pie
                hasil = wsM4_Pie.M4_PieSimpan(param)
            Case "M4_PieUpdateStatus"
                Dim wsM4_Pie As New m4_pie
                hasil = wsM4_Pie.M4_PieUpdateStatus(param)
            Case "M4_PieDelete"
                Dim wsM4_Pie As New m4_pie
                hasil = wsM4_Pie.M4_PieDelete(param)
            Case "M4_PieSearch"
                Dim wsM4_Pie As New m4_pie
                hasil = wsM4_Pie.M4_PieSearch(param)
            Case "M4_PieGetdataById"
                Dim wsM4_Pie As New m4_pie
                hasil = wsM4_Pie.M4_PieGetdataById(param)
            Case "M4_PieTakedataSearch"
                Dim wsM4_Pie As New m4_pie
                hasil = wsM4_Pie.M4_PieTakedataSearch(param)

            Case "M4_Pie_HistorySimpan"
                Dim wsM4_Pie As New m4_pie_history
                hasil = wsM4_Pie.M4_Pie_HistorySimpan(param)
            Case "M4_Pie_HistorySearch"
                Dim wsM4_Pie As New m4_pie_history
                hasil = wsM4_Pie.M4_Pie_HistorySearch(param)
            Case "M4_PieHistoryGetdataById"
                Dim wsM4_Pie As New m4_pie_history
                hasil = wsM4_Pie.M4_PieHistoryGetdataById(param)


                'M4_RFQ
            Case "M4_RfqSimpan"
                Dim wsM4_Rfq As New m4_rfq
                hasil = wsM4_Rfq.M4_RfqSimpan(param)
            Case "M4_RfqUpdateStatus"
                Dim wsM4_Rfq As New m4_rfq
                hasil = wsM4_Rfq.M4_RfqUpdateStatus(param)
            Case "M4_RfqDelete"
                Dim wsM4_Rfq As New m4_rfq
                hasil = wsM4_Rfq.M4_RfqDelete(param)
            Case "M4_RfqSearch"
                Dim wsM4_Rfq As New m4_rfq
                hasil = wsM4_Rfq.M4_RfqSearch(param)
            Case "M4_RfqGetdataById"
                Dim wsM4_Rfq As New m4_rfq
                hasil = wsM4_Rfq.M4_RfqGetdataById(param)

            Case "M4_Rfq_HistorySimpan"
                Dim wsM4_Rfq As New m4_rfq_history
                hasil = wsM4_Rfq.M4_Rfq_HistorySimpan(param)
            Case "M4_Rfq_HistorySearch"
                Dim wsM4_Rfq As New m4_rfq_history
                hasil = wsM4_Rfq.M4_Rfq_HistorySearch(param)
            Case "M4_RfqHistoryGetdataById"
                Dim wsM4_Rfq As New m4_rfq_history
                hasil = wsM4_Rfq.M4_RfqHistoryGetdataById(param)


                '*********************************** M5 '***********************************

                'M5_PRINT
            Case "M5_Print"
                Dim wsM5_Print As New m5_print
                hasil = wsM5_Print.M5_Print(param)

                'M5_NOTES
            Case "M5_NotesSimpan"
                Dim wsM5_Notes As New m5_notes
                hasil = wsM5_Notes.M5_NotesSimpan(param)
            Case "M5_NotesSearch"
                Dim wsM5_Notes As New m5_notes
                hasil = wsM5_Notes.M5_NotesSearch(param)
            Case "M5_NotesDelete"
                Dim wsM5_Notes As New m5_notes
                hasil = wsM5_Notes.M5_NotesDelete(param)

                'M5_FILES
            Case "M5_FilesSimpan"
                Dim wsM5_Files As New m5_files
                hasil = wsM5_Files.M5_FilesSimpan(param)
            Case "M5_FilesSearch"
                Dim wsM5_Files As New m5_files
                hasil = wsM5_Files.M5_FilesSearch(param)
            Case "M5_FilesDelete"
                Dim wsM5_Files As New m5_files
                hasil = wsM5_Files.M5_FilesDelete(param)

                'M5_AS
            Case "M5_AsSimpan"
                Dim wsM5_As As New m5_as
                hasil = wsM5_As.M5_AsSimpan(param)
            Case "M5_AsSearch"
                Dim wsM5_As As New m5_as
                hasil = wsM5_As.M5_AsSearch(param)
            Case "M5_AsDelete"
                If (isDemo = False) Then
                    Dim wsM5_As As New m5_as
                    hasil = wsM5_As.M5_AsDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_AsGetdataById"
                Dim wsM5_As As New m5_as
                hasil = wsM5_As.M5_AsGetdataById(param)
            Case "M5_AsUpdateStatus"
                Dim wsM5_As As New m5_as
                hasil = wsM5_As.M5_AsUpdateStatus(param)
            Case "M5_AsTerkait"
                Dim wsM5_As As New m5_as
                hasil = wsM5_As.M5_AsTerkait(param)
            Case "M5_AsTerkait_S"
                Dim wsM5_As As New m5_as
                hasil = wsM5_As.M5_AsTerkait_S(param)
            Case "M5_As_HistorySimpan"
                Dim wsM5_As As New m5_as_history
                hasil = wsM5_As.M5_As_HistorySimpan(param)
            Case "M5_As_HistorySearch"
                Dim wsM5_As As New m5_as_history
                hasil = wsM5_As.M5_As_HistorySearch(param)
            Case "M5_AsHistoryGetdataById"
                Dim wsM5_As As New m5_as_history
                hasil = wsM5_As.M5_AsHistoryGetdataById(param)

                'M11_RK
            Case "M11_RkSimpan"
                Dim wsM11_Rk As New m11_rk
                hasil = wsM11_Rk.M11_RkSimpan(param)
            Case "M11_RkSearch"
                Dim wsM11_Rk As New m11_rk
                hasil = wsM11_Rk.M11_RkSearch(param)
            Case "M11_RkDelete"
                If (isDemo = False) Then
                    Dim wsM11_Rk As New m11_rk
                    hasil = wsM11_Rk.M11_RkDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M11_RkGetdataById"
                Dim wsM11_Rk As New m11_rk
                hasil = wsM11_Rk.M11_RkGetdataById(param)
            Case "M11_RkUpdateStatus"
                Dim wsM11_Rk As New m11_rk
                hasil = wsM11_Rk.M11_RkUpdateStatus(param)
            Case "M11_RkTerkait"
                Dim wsM11_Rk As New m11_rk
                hasil = wsM11_Rk.M11_RkTerkait(param)

                'M11_PB
            Case "M11_PvSimpan"
                Dim wsM11_Pv As New m11_pb
                hasil = wsM11_Pv.M11_PvSimpan(param)
            Case "M11_PvSearch"
                Dim wsM11_Pv As New m11_pb
                hasil = wsM11_Pv.M11_PvSearch(param)
            Case "M11_PvDelete"
                If (isDemo = False) Then
                    Dim wsM11_Pv As New m11_pb
                    hasil = wsM11_Pv.M11_PvDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M11_PvGetdataById"
                Dim wsM11_Pv As New m11_pb
                hasil = wsM11_Pv.M11_PvGetdataById(param)
            Case "M11_PvUpdateStatus"
                Dim wsM11_Pv As New m11_pb
                hasil = wsM11_Pv.M11_PvUpdateStatus(param)
            Case "M11_PvTerkait"
                Dim wsM11_Pv As New m11_pb
                hasil = wsM11_Pv.M11_PvTerkait(param)

                'M5_DO
            Case "M5_DoSimpan"
                Dim wsM5_Do As New m5_do
                hasil = wsM5_Do.M5_DoSimpan(param)
            Case "M5_DoSearch"
                Dim wsM5_Do As New m5_do
                hasil = wsM5_Do.M5_DoSearch(param)
            Case "M5_DoDelete"
                If (isDemo = False) Then
                    Dim wsM5_Do As New m5_do
                    hasil = wsM5_Do.M5_DoDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_DoGetdataById"
                Dim wsM5_Do As New m5_do
                hasil = wsM5_Do.M5_DoGetdataById(param)
            Case "M5_DoUpdateStatus"
                Dim wsM5_Do As New m5_do
                hasil = wsM5_Do.M5_DoUpdateStatus(param)
            Case "M5_Do_Detail_VSearch"
                Dim wsM5_do As New m5_do
                hasil = wsM5_do.M5_Do_Detail_VSearch(param)
            Case "M5_DoTerkait"
                Dim wsM5_do As New m5_do
                hasil = wsM5_do.M5_DoTerkait(param)
            Case "M5_Do_HistorySimpan"
                Dim wsM5_do As New m5_do_history
                hasil = wsM5_do.m5_Do_HistorySimpan(param)
            Case "M5_Do_HistorySearch"
                Dim wsM5_do As New m5_do_history
                hasil = wsM5_do.M5_Do_HistorySearch(param)
            Case "M5_DoHistoryGetdataById"
                Dim wsM5_do As New m5_do_history
                hasil = wsM5_do.M5_DoHistoryGetdataById(param)

                'M5_DR
            Case "M5_DrSimpan"
                Dim wsM5_Dr As New m5_dr
                hasil = wsM5_Dr.M5_DrSimpan(param)
            Case "M5_DrSearch"
                Dim wsM5_Dr As New m5_dr
                hasil = wsM5_Dr.M5_DrSearch(param)
            Case "M5_DrDelete"
                If (isDemo = False) Then
                    Dim wsM5_Dr As New m5_dr
                    hasil = wsM5_Dr.M5_DrDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_DrGetdataById"
                Dim wsM5_Dr As New m5_dr
                hasil = wsM5_Dr.M5_DrGetdataById(param)
            Case "M5_DrUpdateStatus"
                Dim wsM5_Dr As New m5_dr
                hasil = wsM5_Dr.M5_DrUpdateStatus(param)
            Case "M5_Dr_Detail_VSearch"
                Dim wsM5_dr As New m5_dr
                hasil = wsM5_dr.M5_Dr_Detail_VSearch(param)
            Case "M5_DrTerkait"
                Dim wsM5_dr As New m5_dr
                hasil = wsM5_dr.M5_DrTerkait(param)
            Case "M5_Dr_HistorySimpan"
                Dim wsM5_Dr As New m5_dr_history
                hasil = wsM5_Dr.m5_Dr_HistorySimpan(param)
            Case "M5_Dr_HistorySearch"
                Dim wsM5_Dr As New m5_dr_history
                hasil = wsM5_Dr.M5_Dr_HistorySearch(param)
            Case "M5_DrHistoryGetdataById"
                Dim wsM5_Dr As New m5_dr_history
                hasil = wsM5_Dr.M5_DrHistoryGetdataById(param)

                'M5_IC
            Case "M5_IcSimpan"
                Dim wsM5_Ic As New m5_ic
                hasil = wsM5_Ic.M5_IcSimpan(param)
            Case "M5_IcSearch"
                Dim wsM5_Ic As New m5_ic
                hasil = wsM5_Ic.M5_IcSearch(param)
            Case "M5_IcDelete"
                If (isDemo = False) Then
                    Dim wsM5_Ic As New m5_ic
                    hasil = wsM5_Ic.M5_IcDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_IcGetdataById"
                Dim wsM5_Ic As New m5_ic
                hasil = wsM5_Ic.M5_IcGetdataById(param)
            Case "M5_IcGetdataByIdSerenity"
                Dim wsM5_Ic As New m5_ic
                hasil = wsM5_Ic.M5_IcGetdataByIdSerenity(param)
            Case "M5_IcUpdateStatus"
                Dim wsM5_Ic As New m5_ic
                hasil = wsM5_Ic.M5_IcUpdateStatus(param)
            Case "M5_IcTakedataSearch"
                Dim wsM5_Ic As New m5_ic
                hasil = wsM5_Ic.M5_IcTakedataSearch(param)
            Case "M5_IcTerkait"
                Dim wsM5_Ic As New m5_ic
                hasil = wsM5_Ic.M5_IcTerkait(param)
            Case "M5_Ic_HistorySimpan"
                Dim wsM5_Ic As New m5_ic_history
                hasil = wsM5_Ic.M5_Ic_HistorySimpan(param)
            Case "M5_Ic_HistorySearch"
                Dim wsM5_Ic As New m5_ic_history
                hasil = wsM5_Ic.M5_Ic_HistorySearch(param)
            Case "M5_IcHistoryGetdataById"
                Dim wsM5_Ic As New m5_ic_history
                hasil = wsM5_Ic.M5_IcHistoryGetdataById(param)

                'M5_PI
            Case "M5_PiSimpan"
                Dim wsM5_Pi As New m5_pi
                hasil = wsM5_Pi.M5_PiSimpan(param)
            Case "M5_PiSearch"
                Dim wsM5_Pi As New m5_pi
                hasil = wsM5_Pi.M5_PiSearch(param)
            Case "M5_PiDelete"
                If (isDemo = False) Then
                    Dim wsM5_Pi As New m5_pi
                    hasil = wsM5_Pi.M5_PiDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_PiGetdataById"
                Dim wsM5_Pi As New m5_pi
                hasil = wsM5_Pi.M5_PiGetdataById(param)
            Case "M5_PiUpdateStatus"
                Dim wsM5_Pi As New m5_pi
                hasil = wsM5_Pi.M5_PiUpdateStatus(param)
            Case "M5_Pi_Detail_VSearch"
                Dim wsM5_pi As New m5_pi
                hasil = wsM5_pi.M5_Pi_Detail_VSearch(param)
            Case "M5_PiTerkait"
                Dim wsM5_pi As New m5_pi
                hasil = wsM5_pi.M5_PiTerkait(param)
            Case "M5_Pi_HistorySimpan"
                Dim wsM5_pi As New m5_pi_history
                hasil = wsM5_pi.M5_Pi_HistorySimpan(param)
            Case "M5_Pi_HistorySearch"
                Dim wsM5_pi As New m5_pi_history
                hasil = wsM5_pi.M5_Pi_HistorySearch(param)
            Case "M5_PiHistoryGetdataById"
                Dim wsM5_pi As New m5_pi_history
                hasil = wsM5_pi.M5_PiHistoryGetdataById(param)

                'M5_PV
            Case "M5_PvSimpan"
                Dim wsM5_Pv As New m5_pv
                hasil = wsM5_Pv.M5_PvSimpan(param)
            Case "M5_PvSearch"
                Dim wsM5_Pv As New m5_pv
                hasil = wsM5_Pv.M5_PvSearch(param)
            Case "M5_PvDelete"
                If (isDemo = False) Then
                    Dim wsM5_Pv As New m5_pv
                    hasil = wsM5_Pv.M5_PvDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_PvGetdataById"
                Dim wsM5_Pv As New m5_pv
                hasil = wsM5_Pv.M5_PvGetdataById(param)
            Case "M5_PvGetdataByIdSerenity"
                Dim wsM5_Pv As New m5_pv
                hasil = wsM5_Pv.M5_PvGetdataByIdSerenity(param)
            Case "M5_PvUpdateStatus"
                Dim wsM5_Pv As New m5_pv
                hasil = wsM5_Pv.M5_PvUpdateStatus(param)
            Case "M5_PvTerkait"
                Dim wsM5_Pv As New m5_pv
                hasil = wsM5_Pv.M5_PvTerkait(param)
            Case "M5_Pv_HistorySimpan"
                Dim wsM5_Pv As New m5_pv_history
                hasil = wsM5_Pv.M5_Pv_HistorySimpan(param)
            Case "M5_Pv_HistorySearch"
                Dim wsM5_Pv As New m5_pv_history
                hasil = wsM5_Pv.M5_Pv_HistorySearch(param)
            Case "M5_PvHistoryGetdataById"
                Dim wsM5_Pv As New m5_pv_history
                hasil = wsM5_Pv.M5_PvHistoryGetdataById(param)

                'M5_RNR
            Case "M5_RnrSimpan"
                Dim wsM5_Rnr As New m5_rnr
                hasil = wsM5_Rnr.M5_RnrSimpan(param)
            Case "M5_RnrSearch"
                Dim wsM5_Rnr As New m5_rnr
                hasil = wsM5_Rnr.M5_RnrSearch(param)
            Case "M5_RnrDelete"
                If (isDemo = False) Then
                    Dim wsM5_Rnr As New m5_rnr
                    hasil = wsM5_Rnr.M5_RnrDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_RnrGetdataById"
                Dim wsM5_Rnr As New m5_rnr
                hasil = wsM5_Rnr.M5_RnrGetdataById(param)
            Case "M5_RnrUpdateStatus"
                Dim wsM5_Rnr As New m5_rnr
                hasil = wsM5_Rnr.M5_RnrUpdateStatus(param)
            Case "M5_Rnr_Detail_VSearch"
                Dim wsM5_rnr As New m5_rnr
                hasil = wsM5_rnr.M5_Rnr_Detail_VSearch(param)
            Case "M5_RnrTerkait"
                Dim wsM5_rnr As New m5_rnr
                hasil = wsM5_rnr.M5_RnrTerkait(param)

                'M5_SI
            Case "M5_SINofakturPajakSimpan"
                Dim wsM5_Si As New m5_si
                hasil = wsM5_Si.M5_SINofakturPajakSimpan(param)
            Case "M5_SiSimpan"
                Dim wsM5_Si As New m5_si
                hasil = wsM5_Si.M5_SiSimpan(param)
            Case "M5_SiSimpanAtribut"
                Dim wsM5_Si As New m5_si
                hasil = wsM5_Si.M5_SiSimpanAtribut(param)

            Case "M5_SiSearch"
                Dim wsM5_Si As New m5_si
                hasil = wsM5_Si.M5_SiSearch(param)
            Case "M5_SiDelete"
                If (isDemo = False) Then
                    Dim wsM5_Si As New m5_si
                    hasil = wsM5_Si.M5_SiDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_SiGetdataById"
                Dim wsM5_Si As New m5_si
                hasil = wsM5_Si.M5_SiGetdataById(param)
            Case "M5_SiUpdateStatus"
                Dim wsM5_Si As New m5_si
                hasil = wsM5_Si.M5_SiUpdateStatus(param)
            Case "M5_Si_Detail_VSearch"
                Dim wsM5_si As New m5_si
                hasil = wsM5_si.M5_Si_Detail_VSearch(param)
            Case "M5_SiTerkait"
                Dim wsM5_si As New m5_si
                hasil = wsM5_si.M5_SiTerkait(param)
            Case "M5_SiTerkait_S"
                Dim wsM5_si As New m5_si
                hasil = wsM5_si.M5_SiTerkait_S(param)
            Case "M5_SiGetUpload"
                Dim wsM5_si As New m5_si
                hasil = wsM5_si.M5_SiGetUpload(param)
            Case "M5_SiGetdataUpload"
                Dim wsM5_si As New m5_si
                hasil = wsM5_si.M5_SiGetdataUpload(param)
            Case "M5_SiUploaded"
                Dim wsM5_si As New m5_si
                hasil = wsM5_si.M5_SiUploaded(param)
            Case "M5_Si_HistorySimpan"
                Dim wsM5_si As New m5_si_history
                hasil = wsM5_si.m5_Si_HistorySimpan(param)
            Case "M5_Si_HistorySearch"
                Dim wsM5_si As New m5_si_history
                hasil = wsM5_si.M5_Si_HistorySearch(param)
            Case "M5_SiHistoryGetdataById"
                Dim wsM5_si As New m5_si_history
                hasil = wsM5_si.M5_SiHistoryGetdataById(param)
            Case "M5_SiBalance"
                Dim wsM5_si As New m5_si
                hasil = wsM5_si.M5_SiBalance(param)
            Case "M5_SiBSearch"
                Dim wsM5_Si As New m5_si
                hasil = wsM5_Si.M5_SiBSearch(param)
            Case "M5_SiBUpdateStatus"
                Dim wsM5_si As New m5_si
                hasil = wsM5_si.M5_SiBUpdateStatus(param)
            Case "M5_SiBDelete"
                If (isDemo = False) Then
                    Dim wsM5_Si As New m5_si
                    hasil = wsM5_Si.M5_SiBDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
                'M5_HapusStokKurang
            Case "M5_HapusStokKurang"
                Dim wsM5_Si As New m5_si
                hasil = wsM5_Si.M5_HapusStokKurang(param)
            Case "M5_HapusStokKurangPergudang"
                Dim wsM5_Si As New m5_si
                hasil = wsM5_Si.M5_HapusStokKurangPergudang(param)

                'M5_Rp
            Case "M5_RpSimpan"
                Dim wsM5_Rp As New m5_rp
                hasil = wsM5_Rp.M5_RpSimpan(param)
            Case "M5_RpSearch"
                Dim wsM5_Rp As New m5_rp
                hasil = wsM5_Rp.M5_RpSearch(param)
            Case "M5_RpDelete"
                If (isDemo = False) Then
                    Dim wsM5_Rp As New m5_rp
                    hasil = wsM5_Rp.M5_RpDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_RpGetdataById"
                Dim wsM5_Rp As New m5_rp
                hasil = wsM5_Rp.M5_RpGetdataById(param)
            Case "M5_RpUpdateStatus"
                Dim wsM5_Rp As New m5_rp
                hasil = wsM5_Rp.M5_RpUpdateStatus(param)
            Case "M5_RpTerkait"
                Dim wsM5_Rp As New m5_rp
                hasil = wsM5_Rp.M5_RpTerkait(param)
            Case "M5_Rp_HistorySimpan"
                Dim wsM5_Rp As New m5_rp_history
                hasil = wsM5_Rp.M5_Rp_HistorySimpan(param)
            Case "M5_Rp_HistorySearch"
                Dim wsM5_Rp As New m5_rp_history
                hasil = wsM5_Rp.M5_Rp_HistorySearch(param)
            Case "M5_RpHistoryGetdataById"
                Dim wsM5_Rp As New m5_rp_history
                hasil = wsM5_Rp.M5_RpHistoryGetdataById(param)

                'M5_Ip
            Case "M5_IpSimpan"
                Dim wsM5_Ip As New m5_ip
                hasil = wsM5_Ip.M5_IpSimpan(param)
            Case "M5_IpSearch"
                Dim wsM5_Ip As New m5_ip
                hasil = wsM5_Ip.M5_IpSearch(param)
            Case "M5_IpDelete"
                If (isDemo = False) Then
                    Dim wsM5_Ip As New m5_ip
                    hasil = wsM5_Ip.M5_IpDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_IpGetdataById"
                Dim wsM5_Ip As New m5_ip
                hasil = wsM5_Ip.M5_IpGetdataById(param)
            Case "M5_IpUpdateStatus"
                Dim wsM5_Ip As New m5_ip
                hasil = wsM5_Ip.M5_IpUpdateStatus(param)
            Case "M5_IpTerkait"
                Dim wsM5_Ip As New m5_ip
                hasil = wsM5_Ip.M5_IpTerkait(param)
            Case "M5_Ip_HistorySimpan"
                Dim wsM5_Ip As New m5_ip_history
                hasil = wsM5_Ip.M5_Ip_HistorySimpan(param)
            Case "M5_Ip_HistorySearch"
                Dim wsM5_Ip As New m5_ip_history
                hasil = wsM5_Ip.M5_Ip_HistorySearch(param)
            Case "M5_IpHistoryGetdataById"
                Dim wsM5_Ip As New m5_ip_history
                hasil = wsM5_Ip.M5_Ip_HistoryGetdataById(param)

                'M5_SO
            Case "M5_SoSimpan"
                Dim wsM5_So As New m5_so
                hasil = wsM5_So.M5_SoSimpan(param)
            Case "M5_SoSearch"
                Dim wsM5_So As New m5_so
                hasil = wsM5_So.M5_SoSearch(param)
            Case "M5_SoDelete"
                If (isDemo = False) Then
                    Dim wsM5_So As New m5_so
                    hasil = wsM5_So.M5_SoDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_SoGetdataById"
                Dim wsM5_So As New m5_so
                hasil = wsM5_So.M5_SoGetdataById(param)
            Case "M5_SoUpdateStatus"
                Dim wsM5_So As New m5_so
                hasil = wsM5_So.M5_SoUpdateStatus(param)
            Case "M5_So_Detail_VSearch"
                Dim wsM5_So As New m5_so
                hasil = wsM5_So.M5_So_Detail_VSearch(param)
            Case "M5_So_Detail_VSearchGroup"
                Dim wsM5_So As New m5_so
                hasil = wsM5_So.M5_So_Detail_VSearchGroup(param)
            Case "M5_So_Detail_Outstanding"
                Dim wsM5_So As New m5_so
                hasil = wsM5_So.M5_So_Detail_Outstanding(param)
            Case "M5_SoTerkait"
                Dim wsM5_So As New m5_so
                hasil = wsM5_So.M5_SoTerkait(param)
            Case "M5_SoTerkait_S"
                Dim wsM5_So As New m5_so
                hasil = wsM5_So.M5_SoTerkait_S(param)
            Case "M5_So_HistorySimpan"
                Dim wsM5_So As New m5_so_history
                hasil = wsM5_So.M5_So_HistorySimpan(param)
            Case "M5_So_HistorySearch"
                Dim wsM5_So As New m5_so_history
                hasil = wsM5_So.M5_So_HistorySearch(param)
            Case "M5_SoHistoryGetdataById"
                Dim wsM5_So As New m5_so_history
                hasil = wsM5_So.M5_SoHistoryGetdataById(param)
            Case "M5_SoImport"
                Dim wsM5_So As New m5_so
                hasil = wsM5_So.M5_SoImport(param)

                'M5_SF
            Case "M5_SfSimpan"
                Dim wsM5_Sf As New m5_sf
                hasil = wsM5_Sf.M5_SfSimpan(param)
            Case "M5_SfSearch"
                Dim wsM5_Sf As New m5_sf
                hasil = wsM5_Sf.M5_SfSearch(param)
            Case "M5_SfDelete"
                If (isDemo = False) Then
                    Dim wsM5_Sf As New m5_sf
                    hasil = wsM5_Sf.M5_SfDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_SfGetdataById"
                Dim wsM5_Sf As New m5_sf
                hasil = wsM5_Sf.M5_SfGetdataById(param)
            Case "M5_SfUpdateStatus"
                Dim wsM5_Sf As New m5_sf
                hasil = wsM5_Sf.M5_SfUpdateStatus(param)
            Case "M5_Sf_Detail_VSearch"
                Dim wsM5_Sf As New m5_sf
                hasil = wsM5_Sf.M5_Sf_Detail_VSearch(param)
            Case "M5_SfTerkait"
                Dim wsM5_Sf As New m5_sf
                hasil = wsM5_Sf.M5_SfTerkait(param)
            Case "M5_SfTerkait_S"
                Dim wsM5_Sf As New m5_sf
                hasil = wsM5_Sf.M5_SfTerkait_S(param)
            Case "M5_Sf_HistorySimpan"
                Dim wsM5_Sf As New m5_sf_history
                hasil = wsM5_Sf.M5_Sf_HistorySimpan(param)
            Case "M5_Sf_HistorySearch"
                Dim wsM5_Sf As New m5_sf_history
                hasil = wsM5_Sf.M5_Sf_HistorySearch(param)
            Case "M5_SfHistoryGetdataById"
                Dim wsM5_Sf As New m5_sf_history
                hasil = wsM5_Sf.M5_SfHistoryGetdataById(param)

                'M5_SQ
            Case "M5_SqSimpan"
                Dim wsM5_Sq As New m5_sq
                hasil = wsM5_Sq.M5_SqSimpan(param)
            Case "M5_SqSearch"
                Dim wsM5_Sq As New m5_sq
                hasil = wsM5_Sq.M5_SqSearch(param)
            Case "M5_SqDelete"
                If (isDemo = False) Then
                    Dim wsM5_Sq As New m5_sq
                    hasil = wsM5_Sq.M5_SqDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_SqGetdataById"
                Dim wsM5_Sq As New m5_sq
                hasil = wsM5_Sq.M5_SqGetdataById(param)
            Case "M5_SqUpdateStatus"
                Dim wsM5_Sq As New m5_sq
                hasil = wsM5_Sq.M5_SqUpdateStatus(param)
            Case "M5_Sq_Detail_VSearch"
                Dim wsM5_Sq As New m5_sq
                hasil = wsM5_Sq.M5_Sq_Detail_VSearch(param)
            Case "M5_SQ_OUT_BAHAN"
                Dim wsM5_Sq As New m5_sq
                hasil = wsM5_Sq.M5_SQ_OUT_BAHAN(param)
            Case "M5_SqTerkait"
                Dim wsM5_Sq As New m5_sq
                hasil = wsM5_Sq.M5_SqTerkait(param)
            Case "M5_SqTerkait_S"
                Dim wsM5_Sq As New m5_sq
                hasil = wsM5_Sq.M5_SqTerkait_S(param)
            Case "M5_Sq_HistorySimpan"
                Dim wsM5_Sq As New m5_sq_history
                hasil = wsM5_Sq.M5_Sq_HistorySimpan(param)
            Case "M5_Sq_HistorySearch"
                Dim wsM5_Sq As New m5_sq_history
                hasil = wsM5_Sq.M5_Sq_HistorySearch(param)
            Case "M5_SqHistoryGetdataById"
                Dim wsM5_Sq As New m5_sq_history
                hasil = wsM5_Sq.M5_SqHistoryGetdataById(param)

                'M5_SR
            Case "M5_SrSimpan"
                Dim wsM5_Sr As New m5_sr
                hasil = wsM5_Sr.M5_SrSimpan(param)
            Case "M5_SrSearch"
                Dim wsM5_Sr As New m5_sr
                hasil = wsM5_Sr.M5_SrSearch(param)
            Case "M5_SrDelete"
                If (isDemo = False) Then
                    Dim wsM5_Sr As New m5_sr
                    hasil = wsM5_Sr.M5_SrDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_SrGetdataById"
                Dim wsM5_Sr As New m5_sr
                hasil = wsM5_Sr.M5_SrGetdataById(param)
            Case "M5_SrUpdateStatus"
                Dim wsM5_Sr As New m5_sr
                hasil = wsM5_Sr.M5_SrUpdateStatus(param)
            Case "M5_SrTerkait"
                Dim wsM5_Sr As New m5_sr
                hasil = wsM5_Sr.M5_SrTerkait(param)
            Case "M5_Sr_HistorySimpan"
                Dim wsM5_Sr As New m5_sr_history
                hasil = wsM5_Sr.m5_Sr_HistorySimpan(param)
            Case "M5_Sr_HistorySearch"
                Dim wsM5_Sr As New m5_sr_history
                hasil = wsM5_Sr.M5_Sr_HistorySearch(param)
            Case "M5_SrHistoryGetdataById"
                Dim wsM5_Sr As New m5_sr_history
                hasil = wsM5_Sr.M5_SrHistoryGetdataById(param)
            Case "M5_SrBalance"
                Dim wsM5_Sr As New m5_sr
                hasil = wsM5_Sr.M5_SrBalance(param)
            Case "M5_SrBSearch"
                Dim wsM5_Sr As New m5_sr
                hasil = wsM5_Sr.M5_SrBSearch(param)
            Case "M5_SrBUpdateStatus"
                Dim wsM5_Sr As New m5_sr
                hasil = wsM5_Sr.M5_SrBUpdateStatus(param)
            Case "M5_SrBDelete"
                If (isDemo = False) Then
                    Dim wsM5_Sr As New m5_sr
                    hasil = wsM5_Sr.M5_SrBDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If

                'M5_PL
            Case "M5_PlSimpan"
                Dim wsM5_Pl As New m5_pl
                hasil = wsM5_Pl.M5_PlSimpan(param)
            Case "M5_PlSearch"
                Dim wsM5_Pl As New m5_pl
                hasil = wsM5_Pl.M5_PlSearch(param)
            Case "M5_PlDelete"
                If (isDemo = False) Then
                    Dim wsM5_Pl As New m5_pl
                    hasil = wsM5_Pl.M5_PlDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_PlGetdataById"
                Dim wsM5_Pl As New m5_pl
                hasil = wsM5_Pl.M5_PlGetdataById(param)
            Case "M5_Pl_Detail_VSearch"
                Dim wsM5_Pl As New m5_pl
                hasil = wsM5_Pl.M5_Pl_Detail_VSearch(param)
            Case "M5_PlUpdateStatus"
                Dim wsM5_Pl As New m5_pl
                hasil = wsM5_Pl.M5_PlUpdateStatus(param)
            Case "M5_PlTerkait"
                Dim wsM5_Pl As New m5_pl
                hasil = wsM5_Pl.M5_PlTerkait(param)
            Case "M5_Pl_HistorySimpan"
                Dim wsM5_Pl As New m5_pl_history
                hasil = wsM5_Pl.M5_Pl_HistorySimpan(param)
            Case "M5_Pl_HistorySearch"
                Dim wsM5_Pl As New m5_pl_history
                hasil = wsM5_Pl.M5_Pl_HistorySearch(param)
            Case "M5_PlHistoryGetdataById"
                Dim wsM5_Pl As New m5_pl_history
                hasil = wsM5_Pl.M5_PlHistoryGetdataById(param)


                'M5_SPA
            Case "M5_SpaSimpan"
                Dim wsM5_Spa As New m5_spa
                hasil = wsM5_Spa.M5_SpaSimpan(param)
            Case "M5_SpaSearch"
                Dim wsM5_Spa As New m5_spa
                hasil = wsM5_Spa.M5_SpaSearch(param)
            Case "M5_SpaDelete"
                If (isDemo = False) Then
                    Dim wsM5_Spa As New m5_spa
                    hasil = wsM5_Spa.M5_SpaDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_SpaGetdataById"
                Dim wsM5_Spa As New m5_spa
                hasil = wsM5_Spa.M5_SpaGetdataById(param)
            Case "M5_SpaUpdateStatus"
                Dim wsM5_Spa As New m5_spa
                hasil = wsM5_Spa.M5_SpaUpdateStatus(param)
            Case "M5_SpaTerkait"
                Dim wsM5_Spa As New m5_spa
                hasil = wsM5_Spa.M5_SpaTerkait(param)
            Case "M5_Spa_HistorySimpan"
                Dim wsM5_Spa As New m5_spa_history
                hasil = wsM5_Spa.M5_Spa_HistorySimpan(param)
            Case "M5_Spa_HistorySearch"
                Dim wsM5_Spa As New m5_spa_history
                hasil = wsM5_Spa.M5_Spa_HistorySearch(param)
            Case "M5_SpaHistoryGetdataById"
                Dim wsM5_Spa As New m5_spa_history
                hasil = wsM5_Spa.M5_SpaHistoryGetdataById(param)


                'M5_Sie
            Case "M5_SieSimpan"
                Dim wsM5_Sie As New m5_sie
                hasil = wsM5_Sie.M5_SieSimpan(param)
            Case "M5_SieUpdateStatus"
                Dim wsM5_Sie As New m5_sie
                hasil = wsM5_Sie.M5_SieUpdateStatus(param)
            Case "M5_SieDelete"
                Dim wsM5_Sie As New m5_sie
                hasil = wsM5_Sie.M5_SieDelete(param)
            Case "M5_SieSearch"
                Dim wsM5_Sie As New m5_sie
                hasil = wsM5_Sie.M5_SieSearch(param)
            Case "M5_SieGetdataById"
                Dim wsM5_Sie As New m5_sie
                hasil = wsM5_Sie.M5_SieGetdataById(param)
            Case "M5_SieTakedataSearch"
                Dim wsM5_Sie As New m5_sie
                hasil = wsM5_Sie.M5_SieTakedataSearch(param)

            Case "M5_Sie_HistorySimpan"
                Dim wsM5_Sie As New m5_sie_history
                hasil = wsM5_Sie.M5_Sie_HistorySimpan(param)
            Case "M5_Sie_HistorySearch"
                Dim wsM5_Sie As New m5_sie_history
                hasil = wsM5_Sie.M5_Sie_HistorySearch(param)
            Case "M5_SieHistoryGetdataById"
                Dim wsM5_Sie As New m5_sie_history
                hasil = wsM5_Sie.M5_SieHistoryGetdataById(param)

                'M5_Cl
            Case "M5_ClSimpan"
                Dim wsM5_Cl As New m5_cl
                hasil = wsM5_Cl.M5_ClSimpan(param)
            Case "M5_ClSearch"
                Dim wsM5_Cl As New m5_cl
                hasil = wsM5_Cl.M5_ClSearch(param)
            Case "M5_ClDelete"
                If (isDemo = False) Then
                    Dim wsM5_Cl As New m5_cl
                    hasil = wsM5_Cl.M5_ClDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M5_ClUpdateStatus"
                Dim wsM5_Cl As New m5_cl
                hasil = wsM5_Cl.M5_ClUpdateStatus(param)
            Case "M5_ClTerkait"
                Dim wsM5_Cl As New m5_cl
                hasil = wsM5_Cl.M5_ClTerkait(param)

            Case "M5_Cl_HistorySimpan"
                Dim wsM5_Cl As New m5_cl_history
                hasil = wsM5_Cl.M5_Cl_HistorySimpan(param)
            Case "M5_Cl_HistorySearch"
                Dim wsM5_Cl As New m5_cl_history
                hasil = wsM5_Cl.M5_Cl_HistorySearch(param)


                '*********************************** M6 '***********************************

                'M6_PRINT
            Case "M6_Print"
                Dim wsM6_Print As New m6_print
                hasil = wsM6_Print.M6_Print(param)

                'M6_NOTES
            Case "M6_NotesSimpan"
                Dim wsM6_Notes As New m6_notes
                hasil = wsM6_Notes.M6_NotesSimpan(param)
            Case "M6_NotesSearch"
                Dim wsM6_Notes As New m6_notes
                hasil = wsM6_Notes.M6_NotesSearch(param)
            Case "M6_NotesDelete"
                Dim wsM6_Notes As New m6_notes
                hasil = wsM6_Notes.M6_NotesDelete(param)

                'M6_FILES
            Case "M6_FilesSimpan"
                Dim wsM6_Files As New m6_files
                hasil = wsM6_Files.M6_FilesSimpan(param)
            Case "M6_FilesSearch"
                Dim wsM6_Files As New m6_files
                hasil = wsM6_Files.M6_FilesSearch(param)
            Case "M6_FilesDelete"
                Dim wsM6_Files As New m6_files
                hasil = wsM6_Files.M6_FilesDelete(param)

                'M6_BOM
            Case "M6_BomSimpan"
                Dim wsM6_Bom As New m6_bom
                hasil = wsM6_Bom.M6_BomSimpan(param)

            Case "M6_BomSearch"
                Dim wsM6_Bom As New m6_bom
                hasil = wsM6_Bom.M6_BomSearch(param)
            Case "M6_BomDelete"
                If (isDemo = False) Then
                    Dim wsM6_Bom As New m6_bom
                    hasil = wsM6_Bom.M6_BomDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M6_BomGetdataById"
                Dim wsM6_Bom As New m6_bom
                hasil = wsM6_Bom.M6_BomGetdataById(param)
            Case "M6_BomUpdateStatus"
                Dim wsM6_Bom As New m6_bom
                hasil = wsM6_Bom.M6_BomUpdateStatus(param)
            Case "M6_BomTerkait"
                Dim wsM6_Bom As New m6_bom
                hasil = wsM6_Bom.M6_BomTerkait(param)
            Case "M6_BomTerkait_S"
                Dim wsM6_Bom As New m6_bom
                hasil = wsM6_Bom.M6_BomTerkait_S(param)
            Case "M6_Bom_HistorySimpan"
                Dim wsM6_Bom As New m6_bom_history
                hasil = wsM6_Bom.M6_Bom_HistorySimpan(param)
            Case "M6_Bom_HistorySearch"
                Dim wsM6_Bom As New m6_bom_history
                hasil = wsM6_Bom.M6_Bom_HistorySearch(param)
            Case "M6_BomHistoryGetdataById"
                Dim wsM6_Bom As New m6_bom_history
                hasil = wsM6_Bom.M6_BomHistoryGetdataById(param)
            Case "M6_ItemBomSearch"
                Dim wsM6_Bom As New m6_bom
                hasil = wsM6_Bom.M6_ItemBomSearch(param)


                'M6_PDR
            Case "M6_PdrSimpan"
                Dim wsM6_Pdr As New m6_pdr
                hasil = wsM6_Pdr.M6_PdrSimpan(param)
            Case "M6_PdrSearch"
                Dim wsM6_Pdr As New m6_pdr
                hasil = wsM6_Pdr.M6_PdrSearch(param)
            Case "M6_PdrDelete"
                If (isDemo = False) Then
                    Dim wsM6_Pdr As New m6_pdr
                    hasil = wsM6_Pdr.M6_PdrDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M6_PdrGetdataById"
                Dim wsM6_Pdr As New m6_pdr
                hasil = wsM6_Pdr.M6_PdrGetdataById(param)
            Case "M6_PdrUpdateStatus"
                Dim wsM6_Pdr As New m6_pdr
                hasil = wsM6_Pdr.M6_PdrUpdateStatus(param)
            Case "M6_PdrTerkait"
                Dim wsM6_Pdr As New m6_pdr
                hasil = wsM6_Pdr.M6_PdrTerkait(param)
            Case "M6_Pdr_HistorySimpan"
                Dim wsM6_Pdr As New m6_pdr_history
                hasil = wsM6_Pdr.M6_Pdr_HistorySimpan(param)
            Case "M6_Pdr_HistorySearch"
                Dim wsM6_Pdr As New m6_pdr_history
                hasil = wsM6_Pdr.M6_Pdr_HistorySearch(param)
            Case "M6_PdrHistoryGetdataById"
                Dim wsM6_Pdr As New m6_pdr_history
                hasil = wsM6_Pdr.M6_PdrHistoryGetdataById(param)

                'M6_WO
            Case "M6_WoSimpan"
                Dim wsM6_Wo As New m6_wo
                hasil = wsM6_Wo.M6_WoSimpan(param)
            Case "M6_WoSearch"
                Dim wsM6_Wo As New m6_wo
                hasil = wsM6_Wo.M6_WoSearch(param)
            Case "M6_WoDelete"
                If (isDemo = False) Then
                    Dim wsM6_Wo As New m6_wo
                    hasil = wsM6_Wo.M6_WoDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M6_WoGetdataById"
                Dim wsM6_Wo As New m6_wo
                hasil = wsM6_Wo.M6_WoGetdataById(param)
            Case "M6_WoUpdateStatus"
                Dim wsM6_Wo As New m6_wo
                hasil = wsM6_Wo.M6_WoUpdateStatus(param)
            Case "M6_WoTerkait"
                Dim wsM6_Wo As New m6_wo
                hasil = wsM6_Wo.M6_WoTerkait(param)
            Case "M6_Wo_OutSearch"
                Dim wsM6_Wo As New m6_wo
                hasil = wsM6_Wo.M6_Wo_OutSearch(param)
            Case "M6_Wo_InSearch"
                Dim wsM6_Wo As New m6_wo
                hasil = wsM6_Wo.M6_Wo_InSearch(param)
            Case "M6_Wo_Detail_VSearch"
                Dim wsM6_Wo As New m6_wo
                hasil = wsM6_Wo.M6_Wo_Detail_VSearch(param)
            Case "M6_Wo_HistorySimpan"
                Dim wsM6_Wo As New m6_wo_history
                hasil = wsM6_Wo.M6_Wo_HistorySimpan(param)
            Case "M6_Wo_HistorySearch"
                Dim wsM6_Wo As New m6_wo_history
                hasil = wsM6_Wo.M6_Wo_HistorySearch(param)
            Case "M6_WoHistoryGetdataById"
                Dim wsM6_Wo As New m6_wo_history
                hasil = wsM6_Wo.M6_WoHistoryGetdataById(param)

                'M6_MRS
            Case "M6_MrsSimpan"
                Dim wsM6_Mrs As New m6_mrs
                hasil = wsM6_Mrs.M6_MrsSimpan(param)
            Case "M6_MrsSearch"
                Dim wsM6_Mrs As New m6_mrs
                hasil = wsM6_Mrs.M6_MrsSearch(param)
            Case "M6_MrsDelete"
                If (isDemo = False) Then
                    Dim wsM6_Mrs As New m6_mrs
                    hasil = wsM6_Mrs.M6_MrsDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M6_MrsGetdataById"
                Dim wsM6_Mrs As New m6_mrs
                hasil = wsM6_Mrs.M6_MrsGetdataById(param)
            Case "M6_MrsUpdateStatus"
                Dim wsM6_Mrs As New m6_mrs
                hasil = wsM6_Mrs.M6_MrsUpdateStatus(param)
            Case "M6_MrsTerkait"
                Dim wsM6_Mrs As New m6_mrs
                hasil = wsM6_Mrs.M6_MrsTerkait(param)
            Case "M6_Mrs_OutSearch"
                Dim wsM6_Mrs As New m6_mrs
                hasil = wsM6_Mrs.M6_Mrs_OutSearch(param)
            Case "M6_Mrs_HistorySimpan"
                Dim wsM6_Mrs As New m6_mrs_history
                hasil = wsM6_Mrs.m6_Mrs_HistorySimpan(param)
            Case "M6_Mrs_HistorySearch"
                Dim wsM6_Mrs As New m6_mrs_history
                hasil = wsM6_Mrs.M6_Mrs_HistorySearch(param)
            Case "M6_MrsHistoryGetdataById"
                Dim wsM6_Mrs As New m6_mrs_history
                hasil = wsM6_Mrs.M6_MrsHistoryGetdataById(param)

                'M6_MRN
            Case "M6_MrnSimpan"
                Dim wsM6_Mrn As New m6_mrn
                hasil = wsM6_Mrn.M6_MrnSimpan(param)
            Case "M6_MrnSearch"
                Dim wsM6_Mrn As New m6_mrn
                hasil = wsM6_Mrn.M6_MrnSearch(param)
            Case "M6_MrnDelete"
                If (isDemo = False) Then
                    Dim wsM6_Mrn As New m6_mrn
                    hasil = wsM6_Mrn.M6_MrnDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M6_MrnGetdataById"
                Dim wsM6_Mrn As New m6_mrn
                hasil = wsM6_Mrn.M6_MrnGetdataById(param)
            Case "M6_MrnUpdateStatus"
                Dim wsM6_Mrn As New m6_mrn
                hasil = wsM6_Mrn.M6_MrnUpdateStatus(param)
            Case "M6_MrnTerkait"
                Dim wsM6_Mrn As New m6_mrn
                hasil = wsM6_Mrn.M6_MrnTerkait(param)
            Case "M6_Mrn_HistorySimpan"
                Dim wsM6_Mrn As New m6_mrn_history
                hasil = wsM6_Mrn.m6_Mrn_HistorySimpan(param)
            Case "M6_Mrn_HistorySearch"
                Dim wsM6_Mrn As New m6_mrn_history
                hasil = wsM6_Mrn.M6_Mrn_HistorySearch(param)
            Case "M6_MrnHistoryGetdataById"
                Dim wsM6_Mrn As New m6_mrn_history
                hasil = wsM6_Mrn.M6_MrnHistoryGetdataById(param)

                'M6_PD
            Case "M6_PdSimpan"
                Dim wsM6_Pd As New m6_pd
                hasil = wsM6_Pd.M6_PdSimpan(param)
            Case "M6_PdSimpanImport"
                Dim wsM6_Pd As New m6_pd
                hasil = wsM6_Pd.M6_PdSimpanImport(param)
            Case "M6_PdSearch"
                Dim wsM6_Pd As New m6_pd
                hasil = wsM6_Pd.M6_PdSearch(param)
            Case "M6_PdDelete"
                If (isDemo = False) Then
                    Dim wsM6_Pd As New m6_pd
                    hasil = wsM6_Pd.M6_PdDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M6_PdGetdataById"
                Dim wsM6_Pd As New m6_pd
                hasil = wsM6_Pd.M6_PdGetdataById(param)
            Case "M6_PdUpdateStatus"
                Dim wsM6_Pd As New m6_pd
                hasil = wsM6_Pd.M6_PdUpdateStatus(param)
            Case "M6_PdTerkait"
                Dim wsM6_Pd As New m6_pd
                hasil = wsM6_Pd.M6_PdTerkait(param)
            Case "M6_Pd_HistorySimpan"
                Dim wsM6_Pd As New m6_pd_history
                hasil = wsM6_Pd.m6_Pd_HistorySimpan(param)
            Case "M6_Pd_HistorySearch"
                Dim wsM6_Pd As New m6_pd_history
                hasil = wsM6_Pd.M6_Pd_HistorySearch(param)
            Case "M6_PdHistoryGetdataById"
                Dim wsM6_Pd As New m6_pd_history
                hasil = wsM6_Pd.M6_PdHistoryGetdataById(param)
            Case "M6_PdBomSearch"
                Dim wsM6_Pd As New m6_pd
                hasil = wsM6_Pd.M6_PdBomSearch(param)

                'M6_Machine_Plotting
            Case "M6_Machine_PlottingSimpan"
                Dim wsM6 As New m6_machine_plotting
                hasil = wsM6.M6_Machine_PlottingSimpan(param)
            Case "M6_Machine_PlottingSearch"
                Dim wsM6 As New m6_machine_plotting
                hasil = wsM6.M6_Machine_PlottingSearch(param)
            Case "M6_Machine_PlottingGenerate"
                Dim wsM6 As New m6_machine_plotting
                hasil = wsM6.M6_Machine_PlottingGenerate(param)
            Case "M6_Machine_PlottingGenerateMulti"
                Dim wsM6 As New m6_machine_plotting
                hasil = wsM6.M6_Machine_PlottingGenerateMulti(param)
            Case "M6_Machine_PlottingClose"
                Dim wsM6 As New m6_machine_plotting
                hasil = wsM6.M6_Machine_PlottingClose(param)
            Case "M6_Machine_PlottingDelete"
                Dim wsM6 As New m6_machine_plotting
                hasil = wsM6.M6_Machine_PlottingDelete(param)

                'M6_PDP
            Case "M6_PdpSimpan"
                Dim wsM6_Pdp As New m6_pdp
                hasil = wsM6_Pdp.M6_PdpSimpan(param)
            Case "M6_PdpAllowanceSimpan"
                Dim wsM6_Pdp As New m6_pdp
                hasil = wsM6_Pdp.M6_PdpAllowanceSimpan(param)
            Case "M6_PdpSearch"
                Dim wsM6_Pdp As New m6_pdp
                hasil = wsM6_Pdp.M6_PdpSearch(param)
            Case "M6_PdpGenerate"
                Dim wsM6_Pdp As New m6_pdp
                hasil = wsM6_Pdp.M6_PdpGenerate(param)
            Case "M6_PdpTakeSo"
                Dim wsM6_Pdp As New m6_pdp
                hasil = wsM6_Pdp.M6_PdpTakeSo(param)
            Case "M6_PdpTakeSoPd"
                Dim wsM6_Pdp As New m6_pdp
                hasil = wsM6_Pdp.M6_PdpTakeSoPd(param)
            Case "M6_PdpDelete"
                If (isDemo = False) Then
                    Dim wsM6_Pdp As New m6_pdp
                    hasil = wsM6_Pdp.M6_PdpDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M6_PdpGetdataById"
                Dim wsM6_Pdp As New m6_pdp
                hasil = wsM6_Pdp.M6_PdpGetdataById(param)
            Case "M6_PdpUpdateStatus"
                Dim wsM6_Pdp As New m6_pdp
                hasil = wsM6_Pdp.M6_PdpUpdateStatus(param)
            Case "M6_PdpTerkait"
                Dim wsM6_Pdp As New m6_pdp
                hasil = wsM6_Pdp.M6_PdpTerkait(param)
            Case "M6_Pdp_HistorySimpan"
                Dim wsM6_Pdp As New m6_pdp_history
                hasil = wsM6_Pdp.M6_Pdp_HistorySimpan(param)
            Case "M6_Pdp_HistorySearch"
                Dim wsM6_Pdp As New m6_pdp_history
                hasil = wsM6_Pdp.M6_Pdp_HistorySearch(param)
            Case "M6_PdpHistoryGetdataById"
                Dim wsM6_Pdp As New m6_pdp_history
                hasil = wsM6_Pdp.M6_PdpHistoryGetdataById(param)

                '*********************************** M7 '***********************************

                'M7_PRINT
            Case "M7_Print"
                Dim wsM7_Print As New m7_print
                hasil = wsM7_Print.M7_Print(param)

                'M7_NOTES
            Case "M7_NotesSimpan"
                Dim wsM7_Notes As New m7_notes
                hasil = wsM7_Notes.M7_NotesSimpan(param)
            Case "M7_NotesSearch"
                Dim wsM7_Notes As New m7_notes
                hasil = wsM7_Notes.M7_NotesSearch(param)
            Case "M7_NotesDelete"
                Dim wsM7_Notes As New m7_notes
                hasil = wsM7_Notes.M7_NotesDelete(param)

                'M7_FILES
            Case "M7_FilesSimpan"
                Dim wsM7_Files As New m7_files
                hasil = wsM7_Files.M7_FilesSimpan(param)
            Case "M7_FilesSearch"
                Dim wsM7_Files As New m7_files
                hasil = wsM7_Files.M7_FilesSearch(param)
            Case "M7_FilesDelete"
                Dim wsM7_Files As New m7_files
                hasil = wsM7_Files.M7_FilesDelete(param)

                'M7_ASSET_CATEGORY_TAX
            Case "M7_Asset_Category_TaxSimpan"
                Dim wsM7_Asset_Category_Tax As New m7_asset_category_tax
                hasil = wsM7_Asset_Category_Tax.M7_Asset_Category_TaxSimpan(param)
            Case "M7_Asset_Category_TaxDelete"
                Dim wsM7_Asset_Category_Tax As New m7_asset_category_tax
                hasil = wsM7_Asset_Category_Tax.M7_Asset_Category_TaxDelete(param)
            Case "M7_Asset_Category_TaxSearch"
                Dim wsM7_Asset_Category_Tax As New m7_asset_category_tax
                hasil = wsM7_Asset_Category_Tax.M7_Asset_Category_TaxSearch(param)
            Case "M7_Asset_Category_TaxCekId"
                Dim wsM7_Asset_Category_Tax As New m7_asset_category_tax
                hasil = wsM7_Asset_Category_Tax.M7_Asset_Category_TaxCekId(param)
            Case "M7_Asset_Category_TaxTerkait"
                Dim wsM7_Asset_Category_Tax As New m7_asset_category_tax
                hasil = wsM7_Asset_Category_Tax.M7_Asset_Category_TaxTerkait(param)
            Case "M7_Asset_Category_Tax_HistorySearch"
                Dim wsM7_Asset_Category_Tax_History As New m7_asset_category_tax_history
                hasil = wsM7_Asset_Category_Tax_History.M7_Asset_Category_Tax_HistorySearch(param)


                'M7_ASSET_CATEGORY
            Case "M7_Asset_CategorySimpan"
                Dim wsM7_Asset_Category As New m7_asset_category
                hasil = wsM7_Asset_Category.M7_Asset_CategorySimpan(param)
            Case "M7_Asset_CategoryDelete"
                Dim wsM7_Asset_Category As New m7_asset_category
                hasil = wsM7_Asset_Category.M7_Asset_CategoryDelete(param)
            Case "M7_Asset_CategorySearch"
                Dim wsM7_Asset_Category As New m7_asset_category
                hasil = wsM7_Asset_Category.M7_Asset_CategorySearch(param)
            Case "M7_Asset_CategoryCekId"
                Dim wsM7_Asset_Category As New m7_asset_category
                hasil = wsM7_Asset_Category.M7_Asset_CategoryCekId(param)
            Case "M7_Asset_CategoryTerkait"
                Dim wsM7_Asset_Category As New m7_asset_category
                hasil = wsM7_Asset_Category.M7_Asset_CategoryTerkait(param)
            Case "M7_Asset_Category_HistorySearch"
                Dim wsM7_Asset_Category_History As New m7_asset_category_history
                hasil = wsM7_Asset_Category_History.M7_Asset_Category_HistorySearch(param)

                'M7_ASSET
            Case "M7_AssetSimpan"
                Dim wsM7_Asset As New m7_asset
                hasil = wsM7_Asset.M7_AssetSimpan(param)
            Case "M7_AssetDelete"
                Dim wsM7_Asset As New m7_asset
                hasil = wsM7_Asset.M7_AssetDelete(param)
            Case "M7_AssetSearch"
                Dim wsM7_Asset As New m7_asset
                hasil = wsM7_Asset.M7_AssetSearch(param)
            Case "M7_AssetSearchSerenity"
                Dim wsM7_Asset As New m7_asset
                hasil = wsM7_Asset.M7_AssetSearchSerenity(param)
            Case "M7_AssetCekId"
                Dim wsM7_Asset As New m7_asset
                hasil = wsM7_Asset.M7_AssetCekId(param)
            Case "M7_AssetTerkait"
                Dim wsM7_Asset As New m7_asset
                hasil = wsM7_Asset.M7_AssetTerkait(param)
            Case "M7_Asset_HistorySearch"
                Dim wsM7_Asset_History As New m7_asset_history
                hasil = wsM7_Asset_History.M7_Asset_HistorySearch(param)


                'M7_DA
            Case "M7_DaSimpan"
                Dim wsM7_Da As New m7_da
                hasil = wsM7_Da.M7_DaSimpan(param)
            Case "M7_DaSearch"
                Dim wsM7_Da As New m7_da
                hasil = wsM7_Da.M7_DaSearch(param)
            Case "M7_DaDelete"
                Dim wsM7_Da As New m7_da
                hasil = wsM7_Da.M7_DaDelete(param)
            Case "M7_DaGetdataById"
                Dim wsM7_Da As New m7_da
                hasil = wsM7_Da.M7_DaGetdataById(param)
            Case "M7_DaUpdateStatus"
                Dim wsM7_Da As New m7_da
                hasil = wsM7_Da.M7_DaUpdateStatus(param)
            Case "M7_DaTerkait"
                Dim wsM7_Da As New m7_da
                hasil = wsM7_Da.M7_DaTerkait(param)

                'M7_Ar
            Case "M7_ArSimpan"
                Dim wsM7_Ar As New m7_ar
                hasil = wsM7_Ar.M7_ArSimpan(param)
            Case "M7_ArSearch"
                Dim wsM7_Ar As New m7_ar
                hasil = wsM7_Ar.M7_ArSearch(param)
            Case "M7_ArGetdataById"
                Dim wsM7_Ar As New m7_ar
                hasil = wsM7_Ar.M7_ArGetdataById(param)
            Case "M7_Ar_Detail_VSearch"
                Dim wsM7_Ar As New m7_ar
                hasil = wsM7_Ar.M7_Ar_Detail_VSearch(param)
            Case "M7_ArTerkait"
                Dim wsM7_Ar As New m7_ar
                hasil = wsM7_Ar.M7_ArTerkait(param)
            Case "M7_ArUpdateStatus"
                Dim wsM7_Ar As New m7_ar
                hasil = wsM7_Ar.M7_ArUpdateStatus(param)
            Case "M7_ArDelete"
                Dim wsM7_Ar As New m7_ar
                hasil = wsM7_Ar.M7_ArDelete(param)

                'M7_Aq
            Case "M7_AqSimpan"
                Dim wsM7_Aq As New m7_aq
                hasil = wsM7_Aq.M7_AqSimpan(param)
            Case "M7_AqSearch"
                Dim wsM7_Aq As New m7_aq
                hasil = wsM7_Aq.M7_AqSearch(param)
            Case "M7_AqGetdataById"
                Dim wsM7_Ar As New m7_aq
                hasil = wsM7_Ar.M7_AqGetdataById(param)
            Case "M7_AqTerkait"
                Dim wsM7_Ar As New m7_aq
                hasil = wsM7_Ar.M7_AqTerkait(param)
            Case "M7_Aq_Detail_VSearch"
                Dim wsM7_Ar As New m7_aq
                hasil = wsM7_Ar.M7_Aq_Detail_VSearch(param)
            Case "M7_AqUpdateStatus"
                Dim wsM7_Ar As New m7_aq
                hasil = wsM7_Ar.M7_AqUpdateStatus(param)
            Case "M7_AqDelete"
                Dim wsM7_Ar As New m7_aq
                hasil = wsM7_Ar.M7_AqDelete(param)

                'M7_Ab
            Case "M7_AbSimpan"
                Dim wsM7_Ab As New m7_ab
                hasil = wsM7_Ab.M7_AbSimpan(param)

                'M7_Ao
            Case "M7_AoSimpan"
                Dim wsM7_Ao As New m7_ao
                hasil = wsM7_Ao.M7_AoSimpan(param)
            Case "M7_AoSearch"
                Dim wsM7_Ao As New m7_ao
                hasil = wsM7_Ao.M7_AoSearch(param)
            Case "M7_AoGetdataById"
                Dim wsM7_Ao As New m7_ao
                hasil = wsM7_Ao.M7_AoGetdataById(param)
            Case "M7_Ao_Detail_VSearch"
                Dim wsM7_Ao As New m7_ao
                hasil = wsM7_Ao.M7_Ao_Detail_VSearch(param)
            Case "M7_AoTerkait"
                Dim wsM7_Ao As New m7_ao
                hasil = wsM7_Ao.M7_AoTerkait(param)
            Case "M7_AoUpdateStatus"
                Dim wsM7_Ao As New m7_ao
                hasil = wsM7_Ao.M7_AoUpdateStatus(param)
            Case "M7_AoDelete"
                Dim wsM7_Ao As New m7_ao
                hasil = wsM7_Ao.M7_AoDelete(param)

                'M7_Ae
            Case "M7_AeSimpan"
                Dim wsM7_Ae As New m7_ae
                hasil = wsM7_Ae.M7_AeSimpan(param)
            Case "M7_AeSearch"
                Dim wsM7_Ae As New m7_ae
                hasil = wsM7_Ae.M7_AeSearch(param)
            Case "M7_AeGetdataById"
                Dim wsM7_Ae As New m7_ae
                hasil = wsM7_Ae.M7_AeGetdataById(param)
            Case "M7_AeTerkait"
                Dim wsM7_Ae As New m7_ae
                hasil = wsM7_Ae.M7_AeTerkait(param)

                'M7_At
            Case "M7_AtSimpan"
                Dim wsM7_At As New m7_at
                hasil = wsM7_At.M7_AtSimpan(param)
            Case "M7_AtTakedataSearch"
                Dim wsM7_At As New m7_at
                hasil = wsM7_At.M7_AtTakedataSearch(param)
            Case "M7_AtSearch"
                Dim wsM7_At As New m7_at
                hasil = wsM7_At.M7_AtSearch(param)
            Case "M7_AtGetdataById"
                Dim wsM7_At As New m7_at
                hasil = wsM7_At.M7_AtGetdataById(param)
            Case "M7_AtDelete"
                Dim wsM7_At As New m7_at
                hasil = wsM7_At.M7_AtDelete(param)
            Case "M7_AtUpdateStatus"
                Dim wsM7_At As New m7_at
                hasil = wsM7_At.M7_AtUpdateStatus(param)

                'M7_Ag
            Case "M7_AgSimpan"
                Dim wsM7_Ag As New m7_ag
                hasil = wsM7_Ag.M7_AgSimpan(param)
            Case "M7_AgSearch"
                Dim wsM7_Ag As New m7_ag
                hasil = wsM7_Ag.M7_AgSearch(param)
            Case "M7_AgGetdataById"
                Dim wsM7_Ag As New m7_ag
                hasil = wsM7_Ag.M7_AgGetdataById(param)

            '*********************************** M0 (Serenity) '***********************************
            Case "M0_Language_SSearch"
                Dim wsM0_language_s As New m0_language_s
                hasil = wsM0_language_s.M0_Language_SSearch(param)
            Case "M0_Sentence_SSimpan"
                Dim m0_sentence_s As New m0_sentence_s
                hasil = m0_sentence_s.M0_Sentence_SSimpan(param)
            Case "M0_Sentence_SDelete"
                Dim m0_sentence_s As New m0_sentence_s
                hasil = m0_sentence_s.M0_Sentence_SDelete(param)
            Case "M0_sentence_SDataSearch"
                Dim m0_sentence_s As New m0_sentence_s
                hasil = m0_sentence_s.M0_sentence_SDataSearch(param)
            Case "M0_Sentence_StranslateSimpan"
                Dim m0_sentence_s As New m0_sentence_s
                hasil = m0_sentence_s.M0_Sentence_StranslateSimpan(param)
            Case "M0_Sentence_StranslateSearch"
                Dim m0_sentence_s As New m0_sentence_s
                hasil = m0_sentence_s.M0_Sentence_StranslateSearch(param)
            Case "M0_Sentence_SSearch"
                Dim m0_sentence_s As New m0_sentence_s
                hasil = m0_sentence_s.M0_sentence_SSearch(param)
            Case "M0_Sentence_STranslateGetdataById"
                Dim m0_sentence_s As New m0_sentence_s
                hasil = m0_sentence_s.M0_Sentence_STranslateGetdataById(param)
            Case "M0_Setting_SSearch"
                Dim wsM0_Setting As New m0_setting
                hasil = wsM0_Setting.M0_Setting_SSearch(param)
            Case "M0_Setting_SSimpan"
                Dim wsM0_Setting As New m0_setting
                hasil = wsM0_Setting.M0_Setting_SSimpan(param)

            'M0_LOGIN (Serenity)
            Case "M0_LoginUserS"
                Dim wsM0_Login As New m0_login_s
                hasil = wsM0_Login.M0_LoginUserS(param)
            Case "M0_LoginDataS"
                Dim wsM0_Login As New m0_login_s
                hasil = wsM0_Login.M0_LoginDataS(param)
            Case "M0_Menu_SerenitySearch"
                Dim wsM0_menu As New m0_menu_s
                hasil = wsM0_menu.M0_Menu_SerenitySearch(param)
            Case "M0_Menu_SRoleSearch"
                Dim wsM0_menu As New m0_menu_s
                hasil = wsM0_menu.M0_Menu_SRoleSearch(param)

            '*********************************** (Serenity) '***********************************
            Case "M8_ContentRoleSearch"
                Dim wsM8 As New m8_content_role
                hasil = wsM8.M8_ContentRoleSearch(param)
            Case "M8_ContentSearch"
                Dim wsM8 As New m8_content
                hasil = wsM8.M8_ContentSearch(param)
            Case "M8_ContentDataSearch"
                Dim wsM8 As New m8_content
                hasil = wsM8.M8_ContentDataSearch(param)
            Case "M8_IndicatorSimpan"
                Dim wsM8 As New m8_indicator
                hasil = wsM8.M8_IndicatorSimpan(param)
            Case "M8_IndicatorSearch"
                Dim wsM8 As New m8_indicator
                hasil = wsM8.M8_IndicatorSearch(param)

            Case "M8_KecepatankirimbarangDetail"
                Dim wsM8 As New m8_caridata
                hasil = wsM8.M8_KecepatankirimbarangDetail(param)
            Case "M8_Sa_DetailSearch"
                Dim wsM8 As New m8_content_detail
                hasil = wsM8.M8_Sa_DetailSearch(param)
            Case "M8_Pd_DetailSearch"
                Dim wsM8 As New m8_content_detail
                hasil = wsM8.M8_Pd_DetailSearch(param)

                'detail M4
            Case "M8_PemenuhanPrDetailSearch"
                Dim wsM8 As New m8_content_detail
                hasil = wsM8.M8_PemenuhanPrDetailSearch(param)
            Case "M8_PemenuhanPoDetailSearch"
                Dim wsM8 As New m8_content_detail
                hasil = wsM8.M8_PemenuhanPoDetailSearch(param)
            Case "M8_KecepatanPoDetailSearch"
                Dim wsM8 As New m8_content_detail
                hasil = wsM8.M8_KecepatanPoDetailSearch(param)
            Case "M8_PemenuhanGrnDetailSearch"
                Dim wsM8 As New m8_content_detail
                hasil = wsM8.M8_PemenuhanGrnDetailSearch(param)
            Case "M8_KecepatanGrnDetailSearch"
                Dim wsM8 As New m8_content_detail
                hasil = wsM8.M8_KecepatanGrnDetailSearch(param)
            Case "M8_PemenuhanRiDetailSearch"
                Dim wsM8 As New m8_content_detail
                hasil = wsM8.M8_PemenuhanRiDetailSearch(param)
            Case "M8_KecepatanVpDetailSearch"
                Dim wsM8 As New m8_content_detail
                hasil = wsM8.M8_KecepatanVpDetailSearch(param)
            Case "M8_PemenuhanSoDetailSearch"
                Dim wsM8 As New m8_content_detail
                hasil = wsM8.M8_PemenuhanSoDetailSearch(param)
            Case "M8_PemenuhanDoDetailSearch"
                Dim wsM8 As New m8_content_detail
                hasil = wsM8.M8_PemenuhanDoDetailSearch(param)
            Case "M8_KecepatanDoDetailSearch"
                Dim wsM8 As New m8_content_detail
                hasil = wsM8.M8_KecepatanDoDetailSearch(param)
            Case "M8_PemenuhanSiDetailSearch"
                Dim wsM8 As New m8_content_detail
                hasil = wsM8.M8_PemenuhanSiDetailSearch(param)
            Case "M8_KecepatanSiDetailSearch"
                Dim wsM8 As New m8_content_detail
                hasil = wsM8.M8_KecepatanSiDetailSearch(param)
            Case "M8_KecepatanPvDetailSearch"
                Dim wsM8 As New m8_content_detail
                hasil = wsM8.M8_KecepatanPvDetailSearch(param)

            Case "M8_Content_ChartSearch"
                Dim wsM8 As New m8_content_chart
                hasil = wsM8.M8_Content_ChartSearch(param)
            Case "M8_Content_ChartDataSearch"
                Dim wsM8 As New m8_content_chart
                hasil = wsM8.M8_Content_ChartDataSearch(param)

                'M8_KPI
                'Case "M8_KPIGetData"
                '    Dim wsM8_KPI As New m8_kpi
                '    hasil = wsM8_KPI.M8_KPIGetData(param)
                'Case "M8_KPISearch"
                '    Dim wsM8_KPI As New m8_kpi
                '    hasil = wsM8_KPI.M8_KPISearch(param)

                '    'M8_ARTurnOver
                'Case "M8_ARTurnOver"
                '    Dim wsM8_ARTurnOver As New m8_arturnover
                '    hasil = wsM8_ARTurnOver.M8_ARTurnOver(param)
                'Case "M8_ARTurnOverSearch"
                '    Dim wsM8_ARTurnOver As New m8_arturnover
                '    hasil = wsM8_ARTurnOver.M8_ARTurnOverSearch(param)

                '    'M8_InventoryTurnOver
                'Case "M8_InventoryTurnOver"
                '    Dim wsM8_InvTurnOver As New m8_inventoryturnover
                '    hasil = wsM8_InvTurnOver.M8_InventoryTurnOver(param)
                'Case "M8_ARTurnOverSearch"
                '    Dim wsM8_InvTurnOver As New m8_arturnover
                '    hasil = wsM8_InvTurnOver.M8_ARTurnOverSearch(param)

                '    'M8_APPaymentOnTime
                'Case "M8_APPaymentOnTime"
                '    Dim wsM8_ApOntime As New m8_appaymentontime
                '    hasil = wsM8_ApOntime.M8_APPaymentOnTime(param)
                'Case "M8_APPaymentOnTimeSearch"
                '    Dim wsM8_ApOntime As New m8_appaymentontime
                '    hasil = wsM8_ApOntime.M8_APPaymentOnTimeSearch(param)

                '    'M8_ARPaymentOnTime
                'Case "M8_ARPaymentOnTime"
                '    Dim wsM8_ArOntime As New m8_arpaymentontime
                '    hasil = wsM8_ArOntime.M8_ARPaymentOnTime(param)
                'Case "M8_ARPaymentOnTimeSearch"
                '    Dim wsM8_ArOntime As New m8_arpaymentontime
                '    hasil = wsM8_ArOntime.M8_ARPaymentOnTimeSearch(param)

                '    'M8_CustomerLoyalty
                'Case "M8_CustomerLoyalty"
                '    Dim wsM8_CustLoyalty As New m8_customerloyalty
                '    hasil = wsM8_CustLoyalty.M8_CustomerLoyalty(param)
                'Case "M8_CustomerLoyaltySearch"
                '    Dim wsM8_CustLoyalty As New m8_customerloyalty
                '    hasil = wsM8_CustLoyalty.M8_CustomerLoyaltySearch(param)

                '    'M8_LowerPriceSensitive
                'Case "M8_LowerPriceSensitive"
                '    Dim wsM8_LPS As New m8_lowerpricesensitive
                '    hasil = wsM8_LPS.M8_LowerPriceSensitive(param)
                'Case "M8_LowerPriceSensitiveSearch"
                '    Dim wsM8_LPS As New m8_lowerpricesensitive
                '    hasil = wsM8_LPS.M8_LowerPriceSensitiveSearch(param)

                '    'M8_NewCustomer
                'Case "M8_NewCustomer"
                '    Dim wsM8_NewCustomer As New m8_newcustomer
                '    hasil = wsM8_NewCustomer.M8_NewCustomer(param)
                'Case "M8_NewCustomerSearch"
                '    Dim wsM8_NewCustomer As New m8_newcustomer
                '    hasil = wsM8_NewCustomer.M8_NewCustomerSearch(param)

                '    'M8_CustomerLoss
                'Case "M8_CustomerLoss"
                '    Dim wsM8_CustomerLoss As New m8_customerloss
                '    hasil = wsM8_CustomerLoss.M8_CustomerLoss(param)
                'Case "M8_CustomerLossSearch"
                '    Dim wsM8_CustomerLoss As New m8_customerloss
                '    hasil = wsM8_CustomerLoss.M8_CustomerLossSearch(param)


                '*********************************** M11 '***********************************
                'M11_LU
            Case "M11_LuSimpan"
                Dim wsM11_Lu As New m11_lu
                hasil = wsM11_Lu.M11_LuSimpan(param)
            Case "M11_LuSearch"
                Dim wsM11_Lu As New m11_lu
                hasil = wsM11_Lu.M11_LuSearch(param)
            Case "M11_LuDelete"
                If (isDemo = False) Then
                    Dim wsM11_Lu As New m11_lu
                    hasil = wsM11_Lu.M11_LuDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M11_LuGetdataById"
                Dim wsM11_Lu As New m11_lu
                hasil = wsM11_Lu.M11_LuGetdataById(param)
            Case "M11_LuUpdateStatus"
                Dim wsM11_Lu As New m11_lu
                hasil = wsM11_Lu.M11_LuUpdateStatus(param)
            Case "M11_LuTerkait"
                Dim wsM11_Lu As New m11_lu
                hasil = wsM11_Lu.M11_LuTerkait(param)
            Case "M11_Lu_Detail_VSearch"
                Dim wsM11_Lu As New m11_lu
                hasil = wsM11_Lu.M11_Lu_Detail_VSearch(param)
            Case "M11_Lu_HistorySimpan"
                Dim wsM11_Lu As New m11_lu_history
                hasil = wsM11_Lu.m11_Lu_HistorySimpan(param)
            Case "M11_Lu_HistorySearch"
                Dim wsM11_Lu As New m11_lu_history
                hasil = wsM11_Lu.M11_Lu_HistorySearch(param)
            Case "M11_LuHistoryGetdataById"
                Dim wsM11_Lu As New m11_lu_history
                hasil = wsM11_Lu.M11_LuHistoryGetdataById(param)

                'M11_KJ
            Case "M11_KjSimpan"
                Dim wsM11_Kj As New m11_kj
                hasil = wsM11_Kj.M11_KjSimpan(param)
            Case "M11_Kj_HistorySimpan"
                Dim wsM11_Kj As New m11_kj_history
                hasil = wsM11_Kj.M11_Kj_HistorySimpan(param)
            Case "M11_KjSearch"
                Dim wsM11_Kj As New m11_kj
                hasil = wsM11_Kj.M11_KjSearch(param)
            Case "M11_KjDelete"
                If (isDemo = False) Then
                    Dim wsM11_Kj As New m11_kj
                    hasil = wsM11_Kj.M11_KjDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M11_KjGetdataById"
                Dim wsM11_kj As New m11_kj
                hasil = wsM11_kj.M11_KjGetdataById(param)
            Case "M11_KjUpdateStatus"
                Dim wsM11_Kj As New m11_kj
                hasil = wsM11_Kj.M11_KjUpdateStatus(param)
            Case "M11_KjTerkait"
                Dim wsM11_Kj As New m11_kj
                hasil = wsM11_Kj.M11_KjTerkait(param)
            Case "M11_KjUpdateKeterangan"
                Dim wsM11_Kj As New m11_kj
                hasil = wsM11_Kj.M11_KjUpdateKeterangan(param)

                'M11_KW
            Case "M11_KwSimpan"
                Dim wsM11_Kw As New m11_kw
                hasil = wsM11_Kw.M11_KwSimpan(param)
            Case "M11_KwSearch"
                Dim wsM11_Kw As New m11_kw
                hasil = wsM11_Kw.M11_KwSearch(param)
            Case "M11_KwDelete"
                If (isDemo = False) Then
                    Dim wsM11_Kw As New m11_kw
                    hasil = wsM11_Kw.M11_KwDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M11_KwGetdataById"
                Dim wsM11_Kw As New m11_kw
                hasil = wsM11_Kw.M11_KwGetdataById(param)
            Case "M11_KwUpdateStatus"
                Dim wsM11_Kw As New m11_kw
                hasil = wsM11_Kw.M11_KwUpdateStatus(param)
            Case "M11_KwTakedataSearch"
                Dim wsM11_Kw As New m11_kw
                hasil = wsM11_Kw.M11_KwTakedataSearch(param)
            Case "M11_KwTakedata1Search"
                Dim wsM11_Kw As New m11_kw
                hasil = wsM11_Kw.M11_KwTakedata1Search(param)
            Case "M11_KwTerkait"
                Dim wsM11_Kw As New m11_kw
                hasil = wsM11_Kw.M11_KwTerkait(param)

                'M11_RM
            Case "M11_RmSimpan"
                Dim wsM11_Rm As New m11_rm
                hasil = wsM11_Rm.M11_RmSimpan(param)
                'Case "M11_Rm_HistorySimpan"
                '    Dim wsM11_Rm As New M11_Rm_history
                '    hasil = wsM11_Rm.M11_Rm_HistorySimpan(param)
            Case "M11_RmSearch"
                Dim wsM11_Rm As New m11_rm
                hasil = wsM11_Rm.M11_RmSearch(param)
            Case "M11_RmDelete"
                If (isDemo = False) Then
                    Dim wsM11_Rm As New m11_rm
                    hasil = wsM11_Rm.M11_RmDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M11_RmGetdataById"
                Dim wsM11_Rm As New m11_rm
                hasil = wsM11_Rm.M11_RmGetdataById(param)
            Case "M11_RmUpdateStatus"
                Dim wsM11_Rm As New m11_rm
                hasil = wsM11_Rm.M11_RmUpdateStatus(param)
            Case "M11_RmTerkait"
                Dim wsM11_Rm As New m11_rm
                hasil = wsM11_Rm.M11_RmTerkait(param)


                'M11_KM
            Case "M11_KmSimpan"
                Dim wsM11_Km As New m11_km
                hasil = wsM11_Km.M11_KmSimpan(param)
            Case "M11_KmSearch"
                Dim wsM11_Km As New m11_km
                hasil = wsM11_Km.M11_KmSearch(param)
            Case "M11_KmDelete"
                If (isDemo = False) Then
                    Dim wsM11_Km As New m11_km
                    hasil = wsM11_Km.M11_KmDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M11_KmGetdataById"
                Dim wsM11_Km As New m11_km
                hasil = wsM11_Km.M11_KmGetdataById(param)
            Case "M11_KmUpdateStatus"
                Dim wsM11_Km As New m11_km
                hasil = wsM11_Km.M11_KmUpdateStatus(param)
            Case "M11_KmTerkait"
                Dim wsM11_Km As New m11_km
                hasil = wsM11_Km.M11_KmTerkait(param)
            Case "M11_KmKeluarKamar"
                Dim wsM11_Km As New m11_km
                hasil = wsM11_Km.M11_KmKeluarKamar(param)
            Case "M11_Km_HistorySimpan"
                Dim wsM11_Km As New m11_km_history
                hasil = wsM11_Km.M11_Km_HistorySimpan(param)
            Case "M11_Km_HistorySearch"
                Dim wsM11_Km As New m11_km_history
                hasil = wsM11_Km.M11_Km_HistorySearch(param)
            Case "M11_KmHistoryGetdataById"
                Dim wsM11_Km As New m11_km_history
                hasil = wsM11_Km.M11_KmHistoryGetdataById(param)

                'M11_AK
            Case "M11_AkSimpan"
                Dim wsM11_Ak As New m11_ak
                hasil = wsM11_Ak.M11_AkSimpan(param)
            Case "M11_AkSearch"
                Dim wsM11_Ak As New m11_ak
                hasil = wsM11_Ak.M11_AkSearch(param)
            Case "M11_AkDelete"
                If (isDemo = False) Then
                    Dim wsM11_Ak As New m11_ak
                    hasil = wsM11_Ak.M11_AkDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M11_AkGetdataById"
                Dim wsM11_Ak As New m11_ak
                hasil = wsM11_Ak.M11_AkGetdataById(param)
            Case "M11_AkUpdateStatus"
                Dim wsM11_Ak As New m11_ak
                hasil = wsM11_Ak.M11_AkUpdateStatus(param)
            Case "M11_AkTerkait"
                Dim wsM11_Ak As New m11_ak
                hasil = wsM11_Ak.M11_AkTerkait(param)
            Case "M11_Ak_Detail_VSearch"
                Dim wsM11_Ak As New m11_ak
                hasil = wsM11_Ak.M11_Ak_Detail_VSearch(param)
            Case "M11_AkCekNoRef"
                Dim wsM11_Ak As New m11_ak
                hasil = wsM11_Ak.M11_AkCekNoRef(param)
                'Case "M11_AkNoResepRJ"
                '    Dim wsM11_Ak As New m11_ak
                '    hasil = wsM11_Ak.M11_AkNoResepRJ(param)

                'M11_RO
            Case "M11_RoSimpan"
                Dim wsM11_Ro As New m11_ro
                hasil = wsM11_Ro.M11_RoSimpan(param)
            Case "M11_RoSearch"
                Dim wsM11_Ro As New m11_ro
                hasil = wsM11_Ro.M11_RoSearch(param)
            Case "M11_RoDelete"
                If (isDemo = False) Then
                    Dim wsM11_Ro As New m11_ro
                    hasil = wsM11_Ro.M11_RoDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M11_RoGetdataById"
                Dim wsM11_Ro As New m11_ro
                hasil = wsM11_Ro.M11_RoGetdataById(param)
            Case "M11_RoUpdateStatus"
                Dim wsM11_Ro As New m11_ro
                hasil = wsM11_Ro.M11_RoUpdateStatus(param)
            Case "M11_RoTerkait"
                Dim wsM11_Ro As New m11_ro
                hasil = wsM11_Ro.M11_RoTerkait(param)
            Case "M11_Ro_Detail_VSearch"
                Dim wsM11_Ro As New m11_ro
                hasil = wsM11_Ro.M11_Ro_Detail_VSearch(param)
            Case "M11_RoCekNoRef"
                Dim wsM11_Ro As New m11_ro
                hasil = wsM11_Ro.M11_RoCekNoRef(param)

                'M11_LB
            Case "M11_LbSimpan"
                Dim wsM11_Lb As New m11_lb
                hasil = wsM11_Lb.M11_LbSimpan(param)
            Case "M11_LbSearch"
                Dim wsM11_Lb As New m11_lb
                hasil = wsM11_Lb.M11_LbSearch(param)
            Case "M11_LbDelete"
                If (isDemo = False) Then
                    Dim wsM11_Lb As New m11_lb
                    hasil = wsM11_Lb.M11_LbDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M11_LbGetdataById"
                Dim wsM11_Lb As New m11_lb
                hasil = wsM11_Lb.M11_LbGetdataById(param)
            Case "M11_LbUpdateStatus"
                Dim wsM11_Lb As New m11_lb
                hasil = wsM11_Lb.M11_LbUpdateStatus(param)
            Case "M11_LbTerkait"
                Dim wsM11_Lb As New m11_lb
                hasil = wsM11_Lb.M11_LbTerkait(param)
            Case "M11_Lb_Detail_VSearch"
                Dim wsM11_Lb As New m11_lb
                hasil = wsM11_Lb.M11_Lb_Detail_VSearch(param)

                'M11_PT
            Case "M11_PtSimpan"
                Dim wsM11_Pt As New m11_pt
                hasil = wsM11_Pt.M11_PtSimpan(param)
            Case "M11_PtGetdataById"
                Dim wsM11_Pt As New m11_pt
                hasil = wsM11_Pt.M11_PtGetdataById(param)
            Case "M11_PtSearch"
                Dim wsM11_Pt As New m11_pt
                hasil = wsM11_Pt.M11_PtSearch(param)
            Case "M11_PtUpdateStatus"
                Dim wsM11_Pt As New m11_pt
                hasil = wsM11_Pt.M11_PtUpdateStatus(param)

                'M11_ISK
            Case "M11_IskSimpan"
                Dim wsM11_Isk As New m11_isk
                hasil = wsM11_Isk.M11_IskSimpan(param)
            Case "M11_IskGetdataById"
                Dim wsM11_Isk As New m11_isk
                hasil = wsM11_Isk.M11_IskGetdataById(param)
            Case "M11_IskSearch"
                Dim wsM11_Isk As New m11_isk
                hasil = wsM11_Isk.M11_IskSearch(param)
            Case "M11_IskUpdateStatus"
                Dim wsM11_Isk As New m11_isk
                hasil = wsM11_Isk.M11_IskUpdateStatus(param)

                'M11_UD
            Case "M11_UdSimpan"
                Dim wsM11_Ud As New m11_ud
                hasil = wsM11_Ud.M11_UdSimpan(param)
            Case "M11_UdGetdataById"
                Dim wsM11_Ud As New m11_ud
                hasil = wsM11_Ud.M11_UdGetdataById(param)
            Case "M11_UdSearch"
                Dim wsM11_Ud As New m11_ud
                hasil = wsM11_Ud.M11_UdSearch(param)
            Case "M11_UdUpdateStatus"
                Dim wsM11_Ud As New m11_ud
                hasil = wsM11_Ud.M11_UdUpdateStatus(param)

                '    'M11_SK
                'Case "M11_SkSimpan"
                '    Dim wsM11_Sk As New m11_sk
                '    hasil = wsM11_Sk.M11_SkSimpan(param)
                'Case "M11_SkGetdataById"
                '    Dim wsM11_Sk As New m11_sk
                '    hasil = wsM11_Sk.M11_SkGetdataById(param)
                'Case "M11_SkSearch"
                '    Dim wsM11_Sk As New m11_sk
                '    hasil = wsM11_Sk.M11_SkSearch(param)
                'Case "M11_SkUpdateStatus"
                '    Dim wsM11_Sk As New m11_sk
                '    hasil = wsM11_Sk.M11_SkUpdateStatus(param)

                'M11_ILO
            Case "M11_IloSimpan"
                Dim wsM11_Ilo As New m11_ilo
                hasil = wsM11_Ilo.M11_IloSimpan(param)
            Case "M11_IloGetdataById"
                Dim wsM11_Ilo As New m11_ilo
                hasil = wsM11_Ilo.M11_IloGetdataById(param)
            Case "M11_IloSearch"
                Dim wsM11_Ilo As New m11_ilo
                hasil = wsM11_Ilo.M11_IloSearch(param)
            Case "M11_IloUpdateStatus"
                Dim wsM11_Ilo As New m11_ilo
                hasil = wsM11_Ilo.M11_IloUpdateStatus(param)

                'M11_CHARTPT
            Case "M11_ChartPTSearch"
                Dim wsM11_ChartPT As New m11_chartpt
                hasil = wsM11_ChartPT.M11_ChartPTSearch(param)

                '***************************** REPORT PROGRESS '******************************

            Case "M0_HitungRealisasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_HitungRealisasi(param)

            Case "M0_HitungRealisasiCabang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_HitungRealisasiCabang(param)

            Case "M0_HitungRealisasiLokasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_HitungRealisasiLokasi(param)

            Case "M0_HitungRealisasiCostCenter"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_HitungRealisasiCostCenter(param)

            Case "M0_HitungRealisasiDivisi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_HitungRealisasiDivisi(param)

            Case "M0_HitungRealisasiProyek"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_HitungRealisasiProyek(param)

            Case "M0_GeneralLedger"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_GeneralLedger(param)

            Case "M0_NeracaMutasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_NeracaMutasi(param)

            Case "M0_KasHarian_Global"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_KasHarian_Global(param)

            Case "M0_KasHarian_AkunLawan"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_KasHarian_AkunLawan(param)

            Case "M0_BankHarian_Global"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_BankHarian_Global(param)

            Case "M0_BankHarian_AkunLawan"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_BankHarian_AkunLawan(param)

            Case "M0_PosisiKeuangan"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_PosisiKeuangan(param)

            Case "M0_PosisiKeuanganT"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_PosisiKeuanganT(param)

            Case "M0_LabaRugi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_LabaRugi(param)

            Case "M0_ARCard"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARCard(param)

            Case "M0_ARSummary"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARSummary(param)

            Case "M0_ARVoucher"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARVoucher(param)

            Case "M0_APCard"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_APCard(param)

            Case "M0_APSummary"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_APSummary(param)

            Case "M0_APVoucher"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_APVoucher(param)

            Case "M0_UMPenjualanCard"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_UMPenjualanCard(param)

            Case "M0_UMPenjualanSummary"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_UMPenjualanSummary(param)

            Case "M0_UMPenjualanVoucher"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_UMPenjualanVoucher(param)

            Case "M0_UMPembelianCard"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_UMPembelianCard(param)

            Case "M0_UMPembelianSummary"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_UMPembelianSummary(param)

            Case "M0_UMPembelianVoucher"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_UMPembelianVoucher(param)

            Case "M0_ARPostageCard"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARPostageCard(param)

            Case "M0_ARPostageSummary"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARPostageSummary(param)

            Case "M0_ARPostageVoucher"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARPostageVoucher(param)

            Case "M0_IPCard"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_IPCard(param)

            Case "M0_IPSummary"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_IPSummary(param)

            Case "M0_IPVoucher"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_IPVoucher(param)

            Case "M0_IPList"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_IPList(param)

            Case "M0_Bp_CostCenterSummary"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_Bp_CostCenterSummary(param)

            Case "M0_Bp_CostCenterBukuBesar"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_Bp_CostCenterBukuBesar(param)

            Case "M0_Labarugi_Invoice"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_Labarugi_Invoice(param)

            Case "M0_Mutasi_Keuangan"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_Mutasi_Keuangan(param)

            Case "M0_PersediaanPerGudang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_PersediaanPerGudang(param)

            Case "M0_MutasiStok"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_MutasiStok(param)

            Case "M0_KartuStok_Average"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_KartuStok_Average(param)

            Case "M0_KartuStok_Fifo"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_KartuStok_Fifo(param)

            Case "M0_KartuStok_Khusus"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_KartuStok_Khusus(param)

            Case "M0_PerincianBiaya"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_PerincianBiaya(param)

            Case "M0_ARVoucher_Aging"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARVoucher_Aging(param)

            Case "M0_ARSummary_Aging"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARSummary_Aging(param)

            Case "M0_APVoucher_Aging"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_APVoucher_Aging(param)

            Case "M0_APSummary_Aging"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_APSummary_Aging(param)

            Case "M0_GiroVoucher"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_GiroVoucher(param)

            Case "M0_GiroVoucher_PerBank"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_GiroVoucher_PerBank(param)

            Case "M0_GiroVoucher_Aging"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_GiroVoucher_Aging(param)

            Case "M0_GiroSummary_Aging"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_GiroSummary_Aging(param)

            Case "M0_ArusKasUndirect"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ArusKasUndirect(param)

            Case "M0_GeneralLedger_Detail"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_GeneralLedger_Detail(param)

            Case "M0_LabaRugi_Cabang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_LabaRugi_Cabang(param)

            Case "M0_LabaRugi_Lokasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_LabaRugi_Lokasi(param)

            Case "M0_LabaRugi_Divisi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_LabaRugi_Divisi(param)

            Case "M0_LabaRugi_Proyek"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_LabaRugi_Proyek(param)

            Case "M0_LabaRugi_CostCenter"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_LabaRugi_CostCenter(param)

            Case "M0_PosisiKeuangan_Cabang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_PosisiKeuangan_Cabang(param)

            Case "M0_PosisiKeuanganT_Cabang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_PosisiKeuanganT_Cabang(param)

            Case "M0_PosisiKeuangan_Lokasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_PosisiKeuangan_Lokasi(param)

            Case "M0_PosisiKeuanganT_Lokasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_PosisiKeuanganT_Lokasi(param)

            Case "M0_PosisiKeuangan_Divisi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_PosisiKeuangan_Divisi(param)

            Case "M0_PosisiKeuanganT_Divisi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_PosisiKeuanganT_Divisi(param)

            Case "M0_PosisiKeuangan_Proyek"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_PosisiKeuangan_Proyek(param)

            Case "M0_PosisiKeuanganT_Proyek"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_PosisiKeuanganT_Proyek(param)

            Case "M0_PosisiKeuangan_Costcenter"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_PosisiKeuangan_Costcenter(param)

            Case "M0_PosisiKeuanganT_Costcenter"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_PosisiKeuanganT_Costcenter(param)

            Case "M0_ARCard_Cabang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARCard_Cabang(param)

            Case "M0_ARCard_Lokasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARCard_Lokasi(param)

            Case "M0_ARSummary_Cabang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARSummary_Cabang(param)

            Case "M0_ARSummary_Lokasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARSummary_Lokasi(param)

            Case "M0_ARVoucher_Cabang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARVoucher_Cabang(param)

            Case "M0_ARVoucher_Lokasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARVoucher_Lokasi(param)

            Case "M0_ARVoucher_Aging_Cabang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARVoucher_Aging_Cabang(param)

            Case "M0_ARVoucher_Aging_Lokasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARVoucher_Aging_Lokasi(param)

            Case "M0_ARSummary_Aging_Cabang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARSummary_Aging_Cabang(param)

            Case "M0_ARSummary_Aging_Lokasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_ARSummary_Aging_Lokasi(param)

            Case "M0_APCard_Cabang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_APCard_Cabang(param)

            Case "M0_APCard_Lokasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_APCard_Lokasi(param)

            Case "M0_APSummary_Cabang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_APSummary_Cabang(param)

            Case "M0_APSummary_Lokasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_APSummary_Lokasi(param)

            Case "M0_APVoucher_Cabang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_APVoucher_Cabang(param)

            Case "M0_APVoucher_Lokasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_APVoucher_Lokasi(param)

            Case "M0_APVoucher_Aging_Cabang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_APVoucher_Aging_Cabang(param)

            Case "M0_APVoucher_Aging_Lokasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_APVoucher_Aging_Lokasi(param)

            Case "M0_APSummary_Aging_Cabang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_APSummary_Aging_Cabang(param)

            Case "M0_APSummary_Aging_Lokasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_APSummary_Aging_Lokasi(param)

            Case "M0_MutasiStok_Detail"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_MutasiStok_Detail(param)

            Case "M0_Anggaran"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_Anggaran(param)

            Case "M0_Anggaran_Cabang"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_Anggaran_Cabang(param)

            Case "M0_Anggaran_Lokasi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_Anggaran_Lokasi(param)

            Case "M0_Anggaran_Costcenter"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_Anggaran_Costcenter(param)

            Case "M0_Anggaran_Divisi"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_Anggaran_Divisi(param)

            Case "M0_Anggaran_Proyek"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_Anggaran_Proyek(param)

            Case "M0_CustomerPoint"
                Dim wsM0_Report_Progress As New m0_report_progress
                hasil = wsM0_Report_Progress.M0_CustomerPoint(param)

            Case "m0_cek_perpaket"
                Dim wsM0_validitas_data As New m0_validitas_data
                hasil = wsM0_validitas_data.m0_cek_perpaket(param)

            Case "m0_hitung_perpaket"
                Dim wsM0_validitas_data As New m0_validitas_data
                hasil = wsM0_validitas_data.m0_hitung_perpaket(param)

                '********************************** MOBILE ***********************************

                'MOB_M0_LOGIN
            Case "MobM0_Login"
                Dim wsMobM0_Login As New mob_m0_login
                hasil = wsMobM0_Login.MobM0_Login(param)
            Case "MobM0_LoginMin"
                Dim wsMobM0_Login As New mob_m0_login
                hasil = wsMobM0_Login.MobM0_LoginMin(param)
            Case "MobM0_aktifkanUser"
                Dim wsMobM0_Login As New mob_m0_login
                hasil = wsMobM0_Login.MobM0_aktifkanUser(param)
            Case "MobM0_Logout"
                Dim wsMobM0_Login As New mob_m0_login
                hasil = wsMobM0_Login.MobM0_Logout(param)

                'MOB_M1_ITEM
            Case "MobM1_ItemSearch"
                Dim wsMobM1_Item As New mob_m1_item
                hasil = wsMobM1_Item.MobM1_ItemSearch(param)
                'MOB_M1_TAX
            Case "MobM1_TaxSearch"
                Dim wsMobM1_Item As New mob_m1_tax
                hasil = wsMobM1_Item.MobM1_TaxSearch(param)

                'MOB_M1_CONTACT
            Case "MobM1_ContactSearch"
                Dim wsMobM1_Contact As New mob_m1_contact
                hasil = wsMobM1_Contact.MobM1_ContactSearch(param)
                'MOB_M1_CONTACT
            Case "MobM1_ContactSimpan"
                Dim wsMobM1_Contact As New mob_m1_contact
                hasil = wsMobM1_Contact.MobM1_ContactSimpan(param)

                'MOB_M5_SO
            Case "MobM5_SoSearch"
                Dim wsMobM5_So As New mob_m5_so
                hasil = wsMobM5_So.MobM5_SoSearch(param)
            Case "MobM5_So_DetailSearch"
                Dim wsMobM5_So As New mob_m5_so
                hasil = wsMobM5_So.MobM5_So_DetailSearch(param)
            Case "MobM5_SoSimpan"
                Dim wsMobM5_So As New mob_m5_so
                hasil = wsMobM5_So.MobM5_SoSimpan(param)
            Case "MobM5_SoSimpanAll"
                Dim wsMobM5_So As New mob_m5_so
                hasil = wsMobM5_So.MobM5_SoSimpanAll(param)

                '********************************* M12 POS '*********************************

                'M12_GET_VALUEMEMBER
            Case "M12_GetValueMember"
                Dim wsM12_GetVelueMember As New m12_getValueMember
                hasil = wsM12_GetVelueMember.M12_getSaldo(param)

                'M12_UpdateStatusMemberSlip
            Case "M12_UpdateStatusMemberSlip"
                Dim wsM12_GetVelueMember As New m12_getValueMember
                hasil = wsM12_GetVelueMember.M12_UpdateStatusMemberSlip(param)

                'M12_POS_SETTING
            Case "M12_Pos_SettingSimpan"
                Dim wsM12_Pos_Setting As New m12_pos_setting
                hasil = wsM12_Pos_Setting.M12_Pos_SettingSimpan(param)
            Case "M12_Pos_SettingSearch"
                Dim wsM12_Pos_Setting As New m12_pos_setting
                hasil = wsM12_Pos_Setting.M12_Pos_SettingSearch(param)
            Case "M12_Pos_SettingDelete"
                Dim wsM12_Pos_Setting As New m12_pos_setting
                hasil = wsM12_Pos_Setting.M12_Pos_SettingDelete(param)

            Case "M12_Pos_SettingDownload"
                Dim wsM12_Pos_Setting As New m12_pos_setting
                hasil = wsM12_Pos_Setting.M12_Pos_SettingDownload(param)
            Case "M12_Pos_SettingImport"
                Dim wsM12_Pos_Setting As New m12_pos_setting
                hasil = wsM12_Pos_Setting.M12_Pos_SettingImport(param)


                'M12_POS_CATEGORY
            Case "M12_Pos_CategorySimpan"
                Dim wsM12_Pos_Category As New m12_pos_category
                hasil = wsM12_Pos_Category.M12_Pos_CategorySimpan(param)
            Case "M12_Pos_CategoryDelete"
                Dim wsM12_Pos_Category As New m12_pos_category
                hasil = wsM12_Pos_Category.M12_Pos_CategoryDelete(param)
            Case "M12_Pos_CategorySearch"
                Dim wsM12_Pos_Category As New m12_pos_category
                hasil = wsM12_Pos_Category.M12_Pos_CategorySearch(param)
            Case "M12_Pos_CategoryCekId"
                Dim wsM12_Pos_Category As New m12_pos_category
                hasil = wsM12_Pos_Category.M12_Pos_CategoryCekId(param)
            Case "M12_Pos_CategoryTerkait"
                Dim wsM12_Pos_Category As New m12_pos_category
                hasil = wsM12_Pos_Category.M12_Pos_CategoryTerkait(param)

            Case "M12_Pos_CategoryDownload"
                Dim wsM12_Pos_Category As New m12_pos_category
                hasil = wsM12_Pos_Category.M12_Pos_CategoryDownload(param)
            Case "M12_Pos_CategoryImport"
                Dim wsM12_Pos_Category As New m12_pos_category
                hasil = wsM12_Pos_Category.M12_Pos_CategoryImport(param)

            Case "M12_Pos_Category_HistorySimpan"
                Dim wsM12_Pos_Category As New m12_pos_category_history
                hasil = wsM12_Pos_Category.M12_Pos_Category_HistorySimpan(param)
            Case "M12_Pos_Category_HistorySearch"
                Dim wsM12_Pos_Category As New m12_pos_category_history
                hasil = wsM12_Pos_Category.M12_Pos_Category_HistorySearch(param)

            Case "CdM12_Pos_Category"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM12_Pos_Category(param)
            Case "CdM12_Pos_CategoryAll"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM12_Pos_CategoryAll(param)

            Case "CdM12_Ppa"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM12_Ppa(param)

            Case "CdM12_Ppa_Detail"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM12_Ppa_Detail(param)


                'M12_POS_VOUCHER
            Case "M12_Pos_VoucherSimpan"
                Dim wsM12_Pos_Voucher As New m12_pos_voucher
                hasil = wsM12_Pos_Voucher.M12_Pos_VoucherSimpan(param)
            Case "M12_Pos_VoucherSearch"
                Dim wsM12_Pos_Voucher As New m12_pos_voucher
                hasil = wsM12_Pos_Voucher.M12_Pos_VoucherSearch(param)
            Case "M12_Pos_VoucherDelete"
                Dim wsM12_Pos_Voucher As New m12_pos_voucher
                hasil = wsM12_Pos_Voucher.M12_Pos_VoucherDelete(param)
            Case "M12_Pos_VoucherCode"
                Dim wsM12_Pos_Voucher As New m12_pos_voucher
                hasil = wsM12_Pos_Voucher.M12_Pos_VoucherCode(param)

            Case "M12_Pos_VoucherDownload"
                Dim wsM12_Pos_Voucher As New m12_pos_voucher
                hasil = wsM12_Pos_Voucher.M12_Pos_VoucherDownload(param)
            Case "M12_Pos_VoucherImport"
                Dim wsM12_Pos_Voucher As New m12_pos_voucher
                hasil = wsM12_Pos_Voucher.M12_Pos_VoucherImport(param)

            Case "CdM12_Pos_Voucher"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM12_Pos_Voucher(param)


                'M12_POS_CATEGORY_SETTING
            Case "M12_Pos_Category_SettingSimpan"
                Dim wsM12_Pos_Category_Setting As New m12_pos_category_setting
                hasil = wsM12_Pos_Category_Setting.M12_Pos_Category_SettingSimpan(param)
            Case "M12_Pos_Category_SettingDelete"
                Dim wsM12_Pos_Category_Setting As New m12_pos_category_setting
                hasil = wsM12_Pos_Category_Setting.M12_Pos_Category_SettingDelete(param)
            Case "M12_Pos_Category_SettingSearch"
                Dim wsM12_Pos_Category_Setting As New m12_pos_category_setting
                hasil = wsM12_Pos_Category_Setting.M12_Pos_Category_SettingSearch(param)
            Case "M12_Pos_Category_SettingGetdataById"
                Dim wsM12_Pos_Category_Setting As New m12_pos_category_setting
                hasil = wsM12_Pos_Category_Setting.M12_Pos_Category_SettingGetdataById(param)
            Case "M12_Pos_Category_LocationSearch"
                Dim wsM12_Pos_Category_Setting As New m12_pos_category_setting
                hasil = wsM12_Pos_Category_Setting.M12_Pos_Category_LocationSearch(param)

            Case "M12_Pos_Category_SettingDownload"
                Dim wsM12_Pos_Category_Setting As New m12_pos_category_setting
                hasil = wsM12_Pos_Category_Setting.M12_Pos_Category_SettingDownload(param)
            Case "M12_Pos_Category_SettingImport"
                Dim wsM12_Pos_Category_Setting As New m12_pos_category_setting
                hasil = wsM12_Pos_Category_Setting.M12_Pos_Category_SettingImport(param)



                'M12_POS_ITEM
            Case "M12_Pos_ItemSimpan"
                Dim wsM12_Pos_Item As New m12_pos_item
                hasil = wsM12_Pos_Item.M12_Pos_ItemSimpan(param)
            Case "M12_Pos_ItemSimpanPerKategori"
                Dim wsM12_Pos_Item As New m12_pos_item
                hasil = wsM12_Pos_Item.M12_Pos_ItemSimpanPerKategori(param)
            Case "M12_Pos_ItemSimpanKelasProduk"
                Dim wsM12_Pos_Item As New m12_pos_item
                hasil = wsM12_Pos_Item.M12_Pos_ItemSimpanKelasProduk(param)
            Case "M12_Pos_ItemSimpanSemua"
                Dim wsM12_Pos_Item As New m12_pos_item
                hasil = wsM12_Pos_Item.M12_Pos_ItemSimpanSemua(param)
            Case "M12_Pos_ItemSearch"
                Dim wsM12_Pos_Item As New m12_pos_item
                hasil = wsM12_Pos_Item.M12_Pos_ItemSearch(param)
            Case "M12_Pos_ItemDelete"
                Dim wsM12_Pos_Item As New m12_pos_item
                hasil = wsM12_Pos_Item.M12_Pos_ItemDelete(param)
            Case "M12_Pos_ItemDeletePerKategori"
                Dim wsM12_Pos_Item As New m12_pos_item
                hasil = wsM12_Pos_Item.M12_Pos_ItemDeletePerKategori(param)
            Case "M12_Pos_ItemDeleteKelasProduk"
                Dim wsM12_Pos_Item As New m12_pos_item
                hasil = wsM12_Pos_Item.M12_Pos_ItemDeleteKelasProduk(param)
            Case "M12_Pos_ItemDeleteSemua"
                Dim wsM12_Pos_Item As New m12_pos_item
                hasil = wsM12_Pos_Item.M12_Pos_ItemDeleteSemua(param)
            Case "M12_Pos_ItemSetHargaIndeks"
                Dim wsM12_Pos_Item As New m12_pos_item
                hasil = wsM12_Pos_Item.M12_Pos_ItemSetHargaIndeks(param)
            Case "M12_Pos_ItemSimpanLokasiLain"
                Dim wsM12_Pos_Item As New m12_pos_item
                hasil = wsM12_Pos_Item.M12_Pos_ItemSimpanLokasiLain(param)

            Case "M12_Pos_ItemDownload"
                Dim wsM12_Pos_Item As New m12_pos_item
                hasil = wsM12_Pos_Item.M12_Pos_ItemDownload(param)
            Case "M12_Pos_ItemImport"
                Dim wsM12_Pos_Item As New m12_pos_item
                hasil = wsM12_Pos_Item.M12_Pos_ItemImport(param)

            Case "M12_Pos_ItemTerkait"
                Dim wsM12_Item As New m12_pos_item
                hasil = wsM12_Item.M12_Pos_ItemTerkait(param)


            Case "CdM12_ItemAddCategoryPOS"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM12_ItemAddCategoryPOS(param)
            Case "CdM12_ItemCategoryPOS"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM12_ItemCategoryPOS(param)
            Case "CdM12_Item"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM12_Item(param)
            Case "M12_Item_InformationSearch"
                Dim wsM12_Item As New m12_item
                hasil = wsM12_Item.M12_Item_InformationSearch(param)



            Case "M12_Item_Stock_WarehouseSearch"
                Dim wsM12_Item As New m12_item
                hasil = wsM12_Item.M12_Item_Stock_WarehouseSearch(param)
            Case "CdM12_Contact"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM12_Contact(param)
            Case "M12_ContactSearch"
                Dim wsM12_Contact As New m12_contact
                hasil = wsM12_Contact.M12_ContactSearch(param)
            Case "M12_ContactGetdataById"
                Dim wsM12_Contact As New m12_contact
                hasil = wsM12_Contact.M12_ContactGetdataById(param)


                'M12_POS_DISCOUNT_ITEM
            Case "M12_Pos_Discount_ItemSimpan"
                Dim wsM12_Pos_Discount_Item As New m12_pos_discount_item
                hasil = wsM12_Pos_Discount_Item.M12_Pos_Discount_ItemSimpan(param)
            Case "M12_Pos_Discount_ItemSearch"
                Dim wsM12_Pos_Discount_Item As New m12_pos_discount_item
                hasil = wsM12_Pos_Discount_Item.M12_Pos_Discount_ItemSearch(param)
            Case "M12_Pos_Discount_ItemDelete"
                Dim wsM12_Pos_Discount_Item As New m12_pos_discount_item
                hasil = wsM12_Pos_Discount_Item.M12_Pos_Discount_ItemDelete(param)

            Case "M12_Pos_Discount_ItemDownload"
                Dim wsM12_Pos_Discount_Item As New m12_pos_discount_item
                hasil = wsM12_Pos_Discount_Item.M12_Pos_Discount_ItemDownload(param)
            Case "M12_Pos_Discount_ItemImport"
                Dim wsM12_Pos_Discount_Item As New m12_pos_discount_item
                hasil = wsM12_Pos_Discount_Item.M12_Pos_Discount_ItemImport(param)

                'm12_DI'
            Case "M12_DiSimpan"
                Dim wsM12_Di As New m12_di
                hasil = wsM12_Di.M12_DiSimpan(param)
            Case "M12_DiGetdataById"
                Dim wsM12_Di As New m12_di
                hasil = wsM12_Di.M12_DiGetdataById(param)
            Case "M12_DiSearch"
                Dim wsM12_Di As New m12_di
                hasil = wsM12_Di.M12_DiSearch(param)
            Case "M12_DiUpdateStatus"
                Dim wsM12_Di As New m12_di
                hasil = wsM12_Di.M12_DiUpdateStatus(param)
            Case "M12_DiDelete"
                Dim wsM12_Di As New m12_di
                hasil = wsM12_Di.M12_DiDelete(param)
            Case "M12_Di_HistorySimpan"
                Dim wsM12_Di As New m12_di_history
                hasil = wsM12_Di.M12_Di_HistorySimpan(param)
            Case "M12_Di_HistorySearch"
                Dim wsM12_Di As New m12_di_history
                hasil = wsM12_Di.M12_Di_HistorySearch(param)
            Case "M12_DiHistoryGetdataById"
                Dim wsM12_Di As New m12_di_history
                hasil = wsM12_Di.M12_DiHistoryGetdataById(param)



                'M12_POS_DISCOUNT_CATEGORY_ITEM
            Case "M12_Pos_Discount_Category_ItemSimpan"
                Dim wsM12_Pos_Discount_Category_Item As New m12_pos_discount_category_item
                hasil = wsM12_Pos_Discount_Category_Item.M12_Pos_Discount_Category_ItemSimpan(param)
            Case "M12_Pos_Discount_Category_ItemSearch"
                Dim wsM12_Pos_Discount_Category_Item As New m12_pos_discount_category_item
                hasil = wsM12_Pos_Discount_Category_Item.M12_Pos_Discount_Category_ItemSearch(param)
            Case "M12_Pos_Discount_Category_ItemDelete"
                Dim wsM12_Pos_Discount_Category_Item As New m12_pos_discount_category_item
                hasil = wsM12_Pos_Discount_Category_Item.M12_Pos_Discount_Category_ItemDelete(param)

            Case "M12_Pos_Discount_Category_ItemDownload"
                Dim wsM12_Pos_Discount_Category_Item As New m12_pos_discount_category_item
                hasil = wsM12_Pos_Discount_Category_Item.M12_Pos_Discount_Category_ItemDownload(param)
            Case "M12_Pos_Discount_Category_ItemImport"
                Dim wsM12_Pos_Discount_Category_Item As New m12_pos_discount_category_item
                hasil = wsM12_Pos_Discount_Category_Item.M12_Pos_Discount_Category_ItemImport(param)


                'M12_POS_DISCOUNT_CATEGORY_Customer
            Case "M12_Pos_Discount_Category_CustomerSimpan"
                Dim wsM12_Pos_Discount_Category_Customer As New m12_pos_discount_category_customer
                hasil = wsM12_Pos_Discount_Category_Customer.M12_Pos_Discount_Category_CustomerSimpan(param)
            Case "M12_Pos_Discount_Category_CustomerSearch"
                Dim wsM12_Pos_Discount_Category_Customer As New m12_pos_discount_category_customer
                hasil = wsM12_Pos_Discount_Category_Customer.M12_Pos_Discount_Category_CustomerSearch(param)
            Case "M12_Pos_Discount_Category_CustomerDelete"
                Dim wsM12_Pos_Discount_Category_Customer As New m12_pos_discount_category_customer
                hasil = wsM12_Pos_Discount_Category_Customer.M12_Pos_Discount_Category_CustomerDelete(param)

            Case "M12_Pos_Discount_Category_CustomerDownload"
                Dim wsM12_Pos_Discount_Category_Customer As New m12_pos_discount_category_customer
                hasil = wsM12_Pos_Discount_Category_Customer.M12_Pos_Discount_Category_CustomerDownload(param)
            Case "M12_Pos_Discount_Category_CustomerImport"
                Dim wsM12_Pos_Discount_Category_Customer As New m12_pos_discount_category_customer
                hasil = wsM12_Pos_Discount_Category_Customer.M12_Pos_Discount_Category_CustomerImport(param)


                'M12_POS_POINT_ITEM
            Case "M12_Pos_Point_ItemSimpan"
                Dim wsM12_Pos_Point_Item As New m12_pos_point_item
                hasil = wsM12_Pos_Point_Item.M12_Pos_Point_ItemSimpan(param)
            Case "M12_Pos_Point_ItemSearch"
                Dim wsM12_Pos_Point_Item As New m12_pos_point_item
                hasil = wsM12_Pos_Point_Item.M12_Pos_Point_ItemSearch(param)
            Case "M12_Pos_Point_ItemDelete"
                Dim wsM12_Pos_Point_Item As New m12_pos_point_item
                hasil = wsM12_Pos_Point_Item.M12_Pos_Point_ItemDelete(param)

            Case "M12_Pos_Point_ItemDownload"
                Dim wsM12_Pos_Point_Item As New m12_pos_point_item
                hasil = wsM12_Pos_Point_Item.M12_Pos_Point_ItemDownload(param)
            Case "M12_Pos_Point_ItemImport"
                Dim wsM12_Pos_Point_Item As New m12_pos_point_item
                hasil = wsM12_Pos_Point_Item.M12_Pos_Point_ItemImport(param)

                'm12_ST'

            Case "M12_StSimpan"
                Dim wsM12_St As New m12_st
                hasil = wsM12_St.M12_StSimpan(param)
            Case "M12_StGetdataById"
                Dim wsM12_St As New m12_st
                hasil = wsM12_St.M12_StGetdataById(param)
            Case "M12_StSearch"
                Dim wsM12_St As New m12_st
                hasil = wsM12_St.M12_StSearch(param)
            Case "M12_StUpdateStatus"
                Dim wsM12_St As New m12_st
                hasil = wsM12_St.M12_StUpdateStatus(param)
            Case "M12_StDelete"
                Dim wsM12_St As New m12_st
                hasil = wsM12_St.M12_StDelete(param)


                'M12_POS_POINT_CATEGORY_ITEM
            Case "M12_Pos_Point_Category_ItemSimpan"
                Dim wsM12_Pos_Point_Category_Item As New m12_pos_point_category_item
                hasil = wsM12_Pos_Point_Category_Item.M12_Pos_Point_Category_ItemSimpan(param)
            Case "M12_Pos_Point_Category_ItemSearch"
                Dim wsM12_Pos_Point_Category_Item As New m12_pos_point_category_item
                hasil = wsM12_Pos_Point_Category_Item.M12_Pos_Point_Category_ItemSearch(param)
            Case "M12_Pos_Point_Category_ItemDelete"
                Dim wsM12_Pos_Point_Category_Item As New m12_pos_point_category_item
                hasil = wsM12_Pos_Point_Category_Item.M12_Pos_Point_Category_ItemDelete(param)

            Case "M12_Pos_Point_Category_ItemDownload"
                Dim wsM12_Pos_Point_Category_Item As New m12_pos_point_category_item
                hasil = wsM12_Pos_Point_Category_Item.M12_Pos_Point_Category_ItemDownload(param)
            Case "M12_Pos_Point_Category_ItemImport"
                Dim wsM12_Pos_Point_Category_Item As New m12_pos_point_category_item
                hasil = wsM12_Pos_Point_Category_Item.M12_Pos_Point_Category_ItemImport(param)


                'M12_POS_POINT_TRANSACTION
            Case "M12_Pos_Point_TransactionSimpan"
                Dim wsM12_Pos_Point_Transaction As New m12_pos_point_transaction
                hasil = wsM12_Pos_Point_Transaction.M12_Pos_Point_TransactionSimpan(param)
            Case "M12_Pos_Point_TransactionSearch"
                Dim wsM12_Pos_Point_Transaction As New m12_pos_point_transaction
                hasil = wsM12_Pos_Point_Transaction.M12_Pos_Point_TransactionSearch(param)
            Case "M12_Pos_Point_TransactionDelete"
                Dim wsM12_Pos_Point_Transaction As New m12_pos_point_transaction
                hasil = wsM12_Pos_Point_Transaction.M12_Pos_Point_TransactionDelete(param)
            Case "M12_Pos_Point_TransactionDownload"
                Dim wsM12_Pos_Point_Transaction As New m12_pos_point_transaction
                hasil = wsM12_Pos_Point_Transaction.M12_Pos_Point_TransactionDownload(param)
            Case "M12_Pos_Point_TransactionImport"
                Dim wsM12_Pos_Point_Transaction As New m12_pos_point_transaction
                hasil = wsM12_Pos_Point_Transaction.M12_Pos_Point_TransactionImport(param)

                'M12_POS_PROMO
            Case "M12_Beli_x_dapat_diskonitemSearch"
                Dim wsM12_POS_PROMO As New m12_pos_promo
                hasil = wsM12_POS_PROMO.M12_Beli_x_dapat_diskonitemSearch(param)

                'M12_POS_BONUS_ITEM
            Case "M12_Pos_Bonus_ItemSimpan"
                Dim wsM12_Pos_Bonus_Item As New m12_pos_bonus_item
                hasil = wsM12_Pos_Bonus_Item.M12_Pos_Bonus_ItemSimpan(param)
            Case "M12_Pos_Bonus_ItemSearch"
                Dim wsM12_Pos_Bonus_Item As New m12_pos_bonus_item
                hasil = wsM12_Pos_Bonus_Item.M12_Pos_Bonus_ItemSearch(param)
            Case "M12_Pos_Bonus_ItemGetdataById"
                Dim wsM12_Pos_Bonus_Item As New m12_pos_bonus_item
                hasil = wsM12_Pos_Bonus_Item.M12_Pos_Bonus_ItemGetdataById(param)
            Case "M12_Pos_Bonus_ItemDelete"
                Dim wsM12_Pos_Bonus_Item As New m12_pos_bonus_item
                hasil = wsM12_Pos_Bonus_Item.M12_Pos_Bonus_ItemDelete(param)

            Case "M12_Pos_Bonus_Item_DetailSearch"
                Dim wsM12_Pos_Bonus_Item As New m12_pos_bonus_item
                hasil = wsM12_Pos_Bonus_Item.M12_Pos_Bonus_Item_DetailSearch(param)
            Case "M12_Pos_Bonus_Item_DetailSetting"
                Dim wsM12_Pos_Bonus_Item As New m12_pos_bonus_item
                hasil = wsM12_Pos_Bonus_Item.M12_Pos_Bonus_Item_DetailSetting(param)

            Case "M12_Pos_Bonus_ItemDownload"
                Dim wsM12_Pos_Bonus_Item As New m12_pos_bonus_item
                hasil = wsM12_Pos_Bonus_Item.M12_Pos_Bonus_ItemDownload(param)
            Case "M12_Pos_Bonus_ItemImport"
                Dim wsM12_Pos_Bonus_Item As New m12_pos_bonus_item
                hasil = wsM12_Pos_Bonus_Item.M12_Pos_Bonus_ItemImport(param)


                'M12_POS_BONUS_Trans
            Case "M12_Pos_Bonus_TransSimpan"
                Dim wsM12_Pos_Bonus_Trans As New m12_pos_bonus_trans
                hasil = wsM12_Pos_Bonus_Trans.M12_Pos_Bonus_TransSimpan(param)
            Case "M12_Pos_Bonus_TransSearch"
                Dim wsM12_Pos_Bonus_Trans As New m12_pos_bonus_trans
                hasil = wsM12_Pos_Bonus_Trans.M12_Pos_Bonus_TransSearch(param)
            Case "M12_Pos_Bonus_TransGetdataById"
                Dim wsM12_Pos_Bonus_Trans As New m12_pos_bonus_trans
                hasil = wsM12_Pos_Bonus_Trans.M12_Pos_Bonus_TransGetdataById(param)
            Case "M12_Pos_Bonus_TransDelete"
                Dim wsM12_Pos_Bonus_Trans As New m12_pos_bonus_trans
                hasil = wsM12_Pos_Bonus_Trans.M12_Pos_Bonus_TransDelete(param)

            Case "M12_Pos_Bonus_Trans_DetailSearch"
                Dim wsM12_Pos_Bonus_Trans As New m12_pos_bonus_trans
                hasil = wsM12_Pos_Bonus_Trans.M12_Pos_Bonus_Trans_DetailSearch(param)
            Case "M12_Pos_Bonus_Trans_DetailSetting"
                Dim wsM12_Pos_Bonus_Trans As New m12_pos_bonus_trans
                hasil = wsM12_Pos_Bonus_Trans.M12_Pos_Bonus_Trans_DetailSetting(param)

            Case "M12_Pos_Bonus_TransDownload"
                Dim wsM12_Pos_Bonus_Trans As New m12_pos_bonus_trans
                hasil = wsM12_Pos_Bonus_Trans.M12_Pos_Bonus_TransDownload(param)
            Case "M12_Pos_Bonus_TransImport"
                Dim wsM12_Pos_Bonus_Trans As New m12_pos_bonus_trans
                hasil = wsM12_Pos_Bonus_Trans.M12_Pos_Bonus_TransImport(param)


                'm12_BI'
            Case "M12_BiSimpan"
                Dim wsM12_Bi As New m12_bi
                hasil = wsM12_Bi.M12_BiSimpan(param)
            Case "M12_BiGetdataById"
                Dim wsM12_Bi As New m12_bi
                hasil = wsM12_Bi.M12_BiGetdataById(param)
            Case "M12_BiSearch"
                Dim wsM12_Bi As New m12_bi
                hasil = wsM12_Bi.M12_BiSearch(param)
            Case "M12_BiUpdateStatus"
                Dim wsM12_Bi As New m12_bi
                hasil = wsM12_Bi.M12_BiUpdateStatus(param)
            Case "M12_BiDelete"
                Dim wsM12_Bi As New m12_bi
                hasil = wsM12_Bi.M12_BiDelete(param)
            Case "M12_Bi_HistorySimpan"
                Dim wsM12_Bi As New m12_bi_history
                hasil = wsM12_Bi.M12_Bi_HistorySimpan(param)
            Case "M12_Bi_HistorySearch"
                Dim wsM12_Bi As New m12_bi_history
                hasil = wsM12_Bi.M12_Bi_HistorySearch(param)
            Case "M12_BiHistoryGetdataById"
                Dim wsM12_Bi As New m12_bi_history
                hasil = wsM12_Bi.M12_BiHistoryGetdataById(param)
            Case "M12_BiGetdataById"
                Dim wsM12_Bi As New m12_bi
                hasil = wsM12_Bi.M12_BiGetdataById(param)
            Case "M12_BiSearch"
                Dim wsM12_Bi As New m12_bi
                hasil = wsM12_Bi.M12_BiSearch(param)
            Case "M12_BiUpdateStatus"
                Dim wsM12_Bi As New m12_bi
                hasil = wsM12_Bi.M12_BiUpdateStatus(param)
            Case "M12_BiDelete"
                Dim wsM12_Bi As New m12_bi
                hasil = wsM12_Bi.M12_BiDelete(param)
            Case "M12_Bi_HistorySimpan"
                Dim wsM12_Bi As New m12_bi_history
                hasil = wsM12_Bi.M12_Bi_HistorySimpan(param)
            Case "M12_Bi_HistorySearch"
                Dim wsM12_Bi As New m12_bi_history
                hasil = wsM12_Bi.M12_Bi_HistorySearch(param)
            Case "M12_BiHistoryGetdataById"
                Dim wsM12_Bi As New m12_bi_history
                hasil = wsM12_Bi.M12_BiHistoryGetdataById(param)

                'M12_POS_ADDITIONAL_ITEM
            Case "M12_Pos_Additional_ItemSimpan"
                Dim wsM12_Pos_Additional_Item As New m12_pos_additional_item
                hasil = wsM12_Pos_Additional_Item.M12_Pos_Additional_ItemSimpan(param)
            Case "M12_Pos_Additional_ItemSearch"
                Dim wsM12_Pos_Additional_Item As New m12_pos_additional_item
                hasil = wsM12_Pos_Additional_Item.M12_Pos_Additional_ItemSearch(param)
            Case "M12_Pos_Additional_ItemGetdataById"
                Dim wsM12_Pos_Additional_Item As New m12_pos_additional_item
                hasil = wsM12_Pos_Additional_Item.M12_Pos_Additional_ItemGetdataById(param)
            Case "M12_Pos_Additional_ItemDelete"
                Dim wsM12_Pos_Additional_Item As New m12_pos_additional_item
                hasil = wsM12_Pos_Additional_Item.M12_Pos_Additional_ItemDelete(param)

            Case "M12_Pos_Additional_Item_DetailSearch"
                Dim wsM12_Pos_Additional_Item As New m12_pos_additional_item
                hasil = wsM12_Pos_Additional_Item.M12_Pos_Additional_Item_DetailSearch(param)
            Case "M12_Pos_Additional_Item_DetailSetting"
                Dim wsM12_Pos_Additional_Item As New m12_pos_additional_item
                hasil = wsM12_Pos_Additional_Item.M12_Pos_Additional_Item_DetailSetting(param)

            Case "M12_Pos_Additional_ItemDownload"
                Dim wsM12_Pos_Additional_Item As New m12_pos_additional_item
                hasil = wsM12_Pos_Additional_Item.M12_Pos_Additional_ItemDownload(param)
            Case "M12_Pos_Additional_ItemImport"
                Dim wsM12_Pos_Additional_Item As New m12_pos_additional_item
                hasil = wsM12_Pos_Additional_Item.M12_Pos_Additional_ItemImport(param)

                'm12_AI'
            Case "M12_AiSimpan"
                Dim wsM12_Ai As New m12_ai
                hasil = wsM12_Ai.M12_AiSimpan(param)
            Case "M12_AiGetdataById"
                Dim wsM12_Ai As New m12_ai
                hasil = wsM12_Ai.M12_AiGetdataById(param)
            Case "M12_AiSearch"
                Dim wsM12_Ai As New m12_ai
                hasil = wsM12_Ai.M12_AiSearch(param)
            Case "M12_AiUpdateStatus"
                Dim wsM12_Ai As New m12_ai
                hasil = wsM12_Ai.M12_AiUpdateStatus(param)
            Case "M12_AiDelete"
                Dim wsM12_Ai As New m12_ai
                hasil = wsM12_Ai.M12_AiDelete(param)


                'M12_POS_SUBSTITUTION_ITEM
            Case "M12_Pos_Substitution_ItemSimpan"
                Dim wsM12_Pos_Substitution_Item As New m12_pos_substitution_item
                hasil = wsM12_Pos_Substitution_Item.M12_Pos_Substitution_ItemSimpan(param)
            Case "M12_Pos_Substitution_ItemSearch"
                Dim wsM12_Pos_Substitution_Item As New m12_pos_substitution_item
                hasil = wsM12_Pos_Substitution_Item.M12_Pos_Substitution_ItemSearch(param)
            Case "M12_Pos_Substitution_ItemGetdataById"
                Dim wsM12_Pos_Substitution_Item As New m12_pos_substitution_item
                hasil = wsM12_Pos_Substitution_Item.M12_Pos_Substitution_ItemGetdataById(param)
            Case "M12_Pos_Substitution_ItemDelete"
                Dim wsM12_Pos_Substitution_Item As New m12_pos_substitution_item
                hasil = wsM12_Pos_Substitution_Item.M12_Pos_Substitution_ItemDelete(param)

            Case "M12_Pos_Substitution_Item_DetailSearch"
                Dim wsM12_Pos_Substitution_Item As New m12_pos_substitution_item
                hasil = wsM12_Pos_Substitution_Item.M12_Pos_Substitution_Item_DetailSearch(param)
            Case "M12_Pos_Substitution_Item_DetailSetting"
                Dim wsM12_Pos_Substitution_Item As New m12_pos_substitution_item
                hasil = wsM12_Pos_Substitution_Item.M12_Pos_Substitution_Item_DetailSetting(param)

            Case "M12_Pos_Substitution_ItemDownload"
                Dim wsM12_Pos_Substitution_Item As New m12_pos_substitution_item
                hasil = wsM12_Pos_Substitution_Item.M12_Pos_Substitution_ItemDownload(param)
            Case "M12_Pos_Substitution_ItemImport"
                Dim wsM12_Pos_Substitution_Item As New m12_pos_substitution_item
                hasil = wsM12_Pos_Substitution_Item.M12_Pos_Substitution_ItemImport(param)

                'm12_Sbi'
            Case "M12_SbiSimpan"
                Dim wsM12_Sbi As New m12_sbi
                hasil = wsM12_Sbi.M12_SbiSimpan(param)
            Case "M12_SbiGetdataById"
                Dim wsM12_Sbi As New m12_sbi
                hasil = wsM12_Sbi.M12_SbiGetdataById(param)
            Case "M12_SbiSearch"
                Dim wsM12_Sbi As New m12_sbi
                hasil = wsM12_Sbi.M12_SbiSearch(param)
            Case "M12_SbiUpdateStatus"
                Dim wsM12_Sbi As New m12_sbi
                hasil = wsM12_Sbi.M12_SbiUpdateStatus(param)
            Case "M12_SbiDelete"
                Dim wsM12_Sbi As New m12_sbi
                hasil = wsM12_Sbi.M12_SbiDelete(param)

                'm12_PPA'
            Case "M12_PpaSimpan"
                Dim wsM12_Ppa As New m12_ppa
                hasil = wsM12_Ppa.M12_PpaSimpan(param)
            Case "M12_PpaSearch"
                Dim wsM12_Ppa As New m12_ppa
                hasil = wsM12_Ppa.M12_PpaSearch(param)
            Case "M12_PpaGetdataById"
                Dim wsM12_Ppa As New m12_ppa
                hasil = wsM12_Ppa.M12_PpaGetdataById(param)
            Case "M12_PpaUpdateStatus"
                Dim wsM12_Ppa As New m12_ppa
                hasil = wsM12_Ppa.M12_PpaUpdateStatus(param)
            Case "M12_PpaTerkait"
                Dim wsM12_Ppa As New m12_ppa
                hasil = wsM12_Ppa.M12_PpaTerkait(param)
            Case "M12_Ppa_Detail_VSearch"
                Dim wsM12_Ppa As New m12_ppa
                hasil = wsM12_Ppa.M12_Ppa_Detail_VSearch(param)

                'm12_LP'
            Case "M12_LpSimpan"
                Dim wsM12_Lp As New m12_lp
                hasil = wsM12_Lp.M12_LpSimpan(param)
            Case "M12_LpSearch"
                Dim wsM12_Lp As New m12_lp
                hasil = wsM12_Lp.M12_LpSearch(param)
            Case "M12_LpGetdataById"
                Dim wsM12_Lp As New m12_lp
                hasil = wsM12_Lp.M12_LpGetdataById(param)
            Case "M12_LpUpdateStatus"
                Dim wsM12_Lp As New m12_lp
                hasil = wsM12_Lp.M12_LpUpdateStatus(param)
            Case "M12_LpTerkait"
                Dim wsM12_Lp As New m12_lp
                hasil = wsM12_Lp.M12_LpTerkait(param)

                'M12_Si
            Case "M12_SiSimpan"
                Dim wsM12_Si As New m12_si
                hasil = wsM12_Si.M12_SiSimpan(param)
            Case "M12_SiSearch"
                Dim wsM12_Si As New m12_si
                hasil = wsM12_Si.M12_SiSearch(param)
            Case "M12_SiDelete"
                If (isDemo = False) Then
                    Dim wsM12_Si As New m12_si
                    hasil = wsM12_Si.M12_SiDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M12_SiGetdataById"
                Dim wsM12_Si As New m12_si
                hasil = wsM12_Si.M12_SiGetdataById(param)
            Case "M12_SiUpdateStatus"
                Dim wsM12_Si As New m12_si
                hasil = wsM12_Si.M12_SiUpdateStatus(param)
            Case "M12_Si_Detail_VSearch"
                Dim wsM12_Si As New m12_si
                hasil = wsM12_Si.M12_Si_Detail_VSearch(param)
            Case "M12_SiTerkait"
                Dim wsM12_Si As New m12_si
                hasil = wsM12_Si.M12_SiTerkait(param)
            Case "M12_SiGetUpload"
                Dim wsM12_Si As New m12_si
                hasil = wsM12_Si.M12_SiGetUpload(param)
            Case "M12_SiUploaded"
                Dim wsM12_Si As New m12_si
                hasil = wsM12_Si.M12_SiUploaded(param)
            Case "M12_Si_HistorySimpan"
                Dim wsM12_Si As New m12_si_history
                hasil = wsM12_Si.m12_Si_HistorySimpan(param)
            Case "M12_Si_HistorySearch"
                Dim wsM12_Si As New m12_si_history
                hasil = wsM12_Si.M12_Si_HistorySearch(param)
            Case "M12_SiHistoryGetdataById"
                Dim wsM12_Si As New m12_si_history
                hasil = wsM12_Si.M12_SiHistoryGetdataById(param)
            Case "M12_SiBalance"
                Dim wsM12_Si As New m12_si
                hasil = wsM12_Si.M12_SiBalance(param)
            Case "M12_SiBSearch"
                Dim wsM12_Si As New m12_si
                hasil = wsM12_Si.M12_SiBSearch(param)
            Case "M12_SiBUpdateStatus"
                Dim wsM12_Si As New m12_si
                hasil = wsM12_Si.M12_SiBUpdateStatus(param)
            Case "M12_SiBDelete"
                If (isDemo = False) Then
                    Dim wsM12_Si As New m12_si
                    hasil = wsM12_Si.M12_SiBDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If


                'M12_POS_HARDWARE
            Case "M12_Pos_HardwareSimpan"
                Dim wsM12_PosHardware As New m12_pos_hardware
                hasil = wsM12_PosHardware.M12_Pos_HardwareSimpan(param)
            Case "M12_Pos_HardwareSearch"
                Dim wsM12_PosHardware As New m12_pos_hardware
                hasil = wsM12_PosHardware.M12_Pos_HardwareSearch(param)
            Case "M12_Pos_HardwareDelete"
                Dim wsM12_PosHardware As New m12_pos_hardware
                hasil = wsM12_PosHardware.M12_Pos_HardwareDelete(param)


                'M12_AREA_CATEGORY
            Case "M12_Area_CategorySimpan"
                Dim wsM12_AreaCategory As New m12_area_category
                hasil = wsM12_AreaCategory.M12_Area_CategorySimpan(param)
            Case "M12_Area_CategorySearch"
                Dim wsM12_AreaCategory As New m12_area_category
                hasil = wsM12_AreaCategory.M12_Area_CategorySearch(param)
            Case "M12_Area_CategoryDelete"
                Dim wsM12_AreaCategory As New m12_area_category
                hasil = wsM12_AreaCategory.M12_Area_CategoryDelete(param)
            Case "M12_Area_CategoryCekId"
                Dim wsM12_AreaCategory As New m12_area_category
                hasil = wsM12_AreaCategory.M12_Area_CategoryCekId(param)
            Case "M12_Area_CategoryTerkait"
                Dim wsM12_AreaCategory As New m12_area_category
                hasil = wsM12_AreaCategory.M12_Area_CategoryTerkait(param)
            Case "M12_Area_CategoryDownload"
                Dim wsM12_AreaCategory As New m12_area_category
                hasil = wsM12_AreaCategory.M12_Area_CategoryDownload(param)
            Case "M12_Area_CategoryImport"
                Dim wsM12_AreaCategory As New m12_area_category
                hasil = wsM12_AreaCategory.M12_Area_CategoryImport(param)
            Case "M12_Area_CategoryHistorySimpan"
                Dim wsM12_AreaCategoryHistory As New m12_area_category_history
                hasil = wsM12_AreaCategoryHistory.M12_Area_CategoryHistorySimpan(param)
            Case "M12_Area_CategoryHistorySearch"
                Dim wsM12_AreaCategoryHistory As New m12_area_category_history
                hasil = wsM12_AreaCategoryHistory.M12_Area_CategoryHistorySearch(param)
            Case "CdM12_Area_Category"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM12_Area_Category(param)


                'M12_AREA
            Case "M12_AreaSimpan"
                Dim wsM12_Area As New m12_area
                hasil = wsM12_Area.M12_AreaSimpan(param)
            Case "M12_AreaSearch"
                Dim wsM12_Area As New m12_area
                hasil = wsM12_Area.M12_AreaSearch(param)
            Case "M12_AreaDelete"
                Dim wsM12_Area As New m12_area
                hasil = wsM12_Area.M12_AreaDelete(param)
            Case "M12_AreaCekId"
                Dim wsM12_Area As New m12_area
                hasil = wsM12_Area.M12_AreaCekId(param)
            Case "M12_AreaTerkait"
                Dim wsM12_Area As New m12_area
                hasil = wsM12_Area.M12_AreaTerkait(param)
            Case "M12_AreaDownload"
                Dim wsM12_Area As New m12_area
                hasil = wsM12_Area.M12_AreaDownload(param)
            Case "M12_AreaImport"
                Dim wsM12_Area As New m12_area
                hasil = wsM12_Area.M12_AreaImport(param)
            Case "M12_Area_HistorySimpan"
                Dim wsM12_AreaHistory As New m12_area_history
                hasil = wsM12_AreaHistory.M12_Area_HistorySimpan(param)
            Case "M12_Area_HistorySearch"
                Dim wsM12_AreaHistory As New m12_area_history
                hasil = wsM12_AreaHistory.M12_Area_HistorySearch(param)
            Case "CdM12_Area"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM12_Area(param)


                'M12_Cpa
            Case "M12_CpaSimpan"
                Dim wsM12_Cpa As New m12_cpa
                hasil = wsM12_Cpa.M12_CpaSimpan(param)
            Case "M12_CpaSearch"
                Dim wsM12_Cpa As New m12_cpa
                hasil = wsM12_Cpa.M12_CpaSearch(param)
            Case "M12_CpaDelete"
                If (isDemo = False) Then
                    Dim wsM12_Cpa As New m12_cpa
                    hasil = wsM12_Cpa.M12_CpaDelete(param)
                Else
                    hasil = hasilDemo : GoTo selesai
                End If
            Case "M12_CpaGetdataById"
                Dim wsM12_Cpa As New m12_cpa
                hasil = wsM12_Cpa.M12_CpaGetdataById(param)
            Case "M12_CpaUpdateStatus"
                Dim wsM12_Cpa As New m12_cpa
                hasil = wsM12_Cpa.M12_CpaUpdateStatus(param)
            Case "M12_CpaTerkait"
                Dim wsM12_Cpa As New m12_cpa
                hasil = wsM12_Cpa.M12_CpaTerkait(param)
            Case "M12_Cpa_HistorySimpan"
                Dim wsM12_Cpa As New m12_cpa_history
                hasil = wsM12_Cpa.M12_Cpa_HistorySimpan(param)
            Case "M12_Cpa_HistorySearch"
                Dim wsM12_Cpa As New m12_cpa_history
                hasil = wsM12_Cpa.M12_Cpa_HistorySearch(param)
            Case "M12_CpaHistoryGetdataById"
                Dim wsM12_Cpa As New m12_cpa_history
                hasil = wsM12_Cpa.M12_CpaHistoryGetdataById(param)


                'M12_POS_TYPE
            Case "M12_Pos_TypeSimpan"
                Dim wsM12_PosType As New m12_pos_type
                hasil = wsM12_PosType.M12_Pos_TypeSimpan(param)
            Case "M12_Pos_TypeGetdataById"
                Dim wsM12_PosType As New m12_pos_type
                hasil = wsM12_PosType.M12_Pos_TypeGetdataById(param)
            Case "M12_Pos_TypeSearch"
                Dim wsM12_PosType As New m12_pos_type
                hasil = wsM12_PosType.M12_Pos_TypeSearch(param)
            Case "M12_Pos_TypeDelete"
                Dim wsM12_PosType As New m12_pos_type
                hasil = wsM12_PosType.M12_Pos_TypeDelete(param)
            Case "M12_Pos_TypeCekId"
                Dim wsM12_PosType As New m12_pos_type
                hasil = wsM12_PosType.M12_Pos_TypeCekId(param)
            Case "M12_Pos_TypeTerkait"
                Dim wsM12_PosType As New m12_pos_type
                hasil = wsM12_PosType.M12_Pos_TypeTerkait(param)
            Case "M12_Pos_TypeHistorySimpan"
                Dim wsM12_PosTypeHistory As New m12_pos_type_history
                hasil = wsM12_PosTypeHistory.M12_Pos_TypeHistorySimpan(param)
            Case "M12_Pos_TypeHistorySearch"
                Dim wsM12_PosTypeHistory As New m12_pos_type_history
                hasil = wsM12_PosTypeHistory.M12_Pos_TypeHistorySearch(param)

            Case "CdM12_Pos_Type"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM12_Pos_Type(param)
            Case "CdM12_MemberSlip"
                Dim wsM0_Caridata As New m0_caridata
                hasil = wsM0_Caridata.CdM12_MemberSlip(param)

            Case "M12_PpvTakedataSearch"
                Dim wsM12_PPV As New m12_ppv
                hasil = wsM12_PPV.M12_PpvTakedataSearch(param)
            Case "M12_PpvSimpan"
                Dim wsM12_PPV As New m12_ppv
                hasil = wsM12_PPV.M12_PpvSimpan(param)
            Case "M12_PpvSearch"
                Dim wsM12_PPV As New m12_ppv
                hasil = wsM12_PPV.M12_PpvSearch(param)
            Case "M12_PpvUpdateStatus"
                Dim wsM12_PPV As New m12_ppv
                hasil = wsM12_PPV.M_12_PpvUpdateStatus(param)
            Case "M12_PpvGetdataById"
                Dim wsM12_PPV As New m12_ppv
                hasil = wsM12_PPV.M12_PpvGetdataById(param)
            Case "M12_PpvDelete"
                Dim wsM12_PPV As New m12_ppv
                hasil = wsM12_PPV.M12_PpvDelete(param)

            Case "M12_SiCreateFile"
                Dim wsM12_PPV As New m12_upload
                hasil = wsM12_PPV.M12_SiCreateFile(param)
            Case "M12_ExecuteDbFileAuto"
                Dim wsM12_PPV As New m12_upload
                hasil = wsM12_PPV.M12_ExecuteDbFileAuto(param)
            Case "M12_SiUploadData"
                Dim wsM12_PPV As New m12_upload
                hasil = wsM12_PPV.M12_SiUploadData(param)
                'new
            Case "M12_SITakeData"
                Dim wsM12_PPV As New m12_upload
                hasil = wsM12_PPV.M12_SITakeData(param)
            Case "M12_SiUploadDataNew"
                Dim wsM12_PPV As New m12_upload
                hasil = wsM12_PPV.M12_SiUploadData(param)
            Case "M12_InsertSIUtama"
                Dim wsM12_PPV As New m12_upload
                hasil = wsM12_PPV.M12_InsertSIUtama(param)
            Case "M12_InsertSIDetail"
                Dim wsM12_PPV As New m12_upload
                hasil = wsM12_PPV.M12_InsertSIDetail(param)
            Case "M12_InsertSIPay"
                Dim wsM12_PPV As New m12_upload
                hasil = wsM12_PPV.M12_InsertSIPay(param)
            Case "M12_DeleteSIPenampung"
                Dim wsM12_PPV As New m12_upload
                hasil = wsM12_PPV.M12_DeleteSIPenampung(param)
            Case "M12_DeleteSIPenampungNew"
                Dim wsM12_PPV As New m12_upload
                hasil = wsM12_PPV.M12_DeleteSIPenampungNew(param)
            Case "M12_InsertItemTransaction"
                Dim wsM12_PPV As New m12_upload
                hasil = wsM12_PPV.M12_InsertItemTransaction(param)
            Case "M12_CalculatingStock"
                Dim wsM12_PPV As New m12_upload
                hasil = wsM12_PPV.M12_CalculatingStock(param)
            Case "M12_UpdateGlobalStock"
                Dim wsM12_PPV As New m12_upload
                hasil = wsM12_PPV.M12_UpdateGlobalStock(param)



                '*********************************** ELSE '***********************************

            Case "rekapbarangkonsinyasi"

                Dim tglAwal As String = "2015-06-11"
                Dim tglAkhir As String = "2015-06-20"

                'AMBIL DATA BARANG MASUK (IB, RS, SA MASUK)
                Dim dtMasuk As DataTable = AsDataTableAmbilDariDB("SELECT it.id, it.gudang, it.jenismutasi, it.sumber, it.idutama, it.iddetail, it.notransaksi, it.tgl, it.idbarang, i.bkode, it.namabarang, it.jmlbarang, it.satuanbarang, i.bhargajual1 as harga, 0 as _penjualan, 0 as _retur, 0 as _penyesuaian, it.jmlbarang as saldoawal, 0 as penjualan, 0 as retur, 0 as penyesuaian, it.jmlbarang as saldoakhir, 0 as totalpenjualan FROM m1_item_transaction it JOIN m1_item i ON it.idbarang = i.bid AND ( (it.sumber = 'RS' AND it.jenismutasi = 1) OR (it.sumber = 'IB' AND it.jenismutasi = 1) OR (it.sumber = 'SA' AND it.jenismutasi = 1) ) AND it.tgl <= '" & tglAkhir & "' ORDER BY it.gudang, it.tgl, it.sumber, it.idutama, it.notransaksi, it.iddetail, it.idbarang")
                If dtMasuk.Rows.Count > 0 Then

                    'AMBIL DATA MUTASI BARANG (SI, SR, SA KELUAR)
                    Dim dtMutasi As DataTable = AsDataTableAmbilDariDB("SELECT it.id, it.gudang, it.jenismutasi, it.sumber, it.idutama, it.iddetail, it.notransaksi, it.tgl, it.idbarang, it.namabarang, it.jmlbarang, it.satuanbarang FROM m1_item_transaction it WHERE ( (it.sumber = 'SI' AND it.jenismutasi = 0) OR (it.sumber = 'SR' AND it.jenismutasi = 1) OR (it.sumber = 'SA' AND it.jenismutasi = 0) ) AND it.tgl <= '" & tglAkhir & "' ORDER BY it.gudang, it.tgl, it.sumber, it.idutama, it.notransaksi, it.iddetail, it.idbarang")
                    If dtMutasi.Rows.Count > 0 Then

                        Dim mtGudang As String = "", mtJenismutasi As String = 0, mtSumber As String = "", mtIdbarang As String = 0
                        Dim mtJmlbarang As Double = 0, mtTgl As String = "1900-01-01"
                        Dim dtFilter As New DataTable

                        Dim ms_Penjualan As Double = 0, ms_Retur As Double = 0, ms_Penyesuaian As Double = 0
                        Dim msSaldoAwal As Double = 0, msPenjualan As Double = 0, msRetur As Double = 0, msHarga As Double = 0
                        Dim msPenyesuaian As Double = 0, msSaldoAkhir As Double = 0, msTotalPenjualan As Double = 0
                        Dim msId As String = 0, msJmlbarang As Double = 0

                        Dim ms_PenjualanMinRetur As Double = 0, msPenjualanMinRetur As Double = 0

                        'PERULANGAN SEBANYAK DATA MUTASI
                        For Each mt As DataRow In dtMutasi.Rows

                            mtGudang = mt("gudang") : mtJenismutasi = mt("jenismutasi") : mtSumber = mt("sumber")
                            mtIdbarang = mt("idbarang") : mtJmlbarang = Double.Parse(mt("jmlbarang"))
                            mtTgl = AsFormatTanggal(mt("tgl"))

                            If mtSumber = "SR" Then
                                'JIKA MUTASI SR MAKA AMBIL DATA TANPA FILTER SALDO
                                dtFilter = AsDataTableFilterSortDt(dtMasuk, "gudang = '" & mtGudang & "' AND idbarang = '" & mtIdbarang & "'", "gudang, tgl, sumber, idutama, notransaksi, iddetail, idbarang")
                            Else
                                'JIKA BUKA SR MAKA AMBIL DATA YANG SALDONYA MASIH ADA
                                dtFilter = AsDataTableFilterSortDt(dtMasuk, "gudang = '" & mtGudang & "' AND idbarang = '" & mtIdbarang & "' AND saldoakhir > 0", "gudang, tgl, sumber, idutama, notransaksi, iddetail, idbarang")
                            End If

                            If dtFilter.Rows.Count > 0 Then
                                'PERULANGAN SEBANYAK DATA BARANG MASUK, EXT FOR JIKA JML MUTASI SUDAH TERPENUHI
                                For Each ms As DataRow In dtFilter.Rows

                                    msId = ms("id") : msJmlbarang = ms("jmlbarang")

                                    ms_Penjualan = Double.Parse(ms("_penjualan")) : ms_Retur = Double.Parse(ms("_retur"))
                                    ms_Penyesuaian = Double.Parse(ms("_penyesuaian"))

                                    msSaldoAwal = Double.Parse(ms("saldoawal")) : msPenjualan = Double.Parse(ms("penjualan"))
                                    msRetur = Double.Parse(ms("retur")) : msPenyesuaian = Double.Parse(ms("penyesuaian"))
                                    msSaldoAkhir = Double.Parse(ms("saldoakhir")) : msHarga = Double.Parse(ms("harga"))


                                    'JIKA TGL MUTASI < TGL AWAL --> MAKA IKUT KOLOM SEBELUM SALDO AWAL
                                    If mtTgl < tglAwal Then
                                        If mtSumber = "SI" Then
                                            If msSaldoAwal < mtJmlbarang Then
                                                ms_Penjualan = ms_Penjualan + msSaldoAwal
                                                mtJmlbarang = mtJmlbarang - msSaldoAwal
                                            Else
                                                ms_Penjualan = ms_Penjualan + mtJmlbarang
                                                mtJmlbarang = mtJmlbarang - mtJmlbarang
                                            End If

                                        ElseIf mtSumber = "SA" Then
                                            If msSaldoAwal < mtJmlbarang Then
                                                ms_Penyesuaian = ms_Penyesuaian + msSaldoAwal
                                                mtJmlbarang = mtJmlbarang - msSaldoAwal
                                            Else
                                                ms_Penyesuaian = ms_Penyesuaian + mtJmlbarang
                                                mtJmlbarang = mtJmlbarang - mtJmlbarang
                                            End If

                                        ElseIf mtSumber = "SR" Then
                                            ms_PenjualanMinRetur = ms_Penjualan - ms_Retur
                                            If ms_PenjualanMinRetur < mtJmlbarang Then
                                                ms_Retur = ms_Retur + ms_PenjualanMinRetur
                                                mtJmlbarang = mtJmlbarang - ms_PenjualanMinRetur
                                            Else
                                                ms_Retur = ms_Retur + mtJmlbarang
                                                mtJmlbarang = mtJmlbarang - mtJmlbarang
                                            End If

                                        End If


                                        'JIKA TGL MUTASI BETWEEN TGLAWAL DAN TGLAKHIR --> MAKA IKUT KOLOM MUTASI
                                    Else
                                        If mtSumber = "SI" Then
                                            If msSaldoAkhir < mtJmlbarang Then
                                                msPenjualan = msPenjualan + msSaldoAkhir
                                                mtJmlbarang = mtJmlbarang - msSaldoAkhir
                                            Else
                                                msPenjualan = msPenjualan + mtJmlbarang
                                                mtJmlbarang = mtJmlbarang - mtJmlbarang
                                            End If

                                        ElseIf mtSumber = "SA" Then
                                            If msSaldoAkhir < mtJmlbarang Then
                                                msPenyesuaian = msPenyesuaian + msSaldoAkhir
                                                mtJmlbarang = mtJmlbarang - msSaldoAkhir
                                            Else
                                                msPenyesuaian = msPenyesuaian + mtJmlbarang
                                                mtJmlbarang = mtJmlbarang - mtJmlbarang
                                            End If

                                        ElseIf mtSumber = "SR" Then
                                            msPenjualanMinRetur = msPenjualan - msRetur
                                            If msPenjualanMinRetur < mtJmlbarang Then
                                                msRetur = msRetur + msPenjualanMinRetur
                                                mtJmlbarang = mtJmlbarang - msPenjualanMinRetur
                                            Else
                                                msRetur = msRetur + mtJmlbarang
                                                mtJmlbarang = mtJmlbarang - mtJmlbarang
                                            End If

                                        End If

                                    End If

                                    'HITUNG SALDO
                                    msSaldoAwal = msJmlbarang - ms_Penjualan + ms_Retur - ms_Penyesuaian
                                    msSaldoAkhir = msSaldoAwal - msPenjualan + msRetur - msPenyesuaian
                                    msTotalPenjualan = msPenjualan * msHarga

                                    ''UPDATE DATA BARANG MASUK
                                    AsDataTableUpdateData(dtMasuk, "id = '" & msId & "'", "_penjualan~_retur~_penyesuaian~saldoawal~penjualan~retur~penyesuaian~saldoakhir~totalpenjualan", ms_Penjualan & "~" & ms_Retur & "~" & ms_Penyesuaian & "~" & msSaldoAwal & "~" & msPenjualan & "~" & msRetur & "~" & msPenyesuaian & "~" & msSaldoAkhir & "~" & msTotalPenjualan)

                                    If mtJmlbarang = 0 Then
                                        Exit For
                                    End If

                                Next

                            End If

                        Next

                    End If

                    'AsMemcached.SetCache("rekapbarangkonsinyasi", dtMasuk)
                    hasil = dtMasuk.Rows.Count

                End If


            Case "tes"

                hasil = AsFormatTanggal("2019-01-22 05:14:59","yyyy-MM-dd Hh:mm:ss")

            Case Else
                hasil = paket & sptSubParam & "0" & sptSubParam & "Invalid packet." & sptSubParam & "0" & sptSubParam & sptParam & "0" & _
                    sptSubParam & "0" & sptSubParam & "0" & sptSubParam & "0" & sptParam : GoTo selesai
        End Select

selesai:
        Return hasil
    End Function

    <WebMethod(Description:="Upload a single file to web server. (Data : filePacket, fileExtention)")> _
    Public Function UploadFile(ByVal param As String, ByVal Content As Byte()) As String

        Dim paramSplit(6) As String     'WebsiteAccessKey(0), paket(1), paging(2), userid(3), isUpdate(4), data(5)
        Dim pagingSplit(6) As String    'pageNumber(0), itemLimit(1), strFilter(2), strSort(3), formatTgl(4), formatTglWaktu(5)
        Dim dataUtama() As String

        Dim result(5) As String         'target(0), success(1), errmessage(2), errstep(3), idtransaksi(4)
        Dim resultPaging(5) As String   'ispaging(0), isNext(1), isPrev(2), countPage(3), countRow(4)

        Dim userid As String = "", wsResult As String = "", search As String = "", myPath As String = ""
        Dim filePaket As String = "", fileExtention As String = "", fileNama As String = "", sql As String = "", sqlhapus As String = ""
        Dim strResult, strResultPaging, strResultData As String
        Dim formatTgl As String = "", formatTglWaktu As String = ""

        Dim objFstream As FileStream
        'objFstream = File.Open(myPath & fileNama, FileMode.Create, FileAccess.Write)
        Dim dtimport, dtdb As New DataTable

        'SET DEFAULT RESULT
        result(0) = System.Reflection.MethodBase.GetCurrentMethod.Name 'Mengambil nama method
        result(1) = 0 : result(2) = "" : result(3) = 0 : result(4) = 0

        'SET DEFAULT PAGING
        resultPaging(0) = 0 : resultPaging(1) = 0 : resultPaging(2) = 0 : resultPaging(3) = 0 : resultPaging(4) = 0


        'VALIDASI PARAMETER GLOBAL =========================================================
        'SPILIT PARAM
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

        'Cek apakah WebsiteAccessKey valid
        Dim ClsValidKey As New ClsSecurity
        Dim validKey As RsValidKey
        validKey = ValidateKey(paramSplit(0))
        If Not validKey.success Then result(2) = validKey.errmessage : GoTo selesai

        '///Validasi Hak akses. Cek ModuleID dan MenuID
        If ClsValidKey.ApaBisaAkses(1, 1, 1) = False Then
            result(2) = "Access denied for insert/update data"
        End If
        'END OF VALIDASI WEBSITEACCESSKEY ==================================================


        'VALIDASI DAN SET USERID ===========================================================
        'CEK USERID
        If (Len(paramSplit(3)) = 0) Then
            result(2) = "userid can't be empty." : GoTo selesai
        End If

        'SET USERID
        userid = paramSplit(3)
        'END OF VALIDASI DAN SET USERID ====================================================


        'MAPPING DATA
        'filePaket, fileExtention + fsumber, fidtransaksi, fnamafile, fcatatan, fukuranfile, ftanggal, finputuser, finputtgl, fidtransaksi2, fdefault
        'filePaket(0), fileExtention(1) + fsumber(2), fidtransaksi(3), fnamafile(4), fcatatan(5), fukuranfile(6), ftanggal(7), finputuser(8), finputtgl(9), fidtransaksi2(10), fdefault(11)


        'VALIDASI DAN SET DATA =============================================================
        'SPILIT PARAMETER DATA
        dataUtama = paramSplit(5).Split(sptField)

        'CEK ARRAY DATA
        If (dataUtama.Length <> 12) Then
            result(2) = "Invalid data parameter." : GoTo selesai
        End If

        'CEK filePaket
        If Len(dataUtama(0)) = 0 Then
            result(2) = "filePacket can't be empty." : GoTo selesai
        Else
            filePaket = dataUtama(0).ToString
        End If

        'SET NAMA FOLDER DAN TABLE =================================================
        Dim folderName As String = "", tableName As String = ""

        'PAKET UNTUK IMPORT DATA
        If paramSplit(1) = "importdata" Then
            'fileNama = RandomString(5) + fileExtention
            fileNama = dataUtama(4).ToString
            myPath = HttpContext.Current.Server.MapPath("~/") & "importdata\files\"

            'PAKET UNTUK M0
        ElseIf filePaket = "m0_user" Or filePaket = "m0_language" Or filePaket = "m0_ScreenShot" _
            Or filePaket = "PosCategory" Or filePaket = "PosVoucher" Then
            folderName = "f0"

            'PAKET UNTUK M1
        ElseIf filePaket = "ContactCat" Or filePaket = "CustomerCat" Or filePaket = "SalesmanCat" Or filePaket = "Contact" _
            Or filePaket = "ItemLocation" Or filePaket = "ItemCat" Or filePaket = "ItemType" Or filePaket = "Unit" _
            Or filePaket = "Item" Or filePaket = "Coa" Or filePaket = "Branch" Or filePaket = "Location" Or filePaket = "Warehouse" _
            Or filePaket = "Division" Or filePaket = "Subdivision" Or filePaket = "Project" Or filePaket = "CostCenter" Or filePaket = "Terms" _
            Or filePaket = "Tax" Or filePaket = "Currency" Or filePaket = "Bank" Or filePaket = "TransNote" Or filePaket = "Country" _
            Or filePaket = "Province" Or filePaket = "City" Or filePaket = "Area" Or filePaket = "Expedition" Or filePaket = "TypeSA" _
            Or filePaket = "OtherCost" Or filePaket = "Other" Or filePaket = "DetailTransNote" Or filePaket = "ProductionCat" _
            Or filePaket = "WorkingEstimate" Or filePaket = "SupplierCat" Or filePaket = "AssetCatTax" Or filePaket = "AssetCat" _
            Or filePaket = "Asset" Or filePaket = "ItemHauling" Or filePaket = "CheckingCat" Or filePaket = "SellingPoint" Then
            folderName = "f1" : tableName = "M1_Files"

            'PAKET UNTUK M2
        ElseIf filePaket.ToUpper = "CR" Or filePaket.ToUpper = "CD" Or filePaket.ToUpper = "RM" Or filePaket.ToUpper = "SM" Or filePaket.ToUpper = "GJ" Or filePaket.ToUpper = "AJ" _
            Or filePaket.ToUpper = "RG" Or filePaket.ToUpper = "SG" Or filePaket.ToUpper = "RGC" Or filePaket.ToUpper = "SGC" Or filePaket.ToUpper = "CB" Or filePaket.ToUpper = "RV" _
            Or filePaket.ToUpper = "TJ" Or filePaket.ToUpper = "JM" Then
            folderName = "f2" : tableName = "M2_Files"

            'PAKET UNTUK M3
        ElseIf filePaket.ToUpper = "MR" Or filePaket.ToUpper = "TS" Or filePaket.ToUpper = "RS" Or filePaket.ToUpper = "SA" Or filePaket.ToUpper = "SP" Or filePaket.ToUpper = "PA" Or filePaket.ToUpper = "IB" _
            Or filePaket.ToUpper = "RF" Or filePaket.ToUpper = "DC" Then
            folderName = "f3" : tableName = "M3_Files"

            'PAKET UNTUK M4
        ElseIf filePaket.ToUpper = "PR" Or filePaket.ToUpper = "CS" Or filePaket.ToUpper = "RQ" Or filePaket.ToUpper = "BS" Or filePaket.ToUpper = "PO" Or filePaket.ToUpper = "AP" _
            Or filePaket.ToUpper = "IPC" Or filePaket.ToUpper = "GRN" Or filePaket.ToUpper = "RI" Or filePaket.ToUpper = "DNR" Or filePaket.ToUpper = "PRT" Or filePaket.ToUpper = "VPP" _
            Or filePaket.ToUpper = "VP" Or filePaket.ToUpper = "PP" Then
            folderName = "f4" : tableName = "M4_Files"

            'PAKET UNTUK M5
        ElseIf filePaket.ToUpper = "SQ" Or filePaket.ToUpper = "SO" Or filePaket.ToUpper = "AS" Or filePaket.ToUpper = "PL" Or filePaket.ToUpper = "DO" Or filePaket.ToUpper = "DR" _
            Or filePaket.ToUpper = "IP" Or filePaket.ToUpper = "RP" _
            Or filePaket.ToUpper = "PI" Or filePaket.ToUpper = "SI" Or filePaket.ToUpper = "RNR" Or filePaket.ToUpper = "SR" Or filePaket.ToUpper = "IC" Or filePaket.ToUpper = "PV" Then
            folderName = "f5" : tableName = "M5_Files"

            'PAKET UNTUK M6
        ElseIf filePaket.ToUpper = "BOM" Or filePaket.ToUpper = "PDR" Or filePaket.ToUpper = "WO" Or filePaket.ToUpper = "MRS" Or filePaket.ToUpper = "MRN" Or filePaket.ToUpper = "PD" Then
            folderName = "f6" : tableName = "M6_Files"

            'PAKET UNTUK M7
        ElseIf filePaket.ToUpper = "DA" Then
            folderName = "f7" : tableName = "M7_Files"

        Else
            result(2) = "Invalid file packet." : GoTo selesai
        End If
        'END OF SET NAMA FOLDER DAN TABLE ==========================================


        'CEK fileExtention
        If Len(dataUtama(1)) = 0 Then
            result(2) = "fileExtention can't be empty." : GoTo selesai
        Else
            fileExtention = dataUtama(1).ToString
        End If


        'JIKA lampiran untuk transaksi maka cek parameter 2 s/d 9
        'CEK fsumber
        If Len(dataUtama(2)) = 0 Then
            result(2) = "fsumber can't be empty." : GoTo selesai
        End If
        If Len(dataUtama(2)) > 15 Then
            result(2) = "fsumber should not be more than 15 character." : GoTo selesai
        End If

        'CEK fidtransaksi
        If Not folderName = "f1" Then
            If IsNumeric(dataUtama(3)) = False Then
                result(2) = "fidtransaksi required numeric." : GoTo selesai
            End If
        Else
            If Len(dataUtama(3)) > 100 Then
                result(2) = "fidtransaksi should not be more than 100 character." : GoTo selesai
            End If
        End If

        'CEK fnamafile
        If Len(dataUtama(4)) = 0 Then
            result(2) = "fnamafile can't be empty." : GoTo selesai
        End If
        If Len(dataUtama(4)) > 100 Then
            result(2) = "fnamafile should not be more than 100 character." : GoTo selesai
        End If

        'CEK fcatatan
        If Len(dataUtama(5)) > 250 Then
            result(2) = "fcatatan should not be more than 250 character." : GoTo selesai
        End If

        'CEK fukuranfile
        If Len(dataUtama(6)) = 0 Then
            result(2) = "fukuranfile can't be empty." : GoTo selesai
        End If
        If Len(dataUtama(6)) > 25 Then
            result(2) = "fukuranfile should not be more than 25 character." : GoTo selesai
        End If

        'CEK ftanggal
        If IsDate(dataUtama(7)) = False Then
            result(2) = "ftanggal required date." : GoTo selesai
        End If

        'CEK finputuser
        If IsNumeric(dataUtama(8)) = False Then
            result(2) = "finputuser required numeric." : GoTo selesai
        End If

        'CEK finputtgl
        If IsDate(dataUtama(9)) = False Then
            result(2) = "finputtgl required date." : GoTo selesai
        End If

        'CEK fidtransaksi2
        If Len(dataUtama(10)) > 100 Then
            result(2) = "fidtransaksi2 should not be more than 100 character." : GoTo selesai
        End If

        'CEK fdefault
        If IsNumeric(dataUtama(11)) = False Then
            result(2) = "fdefault required numeric." : GoTo selesai
        End If
        'END OF VALIDASI DAN SET DATA ======================================================


        'SET PATH FOLDER TUJUAN DAN NAMA FILE ==============================================
        'PAKET UNTUK m0_user
        If filePaket = "m0_user" Then
            If IsNumeric(userid) Then
                myPath = HttpContext.Current.Server.MapPath("~/") & "files\" & folderName & "\user\"
                fileNama = filePaket & "-" & userid & "." & fileExtention
            Else
                result(2) = "Userid required numeric." : GoTo selesai
            End If

            'PAKET UNTUK m0_ScreenShot
        ElseIf filePaket = "m0_ScreenShot" Then
            myPath = HttpContext.Current.Server.MapPath("~/") & "report\temp\"
            fileNama = FixQuotes(dataUtama(4)) & "-" & f_Random(4) & "." & fileExtention

            'PAKET UNTUK m0_language
        ElseIf filePaket = "m0_language" Then
            myPath = HttpContext.Current.Server.MapPath("~/") & "files\" & folderName & "\language\"
            fileNama = filePaket & "-" & FixQuotes(dataUtama(4)) & "." & fileExtention

            'PAKET UNTUK SELAIN IMPORT DATA
        ElseIf paramSplit(1) <> "importdata" Then
            myPath = HttpContext.Current.Server.MapPath("~/") & "files\" & folderName & "\" & filePaket & "\"
            fileNama = dataUtama(4).ToString
            If folderName = "f1" Then
                sql = "Insert into " & tableName & " (fsumber, fidtransaksi, fnamafile, fcatatan, fukuranfile, ftanggal, finputuser, finputtgl, fidtransaksi2, fdefault) values('" & FixQuotes(dataUtama(2)) & "', '" & dataUtama(3) & "', '" & FixQuotes(dataUtama(4)) & "', '" & FixQuotes(dataUtama(5)) & "', '" & FixQuotes(dataUtama(6)) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(7))) & "', '" & dataUtama(8) & "',  NOW(), '" & dataUtama(10) & "', '" & dataUtama(11) & "')"
            Else
                sql = "Insert into " & tableName & " (fsumber, fidtransaksi, fnamafile, fcatatan, fukuranfile, ftanggal, finputuser, finputtgl) values('" & FixQuotes(dataUtama(2)) & "', '" & dataUtama(3) & "', '" & FixQuotes(dataUtama(4)) & "', '" & FixQuotes(dataUtama(5)) & "', '" & FixQuotes(dataUtama(6)) & "', '" & FixQuotes(AsFormatTanggal(dataUtama(7))) & "', '" & dataUtama(8) & "',  NOW())"
            End If

        End If
        'END OF SET PATH FOLDER TUJUAN DAN NAMA FILE =======================================


        'PROSES UPLOAD FILE ================================================================
        Try
            'BUAT FOLDER JIKA TIDAK DITEMUKAN
            If (Not System.IO.Directory.Exists(myPath)) Then System.IO.Directory.CreateDirectory(myPath)

            'CEK NAMA FILE
            Dim newFileName As String = fileNama
            Dim filelocation As String = myPath & fileNama
            Dim fileExtension = IO.Path.GetExtension(filelocation)
            Dim fileName = IO.Path.GetFileNameWithoutExtension(filelocation)
            Dim folder = IO.Path.GetDirectoryName(filelocation)
            Dim counter = 0
            If Not folderName = "f0" Then
                While IO.File.Exists(filelocation)
                    counter += 1
                    newFileName = String.Format("{0}_{1}{2}", fileName, counter, fileExtension)
                    filelocation = IO.Path.Combine(folder, newFileName)
                End While
            End If
            'REPLACE NAMA FILE JIKA SUDAH ADA
            fileNama = newFileName
            sql = sql.Replace(dataUtama(4), newFileName)
            dataUtama(4) = newFileName

            'WRITE FILE
            objFstream = File.Open(myPath & fileNama, FileMode.Create, FileAccess.Write)
            Try
                Dim lngLen As Long = Content.Length
                objFstream.Write(Content, 0, CInt(lngLen))
                objFstream.Flush()
                objFstream.Close()

                search = fileNama

            Catch exc As System.UnauthorizedAccessException
                result(2) = "Error uploading file. Desc : " & exc.Message & ". #1" : GoTo selesai

            Catch exc As Exception
                result(2) = "Error uploading file. Desc : " & exc.Message & ". #2" : GoTo selesai

            Finally
                If Not objFstream Is Nothing Then
                    objFstream.Close()
                End If

            End Try
        Catch ex As Exception
            result(2) = Err.Description & ". #3" : GoTo selesai
        End Try

        'END OF PROSES UPLOAD FILE =========================================================


        'PROSES IMPORT DATA ================================================================
        If paramSplit(1) = "importdata" Then

            ''Set folder dan nama file excel
            'Dim sPath As String = myPath + fileNama

            ''PROSES AMBIL SHEET EXCEL ------------------------
            'Dim rsSheet(3) As String 'result, sheet, mapping
            'rsSheet = GetExcelSheet(sPath).Split(sptParam)
            'If Len(rsSheet(0)) > 0 Then
            '    result(2) = rsSheet(0) : GoTo selesai
            'Else
            '    search = rsSheet(1) & sptParam & rsSheet(2)
            'End If
            ''END OF PROSES AMBIL SHEET EXCEL -----------------

            result(1) = 1

            ''PROSES READ FILE EXCEL -------------------------
            ''DataTable untuk menampung data dari excel
            'Dim dtExcelData As New DataTable

            ''Panggil fungsi ReadExcelFile untuk membaca file excel dan ditampung pada datatable
            'Dim rsReadExcel As String = ""
            'rsReadExcel = ReadExcelFile(sPath, dtExcelData)
            'If Len(rsReadExcel) > 0 Then
            '    result(2) = rsReadExcel : GoTo selesai
            'End If
            ''END OF PROSES READ FILE EXCEL ------------------


            ''PROSES IMPORT KE TABEL -------------------------
            'Dim dtTableData As DataTable
            'Dim strImport As String = "", strField As String = "", strValues As String = ""

            ''AMBIL STRUKTUR TABEL TUJUAN IMPORT
            'dtTableData = AsDataTableAmbilDariDB("SHOW COLUMNS FROM " & filePaket) 'Field, Type, Null, Key, Default, Extra
            ''BUAT STRUKTUR NAMA FIELD QUERY INSERT
            'If dtTableData.Rows.Count > 0 Then
            '    For Each dr As DataRow In dtTableData.Rows
            '        strField = IIf(Len(strField.ToString) = 0, "", strField & ", ")
            '        strField = String.Concat(strField, dr("Field"))
            '    Next
            '    If Len(strField) > 0 Then strField = "(" & strField & ")"

            'Else
            '    result(2) = "Table name '" & filePaket & "' doesn't exist in database." : GoTo selesai

            'End If


            ''AMBIL VALUES DARI DATATABLE DATA EXCEL YANG AKAN DIIMPORT
            'If dtExcelData.Rows.Count > 0 Then
            '    Dim sptDataTipe() As String, sptDataLength() As String
            '    Dim namaField As String = "", dataTipe As String = "", dataLength As String = ""
            '    Dim AllowNull As String = "", dataDefault As String = ""

            '    'PERULANGAN SEBANYAK ROW DATA EXCEL
            '    For iRow = 0 To dtExcelData.Rows.Count - 1
            '        strValues = IIf(Len(strValues.ToString) = 0, "", strValues & vbNewLine & ", ")
            '        strValues = String.Concat(strValues, "(")

            '        'PERULANGAN KOLOM SESUAI FIELD STRUKTUR TABEL
            '        For iField = 0 To dtTableData.Rows.Count - 1

            '            'AMBIL NAMA FIELD, ALLOWNULL DAN DEFAULT VALUE
            '            namaField = dtTableData.Rows(iField)("Field").ToString
            '            AllowNull = dtTableData.Rows(iField)("Null").ToString
            '            dataDefault = FxDB(dtTableData.Rows(iField)("Default").ToString, "")

            '            'AMBIL TIPEDATA DAN LENGTH VALUE
            '            sptDataTipe = dtTableData.Rows(iField)("Type").ToString.Split("(")
            '            If sptDataTipe.Length > 1 Then
            '                sptDataLength = sptDataTipe(1).Split(")")
            '            Else
            '                sptDataLength = "".Split("")
            '            End If
            '            dataTipe = sptDataTipe(0) : dataLength = sptDataLength(0)

            '            'SET DEFAULT VALUE
            '            If Len(dtExcelData.Rows(iRow)(iField)) = 0 Then
            '                If Len(dataDefault) > 0 Then
            '                    dtExcelData.Rows(iRow)(iField) = dataDefault

            '                Else
            '                    '    NUMERIC
            '                    If dataTipe.Equals("tinyint") Or dataTipe.Equals("smallint") Or dataTipe.Equals("mediumint") Or _
            '                       dataTipe.Equals("int") Or dataTipe.Equals("integer") Or dataTipe.Equals("bigint") Or _
            '                       dataTipe.Equals("bit") Or dataTipe.Equals("real") Or dataTipe.Equals("double") Or _
            '                       dataTipe.Equals("float") Or dataTipe.Equals("decimal") Or dataTipe.Equals("numeric") Then
            '                        dtExcelData.Rows(iRow)(iField) = 0

            '                        'YEAR
            '                    ElseIf dataTipe.Equals("year") Then
            '                        dtExcelData.Rows(iRow)(iField) = "1900"

            '                        'DATE
            '                    ElseIf dataTipe.Equals("date") Then
            '                        dtExcelData.Rows(iRow)(iField) = "1900-01-01"

            '                        'TIME
            '                    ElseIf dataTipe.Equals("time") Then
            '                        dtExcelData.Rows(iRow)(iField) = "00:00:00"

            '                        'DATETIME
            '                    ElseIf dataTipe.Equals("timestamp") Or dataTipe.Equals("datetime") Then
            '                        dtExcelData.Rows(iRow)(iField) = "1971-01-01 00:00:00"

            '                    End If

            '                End If

            '            End If

            '            'CEK ALLOWNULL
            '            If AllowNull.Equals("NO") And Len(dtExcelData.Rows(iRow)(iField)) = 0 Then
            '                result(2) = " Column '" & namaField & "' cannot be null at row " & iRow + 1 & "." : GoTo selesai
            '            End If


            '            'VALIDASI TIPEDATA DAN LENGTH VALUE
            '            'tinyint, smallint, mediumint, int, integer, bigint, bit, real, double, float, decimal, numeric, 
            '            'char, varchar, date, time, year, timestamp, datetime, tinyblob, blob, mediumblob, longblob, 
            '            'tinytext, text, mediumtext, longtext, enum, set, binary, varbinary

            '            '    NUMERIC
            '            If dataTipe.Equals("tinyint") Or dataTipe.Equals("smallint") Or dataTipe.Equals("mediumint") Or _
            '               dataTipe.Equals("int") Or dataTipe.Equals("integer") Or dataTipe.Equals("bigint") Or _
            '               dataTipe.Equals("bit") Or dataTipe.Equals("real") Or dataTipe.Equals("double") Or _
            '               dataTipe.Equals("float") Or dataTipe.Equals("decimal") Or dataTipe.Equals("numeric") Or _
            '               dataTipe.Equals("year") Then
            '                If IsNumeric(dtExcelData.Rows(iRow)(iField)) = False Then
            '                    result(2) = "Incorrect " & dataTipe & " value: '" & dtExcelData.Rows(iRow)(iField) & "' for column '" & namaField & "' at row " & iRow + 1 & "." : GoTo selesai
            '                End If

            '                'DATE
            '            ElseIf dataTipe.Equals("date") Or dataTipe.Equals("time") Or _
            '               dataTipe.Equals("timestamp") Or dataTipe.Equals("datetime") Then
            '                If IsDate(dtExcelData.Rows(iRow)(iField)) = False Then
            '                    result(2) = "Incorrect " & dataTipe & " value: '" & dtExcelData.Rows(iRow)(iField) & "' for column '" & namaField & "' at row " & iRow + 1 & "." : GoTo selesai
            '                End If
            '                'FORMATTING TANGGAL
            '                If dataTipe.Equals("date") Then
            '                    dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(dtExcelData.Rows(iRow)(iField), "yyyy-MM-dd")
            '                ElseIf dataTipe.Equals("time") Then
            '                    dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(dtExcelData.Rows(iRow)(iField), "H:mm:ss")
            '                ElseIf dataTipe.Equals("timestamp") Then
            '                    dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(dtExcelData.Rows(iRow)(iField), "yyyy-MM-dd H:mm:ss")
            '                ElseIf dataTipe.Equals("datetime") Then
            '                    dtExcelData.Rows(iRow)(iField) = AsFormatTanggal(dtExcelData.Rows(iRow)(iField), "yyyy-MM-dd H:mm:ss")
            '                End If

            '                'SELAIN NUMERIC DAN TANGGAL
            '            Else
            '                'CEK LENGTH DATA
            '                If Len(dataLength) > 0 Then
            '                    If Len(dtExcelData.Rows(iRow)(iField)) > Double.Parse(dataLength) Then
            '                        result(2) = "Data too long for column '" & namaField & "' at row " & iRow + 1 & "." : GoTo selesai
            '                    End If
            '                End If

            '            End If


            '            'TAMBAHKAN VALUES QUERY SQL INSERT
            '            strValues = IIf(iField = 0, strValues, strValues & ", ")
            '            strValues = String.Concat(strValues, "'" & FixQuotes(dtExcelData.Rows(iRow)(iField)) & "'")

            '        Next

            '        strValues = String.Concat(strValues, ")")
            '    Next

            'Else
            '    result(2) = "No data were found to be imported." : GoTo selesai

            'End If


            ''PROSES BUAT QUERY SQL
            'If Len(strField) > 0 And Len(strValues) > 0 Then
            '    sql = "INSERT INTO " & filePaket & " " & strField & " VALUES " & strValues
            'End If
            ''END OF PROSES IMPORT KE TABEL ------------------

            'END OF PROSES IMPORT DATA =========================================================


            '            Dim conOleb As OleDbConnection
            '            Dim dtaOleb As OleDbDataAdapter
            '            Dim dts As DataSet, ketemu As Boolean = False
            '            Dim excel As String = myPath + fileNama

            '            'Create a new instance of connection and set the datasource value to excel's path
            '            conOleb = New OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + excel + ";Extended Properties=Excel 12.0;")
            '            dtaOleb = New OleDbDataAdapter("Select * From [data$]", conOleb)
            '            dts = New DataSet
            '            dtaOleb.Fill(dts, "[data$]")
            '            dtimport = dts.Tables("[data$]")
            '            conOleb.Close()

            '            'cek kolom
            '            dtdb = AsDataTableAmbilDariDB("SHOW COLUMNS FROM " + filePaket)
            '            If dtdb.Rows.Count > 0 Then
            '                For i = 0 To dtdb.Rows.Count - 1
            '                    For j = 0 To dtimport.Columns.Count - 1
            '                        ketemu = False
            '                        If dtdb.Rows(i)(0) = dtimport.Columns.Item(j).ColumnName Then
            '                            ketemu = True
            '                            GoTo goketemu
            '                        End If
            '                    Next
            'goketemu:
            '                    If (ketemu = False) Then
            '                        result(2) = "Unknown file " + dtdb.Rows(i)(0).ToString : GoTo selesai
            '                    End If
            '                Next

            '            Else
            '                result(2) = "Table name '" & filePaket & "' doesn't exist in database." : GoTo selesai
            '            End If
            'result(1) = 1


        ElseIf Not folderName = "f0" Then

            'PROSES SIMPAN KE DATABASE =========================================================
            Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
            Dim Trans As MySql.Data.MySqlClient.MySqlTransaction

            Con1 = New MySql.Data.MySqlClient.MySqlConnection(Application("As_ConStr1"))
            Con1.Open()

            '*** Start Transaction ***'  
            Trans = Con1.BeginTransaction(IsolationLevel.ReadCommitted)

            Dim dtupdate As New DataTable
            Dim rowUpdate As Integer = 0

            Try

                If Len(sql) > 0 Then
                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                    With objCmd
                        .Connection = Con1
                        .Transaction = Trans
                        .CommandType = CommandType.Text
                        .CommandText = sql
                    End With
                    objCmd.ExecuteNonQuery()
                End If

                Trans.Commit()  '*** Commit Transaction ***'
                result(1) = 1
                result(2) = ""
                result(3) = 0
                result(4) = result(4)

                'VALIDASI PARAMETER PAGING =========================================================
                'SPLIT PARAMETER PAGING
                pagingSplit = paramSplit(2).Split(sptSubParam)

                'CEK ARRAY PAGING
                If (pagingSplit.Length <> 6) Then
                    result(2) = "Invalid paging parameter." : GoTo selesai
                End If

                'CEK PAGENUMBER
                If (IsNumeric(pagingSplit(0)) = False) Then
                    result(2) = "pageNumber required numeric." : GoTo selesai
                End If

                'CEK ITEMLIMIT
                If (IsNumeric(pagingSplit(1)) = False) Then
                    result(2) = "itemLimit required numeric." : GoTo selesai
                End If

                'CEK FORMATTGL
                If Len(pagingSplit(4)) = 0 Then
                    formatTgl = "yyyy-MM-dd"
                Else
                    formatTgl = pagingSplit(4)
                End If

                'CEK FORMATTGLWAKTU
                If Len(pagingSplit(5)) = 0 Then
                    formatTglWaktu = "yyy-MM-dd H:mm:ss"
                Else
                    formatTglWaktu = pagingSplit(5)
                End If
                'END OF VALIDASI PARAMETER PAGING ==================================================


                'AMBIL DATA ========================================================================
                Dim paramSearch As String = ""
                Dim filter As String = "fsumber='" & FixQuotes(dataUtama(2)) & "' AND fidtransaksi='" & dataUtama(3) & "'"
                'tambahkan filter idtransaksi2 utk master data yg memiliki 2 primary key
                If folderName = "f1" And Len(dataUtama(10)) > 0 Then filter = filter & " AND fidtransaksi2='" & dataUtama(10) & "'"

                If paramSplit(1) <> "importdata" Then

                    Select Case folderName
                        Case "f1"
                            Dim wsM1_Files As New m1_files
                            paramSearch = wsM1_Files.M1_FilesSearch(PostWsSearch(paramSplit(0), "M1_FilesSearch", pagingSplit(0), pagingSplit(1), filter, pagingSplit(3), formatTgl, formatTglWaktu))
                        Case "f2"
                            Dim wsM2_Files As New m2_files
                            paramSearch = wsM2_Files.M2_FilesSearch(PostWsSearch(paramSplit(0), "M2_FilesSearch", pagingSplit(0), pagingSplit(1), filter, pagingSplit(3), formatTgl, formatTglWaktu))
                        Case "f3"
                            Dim wsM3_Files As New m3_files
                            paramSearch = wsM3_Files.M3_FilesSearch(PostWsSearch(paramSplit(0), "M3_FilesSearch", pagingSplit(0), pagingSplit(1), filter, pagingSplit(3), formatTgl, formatTglWaktu))
                        Case "f4"
                            Dim wsM4_Files As New m4_files
                            paramSearch = wsM4_Files.M4_FilesSearch(PostWsSearch(paramSplit(0), "M4_FilesSearch", pagingSplit(0), pagingSplit(1), filter, pagingSplit(3), formatTgl, formatTglWaktu))
                        Case "f5"
                            Dim wsM5_Files As New m5_files
                            paramSearch = wsM5_Files.M5_FilesSearch(PostWsSearch(paramSplit(0), "M5_FilesSearch", pagingSplit(0), pagingSplit(1), filter, pagingSplit(3), formatTgl, formatTglWaktu))
                        Case "f6"
                            Dim wsM6_Files As New m6_files
                            paramSearch = wsM6_Files.M6_FilesSearch(PostWsSearch(paramSplit(0), "M6_FilesSearch", pagingSplit(0), pagingSplit(1), filter, pagingSplit(3), formatTgl, formatTglWaktu))
                        Case "f7"
                            Dim wsM7_Files As New m7_files
                            paramSearch = wsM7_Files.M7_FilesSearch(PostWsSearch(paramSplit(0), "M7_FilesSearch", pagingSplit(0), pagingSplit(1), filter, pagingSplit(3), formatTgl, formatTglWaktu))
                    End Select


                    Dim hasilSearch As New RsHasilWsSearch
                    hasilSearch = GetWsSearch(paramSearch)

                    result(1) = hasilSearch.success
                    result(2) = hasilSearch.errmessage

                    resultPaging(0) = hasilSearch.isPaging
                    resultPaging(1) = hasilSearch.isNext
                    resultPaging(2) = hasilSearch.isPrevious
                    resultPaging(3) = hasilSearch.countPage
                    resultPaging(4) = hasilSearch.countRow

                    search = hasilSearch.data

                End If
                'END OF AMBIL DATA =================================================================

            Catch ex As Exception

                Trans.Rollback() '*** RollBack Transaction ***'  
                result(1) = 0
                result(2) = "Transaction Rollback : " & ex.Message
                result(3) = 0
                result(4) = result(4)

            End Try

            objCmd = Nothing
            'END OF PROSES SIMPAN KE DATABASE ==================================================
        Else
            result(1) = 1
            result(2) = ""
            result(3) = 0
            result(4) = result(4)
        End If

selesai:
        If result(1) = 0 Then
            If Len(result(2)) = 0 Then result(2) = "Nomor : " & Err.Number & ". Sumber : " & Err.Source & ".  Fungsi : " & System.Reflection.MethodBase.GetCurrentMethod.Name & ". Uraian : " & Err.Description & ". "
        End If

        strResult = String.Join(sptSubParam, result)
        strResultPaging = String.Join(sptSubParam, resultPaging)
        'If paramSplit(1) = "importdata" Then
        '    Dim ColumnName As String = ""
        '    strResultData = ""
        '    For i = 0 To dtimport.Rows.Count - 1
        '        ' Pemisah baris
        '        If i > 0 Then
        '            strResultData += sptRow
        '        End If
        '        For j = 0 To dtimport.Columns.Count - 1
        '            ' Maping Kolom Nama
        '            If i = 0 Then
        '                ColumnName += dtimport.Columns(j).ColumnName + sptField
        '            End If
        '            'Content
        '            If j > 0 Then
        '                strResultData += sptField
        '            End If
        '            ' hasil di gabungkan content dan nama kolom
        '            strResultData += dtimport.Rows(i)(dtimport.Columns(j).ColumnName).ToString
        '        Next
        '    Next
        '    strResultData += sptParam + ColumnName.Substring(0, ColumnName.Length - 1)
        'Else
        strResultData = search
        'End If
        wsResult = String.Concat(strResult.Substring(0, strResult.Length - 1), sptParam, strResultPaging.Substring(0, strResultPaging.Length - 1), sptParam, strResultData)

        Return wsResult

    End Function

End Class