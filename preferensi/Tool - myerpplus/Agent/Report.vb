Imports System.IO
Imports Stimulsoft.Report
Imports Stimulsoft.Report.Dictionary
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports MySql.Data.MySqlClient
Imports Stimulsoft.Report.Components
Imports Stimulsoft.Base.Drawing
Imports Stimulsoft.Base
Imports System.Net.Mail

Public Class Report

    Public Sub generator(ByVal param() As String)
        Dim dt As DataTable, Query As String, s(2), q, ar(2), filter2, orderby2, groupby2 As String
        Dim LogoCompany As String, Alamat As String, Notlp As String
        Try
            'Parameter dari MSMQ
            'IDGenerate(0), WebAccessKey(1), Module(2), Menu(3), Item(4), Filter(5), OrderBy(6), GroupBy(7), RQuery(8), Extension(9), Param(10), userid(11), unama(12), IDTransaksi(13), Sumber(14), NamaPerusahaan(15), Watermark(18), ubahasa(19), filename(20)
            'Cek jumlah parameter

            If param.Length <> 22 Then
                SimpanLogToFile("Error 'Parameter' : Invalid key parameter.") : GoTo selesai
            End If

            'Validasi IDGenerate
            If Len(param(0)) = 0 Then
                SimpanLogToFile("Error 'Parameter' : IDGenerate can't be empty.") : GoTo selesai
            End If

            'Validasi WebAccessKey
            If Len(param(1)) = 0 Then
                SimpanLogToFile("Error 'Parameter' : WebAccessKey can't be empty.") : GoTo selesai
            End If

            'Validasi Module
            If IsNumeric(param(2)) = False Then
                SimpanLogToFile("Error 'Parameter' : Module required numeric.") : GoTo selesai
            End If

            'Validasi Menu
            If IsNumeric(param(3)) = False Then
                SimpanLogToFile("Error 'Parameter' : Menu required numeric.") : GoTo selesai
            End If

            'Validasi Item
            If IsNumeric(param(4)) = False Then
                SimpanLogToFile("Error 'Parameter' : Item required numeric.") : GoTo selesai
            End If

            'Validasi Filter
            'If Len(param(5)) = 0 Then
            '    SimpanLogToFile("Error 'Parameter' : Filter can't be empty.") : GoTo selesai
            'End If

            'Validasi OrderBy
            'If Len(param(6)) = 0 Then
            '    SimpanLogToFile("Error 'Parameter' : OrderBy can't be empty.") : GoTo selesai
            'End If

            'Validasi GroupBy
            'If Len(param(7)) = 0 Then
            '    SimpanLogToFile("Error 'Parameter' : GroupBy can't be empty.") : GoTo selesai
            'End If

            'Validasi RQuery
            If IsNumeric(param(8)) = False Then
                SimpanLogToFile("Error 'Parameter' : RQuery required numeric.") : GoTo selesai
            End If

            'Validasi Extension
            If IsNumeric(param(9)) = False Then
                SimpanLogToFile("Error 'Parameter' : Extension required numeric.") : GoTo selesai
            End If

            'Validasi Param
            'If Len(param(10)) = 0 Then
            '    SimpanLogToFile("Error 'Parameter' : Param can't be empty.") : GoTo selesai
            'End If

            'Validasi userid
            If IsNumeric(param(11)) = False Then
                SimpanLogToFile("Error 'Parameter' : userid required numeric.") : GoTo selesai
            End If

            'Validasi unama
            If Len(param(12)) = 0 Then
                SimpanLogToFile("Error 'Parameter' : unama can't be empty.") : GoTo selesai
            End If

            'Validasi IDTransaksi
            If IsNumeric(param(13)) = False Then
                SimpanLogToFile("Error 'Parameter' : IDTransaksi required numeric.") : GoTo selesai
            End If

            'Validasi Sumber
            'If Len(param(14)) = 0 Then
            '    SimpanLogToFile("Error 'Parameter' : Sumber can't be empty.") : GoTo selesai
            'End If

            'Validasi NamaPerusahaan
            If Len(param(15)) = 0 Then
                SimpanLogToFile("Error 'Parameter' : NamaPerusahaan can't be empty.") : GoTo selesai
            End If
            xstep = 0
            'Validasi Title
            If Len(param(16)) = 0 Then
                SimpanLogToFile("Error 'Parameter' : Title can't be empty.") : GoTo selesai
            End If

            'Validasi Kota TTD
            If Len(param(17)) = 0 Then
                SimpanLogToFile("Error 'Parameter' : KotaTTD can't be empty.") : GoTo selesai
            End If

            'Validasi FileName
            If Len(param(19)) = 0 Then
                SimpanLogToFile("Error 'Parameter' : ubahasa can't be empty.") : GoTo selesai
            End If

            'Validasi FileName
            If Len(param(20)) = 0 Then
                SimpanLogToFile("Error 'Parameter' : FileName can't be empty.") : GoTo selesai
            End If
            'Set Variabel dari parameter
            xstep = 1
            IDGenerate = param(0) : WebAccessKey = param(1) : Modul = param(2) : MenuReport = param(3)
            Item = param(4) : Filter = param(5) : OrderBy = param(6)
            GroupBy = param(7) : RQuery = param(8) : Extension = param(9)
            Param1 = param(10) : userid = param(11) : unama = param(12)
            IDTransaksi = param(13) : Sumber = param(14) : NamaPerusahaan = param(15)
            Title = param(16) : ubahasa = param(19)
            xstep = 2
            'Update M0_Msmq jadi status prosessing
            f_query("Update m0_msmq set progress = 1 WHERE id = '" & IDGenerate & "'")

            dt = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'company' AND (skode = 'LogoPerusahaan' OR skode = 'AlamatPerusahaan' OR skode = 'NoTlp') ORDER BY sgrup, skode, snama", strCon)
            If dt.Rows.Count <> 3 Then
                SimpanLogToFile("[Agent] Error generator : Data setting not found")
                GoTo selesai
            End If

            Alamat = dt.Rows(0)("snilai").ToString()
            LogoCompany = dt.Rows(1)("snilai").ToString()
            Notlp = dt.Rows(2)("snilai").ToString()


            'Ambil data dari m0_report filter dari modul, menu dan item
            dt = AsDataTableAmbilDariDB("SELECT rsql, rfrom, rfilter, rorderby, rgroupby, rfilename, rparam2, rparam3, rparam4, rparam5, rparam6, rparam7, rparam8, rparam9, rparam10 FROM m0_report WHERE rmoduleid = " + Modul.ToString + " AND rmenuid = " + MenuReport.ToString + " AND ritem = " + Item.ToString, strCon)
            'cek bila kosong
            If dt.Rows.Count = 0 Then
                SimpanLogToFile("[Agent] Error generator : Data report not found rmoduleid = " + Modul.ToString + " AND rmenuid = " + MenuReport.ToString + " AND ritem = " + Item.ToString)
                GoTo selesai
            End If

            dt.Rows(0)("rfilter") = dt.Rows(0)("rfilter").ToString()
            dt.Rows(0)("rorderby") = dt.Rows(0)("rorderby").ToString()
            dt.Rows(0)("rgroupby") = dt.Rows(0)("rgroupby").ToString()
            dt.Rows(0)("rparam2") = dt.Rows(0)("rparam2").ToString()
            dt.Rows(0)("rparam3") = dt.Rows(0)("rparam3").ToString()
            dt.Rows(0)("rparam4") = dt.Rows(0)("rparam4").ToString()
            dt.Rows(0)("rparam5") = dt.Rows(0)("rparam5").ToString()
            dt.Rows(0)("rparam6") = dt.Rows(0)("rparam6").ToString()
            dt.Rows(0)("rparam7") = dt.Rows(0)("rparam7").ToString()
            dt.Rows(0)("rparam8") = dt.Rows(0)("rparam8").ToString()
            dt.Rows(0)("rparam9") = dt.Rows(0)("rparam9").ToString()
            dt.Rows(0)("rparam10") = dt.Rows(0)("rparam10").ToString()
            'gabungkan query (sql, form, filter, orderby, groupby)
            Query = dt.Rows(0)("rsql") + " FROM " + dt.Rows(0)("rfrom")

            Query = buatQuery(Query, dt.Rows(0)("rfilter"), Filter, dt.Rows(0)("rorderby"), OrderBy, dt.Rows(0)("rgroupby"), GroupBy)
            xstep = 3
            'Gabungkan Query DS2, DS3, DS4, DS5
            For d = 0 To 8
                'set rparam ke varibel temp 'q'
                q = dt.Rows(0)("rparam" & (d + 2)).Replace(vbCrLf, " ")

                'cek jika variabel temp 'q' berisi
                If q.ToString.Length > 0 Then
                    'set kosong variabel filter2, orderby2, groupby2
                    filter2 = "" : orderby2 = "" : groupby2 = ""

                    'cek where di query
                    ar = q.Replace("WHERE", sptField).Split(sptField)
                    If ar.Length >= 2 Then
                        q = JoinSplitCharMinSatu(ar, "WHERE")
                        filter2 = ar(ar.Length - 1)

                        'setelah where cek groupby di query
                        ar = filter2.Replace("GROUP BY", sptField).Split(sptField)
                        If ar.Length > 1 Then
                            filter2 = JoinSplitCharMinSatu(ar, "GROUP BY")
                            groupby2 = ar(ar.Length - 1)

                            'setelah where, groupby cek orderby di query
                            ar = groupby2.Replace("ORDER BY", sptField).Split(sptField)
                            If ar.Length > 1 Then
                                groupby2 = JoinSplitCharMinSatu(ar, "ORDER BY")
                                orderby2 = ar(ar.Length - 1)
                            End If

                            'setelah where cek orderby jika groupby kososng di query
                        ElseIf filter2.Replace("ORDER BY", sptField).Split(sptField).Length >= 2 Then
                            ar = filter2.Replace("ORDER BY", sptField).Split(sptField)
                            filter2 = JoinSplitCharMinSatu(ar, "ORDER BY")
                            orderby2 = ar(ar.Length - 1)
                        End If

                        'cek groupby di query
                    ElseIf q.Replace("GROUP BY", sptField).Split(sptField).Length >= 2 Then
                        ar = q.Replace("GROUP BY", sptField).Split(sptField)
                        q = JoinSplitCharMinSatu(ar, "GROUP BY")
                        groupby2 = ar(ar.Length - 1)

                        'setelah groupby cek order by di query
                        ar = groupby2.Replace("ORDER BY", sptField).Split(sptField)
                        If ar.Length > 1 Then
                            groupby2 = JoinSplitCharMinSatu(ar, "ORDER BY")
                            orderby2 = ar(ar.Length - 1)
                        End If

                        'cek orderby di query
                    ElseIf q.Replace("ORDER BY", sptField).Split(sptField).Length = 2 Then
                        ar = q.Replace("ORDER BY", sptField).Split(sptField)
                        q = JoinSplitCharMinSatu(ar, "ORDER BY")
                        orderby2 = ar(ar.Length - 1)
                    End If

                    'buat query
                    dt.Rows(0)("rparam" & (d + 2)) = buatQuery(q, filter2, Filter, orderby2, OrderBy, groupby2, GroupBy)
                End If
            Next

            'default true || false
            s(0) = 1
            xstep = 4
            'jika query tipe progress //report berat
            If RQuery = 2 Then
                s = F_ReportProgress(WebAccessKey, Modul, MenuReport, Item, dt.Rows(0)("rfilename"), Query, Extension, Param1, "", "", "", "", NamaPerusahaan, Title, dt.Rows(0)("rparam2"), dt.Rows(0)("rparam3"), dt.Rows(0)("rparam4"), dt.Rows(0)("rparam5"), IDGenerate, userid, Filter)
            End If

            xstep = 5
            'Render Report Stimulsoft
            If s(0) = 1 Then
                s = render(Modul, dt.Rows(0)("rfilename"), Query, Extension, Param1, NamaPerusahaan, Title, dt.Rows(0)("rparam2"), dt.Rows(0)("rparam3"), dt.Rows(0)("rparam4"), dt.Rows(0)("rparam5"), param(20), strCon, param(17), param(18), ubahasa, dt.Rows(0)("rparam6"), dt.Rows(0)("rparam7"), dt.Rows(0)("rparam8"), dt.Rows(0)("rparam9"), dt.Rows(0)("rparam10"), LogoCompany, Alamat, Notlp)
            End If

            xstep = 6
            If s(0) = 1 Then
                'Update M0_Msmq jadi status Success
                f_query("Update M0_Msmq set progress = 2, tglselesai = NOW(), pesan = '" & FixQuotes(s(1)) & "' WHERE id = '" & IDGenerate & "'")
            Else
                'Update M0_Msmq jadi status Failed
                f_query("Update M0_Msmq set progress = 3, tglselesai = NOW(), pesan = '" & FixQuotes(s(1)) & "' WHERE id = '" & IDGenerate & "'")
            End If

        Catch ex As Exception
            SimpanLogToFile("[Agent] Error generator : Step : " + xstep.ToString + " Desc : " + Err.Description + "test2" + strCon + "@")
        End Try
selesai:
    End Sub

    Public Function render(ByVal ModuleId As Integer, ByVal MenuName As String, ByVal Query As String, ByVal FileFormat As Integer, ByVal Param1 As String, ByVal namaPerusahaan As String, ByVal namaReport As String, ByVal rp2 As String, ByVal rp3 As String, ByVal rp4 As String, ByVal rp5 As String, ByVal idreport As String, ByVal strconn As String, ByVal kotattd As String, ByVal wm As String, ByVal bahasa As String, ByVal rp6 As String, ByVal rp7 As String, ByVal rp8 As String, ByVal rp9 As String, ByVal rp10 As String, ByVal logoComp As String, ByVal alamat As String, ByVal notlp As String) As String()

        Dim s(2) As String
        s(0) = 0 : s(1) = ""
        Try
            Dim rpt As StiReport
            rpt = New StiReport()

            If FileFormat <> 6 Then
                rpt.Load(LocationMRT + "m" & ModuleId & "\" & MenuName & ".mrt")


                If bahasa = "ENG" Then
                    libs = File.ReadAllText(getlokasi + "\ENG.json")
                    Dim lang As JObject = JObject.Parse(libs)
                    For i = 0 To lang("libs").Count - 1
                        Try
                            If lang("libs")(i)("t").ToString.Length > 0 Then
                                CType(rpt.GetComponents(lang("libs")(i)("l").ToString), Stimulsoft.Report.Components.StiText).Text.Value = lang("libs")(i)("t").ToString
                            End If
                        Catch
                        End Try
                    Next
                End If
                'TryCast(rpt.Dictionary.Databases("myerpplus"), Stimulsoft.Report.Dictionary.StiOdbcSource).ConnectionString = strConStieReport
                TryCast(rpt.Dictionary.Databases("myerpplus"), Stimulsoft.Report.Dictionary.StiOdbcDatabase).ConnectionString = strConStieReport

                'http://forum.stimulsoft.com/viewtopic.php?t=3628

                'StiReport sr = new StiReport(); 

                '    sr = (StiReport)nameersali.Clone(); 
                '    sr.Dictionary.Databases.Clear(); 
                '    sr.Dictionary.DataSources.Clear(); 
                '    sr.DataSources.Clear(); 

                'rpt.Dictionary.Databases.Clear()
                'rpt.Dictionary.DataSources.Clear()
                'rpt.DataSources.Clear()

                'Dim cs As String = "Password=1234;Data Source=mansourpc\mahmadi;Integrated Security=False;Initial Catalog=Commision;User ID=mahmadi;"
                '.Dictionary.Databases.Add(New Stimulsoft.Report.Dictionary.StiSqlDatabase("myerpplus", strConStieReport))


                Try
                    CType(rpt.GetComponents("LogoPerusahaan"), Stimulsoft.Report.Components.StiText).Text.Value = logoComp
                Catch
                End Try


                Try
                    CType(rpt.GetComponents("AlamatPerusahaan"), Stimulsoft.Report.Components.StiText).Text.Value = alamat
                Catch
                End Try


                Try
                    CType(rpt.GetComponents("NoTlp"), Stimulsoft.Report.Components.StiText).Text.Value = notlp
                Catch
                End Try

                Try
                    TryCast(rpt.Dictionary.DataSources("DS1"), Stimulsoft.Report.Dictionary.StiSqlSource).SqlCommand = Query
                    TryCast(rpt.Dictionary.DataSources("DS1"), Stimulsoft.Report.Dictionary.StiSqlSource).CommandTimeout = 31536000
                Catch
                End Try
                Try
                    TryCast(rpt.Dictionary.DataSources("DS2"), Stimulsoft.Report.Dictionary.StiSqlSource).SqlCommand = rp2
                    TryCast(rpt.Dictionary.DataSources("DS2"), Stimulsoft.Report.Dictionary.StiSqlSource).CommandTimeout = 31536000
                    Query += " - " + rp2
                Catch
                End Try
                Try
                    TryCast(rpt.Dictionary.DataSources("DS3"), Stimulsoft.Report.Dictionary.StiSqlSource).SqlCommand = rp3
                    TryCast(rpt.Dictionary.DataSources("DS3"), Stimulsoft.Report.Dictionary.StiSqlSource).CommandTimeout = 31536000
                    Query += " - " + rp3
                Catch
                End Try
                Try
                    TryCast(rpt.Dictionary.DataSources("DS4"), Stimulsoft.Report.Dictionary.StiSqlSource).SqlCommand = rp4
                    TryCast(rpt.Dictionary.DataSources("DS4"), Stimulsoft.Report.Dictionary.StiSqlSource).CommandTimeout = 31536000
                    Query += " - " + rp4
                Catch
                End Try
                Try
                    TryCast(rpt.Dictionary.DataSources("DS5"), Stimulsoft.Report.Dictionary.StiSqlSource).SqlCommand = rp5
                    TryCast(rpt.Dictionary.DataSources("DS5"), Stimulsoft.Report.Dictionary.StiSqlSource).CommandTimeout = 31536000
                    Query += " - " + rp5
                Catch
                End Try

                Try
                    TryCast(rpt.Dictionary.DataSources("DS6"), Stimulsoft.Report.Dictionary.StiSqlSource).SqlCommand = rp6
                    TryCast(rpt.Dictionary.DataSources("DS6"), Stimulsoft.Report.Dictionary.StiSqlSource).CommandTimeout = 31536000
                    Query += " - " + rp6
                Catch
                End Try

                Try
                    TryCast(rpt.Dictionary.DataSources("DS7"), Stimulsoft.Report.Dictionary.StiSqlSource).SqlCommand = rp7
                    TryCast(rpt.Dictionary.DataSources("DS7"), Stimulsoft.Report.Dictionary.StiSqlSource).CommandTimeout = 31536000
                    Query += " - " + rp7
                Catch
                End Try

                Try
                    TryCast(rpt.Dictionary.DataSources("DS8"), Stimulsoft.Report.Dictionary.StiSqlSource).SqlCommand = rp8
                    TryCast(rpt.Dictionary.DataSources("DS8"), Stimulsoft.Report.Dictionary.StiSqlSource).CommandTimeout = 31536000
                    Query += " - " + rp8
                Catch
                End Try

                Try
                    TryCast(rpt.Dictionary.DataSources("DS9"), Stimulsoft.Report.Dictionary.StiSqlSource).SqlCommand = rp9
                    TryCast(rpt.Dictionary.DataSources("DS9"), Stimulsoft.Report.Dictionary.StiSqlSource).CommandTimeout = 31536000
                    Query += " - " + rp9
                Catch
                End Try

                Try
                    TryCast(rpt.Dictionary.DataSources("DS10"), Stimulsoft.Report.Dictionary.StiSqlSource).SqlCommand = rp10
                    TryCast(rpt.Dictionary.DataSources("DS10"), Stimulsoft.Report.Dictionary.StiSqlSource).CommandTimeout = 31536000
                    Query += " - " + rp10
                Catch
                End Try

                Try
                    CType(rpt.GetComponents("PTNAMA"), Stimulsoft.Report.Components.StiText).Text.Value = namaPerusahaan
                Catch
                End Try
                Try
                    CType(rpt.GetComponents("PTKOTATTD"), Stimulsoft.Report.Components.StiText).Text.Value = kotattd
                Catch
                End Try
                Try
                    CType(rpt.GetComponents("rtitle"), Stimulsoft.Report.Components.StiText).Text.Value = namaReport
                Catch
                End Try
                Try
                    CType(rpt.GetComponents("param1"), Stimulsoft.Report.Components.StiText).Text.Value = Param1.Replace("newline", vbCrLf).Replace("&space ", "").Replace(" &space", "")
                Catch
                End Try

                If Len(wm) <> 0 Then
                    rpt.Pages(0).Watermark.Enabled = True
                    rpt.Pages(0).Watermark.Angle = 45
                    'rpt.Pages(0).Watermark.Text = wm
                End If
                rpt.Render()

                Try

                    Select Case FileFormat
                        Case 0 'PDF
                            rpt.ExportDocument(StiExportFormat.Pdf, LocationResultReport + idreport + ".pdf")
                        Case 1 'Excel
                            rpt.ExportDocument(StiExportFormat.Excel, LocationResultReport + idreport + ".xls")
                        Case 2 'HTML
                            rpt.ExportDocument(StiExportFormat.Html, LocationResultReport + idreport + ".html")
                        Case 3 'Word
                            rpt.ExportDocument(StiExportFormat.Word2007, LocationResultReport + idreport + ".doc")
                        Case 4 'Txt
                            rpt.ExportDocument(StiExportFormat.Text, LocationResultReport + idreport + ".txt")
                        Case 5 'Image
                            rpt.ExportDocument(StiExportFormat.ImageJpeg, LocationResultReport + idreport + ".jpg")
                    End Select
                Catch ex As Exception
                    rpt.Render()
                End Try


                ' Perintah kirim email
                Dim sendEmail As Boolean = False

                'If MenuName = "salesorderdetail1" Or MenuName = "PurchaseRequestDetail_new" Or MenuName = "BiddingSheetsDetail1" Or MenuName = "PurchaseOrderDetail2" Or MenuName = "ReceiveInvoiceDetail1_new" Or MenuName = "GoodReceiptNoteDetail1_new" Or MenuName = "anggarandetail" Then
                '    sendEmail = True
                'End If
                sendEmail = True

                'If sendEmail And IDTransaksi = 0 Then
                If sendEmail Then
                    Dim dtNomor As DataTable = AsDataTableAmbilDariDB("SELECT kodetabel FROM m0_nomor WHERE moduleid = " & ModuleId.ToString & " AND menuid = " & MenuReport, strCon)
                    If dtNomor.Rows.Count > 0 Then
                        Sumber = dtNomor.Rows(0)(0)
                    Else
                        Sumber = ""
                    End If

                    Dim dtNotifikasi, dtTr As New DataTable
                    'JIKA TRANSAKSI PR MAKA AMBIL USERID BERDASARKAN PRDIMINTAOLEH
                    If ModuleId.ToString.Equals("4") And MenuReport.ToString.Equals("3") Then
                        'dtTr = AsDataTableAmbilDariDB("SELECT prdimintaoleh, prstatus FROM m4_pr WHERE " + Filter, strCon)
                        'dtTr = AsDataTableAmbilDariDB("SELECT prdimintaoleh, prstatus, bdivisi FROM m4_pr JOIN m4_pr_detail ON prid = idpr JOIN m1_item ON idbarang = bid WHERE " + Filter + " GROUP BY prid", strCon)
                        dtTr = AsDataTableAmbilDariDB("SELECT prdimintaoleh, prstatus, prlokasi FROM m4_pr WHERE " + Filter, strCon)
                        If dtTr.Rows.Count > 0 Then
                            dtNotifikasi = AsDataTableAmbilDariDB("SELECT * FROM m0_notifikasi_email WHERE moduleid = " & ModuleId.ToString & " AND menuid = " & MenuReport & " AND statusnotifikasi = " & FxDB(dtTr.Rows(0)("prstatus"), 0) & " AND userid = " & FxDB(dtTr.Rows(0)("prdimintaoleh"), 0) & " AND divisi = '" & FxDB(dtTr.Rows(0)("prlokasi"), 0) & "'", strCon)
                        End If
                    ElseIf ModuleId.ToString.Equals("4") And MenuReport.ToString.Equals("7") Then
                        'dtTr = AsDataTableAmbilDariDB("SELECT pojenispembeliankategori, potgl, ponotransaksi, pomatauang, pokurs, pototaltransaksi, pokurs * pototaltransaksi as pototalfungsional, postatus, bdivisi FROM m4_po JOIN m4_po_detail ON poid = idpo JOIN m1_item ON idbarang = bid WHERE " + Filter + " GROUP BY poid", strCon)
                        dtTr = AsDataTableAmbilDariDB("SELECT pojenispembeliankategori, potgl, ponotransaksi, pomatauang, pokurs, pototaltransaksi, pokurs * pototaltransaksi as pototalfungsional, postatus, polokasi FROM m4_po WHERE " + Filter, strCon)
                        If dtTr.Rows.Count > 0 Then
                            dtNotifikasi = AsDataTableAmbilDariDB("SELECT * FROM m0_notifikasi_email WHERE moduleid = " & ModuleId.ToString & " AND menuid = " & MenuReport & " AND statusnotifikasi = " & FxDB(dtTr.Rows(0)("postatus"), 0) & " AND statusupdate = " & FxDB(dtTr.Rows(0)("pojenispembeliankategori"), 0) & " AND divisi = '" & FxDB(dtTr.Rows(0)("polokasi"), 0) & "'" & " AND " & FxDB(dtTr.Rows(0)("pototalfungsional"), 0) & " <= userid ORDER BY userid LIMIT 1", strCon)
                        End If
                    Else
                        dtNotifikasi = AsDataTableAmbilDariDB("SELECT * FROM m0_notifikasi_email WHERE moduleid = " & ModuleId.ToString & " AND menuid = " & MenuReport & " AND userid = " & userid.ToString, strCon)
                    End If

                    If dtNotifikasi.Rows.Count > 0 Then 'if
                        For Each drn As DataRow In dtNotifikasi.Rows
                            If Filter.ToLower.Contains(Sumber.ToLower & "id") Then
                                'Sumber = Filter.Split("id")(0)
                                IDTransaksi = Replace(Filter, "id = ", "@").Split("@")(1)
                                'IDTransaksi = 10
                                Dim dtTabel As DataTable = AsDataTableAmbilDariDB("SELECT " + Sumber + "status, " + Sumber + "notransaksi, " + Sumber + "tgl, " + Sumber + "matauang, " + Sumber + "totaltransaksi FROM m" + ModuleId.ToString + "_" + Sumber + " WHERE " + Filter, strCon)
                                If dtTabel.Rows.Count > 0 Then
                                    If drn("statusnotifikasi") = dtTabel.Rows(0)(0) Then
                                        'If Sumber = "PR" Then
                                        '    Sumber = "SPP"
                                        'End If 
                                        'Dim dtsumber As DataTable = AsDataTableAmbilDariDB("SELECT nama FROM m0_status WHERE kode = " & drn("statusupdate"), strCon)
                                        Dim dtsumber As DataTable = AsDataTableAmbilDariDB("SELECT nama FROM m0_status WHERE kode = " & drn("statusnotifikasi"), strCon)
                                        Dim statusmail As String = "kosong"
                                        If dtsumber.Rows.Count > 0 Then
                                            statusmail = dtsumber.Rows(0)("nama")
                                        End If
                                        Dim fromAddress = New MailAddress(drn("emailpengirim").ToString, "Notification " & Sumber)
                                        Dim toAddress = New MailAddress(drn("email").ToString)
                                        Dim fromPassword As String = drn("passwordpengirim").ToString
                                        Dim subject As String = "MyERPPlus - " & Sumber & " " & statusmail & " - " & dtTabel.Rows(0)(1) & " - " & DateTime.Now
                                        'Dim body As String = "MyERPPlus - " & Sumber & " " & statusmail & " - " & dtTabel.Rows(0)(1) & " - " & DateTime.Now
                                        Dim body As String = "", vUrut As Double = 0

                                        Dim smtp = New SmtpClient()
                                        smtp.Host = "smtp.gmail.com"
                                        smtp.Port = 587
                                        smtp.EnableSsl = True
                                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network
                                        'smtp.UseDefaultCredentials = True
                                        smtp.UseDefaultCredentials = False
                                        smtp.Credentials = New System.Net.NetworkCredential(fromAddress.Address, fromPassword)

                                        Dim dtDashboard As DataTable = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE sgrup = 'dashboard' AND skode = 'url'", strCon)

                                        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
                                        Dim myConn As New MySql.Data.MySqlClient.MySqlConnection(strCon)
                                        myConn.Open()

                                        '*** Start Transaction ***'  
                                        objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                                        With objCmd
                                            .Connection = myConn
                                            .CommandType = CommandType.Text
                                            .CommandText = "Insert into m0_dashboardmail(did, durl, dtable, dsumber, didtransaksi, diduser, dketerangan, dview, dinputtgl, dmodiftgl) values (0, '" & dtDashboard.Rows(0)(0) & "', 'm" & ModuleId.ToString + "_" + Sumber & "', '" & Sumber & "', " & IDTransaksi & ", " & drn("useridmail") & ", '', 0, NOW(), NOW())"
                                        End With
                                        objCmd.ExecuteNonQuery()

                                        Using message = New MailMessage(fromAddress, toAddress)
                                            message.Subject = subject
                                            Dim dtSelect As DataTable = AsDataTableAmbilDariDB("SELECT CONCAT (durl,'/app/autologin.aspx?iduser=',diduser,'&sumber=',dsumber,'&idtransaksi=',didtransaksi) AS isi FROM m0_dashboardmail WHERE dsumber = '" & Sumber & "' AND didtransaksi  = " & IDTransaksi & " AND diduser = " & drn("useridmail") & " ORDER BY dinputtgl DESC LIMIT 1 ", strCon)

                                            message.IsBodyHtml = True
                                            body = "Notification regarding the pending approval for the transaction number mentioned below."
                                            body &= "<br>"
                                            body &= "Kindly process the approval at your earliest convenience by clicking the link provided."
                                            body &= "<br><br>"
                                            body &= drn("namamenu")
                                            body &= "<br><br>"
                                            body &= "<table border=1 cellpadding=5>"
                                            body &= "<tr>"
                                            body &= "<th>No </th>"
                                            body &= "<th> Trans No </th>"
                                            body &= "<th>Date</th>"
                                            body &= "<th>Curr</th>"
                                            body &= "<th>Total</th>"
                                            body &= "</tr>"

                                            vUrut += 1

                                            body &= "<tr>"
                                            body &= "<td>" & vUrut & "</td>"
                                            body &= "<td><a href='" & dtSelect.Rows(0)(0) & "'>" & dtTabel.Rows(0)(1) & "</a></td>"
                                            body &= "<td>" & dtTabel.Rows(0)(2) & "</td>"
                                            body &= "<td>" & dtTabel.Rows(0)(3) & "</td>"
                                            body &= "<td style='text-align:right;'>" & FormatNumber(dtTabel.Rows(0)(4)) & "</td>"
                                            body &= "</tr>"

                                            body &= "</table>"
                                            message.Body = body

                                            'message.Body = body & "<br><br>" & "<a href='" & dtSelect.Rows(0)(0) & "'>Click here to process approval</a>" & ""
                                            'message.Body &= "<br><br><br><br>" & dtSelect.Rows(0)(0)

                                            Select Case FileFormat
                                                Case 0 'PDF
                                                    Dim attach As New Attachment(LocationResultReport + idreport + ".pdf")
                                                    message.Attachments.Add(attach)
                                                Case 1 'Excel
                                                    Dim attach As New Attachment(LocationResultReport + idreport + ".xls")
                                                    message.Attachments.Add(attach)
                                                Case 2 'HTML
                                                    Dim attach As New Attachment(LocationResultReport + idreport + ".html")
                                                    message.Attachments.Add(attach)
                                                Case 3 'Word
                                                    Dim attach As New Attachment(LocationResultReport + idreport + ".doc")
                                                    message.Attachments.Add(attach)
                                                Case 4 'Txt
                                                    Dim attach As New Attachment(LocationResultReport + idreport + ".txt")
                                                    message.Attachments.Add(attach)
                                                Case 5 'Image
                                                    Dim attach As New Attachment(LocationResultReport + idreport + ".jpg")
                                                    message.Attachments.Add(attach)
                                            End Select
                                            smtp.Send(message)
                                        End Using
                                    End If
                                Else
                                    Query = "SELECT " + Sumber + "status FROM m" + ModuleId.ToString + "_" + Sumber + " WHERE " + Filter
                                End If
                            End If

                        Next
                    End If 'end if

                End If ' end send mail


                '' Perintah kirim email
                'Dim sendEmail As Boolean = False

                ''If MenuName = "salesorderdetail1" Or MenuName = "PurchaseRequestDetail_new" Or MenuName = "BiddingSheetsDetail1" Or MenuName = "PurchaseOrderDetail2" Or MenuName = "ReceiveInvoiceDetail1_new" Or MenuName = "GoodReceiptNoteDetail1_new" Or MenuName = "anggarandetail" Then
                ''    sendEmail = True
                ''End If
                ''sendEmail = True

                ''If sendEmail And IDTransaksi = 0 Then
                'SimpanLogToFile("a")
                'If sendEmail Then
                '    Dim dtNotifikasi As DataTable = AsDataTableAmbilDariDB("SELECT * FROM m0_notifikasi_email WHERE moduleid = " & ModuleId.ToString & " AND menuid = " & MenuReport & " AND userid = " & userid.ToString, strCon)
                '    SimpanLogToFile("a : " & dtNotifikasi.Rows.Count.ToString)
                '    If dtNotifikasi.Rows.Count > 0 Then 'if
                '        SimpanLogToFile("b")
                '        For Each drn As DataRow In dtNotifikasi.Rows
                '            If Len(Filter) = 0 Then GoTo berakhir
                '            SimpanLogToFile(" Filter : " & Filter)
                '            SimpanLogToFile("c")
                '            SimpanLogToFile(" 1 lanjut" & Replace(Filter, "id = ", "@").Split("@").Length.ToString)
                '            If Replace(Filter, "id = ", "@").Split("@").Length = 1 Then GoTo berakhir
                '            SimpanLogToFile(" 2 lanjut" & Replace(Filter, "id = ", "@").Split("@").Length.ToString)
                '            Sumber = Filter.Split("id")(0)
                '            IDTransaksi = Replace(Filter, "id = ", "@").Split("@")(1)
                '            'IDTransaksi = 10
                '            Dim dtTabel As DataTable = AsDataTableAmbilDariDB("SELECT " + Sumber + "status FROM m" + ModuleId.ToString + "_" + Sumber + " WHERE " + Filter, strCon)
                '            If dtTabel.Rows.Count > 0 Then
                '                If drn("statusnotifikasi") = dtTabel.Rows(0)(0) Then
                '                    'If Sumber = "PR" Then
                '                    '    Sumber = "SPP"
                '                    'End If 
                '                    Dim dtsumber As DataTable = AsDataTableAmbilDariDB("SELECT nama FROM m0_status WHERE kode = " & drn("statusupdate"), strCon)
                '                    Dim statusmail As String = "kosong"
                '                    If dtsumber.Rows.Count > 0 Then
                '                        statusmail = dtsumber.Rows(0)("nama")
                '                    End If
                '                    Dim fromAddress = New MailAddress(drn("emailpengirim").ToString, "Notifikasi " & Sumber)
                '                    Dim toAddress = New MailAddress(drn("email").ToString, "Notifikasi " & Sumber)
                '                    Dim fromPassword As String = drn("passwordpengirim").ToString
                '                    Dim subject As String = "MyERPPLUS - " & Sumber & " " & statusmail & " " & DateTime.Now
                '                    Dim body As String = "MyERPPLUS - " & Sumber & " " & statusmail & " " & DateTime.Now

                '                    Dim smtp = New SmtpClient()
                '                    smtp.Host = "smtp.gmail.com"
                '                    smtp.Port = 587
                '                    smtp.EnableSsl = True
                '                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network
                '                    smtp.UseDefaultCredentials = True
                '                    smtp.Credentials = New System.Net.NetworkCredential(fromAddress.Address, fromPassword)

                '                    Dim dtDashboard As DataTable = AsDataTableAmbilDariDB("SELECT snilai FROM m0_setting WHERE sgrup = 'dashboard' AND skode = 'url'", strCon)

                '                    Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
                '                    Dim myConn As New MySql.Data.MySqlClient.MySqlConnection(strCon)
                '                    myConn.Open()

                '                    '*** Start Transaction ***'  
                '                    objCmd = New MySql.Data.MySqlClient.MySqlCommand()
                '                    With objCmd
                '                        .Connection = myConn
                '                        .CommandType = CommandType.Text
                '                        .CommandText = "Insert into m0_dashboardmail(did, durl, dtable, dsumber, didtransaksi, diduser, dketerangan, dview, dinputtgl, dmodiftgl) values (0, '" & dtDashboard.Rows(0)(0) & "', 'm" & ModuleId.ToString + "_" + Sumber & "', '" & Sumber & "', " & IDTransaksi & ", " & drn("useridmail") & ", '', 0, NOW(), NOW())"
                '                    End With
                '                    objCmd.ExecuteNonQuery()

                '                    Using message = New MailMessage(fromAddress, toAddress)
                '                        message.Subject = subject
                '                        Dim dtSelect As DataTable = AsDataTableAmbilDariDB("SELECT CONCAT (durl,'/app/autologin.aspx?iduser=',diduser,'&sumber=',dsumber,'&idtransaksi=',didtransaksi) AS isi FROM m0_dashboardmail WHERE dsumber = '" & Sumber & "' AND didtransaksi  = " & IDTransaksi & " AND diduser = " & drn("useridmail"), strCon)
                '                        message.Body = dtSelect.Rows(0)(0) & vbCrLf & body
                '                        Select Case FileFormat
                '                            Case 0 'PDF
                '                                Dim attach As New Attachment(LocationResultReport + idreport + ".pdf")
                '                                message.Attachments.Add(attach)
                '                            Case 1 'Excel
                '                                Dim attach As New Attachment(LocationResultReport + idreport + ".xls")
                '                                message.Attachments.Add(attach)
                '                            Case 2 'HTML
                '                                Dim attach As New Attachment(LocationResultReport + idreport + ".html")
                '                                message.Attachments.Add(attach)
                '                            Case 3 'Word
                '                                Dim attach As New Attachment(LocationResultReport + idreport + ".doc")
                '                                message.Attachments.Add(attach)
                '                            Case 4 'Txt
                '                                Dim attach As New Attachment(LocationResultReport + idreport + ".txt")
                '                                message.Attachments.Add(attach)
                '                            Case 5 'Image
                '                                Dim attach As New Attachment(LocationResultReport + idreport + ".jpg")
                '                                message.Attachments.Add(attach)
                '                        End Select
                '                        smtp.Send(message)
                '                    End Using
                '                End If
                '            Else
                '                Query = "SELECT " + Sumber + "status FROM m" + ModuleId.ToString + "_" + Sumber + " WHERE " + Filter
                '            End If
                '        Next
                '    End If 'end if

                'End If ' end send mail
berakhir:
                'log("baris setelah blok kode kirim email")
            Else
                'Export Data
                'Dim dtData As DataTable = AsDataTableAmbilDariDB(Query, strConStieReport)
                Dim dtData As DataTable = New DataTable
                Dim ConX As MySqlConnection = New MySqlConnection
                ConX.ConnectionString = strCon
                ConX.Open()
                Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(Query, ConX)
                da1.SelectCommand.CommandTimeout = 31536000
                da1.Fill(dtData)

                rpt.ScriptLanguage = StiReportLanguageType.VB

                'Add data to datastore
                rpt.RegData("data", dtData)
                'Fill dictionary
                rpt.Dictionary.Synchronize()
                rpt.Dictionary.DataSources.Item(0).Name = "data"
                rpt.Dictionary.DataSources.Item(0).Alias = "data"

                Dim Page As StiPage = rpt.Pages.Item(0)
                Page.Height = dtData.Rows.Count
                'Create HeaderBand
                Dim HeaderBand As New StiHeaderBand()
                HeaderBand.Height = 0.175
                HeaderBand.Name = "HeaderBand"
                Page.Components.Add(HeaderBand)

                'Create Databand
                Dim Databand As New StiDataBand()
                Databand.DataSourceName = "data"
                Databand.Height = 0.175
                Databand.Name = "DataBand"
                Page.Components.Add(Databand)

                'Create texts
                Dim Pos As Double = 0
                Dim ColumnWidth As Double = StiAlignValue.AlignToMinGrid(Page.Width / dtData.Columns.Count, 0.1, True)
                Dim NameIndex As Integer = 1
                Dim Column As DataColumn

                For Each Column In dtData.Columns

                    If MenuName <> "FAKTURPAJAKEXSPORT" Then

                        'Create text on header
                        Dim HeaderText As New StiText(New RectangleD(Pos, 0, ColumnWidth, 0.175))
                        HeaderText.Text.Value = Column.Caption
                        HeaderText.HorAlignment = StiTextHorAlignment.Center
                        HeaderText.Name = "HeaderText" + NameIndex.ToString()
                        HeaderBand.Components.Add(HeaderText)

                        'Create text on Data Band
                        Dim DataText As New StiText(New RectangleD(Pos, 0, ColumnWidth, 0.175))
                        DataText.Text.Value = "{data." + Column.ColumnName + "}"
                        DataText.Name = "DataText" + NameIndex.ToString()

                        Databand.Components.Add(DataText)

                        Pos = Pos + ColumnWidth

                        NameIndex = NameIndex + 1

                    Else

                        'Create text on header
                        Dim HeaderText As New StiText(New RectangleD(Pos, 0, ColumnWidth, 0.2))
                        HeaderText.Text.Value = Column.Caption
                        HeaderText.HorAlignment = StiTextHorAlignment.Left
                        HeaderText.Name = "HeaderText" + NameIndex.ToString()
                        HeaderBand.Components.Add(HeaderText)
                        HeaderText.Font = New Font("Calibri", 11.0F)

                        'Create text on Data Band
                        Dim DataText As New StiText(New RectangleD(Pos, 0, ColumnWidth, 0.2))
                        DataText.Font = New Font("Calibri", 11.0F)
                        If Column.ColumnName.Contains("tgl") Or Column.ColumnName.Contains("TANGGAL_FAKTUR") Then
                            DataText.TextFormat = New Stimulsoft.Report.Components.TextFormats.StiDateFormatService("dd/MM/yyyy", String.Empty)
                        End If
                        DataText.Text.Value = "{data." + Column.ColumnName + "}"
                        DataText.Name = "DataText" + NameIndex.ToString()

                        Databand.Components.Add(DataText)

                        Pos = Pos + ColumnWidth

                        NameIndex = NameIndex + 1

                    End If

                Next

                If MenuName <> "FAKTURPAJAKEXSPORT" Then
                    'Create FooterBand
                    Dim FooterBand As New StiFooterBand()
                    FooterBand.Height = 0.175
                    FooterBand.Name = "FooterBand"
                    Page.Components.Add(FooterBand)

                    'Create text on footer
                    Dim FooterText As New StiText(New RectangleD(0, 0, Page.Width, 0.175))
                    FooterText.Text.Value = "Count - {Count()}"
                    FooterText.HorAlignment = StiTextHorAlignment.Right
                    FooterText.Name = "FooterText"
                    FooterBand.Components.Add(FooterText)
                End If


                rpt.Render()
                rpt.ExportDocument(StiExportFormat.Excel, LocationResultReport + idreport + ".xls")
            End If

            s(0) = 1
            s(1) = Query
        Catch ex As Exception
            s(0) = 0
            s(1) = ex.Message & " - " & Query
            SimpanLogToFile("Info render : " + Err.Description)
        End Try
        Return s
    End Function

    Public Sub f_query(ByVal sql As String)
        Dim objCmd As MySql.Data.MySqlClient.MySqlCommand
        Dim myConn As New MySql.Data.MySqlClient.MySqlConnection(strCon)
        myConn.Open()

        '*** Start Transaction ***'  
        Trans = myConn.BeginTransaction(IsolationLevel.ReadCommitted)

        Try
            objCmd = New MySql.Data.MySqlClient.MySqlCommand()
            With objCmd
                .Connection = myConn
                .Transaction = Trans
                .CommandType = CommandType.Text
                .CommandText = sql
            End With
            objCmd.ExecuteNonQuery()

            Trans.Commit()  '*** Commit Transaction ***'
        Catch ex As Exception
            Trans.Rollback() '*** RollBack Transaction ***'  
            SimpanLogToFile("Error : " + Err.Description + " f_query()")
        End Try

        objCmd = Nothing
        myConn.Close()
    End Sub

    Public Function buatQuery(ByVal sql As String, ByVal filter1 As String, ByVal filter2 As String, ByVal orderby1 As String, ByVal orderby2 As String, ByVal groupby1 As String, ByVal groupby2 As String) As String
        Try
            If Len(sql) = 0 Then
                SimpanLogToFile("[Agent] Error buatQuery : sql not found") : GoTo selesai
            End If
            If RQuery = 2 Then ' Jika Report Berat maka filter dulu idlogin sama msmq dan klo ada filter dari tebal gabungin sekalian
                sql += " WHERE idlogin = '" + WebAccessKey + "' AND idmsmq = '" + IDGenerate + "'"
                If Len(filter1) > 0 Then
                    sql += " AND " + filter1
                End If
            ElseIf Len(filter1) > 0 And Len(filter2) > 0 Then
                sql += " WHERE " + filter1 + " AND " + filter2
            ElseIf Len(filter1) > 0 Then
                sql += " WHERE " + filter1
            ElseIf Len(filter2) > 0 Then
                sql += " WHERE " + filter2
            End If

            If Len(groupby1) > 0 And Len(groupby2) > 0 Then
                sql += " GROUP BY " + groupby1 + ", " + groupby2
            ElseIf Len(groupby1) > 0 Then
                sql += " GROUP BY " + groupby1
            ElseIf Len(groupby2) > 0 Then
                sql += " GROUP BY " + groupby2
            End If

            If Len(orderby1) > 0 And Len(orderby2) > 0 Then
                sql += " ORDER BY " + orderby1 + ", " + orderby2
            ElseIf Len(orderby1) > 0 Then
                sql += " ORDER BY " + orderby1
            ElseIf Len(orderby2) > 0 Then
                sql += " ORDER BY " + orderby2
            End If

            Return sql.Replace(vbCrLf, " ")
        Catch ex As Exception
            SimpanLogToFile("[Agent] Error buatQuery : " + Err.Description)
        End Try

selesai:
        Return ""
    End Function

    Private Function JoinSplitCharMinSatu(ByVal ar() As String, ByVal charSplit As String) As String
        Dim hasil As String = ar(0)
        For i = 1 To ar.Length - 2
            hasil += charSplit + ar(i)
        Next
        Return hasil
    End Function

    '//PROSES REPORT DENGAN PROGRESS
    Public Function F_ReportProgress(ByVal WebsiteAccessKey As String, ByVal ModuleId As Integer, ByVal MenuId As Integer, ByVal RItem As Integer, ByVal MenuName As String, ByVal Query As String, ByVal FileFormat As Integer, ByVal Param1 As String, ByVal Param2 As String, ByVal Param3 As String, ByVal Param4 As String, ByVal Param5 As String, ByVal namaPerusahaan As String, ByVal namaReport As String, ByVal rp2 As String, ByVal rp3 As String, ByVal rp4 As String, ByVal rp5 As String, ByVal idMsmq As String, ByVal userId As Integer, ByVal paramDetail As String) As String()
        '// PROSES REPORT DENGAN PROGRESS
        Dim clsRptProgress As New ReportProgress
        Dim clsRpt As New Report
        Dim strRptProgress As String = "", paramDetailSplit() As String
        Dim Utama As String = "", detail As String = "", s(2) As String

        Try

            '//PANGGIL FUNGSI REPORT SESUAI MODULID, MENUID, DAN RITEM
            '//mapping utama :    ModuleId,           MenuName,           Query,           FileFormat,           Param1,           Param2,           Param3,           Param4,           Param5,           namaPerusahaan,           namaReport,           rp2,           rp3,           rp4,           rp5,           idMsmq
            Utama = String.Concat(ModuleId, sptField, MenuName, sptField, Query, sptField, FileFormat, sptField, Param1, sptField, Param2, sptField, Param3, sptField, Param4, sptField, Param5, sptField, namaPerusahaan, sptField, namaReport, sptField, rp2, sptField, rp3, sptField, rp4, sptField, rp5, sptField, idMsmq)
            '//SPLIT PARAMETER DETAIL
            paramDetailSplit = paramDetail.Split(sptRow)

            '//mapping detail menyesuaikan reportnya
            'MODULEID = 2, MENUID = 41, RITEM = 1 ====================================> BUKU BESAR DETAIL
            If ModuleId = 2 And MenuId = 41 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,              matauang,           perTanggal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, 0)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GeneralLedger(WebsiteAccessKey & "★M0_GeneralLedger★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                ''CEK PARAMETER REPORT
                'If paramDetailSplit.Length <> 5 Then
                '    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                '    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                'End If

                ''mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,              matauang,           perTanggal,    uraian
                'detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, 0, sptField, paramDetailSplit(4))

                ''PANGGIL FUNGSI REPORT PROGRESS
                'strRptProgress = clsRptProgress.M0_GeneralLedgerMakmur(WebsiteAccessKey & "★M0_GeneralLedgerMakmur★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 2, MENUID = 41, RITEM = 2 ================================> BUKU BESAR GLOBAL PER TANGGAL
            ElseIf ModuleId = 2 And MenuId = 41 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,              matauang,           perTanggal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, 1)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GeneralLedger(WebsiteAccessKey & "★M0_GeneralLedger★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 90, RITEM = 1 ====================================> BUKU BESAR DETAIL PER KONTAK
            ElseIf ModuleId = 2 And MenuId = 90 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                    norekAkhir,     matauang,  perTanggal,                    kontakAwal,                   kontakAkhir,           OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, 0, sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ContactGeneralLedger(WebsiteAccessKey & "★M0_ContactGeneralLedger★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 90, RITEM = 2 ====================================> BUKU BESAR GLOBAL PER KONTAK
            ElseIf ModuleId = 2 And MenuId = 90 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                    norekAkhir,     matauang,  perTanggal,                    kontakAwal,                   kontakAkhir,           OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, 1, sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ContactGeneralLedger(WebsiteAccessKey & "★M0_ContactGeneralLedger★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 90, RITEM = 3 ================================> REKAP PER KONTAK
            ElseIf ModuleId = 2 And MenuId = 90 And RItem = 3 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                 norekAkhir,costCenterAwal,costCenterAkhir,         area,kontakCategory,customerCategory,supplierCategory,             kontakAwal,                   kontakAkhir,salesmanCategory,  salesman,                      matauang,                 orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "bpkontakkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ContactSummary(WebsiteAccessKey & "★M0_ContactSummary★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)



                'MODULEID = 2, MENUID = 90, RITEM = 4 ====================================> SURAT PEMESANAN TANAH
            ElseIf ModuleId = 2 And MenuId = 90 And RItem = 4 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                    norekAkhir,     matauang,  perTanggal,                    kontakAwal,                   kontakAkhir,           OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, 0, sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ContactGeneralLedger(WebsiteAccessKey & "★M0_ContactGeneralLedger★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 90, RITEM = 5 ====================================> TW-4
            ElseIf ModuleId = 2 And MenuId = 90 And RItem = 5 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                    norekAkhir,     matauang,  perTanggal,                    kontakAwal,                   kontakAkhir,           OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, 0, sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ContactGeneralLedger(WebsiteAccessKey & "★M0_ContactGeneralLedger★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 42, RITEM = 1 ================================> NERACA MUTASI
            ElseIf ModuleId = 2 And MenuId = 42 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,              matauang,           saldoNol
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_NeracaMutasi(WebsiteAccessKey & "★M0_NeracaMutasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 43, RITEM = 1 ================================> KAS HARIAN (GLOBAL)
            ElseIf ModuleId = 2 And MenuId = 43 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,              matauang,                      saldoNol
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_KasHarian_Global(WebsiteAccessKey & "★M0_KasHarian_Global★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 43, RITEM = 2 ================================> KAS HARIAN (AKUN LAWAN)
            ElseIf ModuleId = 2 And MenuId = 43 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,                    matauang
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_KasHarian_AkunLawan(WebsiteAccessKey & "★M0_KasHarian_AkunLawan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 2, MENUID = 43, RITEM = 3 ================================> Kas Harian (Global) Per Akun Rekap Detail
            ElseIf ModuleId = 2 And MenuId = 43 And RItem = 3 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,                    matauang
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_KasHarian_AkunLawan(WebsiteAccessKey & "★M0_KasHarian_AkunLawan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)




                'MODULEID = 2, MENUID = 43, RITEM = 4 ================================> Kas Harian (Global) Per Akun Rekap Detail
            ElseIf ModuleId = 2 And MenuId = 43 And RItem = 4 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,                    matauang
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_KasHarian_AkunLawan(WebsiteAccessKey & "★M0_KasHarian_AkunLawan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 44, RITEM = 1 ================================> BANK HARIAN (GLOBAL)
            ElseIf ModuleId = 2 And MenuId = 44 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,                    matauang,     saldoNol
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_BankHarian_Global(WebsiteAccessKey & "★M0_BankHarian_Global★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 44, RITEM = 2 ================================> BANK HARIAN (AKUN LAWAN)
            ElseIf ModuleId = 2 And MenuId = 44 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,                    matauang
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_BankHarian_AkunLawan(WebsiteAccessKey & "★M0_BankHarian_Global★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 45, RITEM = 1 ================================> POSISI KEUANGAN (NERACA)
            ElseIf ModuleId = 2 And MenuId = 45 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PosisiKeuangan(WebsiteAccessKey & "★M0_PosisiKeuangan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 45, RITEM = 2 ================================> POSISI KEUANGAN T (NERACA T)
            ElseIf ModuleId = 2 And MenuId = 45 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PosisiKeuanganT(WebsiteAccessKey & "★M0_PosisiKeuanganT★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 46, RITEM = 1 ================================> LABA RUGI
            ElseIf ModuleId = 2 And MenuId = 46 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_LabaRugi(WebsiteAccessKey & "★M0_LabaRugi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 46, RITEM = 5 ================================> LABA RUGI ANGGARAN
            ElseIf ModuleId = 2 And MenuId = 46 And RItem = 5 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_LabaRugiAnggaran(WebsiteAccessKey & "★M0_LabaRugiAnggaran★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 46, RITEM = 2 ================================> LABA RUGI TAHUN BERJALAN
            ElseIf ModuleId = 2 And MenuId = 46 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_LabaRugiTahunBerjalan(WebsiteAccessKey & "★M0_LabaRugiTahunBerjalan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 46, RITEM = 6 ================================> LABA RUGI TAHUN BERJALAN ANGGARAN
            ElseIf ModuleId = 2 And MenuId = 46 And RItem = 6 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_LabaRugiTahunBerjalanAnggaran(WebsiteAccessKey & "★M0_LabaRugiTahunBerjalanAnggaran★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 46, RITEM = 3 ================================> LABA RUGI PERTAHUN
            ElseIf ModuleId = 2 And MenuId = 46 And RItem = 3 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         level,                      saldoNol,           pembagiNominal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_LabaRugi_PerTahun(WebsiteAccessKey & "★M0_LabaRugi_PerTahun★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 46, RITEM = 4 ================================> LABA RUGI TAHUN
            ElseIf ModuleId = 2 And MenuId = 46 And RItem = 4 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,         bulan,                         level,                      saldoNol,           pembagiNominal
                detail = String.Concat(paramDetailSplit(0), sptField, 12, sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_LabaRugiTahunan(WebsiteAccessKey & "★M0_LabaRugiTahunan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 46, RITEM = 7 ================================> LABA RUGI MULTI PERIODE
            ElseIf ModuleId = 2 And MenuId = 46 And RItem = 7 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,       bulan,                         level,                      saldoNol,           pembagiNominal
                detail = String.Concat(paramDetailSplit(0), sptField, 0, sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_LabaRugiTahun(WebsiteAccessKey & "★M0_LabaRugiTahun★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 47, RITEM = 1 ================================> KARTU PIUTANG 
            ElseIf ModuleId = 2 And MenuId = 47 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 9 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARCard(WebsiteAccessKey & "★M0_ARCard★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 48, RITEM = 1 ================================> REKAP PIUTANG/PERINCIAN PIUTANG
            ElseIf ModuleId = 2 And MenuId = 48 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 9 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARSummary(WebsiteAccessKey & "★M0_ARSummary★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 48, RITEM = 2 ================================> REKAP PIUTANG/PERINCIAN PIUTANG SPLIT
            ElseIf ModuleId = 2 And MenuId = 48 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 9 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARSummarySplit_PCI(WebsiteAccessKey & "★M0_ARSummarySplit_PCI★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 48, RITEM = 3 ================================> REKAP PIUTANG DETAIL
            ElseIf ModuleId = 2 And MenuId = 48 And RItem = 3 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 9 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARSummary_PCI(WebsiteAccessKey & "★M0_ARSummary_PCI★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 49, RITEM = 1 ================================> VOUCHER PIUTANG
            ElseIf ModuleId = 2 And MenuId = 49 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 12 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,                      statusJT,                      tglJTAwal,                      tglJTAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode", sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARVoucher(WebsiteAccessKey & "★M0_ARVoucher★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 49, RITEM = 2 ================================> VOUCHER PIUTANG PERSALESMAN
            ElseIf ModuleId = 2 And MenuId = 49 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 12 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,                      statusJT,                      tglJTAwal,                      tglJTAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode", sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARVoucher_PerSalesman(WebsiteAccessKey & "★M0_ARVoucher_PerSalesman★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 49, RITEM = 3 ================================> AR Estimate
            ElseIf ModuleId = 2 And MenuId = 49 And RItem = 3 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 12 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,                      statusJT,                      tglJTAwal,                      tglJTAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode", sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_AREstimate(WebsiteAccessKey & "★M0_AREstimate★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)



                'MODULEID = 2, MENUID = 50, RITEM = 1 ================================> KARTU HUTANG 
            ElseIf ModuleId = 2 And MenuId = 50 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, "", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, "", sptField, paramDetailSplit(5), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APCard(WebsiteAccessKey & "★M0_APCard★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 51, RITEM = 1 ================================> REKAP HUTANG/PERINCIAN HUTANG
            ElseIf ModuleId = 2 And MenuId = 51 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, "", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, "", sptField, paramDetailSplit(5), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APSummary(WebsiteAccessKey & "★M0_APSummary★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 51, RITEM = 2 ================================> REKAP HUTANG/PERINCIAN HUTANG SPLIT
            ElseIf ModuleId = 2 And MenuId = 51 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, "", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, "", sptField, paramDetailSplit(5), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APSummarySplit(WebsiteAccessKey & "★M0_APSummarySplit★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 52, RITEM = 1 ================================> VOUCHER HUTANG
            ElseIf ModuleId = 2 And MenuId = 52 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 9 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy,                       statusJT,                     tglJTAwal,                    tglJTAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, "", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, "", sptField, paramDetailSplit(5), sptField, "kkode", sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APVoucher(WebsiteAccessKey & "★M0_APVoucher★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 52, RITEM = 2 ================================> Daily AP Estimate
            ElseIf ModuleId = 2 And MenuId = 52 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 9 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy,                       statusJT,                     tglJTAwal,                    tglJTAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, "", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, "", sptField, paramDetailSplit(5), sptField, "kkode", sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APEstimate(WebsiteAccessKey & "★M0_APEstimate★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 53, RITEM = 1 ================================> KARTU UM PENJUALAN 
            ElseIf ModuleId = 2 And MenuId = 53 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 9 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_UMPenjualanCard(WebsiteAccessKey & "★M0_UMPenjualanCard★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 54, RITEM = 1 ================================> REKAP UM PENJUALAN
            ElseIf ModuleId = 2 And MenuId = 54 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 9 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_UMPenjualanSummary(WebsiteAccessKey & "★M0_UMPenjualanSummary★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 55, RITEM = 1 ================================> VOUCHER UM PENJUALAN
            ElseIf ModuleId = 2 And MenuId = 55 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,                      statusJT
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode", sptField, paramDetailSplit(9))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_UMPenjualanVoucher(WebsiteAccessKey & "★M0_UMPenjualanVoucher★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 56, RITEM = 1 ================================> KARTU UM PEMBELIAN 
            ElseIf ModuleId = 2 And MenuId = 56 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, "", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, "", sptField, paramDetailSplit(5), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_UMPembelianCard(WebsiteAccessKey & "★M0_UMPembelianCard★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 57, RITEM = 1 ================================> REKAP UM PEMBELIAN
            ElseIf ModuleId = 2 And MenuId = 57 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, "", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, "", sptField, paramDetailSplit(5), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_UMPembelianSummary(WebsiteAccessKey & "★M0_UMPembelianSummary★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 58, RITEM = 1 ================================> VOUCHER UM PEMBELIAN
            ElseIf ModuleId = 2 And MenuId = 58 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy,                       statusJT
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, "", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, "", sptField, paramDetailSplit(5), sptField, "kkode", sptField, paramDetailSplit(6))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_UMPembelianVoucher(WebsiteAccessKey & "★M0_UMPembelianVoucher★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 59, RITEM = 1 ================================> KARTU PIUTANG ONGKOS KIRIM
            ElseIf ModuleId = 2 And MenuId = 59 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 9 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARPostageCard(WebsiteAccessKey & "★M0_ARPostageCard★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 60, RITEM = 1 ================================> REKAP PIUTANG ONGKOS KIRIM
            ElseIf ModuleId = 2 And MenuId = 60 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 9 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARPostageSummary(WebsiteAccessKey & "★M0_ARPostageSummary★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 61, RITEM = 1 ================================> VOUCHER PIUTANG ONGKOS KIRIM
            ElseIf ModuleId = 2 And MenuId = 61 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,                      statusJT
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode", sptField, paramDetailSplit(9))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARPostageVoucher(WebsiteAccessKey & "★M0_ARPostageVoucher★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 62, RITEM = 1 ================================> KARTU TERIMA PEMBAYARAN
            ElseIf ModuleId = 2 And MenuId = 62 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 9 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_IPCard(WebsiteAccessKey & "★M0_IPCard★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 63, RITEM = 1 ================================> REKAP TERIMA PEMBAYARAN
            ElseIf ModuleId = 2 And MenuId = 63 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 9 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_IPSummary(WebsiteAccessKey & "★M0_IPSummary★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 64, RITEM = 1 ================================> VOUCHER TERIMA PEMBAYARAN
            ElseIf ModuleId = 2 And MenuId = 64 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,                      statusJT
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode", sptField, paramDetailSplit(9))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_IPVoucher(WebsiteAccessKey & "★M0_IPVoucher★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 68, RITEM = 1 ================================> REKAP PER COST CENTER
            ElseIf ModuleId = 2 And MenuId = 68 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                    norekAkhir,                costCenterAwal,               costCenterAkhir,         area,kontakCategory,customerCategory,supplierCategory,kontakAwal,kontakAkhir,salesmanCategory,salesman,                      matauang,                 orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "bpcostcenter")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Bp_CostCenterSummary(WebsiteAccessKey & "★M0_Bp_CostCenterSummary★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 69, RITEM = 1 ================================> BUKU BESAR PER COST CENTER
            ElseIf ModuleId = 2 And MenuId = 69 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                    norekAkhir,                costCenterAwal,               costCenterAkhir,         area,kontakCategory,customerCategory,supplierCategory,kontakAwal,kontakAkhir,salesmanCategory,salesman,                      matauang,                 orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "bpcostcenter")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Bp_CostCenterBukuBesar(WebsiteAccessKey & "★M0_Bp_CostCenterBukuBesar★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 44, RITEM = 5 ================================> DAFTAR TERIMA PEMBAYARAN DETAIL
            ElseIf ModuleId = 5 And MenuId = 44 And RItem = 5 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 9 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,           statusJT
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode", sptField, 2)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_IPList(WebsiteAccessKey & "★M0_IPList★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 78, RITEM = 1 ================================> PERSEDIAAN BARANG PER GUDANG
            ElseIf ModuleId = 2 And MenuId = 78 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PersediaanPerGudang(WebsiteAccessKey & "★M0_PersediaanPerGudang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 78, RITEM = 2 ================================> PERSEDIAAN BARANG PER KATEGORI
            ElseIf ModuleId = 2 And MenuId = 78 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PersediaanPerGudang(WebsiteAccessKey & "★M0_PersediaanPerGudang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                '    'MODULEID = 3, MENUID = 37, RITEM = 1 ================================> MUTASI STOK
                'ElseIf ModuleId = 3 And MenuId = 37 And RItem = 1 Then

                '    'CEK PARAMETER REPORT
                '    If paramDetailSplit.Length <> 6 Then
                '        'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                '        s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                '    End If

                '    'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,                    gudangAwal,                    gudangAwal,            OrderBy
                '    detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "bkode")

                '    'PANGGIL FUNGSI REPORT PROGRESS
                '    strRptProgress = clsRptProgress.M0_MutasiStok(WebsiteAccessKey & "★M0_MutasiStok★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)




                '    'MODULEID = 3, MENUID = 37, RITEM = 1 ================================> MUTASI STOK
                'ElseIf ModuleId = 3 And MenuId = 37 And RItem = 1 Then

                '    'CEK PARAMETER REPORT
                '    If paramDetailSplit.Length <> 10 Then
                '        'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                '        s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                '    End If

                '    'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,                    cabangAwal,                   cabangAkhir,                    lokasiAwal,                   lokasiAkhir,                    gudangAwal,                    gudangAwal, costcenterAwal, costcenterAkhir, divisiawal, divisiAkhir, subdivisiAwal, subdivisiakhir, proyekAwal, proyekAkhir,         OrderBy
                '    detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "bkode")

                '    'PANGGIL FUNGSI REPORT PROGRESS
                '    Dim clsRptOptimasi As New ReportOptimasi
                '    strRptProgress = clsRptOptimasi.M0_MutasiStok_Detail(WebsiteAccessKey & "★M0_MutasiStok_Detail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 37, RITEM = 1 ================================> MUTASI STOK
            ElseIf ModuleId = 3 And MenuId = 37 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,                    cabangAwal,                   cabangAkhir,                    lokasiAwal,                   lokasiAkhir,                    gudangAwal,                    gudangAwal, costcenterAwal, costcenterAkhir, divisiawal, divisiAkhir, subdivisiAwal, subdivisiakhir, proyekAwal, proyekAkhir,         OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                Dim clsRptOptimasi As New ReportOptimasi
                strRptProgress = clsRptOptimasi.M0_MutasiStok_Detail(WebsiteAccessKey & "★M0_MutasiStok_Detail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 38, RITEM = 3 ================================> REKAP STOK BATCH PERTANGGAL
            ElseIf ModuleId = 3 And MenuId = 38 And RItem = 3 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    gudangAwal,                   gudangAkhir,            kategoriBarangAwal,           kategoriBarangAkhir,                    barangAwal,                   barangAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_StokBatchPertanggal(WebsiteAccessKey & "★M0_StokBatchPertanggal★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 38, RITEM = 4 ================================> MUTASI STOK BATCH PERTANGGAL
            ElseIf ModuleId = 3 And MenuId = 38 And RItem = 4 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    gudangAwal,                   gudangAkhir,            kategoriBarangAwal,           kategoriBarangAkhir,                    barangAwal,                   barangAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_MutasiStokBatchPertanggal(WebsiteAccessKey & "★M0_MutasiStokBatchPertanggal★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 7, RITEM = 28 ================================> STOK OPNAME
            ElseIf ModuleId = 3 And MenuId = 7 And RItem = 28 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    gudangAwal,                   gudangAkhir,                    barangAwal,                    barangAkhir                  combo                      
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_StokOpname(WebsiteAccessKey & "★M0_StokOpname★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 7, RITEM = 31 ================================> STOK OPNAME
            ElseIf ModuleId = 3 And MenuId = 7 And RItem = 31 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    gudangAwal,                   gudangAkhir,                    barangAwal,                    barangAkhir                  combo
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_StokOpname(WebsiteAccessKey & "★M0_StokOpname★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 7, RITEM = 32 ================================> STOK OPNAME
            ElseIf ModuleId = 3 And MenuId = 7 And RItem = 32 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    gudangAwal,                   gudangAkhir,                    barangAwal,                    barangAkhir                  combo
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_StokOpname(WebsiteAccessKey & "★M0_StokOpname★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)




                'MODULEID = 3, MENUID = 37, RITEM = 5 ================================> MUTASI STOK REKAP
            ElseIf ModuleId = 3 And MenuId = 37 And RItem = 5 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 11 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,                    cabangAwal,                   cabangAkhir,                    lokasiAwal,                   lokasiAkhir,                    gudangAwal,                    gudangAwal,           costcenterAwal, costcenterAkhir, divisiawal, divisiAkhir, subdivisiAwal, subdivisiakhir, proyekAwal, proyekAkhir,         OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "", sptField, "", sptField, paramDetailSplit(10), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "bkode")
                's(1) = detail & "-" & paramDetailSplit.Length.ToString : GoTo selesai
                'PANGGIL FUNGSI REPORT PROGRESS
                Dim clsRptOptimasi As New ReportOptimasi
                strRptProgress = clsRptOptimasi.M0_MutasiStok_Rekap(WebsiteAccessKey & "★M0_MutasiStok_Rekap★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 37, RITEM = 9 ================================> MUTASI STOK REKAP PER SUMBER
            ElseIf ModuleId = 3 And MenuId = 37 And RItem = 9 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 40 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :               tanggalAwal,                tanggalAkhir,                   cabangAwal,                     cabangAkhir,                    lokasiAwal,                     lokasiAkhir,                    gudangAwal,                 gudangAkhir,                    kodeAwal,                       kodeAkhir,                  kategoriAwal,                   kategoriAkhir, costcenterAwal, costcenterAkhir,               divisiAwal,                     divisiAkhir,                subdivisiAwal,                  subdivisiAkhir,   proyekAwal,  proyekAkhir,                departemenAwal,                 departemenAkhir,            subdepartemenAwal,              subdepartemenAkhir,                     classAwal,                      classAkhir,                 subclassAwal,                   subclassAkhir,                      styleAwal,                      styleAkhir,                 materialAwal,                    materialAkhir,                     colorAwal,                      colorAkhir,                     sizeAwal,                       sizeAkhir,                  sectionAwal,                    sectionAkhir,                       vendorAwal,                 vendorAkhir,                        brandAwal,                   brandAkhir,                    designerAwal,                   designerAkhir,        oemAwal,    oemAkhir,    satuanAwal,     satuanAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, "", sptField, "", sptField, paramDetailSplit(12), sptField, paramDetailSplit(13), sptField, paramDetailSplit(14), sptField, paramDetailSplit(15), sptField, "", sptField, "", sptField, paramDetailSplit(16), sptField, paramDetailSplit(17), sptField, paramDetailSplit(18), sptField, paramDetailSplit(19), sptField, paramDetailSplit(20), sptField, paramDetailSplit(21), sptField, paramDetailSplit(22), sptField, paramDetailSplit(23), sptField, paramDetailSplit(24), sptField, paramDetailSplit(25), sptField, paramDetailSplit(26), sptField, paramDetailSplit(27), sptField, paramDetailSplit(28), sptField, paramDetailSplit(29), sptField, paramDetailSplit(30), sptField, paramDetailSplit(31), sptField, paramDetailSplit(32), sptField, paramDetailSplit(33), sptField, paramDetailSplit(34), sptField, paramDetailSplit(35), sptField, paramDetailSplit(36), sptField, paramDetailSplit(37), sptField, paramDetailSplit(38), sptField, paramDetailSplit(39), sptField, "", sptField, "", sptField, "", sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_MutasiStok_RekapSumber(WebsiteAccessKey & "★M0_MutasiStok_RekapSumber★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 37, RITEM = 8 ================================> INVENTORY HISTORIS PER GRUP
            ElseIf ModuleId = 3 And MenuId = 37 And RItem = 8 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,                    cabangAwal,                   cabangAkhir,                    lokasiAwal,                   lokasiAkhir,                    gudangAwal,                    gudangAwal, costcenterAwal, costcenterAkhir, divisiawal, divisiAkhir, subdivisiAwal, subdivisiakhir, proyekAwal, proyekAkhir,         OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_MutasiStok_Rekap(WebsiteAccessKey & "★M0_MutasiStok_Rekap★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 37, RITEM = 6 ================================> MUTASI STOK REKAP PER SHIFT 
            ElseIf ModuleId = 3 And MenuId = 37 And RItem = 6 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 11 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,                    cabangAwal,                   cabangAkhir,                    lokasiAwal,                   lokasiAkhir,                    gudangAwal,                    gudangAwal,                          shift   costcenterAwal, costcenterAkhir, divisiawal, divisiAkhir, subdivisiAwal, subdivisiakhir, proyekAwal, proyekAkhir,         OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_MutasiStok_Rekap_pershift(WebsiteAccessKey & "★M0_MutasiStok_Rekap_pershift★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

            ElseIf ModuleId = 3 And MenuId = 37 And RItem = 7 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 11 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,                    cabangAwal,                   cabangAkhir,                    lokasiAwal,                   lokasiAkhir,                    gudangAwal,                    gudangAwal,                          shift   costcenterAwal, costcenterAkhir, divisiawal, divisiAkhir, subdivisiAwal, subdivisiakhir, proyekAwal, proyekAkhir,         OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_MutasiStok_Rekap_pershift(WebsiteAccessKey & "★M0_MutasiStok_Rekap_pershift★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)



                'MODULEID = 3, MENUID = 37, RITEM = 2 ================================> MUTASI STOK COSTCENTER
            ElseIf ModuleId = 3 And MenuId = 37 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 12 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,                    cabangAwal,                   cabangAkhir,                    lokasiAwal,                   lokasiAkhir,                    gudangAwal,                    gudangAwal,                 costcenterAwal,                costcenterAkhir, divisiawal, divisiAkhir, subdivisiAwal, subdivisiakhir, proyekAwal, proyekAkhir,         OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_MutasiStok_Detail(WebsiteAccessKey & "★M0_MutasiStok_Detail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 37, RITEM = 3 ================================> MUTASI STOK DIVISI
            ElseIf ModuleId = 3 And MenuId = 37 And RItem = 3 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 12 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,                    cabangAwal,                   cabangAkhir,                    lokasiAwal,                   lokasiAkhir,                    gudangAwal,                    gudangAwal, costcenterAwal, costcenterAkhir,                divisiawal,                    divisiAkhir, subdivisiAwal, subdivisiakhir, proyekAwal, proyekAkhir,         OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "", sptField, "", sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_MutasiStok_Detail(WebsiteAccessKey & "★M0_MutasiStok_Detail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 37, RITEM = 4 ================================> MUTASI STOK PROYEK
            ElseIf ModuleId = 3 And MenuId = 37 And RItem = 4 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 12 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,                    cabangAwal,                   cabangAkhir,                    lokasiAwal,                   lokasiAkhir,                    gudangAwal,                    gudangAwal, costcenterAwal, costcenterAkhir, divisiawal, divisiAkhir, subdivisiAwal, subdivisiakhir,                proyekAwal,                    proyekAkhir,         OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_MutasiStok_Detail(WebsiteAccessKey & "★M0_MutasiStok_Detail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 36, RITEM = 2 ================================> KARTU STOK AVERAGE
            ElseIf ModuleId = 3 And MenuId = 36 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_KartuStok_Average(WebsiteAccessKey & "★M0_KartuStok_Average★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 36, RITEM = 1 ================================> KARTU STOK FIFO
            ElseIf ModuleId = 3 And MenuId = 36 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_KartuStok_Fifo(WebsiteAccessKey & "★M0_KartuStok_Fifo★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 3, MENUID = 36, RITEM = 6 ================================> KARTU STOK FIFO
            ElseIf ModuleId = 3 And MenuId = 36 And RItem = 6 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_KartuStok_Fifo_Rekap(WebsiteAccessKey & "★M0_KartuStok_Fifo_Rekap★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 36, RITEM = 5 ================================> KARTU STOK FIFO
            ElseIf ModuleId = 3 And MenuId = 36 And RItem = 5 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_KartuStok_Fifo(WebsiteAccessKey & "★M0_KartuStok_Fifo★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 3, MENUID = 36, RITEM = 4 ================================> KARTU STOK FIFO 2
            ElseIf ModuleId = 3 And MenuId = 36 And RItem = 4 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_KartuStok_Fifo(WebsiteAccessKey & "★M0_KartuStok_Fifo★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 3, MENUID = 36, RITEM = 3 ================================> KARTU STOK KHUSUS
            ElseIf ModuleId = 3 And MenuId = 36 And RItem = 3 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_KartuStok_Khusus(WebsiteAccessKey & "★M0_KartuStok_Khusus★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 3, MENUID = 36, RITEM = 10 ================================> LAPORAN STOK
            ElseIf ModuleId = 3 And MenuId = 36 And RItem = 10 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 13 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                supplier                 barangAwal,                      barangAkhir,                    gudang1,                   gudang2,                     gudang3,                    gudang4,                            gudang5,                    gudang6,                        gudang7,                        gudang8,                        gudang9,                    gudang10   
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.m0_stok(WebsiteAccessKey & "★m0_stok★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 3, MENUID = 36, RITEM = 11 ================================> KARTU STOK AVERAGE
            ElseIf ModuleId = 3 And MenuId = 36 And RItem = 11 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_KartuStok_Average(WebsiteAccessKey & "★M0_KartuStok_Average★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)



                'MODULEID = 2, MENUID = 81, RITEM = 1 ================================> Laba Rugi Invoice (Global)
            ElseIf ModuleId = 2 And MenuId = 81 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 15 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                      cabangAWal                  cabangAkhir                     lokasiAWal                  lokasiAkhir              kodeSalesman,              kodeCustomerAwal,             kodeCustomerAkhir,                 sinotransaksi,            sinotransaksiakhir,                           hpp,                   jenisbarang,                kodeBarangAwal,               kodeBarangAkhir, tampilkandetail
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12), sptField, paramDetailSplit(13), sptField, paramDetailSplit(14), sptField, 0)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Labarugi_Invoice(WebsiteAccessKey & "★M0_Labarugi_Invoice★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 81, RITEM = 2 ================================> Laba Rugi Invoice (Detail)
            ElseIf ModuleId = 2 And MenuId = 81 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 15 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                      cabangAWal                      cabangAkhir                     lokasiAwal              lokasiAkhier              kodeSalesman,              kodeCustomerAwal,             kodeCustomerAkhir,                 sinotransaksi,            sinotransaksiakhir,                           hpp,                   jenisbarang,                kodeBarangAwal,               kodeBarangAkhir, tampilkandetail
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12), sptField, paramDetailSplit(13), sptField, paramDetailSplit(14), sptField, 1)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Labarugi_Invoice(WebsiteAccessKey & "★M0_Labarugi_Invoice★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 81, RITEM = 3 ================================> SUMMARY SALES REPORT
            ElseIf ModuleId = 2 And MenuId = 81 And RItem = 3 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 15 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                      cabangAWal                      cabangAkhir                     lokasiAwal              lokasiAkhier              kodeSalesman,              kodeCustomerAwal,             kodeCustomerAkhir,                 sinotransaksi,            sinotransaksiakhir,                           hpp,                   jenisbarang,                kodeBarangAwal,               kodeBarangAkhir, tampilkandetail
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12), sptField, paramDetailSplit(13), sptField, paramDetailSplit(14), sptField, 1)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Labarugi_Invoice(WebsiteAccessKey & "★M0_Labarugi_Invoice★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 12, MENUID = 8, RITEM = 22 ================================> Laba Rugi Per Hari
            ElseIf ModuleId = 12 And MenuId = 8 And RItem = 22 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 15 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                      cabangAWal                  cabangAkhir                     lokasiAWal                  lokasiAkhir              kodeSalesman,              kodeCustomerAwal,             kodeCustomerAkhir,                 sinotransaksi,            sinotransaksiakhir,                           hpp,                   jenisbarang,                kodeBarangAwal,               kodeBarangAkhir, tampilkandetail
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12), sptField, paramDetailSplit(13), sptField, paramDetailSplit(14), sptField, 0)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Labarugi_Invoice(WebsiteAccessKey & "★M0_Labarugi_Invoice★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 12, MENUID = 8, RITEM = 23 ================================> Laba Rugi Per Lokasi
            ElseIf ModuleId = 12 And MenuId = 8 And RItem = 23 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 15 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                      cabangAWal                  cabangAkhir                     lokasiAWal                  lokasiAkhir              kodeSalesman,              kodeCustomerAwal,             kodeCustomerAkhir,                 sinotransaksi,            sinotransaksiakhir,                           hpp,                   jenisbarang,                kodeBarangAwal,               kodeBarangAkhir, tampilkandetail
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12), sptField, paramDetailSplit(13), sptField, paramDetailSplit(14), sptField, 0)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Labarugi_Invoice(WebsiteAccessKey & "★M0_Labarugi_Invoice★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 12, MENUID = 8, RITEM = 45 ================================> Laporan Penjualan Bulanan Per Tanggal 2
            ElseIf ModuleId = 12 And MenuId = 8 And RItem = 45 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                      cabangAWal                  cabangAkhir                     lokasiAWal                  lokasiAkhir                     katcustomAwal                katcustomAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan_Bulanan_Pertgl(WebsiteAccessKey & "★M0_Penjualan_Bulanan_Pertgl★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 82, RITEM = 1 ================================> Mutasi Keuangan
            ElseIf ModuleId = 2 And MenuId = 82 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 2 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tahun,                           bulan
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Mutasi_Keuangan(WebsiteAccessKey & "★M0_Mutasi_Keuangan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 41, RITEM = 3 ================================> BUKU BESAR VALAS DETAIL
            ElseIf ModuleId = 2 And MenuId = 41 And RItem = 3 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,              matauang,                            perTanggal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, 0)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GeneralLedger(WebsiteAccessKey & "★M0_GeneralLedger★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 41, RITEM = 4 ================================> BUKU BESAR VALAS GLOBAL PER TANGGAL
            ElseIf ModuleId = 2 And MenuId = 41 And RItem = 4 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,              matauang,                            perTanggal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, 1)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GeneralLedger(WebsiteAccessKey & "★M0_GeneralLedger★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 2, MENUID = 41, RITEM = 5 ================================> BUKU BESAR (AKUN LAWAN)
            ElseIf ModuleId = 2 And MenuId = 41 And RItem = 5 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,                    matauang
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_BukuBesar_AkunLawan(WebsiteAccessKey & "★M0_BukuBesar_AkunLawan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 78, RITEM = 3 ================================> PERSEDIAAN BARANG DETAIL
            ElseIf ModuleId = 2 And MenuId = 78 And RItem = 3 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PersediaanDetail(WebsiteAccessKey & "★M0_PersediaanDetail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 91, RITEM = 1 ================================> PERINCIAN BIAYA
            ElseIf ModuleId = 2 And MenuId = 91 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PerincianBiaya(WebsiteAccessKey & "★M0_PerincianBiaya★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 92, RITEM = 1 ================================> REKAP UMUR PIUTANG/PERINCIAN PIUTANG
            ElseIf ModuleId = 2 And MenuId = 92 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,                      statusJT
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode", sptField, paramDetailSplit(9))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARSummary_Aging(WebsiteAccessKey & "★M0_ARSummary_Aging★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 92, RITEM = 2 ================================> VOUCHER UMUR PIUTANG
            ElseIf ModuleId = 2 And MenuId = 92 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,                      statusJT
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, "kkode", sptField, paramDetailSplit(9))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARVoucher_Aging(WebsiteAccessKey & "★M0_ARVoucher_Aging★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 93, RITEM = 1 ================================> REKAP UMUR HUTANG/PERINCIAN HUTANG
            ElseIf ModuleId = 2 And MenuId = 93 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy,                       statusJT
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, "", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, "", sptField, paramDetailSplit(5), sptField, "kkode", sptField, paramDetailSplit(6))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APSummary_Aging(WebsiteAccessKey & "★M0_APSummary_Aging★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 93, RITEM = 2 ================================> VOUCHER UMUR HUTANG
            ElseIf ModuleId = 2 And MenuId = 93 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy,                       statusJT
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, "", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, "", sptField, paramDetailSplit(5), sptField, "kkode", sptField, paramDetailSplit(6))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APVoucher_Aging(WebsiteAccessKey & "★M0_APVoucher_Aging★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)



                'MODULEID = 2, MENUID = 10, RITEM = 7 ================================> GIRO KELUAR PERTANGGAL
            ElseIf ModuleId = 2 And MenuId = 10 And RItem = 7 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 15 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     tglJTAwal,                    tglJTAkhir,                    kontakAwal,                   kontakAkhir,                      bankAwal,                     bankAkhir,                    nogiroAwal,                   nogiroAkhir,                      norekAwal,                     norekAkhir,                       matauang,                         status,                       statusJT,   jenisGiro,       orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12), sptField, paramDetailSplit(13), sptField, paramDetailSplit(14), sptField, 1, sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GiroVoucher_PerBank(WebsiteAccessKey & "★M0_GiroVoucher_PerBank★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 9, RITEM = 8 ================================> UMUR GIRO MASUK PERTANGGAL GLOBAL
            ElseIf ModuleId = 2 And MenuId = 9 And RItem = 8 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 15 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     tglJTAwal,                    tglJTAkhir,                    kontakAwal,                   kontakAkhir,                      bankAwal,                     bankAkhir,                    nogiroAwal,                   nogiroAkhir,                      norekAwal,                     norekAkhir,                       matauang,                         status,                       statusJT,   jenisGiro,       orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12), sptField, paramDetailSplit(13), sptField, paramDetailSplit(14), sptField, 0, sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GiroSummary_Aging(WebsiteAccessKey & "★M0_GiroSummary_Aging★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 9, RITEM = 9 ================================> UMUR GIRO MASUK PERTANGGAL DETAIL
            ElseIf ModuleId = 2 And MenuId = 9 And RItem = 9 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 15 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     tglJTAwal,                    tglJTAkhir,                    kontakAwal,                   kontakAkhir,                      bankAwal,                     bankAkhir,                    nogiroAwal,                   nogiroAkhir,                      norekAwal,                     norekAkhir,                       matauang,                         status,                       statusJT,   jenisGiro,       orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12), sptField, paramDetailSplit(13), sptField, paramDetailSplit(14), sptField, 0, sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GiroVoucher_Aging(WebsiteAccessKey & "★M0_GiroVoucher_Aging★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 9, RITEM = 10 ================================> GIRO MASUK PERTANGGAL
            ElseIf ModuleId = 2 And MenuId = 9 And RItem = 10 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 15 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     tglJTAwal,                    tglJTAkhir,                    kontakAwal,                   kontakAkhir,                      bankAwal,                     bankAkhir,                    nogiroAwal,                   nogiroAkhir,                      norekAwal,                     norekAkhir,                       matauang,                         status,                       statusJT,   jenisGiro,       orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12), sptField, paramDetailSplit(13), sptField, paramDetailSplit(14), sptField, 0, sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GiroVoucher_PerBank(WebsiteAccessKey & "★M0_GiroVoucher_PerBank★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 10, RITEM = 9 ================================> UMUR GIRO KELUAR PERTANGGAL GLOBAL
            ElseIf ModuleId = 2 And MenuId = 10 And RItem = 9 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 15 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     tglJTAwal,                    tglJTAkhir,                    kontakAwal,                   kontakAkhir,                      bankAwal,                     bankAkhir,                    nogiroAwal,                   nogiroAkhir,                      norekAwal,                     norekAkhir,                       matauang,                         status,                       statusJT,   jenisGiro,       orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12), sptField, paramDetailSplit(13), sptField, paramDetailSplit(14), sptField, 1, sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GiroSummary_Aging(WebsiteAccessKey & "★M0_GiroSummary_Aging★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 10, RITEM = 10 ================================> UMUR GIRO KELUAR PERTANGGAL DETAIL
            ElseIf ModuleId = 2 And MenuId = 10 And RItem = 10 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 15 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     tglJTAwal,                    tglJTAkhir,                    kontakAwal,                   kontakAkhir,                      bankAwal,                     bankAkhir,                    nogiroAwal,                   nogiroAkhir,                      norekAwal,                     norekAkhir,                       matauang,                         status,                       statusJT,   jenisGiro,       orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12), sptField, paramDetailSplit(13), sptField, paramDetailSplit(14), sptField, 1, sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GiroVoucher_Aging(WebsiteAccessKey & "★M0_GiroVoucher_Aging★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 104, RITEM = 1 ================================> ARUS KAS TIDAK LANGSUNG
            ElseIf ModuleId = 2 And MenuId = 104 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                      saldoNol,           pembagiNominal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ArusKasUndirect(WebsiteAccessKey & "★M0_ArusKasUndirect★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 105, RITEM = 1 ================================> BUKU BESAR CABANG
            ElseIf ModuleId = 2 And MenuId = 105 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                    norekAkhir,                    cabangAwal,                   cabangAkhir, lokasiAwal, lokasiAkhir, costCenterAwal, costCenterAkhir, divisiAwal, divisiAkhir, subdivisiAwal, subdivisiAkhir, proyekAwal, proyekAkhir, area, kontakCategory, customerCategory, supplierCategory, kontakAwal, kontakAkhir, salesmanCategory, salesman,                        matauang,              orderBy,           jenisGrup,  pertanggal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "br.bkode", sptField, "tcabang", sptField, 0)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GeneralLedger_Detail(WebsiteAccessKey & "★M0_GeneralLedger_Detail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 105, RITEM = 2 ================================> BUKU BESAR CABANG GLOBAL PERTANGGAL
            ElseIf ModuleId = 2 And MenuId = 105 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                    norekAkhir,                    cabangAwal,                   cabangAkhir, lokasiAwal, lokasiAkhir, costCenterAwal, costCenterAkhir, divisiAwal, divisiAkhir, subdivisiAwal, subdivisiAkhir, proyekAwal, proyekAkhir, area, kontakCategory, customerCategory, supplierCategory, kontakAwal, kontakAkhir, salesmanCategory, salesman,                        matauang,              orderBy,           jenisGrup,  pertanggal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "br.bkode", sptField, "tcabang", sptField, 1)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GeneralLedger_Detail(WebsiteAccessKey & "★M0_GeneralLedger_Detail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 106, RITEM = 1 ================================> BUKU BESAR LOKASI
            ElseIf ModuleId = 2 And MenuId = 106 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                    norekAkhir,   cabangAwal,  cabangAkhir,                    lokasiAwal,                   lokasiAkhir, costCenterAwal, costCenterAkhir, divisiAwal, divisiAkhir, subdivisiAwal, subdivisiAkhir, proyekAwal, proyekAkhir, area, kontakCategory, customerCategory, supplierCategory, kontakAwal, kontakAkhir, salesmanCategory, salesman,                     matauang,              orderBy,           jenisGrup,  pertanggal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, "", sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "lc.lkode", sptField, "tlokasi", sptField, 0)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GeneralLedger_Detail(WebsiteAccessKey & "★M0_GeneralLedger_Detail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 106, RITEM = 2 ================================> BUKU BESAR LOKASI GLOBAL PERTANGGAL
            ElseIf ModuleId = 2 And MenuId = 106 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                    norekAkhir,   cabangAwal,  cabangAkhir,                    lokasiAwal,                   lokasiAkhir, costCenterAwal, costCenterAkhir, divisiAwal, divisiAkhir, subdivisiAwal, subdivisiAkhir, proyekAwal, proyekAkhir, area, kontakCategory, customerCategory, supplierCategory, kontakAwal, kontakAkhir, salesmanCategory, salesman,                     matauang,              orderBy,           jenisGrup,  pertanggal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, "", sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "lc.lkode", sptField, "tlokasi", sptField, 1)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GeneralLedger_Detail(WebsiteAccessKey & "★M0_GeneralLedger_Detail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 107, RITEM = 1 ================================> BUKU BESAR DIVISI
            ElseIf ModuleId = 2 And MenuId = 107 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                    norekAkhir,   cabangAwal,  cabangAkhir,   lokasiAwal,  lokasiAkhir, costCenterAwal, costCenterAkhir,               divisiAwal,                   divisiAkhir, subdivisiAwal, subdivisiAkhir, proyekAwal, proyekAkhir, area, kontakCategory, customerCategory, supplierCategory, kontakAwal, kontakAkhir, salesmanCategory, salesman,                       matauang,             orderBy,           jenisGrup,  pertanggal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "d.dkode", sptField, "tdivisi", sptField, 0)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GeneralLedger_Detail(WebsiteAccessKey & "★M0_GeneralLedger_Detail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 107, RITEM = 2 ================================> BUKU BESAR DIVISI GLOBAL PERTANGGAL
            ElseIf ModuleId = 2 And MenuId = 107 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                    norekAkhir,   cabangAwal,  cabangAkhir,   lokasiAwal,  lokasiAkhir, costCenterAwal, costCenterAkhir,               divisiAwal,                   divisiAkhir, subdivisiAwal, subdivisiAkhir, proyekAwal, proyekAkhir, area, kontakCategory, customerCategory, supplierCategory, kontakAwal, kontakAkhir, salesmanCategory, salesman,                       matauang,             orderBy,           jenisGrup,  pertanggal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "d.dkode", sptField, "tdivisi", sptField, 1)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GeneralLedger_Detail(WebsiteAccessKey & "★M0_GeneralLedger_Detail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 108, RITEM = 1 ================================> BUKU BESAR PROYEK
            ElseIf ModuleId = 2 And MenuId = 108 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                    norekAkhir,   cabangAwal,  cabangAkhir,   lokasiAwal,  lokasiAkhir, costCenterAwal, costCenterAkhir, divisiAwal, divisiAkhir, subdivisiAwal, subdivisiAkhir,               proyekAwal,                   proyekAkhir, area, kontakCategory, customerCategory, supplierCategory, kontakAwal, kontakAkhir, salesmanCategory, salesman,                       matauang,             orderBy,           jenisGrup,  pertanggal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "p.pkode", sptField, "tproyek", sptField, 0)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GeneralLedger_Detail(WebsiteAccessKey & "★M0_GeneralLedger_Detail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 108, RITEM = 2 ================================> BUKU BESAR PROYEK GLOBAL PERTANGGAL
            ElseIf ModuleId = 2 And MenuId = 108 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                    norekAkhir,   cabangAwal,  cabangAkhir,   lokasiAwal,  lokasiAkhir, costCenterAwal, costCenterAkhir, divisiAwal, divisiAkhir, subdivisiAwal, subdivisiAkhir,               proyekAwal,                   proyekAkhir, area, kontakCategory, customerCategory, supplierCategory, kontakAwal, kontakAkhir, salesmanCategory, salesman,                       matauang,             orderBy,           jenisGrup,  pertanggal
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "p.pkode", sptField, "tproyek", sptField, 1)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GeneralLedger_Detail(WebsiteAccessKey & "★M0_GeneralLedger_Detail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 109, RITEM = 1 ================================> LABA RUGI CABANG
            ElseIf ModuleId = 2 And MenuId = 109 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal,                cabang
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_LabaRugi_Cabang(WebsiteAccessKey & "★M0_LabaRugi_Cabang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 110, RITEM = 1 ================================> LABA RUGI LOKASI
            ElseIf ModuleId = 2 And MenuId = 110 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal,                lokasi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_LabaRugi_Lokasi(WebsiteAccessKey & "★M0_LabaRugi_Lokasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 111, RITEM = 1 ================================> LABA RUGI DIVISI
            ElseIf ModuleId = 2 And MenuId = 111 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal,                divisi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_LabaRugi_Divisi(WebsiteAccessKey & "★M0_LabaRugi_Divisi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 112, RITEM = 1 ================================> LABA RUGI PROYEK
            ElseIf ModuleId = 2 And MenuId = 112 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal,                proyek
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_LabaRugi_ProyekGlobal(WebsiteAccessKey & "★M0_LabaRugi_ProyekGlobal★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 113, RITEM = 1 ================================> LABA RUGI COST CENTER
            ElseIf ModuleId = 2 And MenuId = 113 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal,                costcenter
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_LabaRugi_CostCenter(WebsiteAccessKey & "★M0_LabaRugi_CostCenter★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 114, RITEM = 1 ================================> POSISI KEUANGAN CABANG (NERACA)
            ElseIf ModuleId = 2 And MenuId = 114 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal,                cabang
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PosisiKeuangan_Cabang(WebsiteAccessKey & "★M0_PosisiKeuangan_Cabang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 114, RITEM = 2 ================================> POSISI KEUANGAN T CABANG (NERACA T)
            ElseIf ModuleId = 2 And MenuId = 114 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal,                cabang
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PosisiKeuanganT_Cabang(WebsiteAccessKey & "★M0_PosisiKeuanganT_Cabang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 115, RITEM = 1 ================================> POSISI KEUANGAN LOKASI (NERACA)
            ElseIf ModuleId = 2 And MenuId = 115 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal,                lokasi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PosisiKeuangan_Lokasi(WebsiteAccessKey & "★M0_PosisiKeuangan_Lokasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 115, RITEM = 2 ================================> POSISI KEUANGAN T LOKASI (NERACA T)
            ElseIf ModuleId = 2 And MenuId = 115 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal,                lokasi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PosisiKeuanganT_Lokasi(WebsiteAccessKey & "★M0_PosisiKeuanganT_Lokasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 116, RITEM = 1 ================================> POSISI KEUANGAN DIVISI (NERACA)
            ElseIf ModuleId = 2 And MenuId = 116 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal,                divisi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PosisiKeuangan_Divisi(WebsiteAccessKey & "★M0_PosisiKeuangan_Divisi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 116, RITEM = 2 ================================> POSISI KEUANGAN T DIVISI (NERACA T)
            ElseIf ModuleId = 2 And MenuId = 116 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal,                divisi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PosisiKeuanganT_Divisi(WebsiteAccessKey & "★M0_PosisiKeuanganT_Divisi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 117, RITEM = 1 ================================> POSISI KEUANGAN PROYEK (NERACA)
            ElseIf ModuleId = 2 And MenuId = 117 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal,                proyek
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PosisiKeuangan_Proyek(WebsiteAccessKey & "★M0_PosisiKeuangan_Proyek★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 117, RITEM = 2 ================================> POSISI KEUANGAN T PROYEK (NERACA T)
            ElseIf ModuleId = 2 And MenuId = 117 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal,                proyek
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PosisiKeuanganT_Proyek(WebsiteAccessKey & "★M0_PosisiKeuanganT_Proyek★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 118, RITEM = 1 ================================> POSISI KEUANGAN COST CENTER (NERACA)
            ElseIf ModuleId = 2 And MenuId = 118 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal,                costcenter
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PosisiKeuangan_Costcenter(WebsiteAccessKey & "★M0_PosisiKeuangan_Costcenter★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 118, RITEM = 2 ================================> POSISI KEUANGAN T COST CENTER (NERACA T)
            ElseIf ModuleId = 2 And MenuId = 118 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,                         bulan,                         level,                      saldoNol,           pembagiNominal,                costcenter
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PosisiKeuanganT_Costcenter(WebsiteAccessKey & "★M0_PosisiKeuanganT_Costcenter★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 119, RITEM = 1 ================================> KARTU PIUTANG CABANG
            ElseIf ModuleId = 2 And MenuId = 119 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,           cabang
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "kkode", sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARCard_Cabang(WebsiteAccessKey & "★M0_ARCard_Cabang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 120, RITEM = 1 ================================> KARTU PIUTANG LOKASI
            ElseIf ModuleId = 2 And MenuId = 120 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,           lokasi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "kkode", sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARCard_Lokasi(WebsiteAccessKey & "★M0_ARCard_Lokasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 121, RITEM = 1 ================================> REKAP PIUTANG/PERINCIAN PIUTANG CABANG
            ElseIf ModuleId = 2 And MenuId = 121 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,           cabang
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "kkode", sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARSummary_Cabang(WebsiteAccessKey & "★M0_ARSummary_Cabang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 122, RITEM = 1 ================================> REKAP PIUTANG/PERINCIAN PIUTANG LOKASI
            ElseIf ModuleId = 2 And MenuId = 122 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,           lokasi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "kkode", sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARSummary_Lokasi(WebsiteAccessKey & "★M0_ARSummary_Lokasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 123, RITEM = 1 ================================> VOUCHER PIUTANG CABANG
            ElseIf ModuleId = 2 And MenuId = 123 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 13 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,                      statusJT,           cabang,                                   tglJTAwal,                      tglJTAkhir
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "kkode", sptField, paramDetailSplit(10), sptField, paramDetailSplit(0), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARVoucher_Cabang(WebsiteAccessKey & "★M0_ARVoucher_Cabang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 124, RITEM = 1 ================================> VOUCHER PIUTANG LOKASI 
            ElseIf ModuleId = 2 And MenuId = 124 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 13 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,                      statusJT,           lokasi,                                   tglJTAwal,                      tglJTAkhir
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "kkode", sptField, paramDetailSplit(10), sptField, paramDetailSplit(0), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARVoucher_Lokasi(WebsiteAccessKey & "★M0_ARVoucher_Lokasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 125, RITEM = 1 ================================> REKAP UMUR PIUTANG/PERINCIAN PIUTANG CABANG
            ElseIf ModuleId = 2 And MenuId = 125 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 11 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,                      statusJT,           cabang
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "kkode", sptField, paramDetailSplit(10), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARSummary_Aging_Cabang(WebsiteAccessKey & "★M0_ARSummary_Aging_Cabang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 125, RITEM = 2 ================================> VOUCHER UMUR PIUTANG CABANG
            ElseIf ModuleId = 2 And MenuId = 125 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 11 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,                      statusJT,           cabang
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "kkode", sptField, paramDetailSplit(10), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARVoucher_Aging_Cabang(WebsiteAccessKey & "★M0_ARVoucher_Aging_Cabang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 126, RITEM = 1 ================================> REKAP UMUR PIUTANG/PERINCIAN PIUTANG LOKASI
            ElseIf ModuleId = 2 And MenuId = 126 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 11 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,                      statusJT,           lokasi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "kkode", sptField, paramDetailSplit(10), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARSummary_Aging_Lokasi(WebsiteAccessKey & "★M0_ARSummary_Aging_Lokasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 126, RITEM = 2 ================================> VOUCHER UMUR PIUTANG LOKASI
            ElseIf ModuleId = 2 And MenuId = 126 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 11 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                          area,              customerCategory,                  customerAwal,                 customerAkhir,              salesmanCategory,                      salesman,                      matauang,           orderBy,                      statusJT,           lokasi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, "kkode", sptField, paramDetailSplit(10), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_ARVoucher_Aging_Lokasi(WebsiteAccessKey & "★M0_ARVoucher_Aging_Lokasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 127, RITEM = 1 ================================> KARTU HUTANG CABANG
            ElseIf ModuleId = 2 And MenuId = 127 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy,            cabang
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, "", sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "kkode", sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APCard_Cabang(WebsiteAccessKey & "★M0_APCard_Cabang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 128, RITEM = 1 ================================> KARTU HUTANG LOKASI
            ElseIf ModuleId = 2 And MenuId = 128 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy,            lokasi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, "", sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "kkode", sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APCard_Lokasi(WebsiteAccessKey & "★M0_APCard_Lokasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 129, RITEM = 1 ================================> REKAP HUTANG/PERINCIAN HUTANG CABANG
            ElseIf ModuleId = 2 And MenuId = 129 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy,            cabang
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, "", sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "kkode", sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APSummary_Cabang(WebsiteAccessKey & "★M0_APSummary_Cabang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 130, RITEM = 1 ================================> REKAP HUTANG/PERINCIAN HUTANG LOKASI
            ElseIf ModuleId = 2 And MenuId = 130 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy,            lokasi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, "", sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "kkode", sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APSummary_Lokasi(WebsiteAccessKey & "★M0_APSummary_Lokasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 131, RITEM = 1 ================================> VOUCHER HUTANG CABANG
            ElseIf ModuleId = 2 And MenuId = 131 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy,                       statusJT,           cabang,                                    tglJTAwal,                      tglJTAkhir
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, "", sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "kkode", sptField, paramDetailSplit(7), sptField, paramDetailSplit(0), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APVoucher_Cabang(WebsiteAccessKey & "★M0_APVoucher_Cabang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 132, RITEM = 1 ================================> VOUCHER HUTANG LOKASI
            ElseIf ModuleId = 2 And MenuId = 132 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy,                       statusJT,           lokasi,                                    tglJTAwal,                      tglJTAkhir
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, "", sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "kkode", sptField, paramDetailSplit(7), sptField, paramDetailSplit(0), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APVoucher_Lokasi(WebsiteAccessKey & "★M0_APVoucher_Lokasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 133, RITEM = 1 ================================> REKAP UMUR HUTANG/PERINCIAN HUTANG CABANG
            ElseIf ModuleId = 2 And MenuId = 133 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy,                       statusJT,           cabang
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, "", sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "kkode", sptField, paramDetailSplit(7), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APSummary_Aging_Cabang(WebsiteAccessKey & "★M0_APSummary_Aging_Cabang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 133, RITEM = 2 ================================> VOUCHER UMUR HUTANG CABANG
            ElseIf ModuleId = 2 And MenuId = 133 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy,                       statusJT,           cabang
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, "", sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "kkode", sptField, paramDetailSplit(7), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APVoucher_Aging_Cabang(WebsiteAccessKey & "★M0_APVoucher_Aging_Cabang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 134, RITEM = 1 ================================> REKAP UMUR HUTANG/PERINCIAN HUTANG LOKASI
            ElseIf ModuleId = 2 And MenuId = 134 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy,                       statusJT,           lokasi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, "", sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "kkode", sptField, paramDetailSplit(7), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APSummary_Aging_Lokasi(WebsiteAccessKey & "★M0_APSummary_Aging_Lokasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 134, RITEM = 2 ================================> VOUCHER UMUR HUTANG LOKASI
            ElseIf ModuleId = 2 And MenuId = 134 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,         area,              supplierCategory,                 supplierAwal,                 supplierAkhir, salesmanCategory, salesman,                      matauang,           orderBy,                       statusJT,           lokasi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, "", sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "kkode", sptField, paramDetailSplit(7), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_APVoucher_Aging_Lokasi(WebsiteAccessKey & "★M0_APVoucher_Aging_Lokasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 135, RITEM = 1 ================================> ANGGARAN DAN REALISASI GLOBAL
            ElseIf ModuleId = 2 And MenuId = 135 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,              matauang,           saldoNol
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Anggaran(WebsiteAccessKey & "★M0_Anggaran★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 135, RITEM = 2 ================================> ANGGARAN DAN REALISASI CABANG
            ElseIf ModuleId = 2 And MenuId = 135 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,              matauang,           saldoNol,                      cabang
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Anggaran_Cabang(WebsiteAccessKey & "★M0_Anggaran_Cabang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 135, RITEM = 3 ================================> ANGGARAN DAN REALISASI LOKASI
            ElseIf ModuleId = 2 And MenuId = 135 And RItem = 3 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,              matauang,           saldoNol,                      lokasi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Anggaran_Lokasi(WebsiteAccessKey & "★M0_Anggaran_Lokasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 135, RITEM = 4 ================================> ANGGARAN DAN REALISASI COST CENTER
            ElseIf ModuleId = 2 And MenuId = 135 And RItem = 4 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,              matauang,           saldoNol,                      costcenter
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Anggaran_Costcenter(WebsiteAccessKey & "★M0_Anggaran_Costcenter★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 135, RITEM = 5 ================================> ANGGARAN DAN REALISASI DIVISI
            ElseIf ModuleId = 2 And MenuId = 135 And RItem = 5 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,              matauang,           saldoNol,                      divisi
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Anggaran_Divisi(WebsiteAccessKey & "★M0_Anggaran_Divisi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 135, RITEM = 6 ================================> ANGGARAN DAN REALISASI PROYEK
            ElseIf ModuleId = 2 And MenuId = 135 And RItem = 6 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,              matauang,           saldoNol,                      proyek
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, paramDetailSplit(5), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Anggaran_Proyek(WebsiteAccessKey & "★M0_Anggaran_Proyek★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 144, RITEM = 1 ================================> NERACA MUTASI CABANG
            ElseIf ModuleId = 2 And MenuId = 144 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                   cabang               gurung tglAwal,                     tglAkhir,                      norekAwal,                     norekAkhir,          matauang,           saldoNol
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, paramDetailSplit(5))


                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_NeracaMutasi_Cabang(WebsiteAccessKey & "★M0_NeracaMutasi_Cabang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 2, MENUID = 145, RITEM = 1 ================================> NERACA MUTASI CABANG
            ElseIf ModuleId = 2 And MenuId = 145 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                   lokasi               gurung tglAwal,                     tglAkhir,                      norekAwal,                     norekAkhir,          matauang,           saldoNol
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, "", sptField, paramDetailSplit(5))


                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_NeracaMutasi_Lokasi(WebsiteAccessKey & "★M0_NeracaMutasi_Lokasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 57, RITEM = 2 ================================> SALESMAN POIN (GLOBAL)
            ElseIf ModuleId = 5 And MenuId = 57 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      salesmanCatAwal,               salesmanCatAkhir,              salesmanAreaAwal,              salesmanAreaAkhir,             salesmanAwal,                  salesmanAkhir,  customerCatAwal, customerCatAkhir, customerAreaAwal, customerAreaAkhir, customerAwal, customerAkhir, orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "salesmankode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_SalesmanPoint(WebsiteAccessKey & "★M0_SalesmanPoint★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 57, RITEM = 3 ================================> SALESMAN POIN (DETAIL)
            ElseIf ModuleId = 5 And MenuId = 57 And RItem = 3 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      salesmanCatAwal,               salesmanCatAkhir,              salesmanAreaAwal,              salesmanAreaAkhir,             salesmanAwal,                  salesmanAkhir,  customerCatAwal, customerCatAkhir, customerAreaAwal, customerAreaAkhir, customerAwal, customerAkhir, orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "salesmankode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_SalesmanPointDetail(WebsiteAccessKey & "★M0_SalesmanPointDetail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 57, RITEM = 4 ================================> SALESMAN REBATE (GLOBAL)
            ElseIf ModuleId = 5 And MenuId = 57 And RItem = 4 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      salesmanCatAwal,               salesmanCatAkhir,              salesmanAreaAwal,              salesmanAreaAkhir,             salesmanAwal,                  salesmanAkhir,  customerCatAwal, customerCatAkhir, customerAreaAwal, customerAreaAkhir, customerAwal, customerAkhir, orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "salesmankode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_SalesmanRebate(WebsiteAccessKey & "★M0_SalesmanRebate★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 57, RITEM = 5 ================================> SALESMAN REBATE (DETAIL)
            ElseIf ModuleId = 5 And MenuId = 57 And RItem = 5 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      salesmanCatAwal,               salesmanCatAkhir,              salesmanAreaAwal,              salesmanAreaAkhir,             salesmanAwal,                  salesmanAkhir,  customerCatAwal, customerCatAkhir, customerAreaAwal, customerAreaAkhir, customerAwal, customerAkhir, orderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "salesmankode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_SalesmanRebateDetail(WebsiteAccessKey & "★M0_SalesmanRebateDetail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 5, MENUID = 57, RITEM = 5 ================================> M2_COBA
            ElseIf ModuleId = 5 And MenuId = 10 And RItem = 41 Then

                If paramDetailSplit.Length <> 2 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'MODULEID = 12, MENUID = 28, RITEM = 1 ================================> POIN PELANGGAN (GLOBAL)
                strRptProgress = clsRptProgress.M0_COBA(WebsiteAccessKey & "★M0_COBA★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

            ElseIf ModuleId = 12 And MenuId = 28 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      cabangAwal,   cabangAkhir,  lokasiAwal,   lokasiAkhir,  customerCatAwal,               customerCatAkhir,              customerAwal,                  customerAkhir,                 OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_CustomerPointGlobal(WebsiteAccessKey & "★M0_CustomerPointGlobal★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)



                'MODULEID = 12, MENUID = 28, RITEM = 2 ================================> POIN PELANGGAN (DETAIL)
            ElseIf ModuleId = 12 And MenuId = 28 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      cabangAwal,   cabangAkhir,  lokasiAwal,   lokasiAkhir,  customerCatAwal,               customerCatAkhir,              customerAwal,                  customerAkhir,                 OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_CustomerPoint(WebsiteAccessKey & "★M0_CustomerPoint★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 78, RITEM = 5 ================================> NILAI PERSEDIAAN PERTANGGAL
            ElseIf ModuleId = 2 And MenuId = 78 And RItem = 5 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 3 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PersediaanPertanggal(WebsiteAccessKey & "★M0_PersediaanPertanggal★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 2, MENUID = 78, RITEM = 13 ================================> NILAI PERSEDIAAN PERTANGGAL SATUAN (ALAM INDO)
            ElseIf ModuleId = 2 And MenuId = 78 And RItem = 14 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 3 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, "bkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PersediaanPertanggal(WebsiteAccessKey & "★M0_PersediaanPertanggal★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)



                'MODULEID = 2, MENUID = 78, RITEM = 6 ================================> NILAI PERSEDIAAN PERGUDANG DETAIL
            ElseIf ModuleId = 2 And MenuId = 78 And RItem = 6 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy,                   gudangAwal,                   gudangAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "bkode", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_NilaiPersediaanGudangDetail(WebsiteAccessKey & "★M0_NilaiPersediaanGudangDetail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 78, RITEM = 7 ================================> NILAI PERSEDIAAN PERGUDANG GLOBAL
            ElseIf ModuleId = 2 And MenuId = 78 And RItem = 7 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy,                   gudangAwal,                   gudangAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "bkode", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_NilaiPersediaanGudangDetail(WebsiteAccessKey & "★M0_NilaiPersediaanGudangDetail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 78, RITEM = 11 ================================> SALDO PERSEDIAAN PERGUDANG DETAIL
            ElseIf ModuleId = 2 And MenuId = 78 And RItem = 11 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy,                   gudangAwal,                   gudangAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "bkode", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_NilaiPersediaanGudangDetail(WebsiteAccessKey & "★M0_NilaiPersediaanGudangDetail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 78, RITEM = 12 ================================> SALDO PERSEDIAAN PERGUDANG GLOBAL
            ElseIf ModuleId = 2 And MenuId = 78 And RItem = 12 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy,                   gudangAwal,                   gudangAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "bkode", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_NilaiPersediaanGudangDetail(WebsiteAccessKey & "★M0_NilaiPersediaanGudangDetail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 2, MENUID = 78, RITEM = 13 ================================> NILAI PERSEDIAAN PERTANGGAL
            ElseIf ModuleId = 2 And MenuId = 78 And RItem = 13 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy                      kategoriAwal                kategoriAKhir          
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, "bkategori", sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_PersediaanPertanggal_perkategori(WebsiteAccessKey & "★M0_PersediaanPertanggal_perkategori★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 78, RITEM = 25 ================================> Nilai Persediaan Per Tanggal Per Gudang Per Departemen
            ElseIf ModuleId = 2 And MenuId = 78 And RItem = 25 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) & detail : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy,                   gudangAwal,                   gudangAkhir                        departemenAwal          departemenAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "gudang", sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_NilaiPersediaanGudangDetail_alamindo(WebsiteAccessKey & "★M0_NilaiPersediaanGudangDetail_alamindo★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 2, MENUID = 78, RITEM = 26 ================================> Rekap Penjualan Per Barang (Week Cover)
            ElseIf ModuleId = 2 And MenuId = 78 And RItem = 26 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) & detail : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy,                   gudangAwal,                   gudangAkhir                        departemenAwal          departemenAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "gudang", sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_NilaiPersediaanGudangDetail_alamindo(WebsiteAccessKey & "★M0_NilaiPersediaanGudangDetail_alamindo★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)



                'MODULEID = 2, MENUID = 78, RITEM = 10 ================================> NILAI PERSEDIAAN PERGUDANG WIJAYA
            ElseIf ModuleId = 2 And MenuId = 78 And RItem = 10 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy,  gudangAwal,   gudangAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "bkode", sptField, "", sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_NilaiPersediaanGudangDetailWijaya(WebsiteAccessKey & "★M0_NilaiPersediaanGudangDetail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 7, MENUID = 69, RITEM = 1 ================================> ANALISA PENYUSUTAN PERTANGGAL
            ElseIf ModuleId = 7 And MenuId = 69 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tahun,                      bulan,                    kategoriAwal,                   kategoriAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_AnalisaPenyusutan(WebsiteAccessKey & "★M0_AnalisaPenyusutan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 12, MENUID = 72, RITEM = 1 ================================> Barang Harga Jual Dibawah Margin (Per Kategori POS)
            ElseIf ModuleId = 12 And MenuId = 72 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :          kategoriposAwal,              kategoriposAkhir,            kategoribarangAwal,           kategoribarangAkhir,                kodebarangAwal,               kodebarangAkhir,                        margin,                   tingkatjual,           jenis (0 = global, 1 = kategori pos)
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, 1)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_BarangDibawahMargin(WebsiteAccessKey & "★M0_BarangDibawahMargin★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 1, MENUID = 11, RITEM = 5 ================================> Barang Harga Jual Dibawah Margin (Global)
            ElseIf ModuleId = 1 And MenuId = 11 And RItem = 5 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail : kategoriposAwal, kategoriposAkhir, kategoribarangAwal,          kategoribarangAkhir,                kodebarangAwal,               kodebarangAkhir,                        margin,                   tingkatjual,           jenis (0 = global, 1 = kategori pos)
                detail = String.Concat("", sptField, "", sptField, paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, 0)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_BarangDibawahMargin(WebsiteAccessKey & "★M0_BarangDibawahMargin★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)



                'MODULEID = 2, MENUID = 107, RITEM = 3 ================================> Buku Besar Divisi (Orang)
            ElseIf ModuleId = 2 And MenuId = 107 And RItem = 3 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :              tglAwal                         tglAkhir                        akunAwal                        akunAkhir                           divisiAwal                  divisiAkhir                 comboDK
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.m2r_bb_divisi(WebsiteAccessKey & "★m2r_bb_divisi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 69, RITEM = 2 ================================> Buku Besar Cost Center (Mesin)
            ElseIf ModuleId = 2 And MenuId = 69 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 7 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :              tglAwal                         tglAkhir                        akunAwal                        akunAkhir                           divisiAwal                  divisiAkhir                 comboDK
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.m2r_bb_costcenter(WebsiteAccessKey & "★m2r_bb_costcenter★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 6, MENUID = 26, RITEM = 2 ================================> Laporan Produksi VS Penjualan VS Stok
            ElseIf ModuleId = 6 And MenuId = 26 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tahun,            kategoriBarangAwal,           kategoriBarangAkhir,                    barangAwal,                   barangAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_StokProduksiVsPenjualan(WebsiteAccessKey & "★M0_StokProduksiVsPenjualan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)



                'MODULEID = 4, MENUID = 51, RITEM = 1 ================================> Laporan Saldo Awal Hutang Global
            ElseIf ModuleId = 4 And MenuId = 51 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    sumber,            tglAwal,                    tglAkhir,                       kontakAwal,                   kontakAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_SaldoAwalHutang(WebsiteAccessKey & "★M0_SaldoAwalHutang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 4, MENUID = 51, RITEM = 2 ================================> Laporan Saldo Awal Hutang Detail
            ElseIf ModuleId = 4 And MenuId = 51 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    sumber,            tglAwal,                    tglAkhir,                       kontakAwal,                   kontakAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_SaldoAwalHutang(WebsiteAccessKey & "★M0_SaldoAwalHutang★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 11, MENUID = 67, RITEM = 4 ================================> GAJI DOKTER (dokter)
            ElseIf ModuleId = 11 And MenuId = 67 And RItem = 4 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    sumber,            tglAwal,                    tglAkhir,                       kontakAwal,                   kontakAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Gaji_Dokter(WebsiteAccessKey & "★M0_Gaji_Dokter★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 11, MENUID = 67, RITEM = 2 ================================> GAJI DOKTER (dokter_detail)
            ElseIf ModuleId = 11 And MenuId = 67 And RItem = 2 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    sumber,            tglAwal,                    tglAkhir,                       kontakAwal,                   kontakAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Gaji_Dokter(WebsiteAccessKey & "★M0_Gaji_Dokter★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 11, MENUID = 67, RITEM = 1 ================================> GAJI DOKTER (dokter_layanan)
            ElseIf ModuleId = 11 And MenuId = 67 And RItem = 1 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    sumber,            tglAwal,                    tglAkhir,                       kontakAwal,                   kontakAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Gaji_Dokter(WebsiteAccessKey & "★M0_Gaji_Dokter★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 11, MENUID = 67, RITEM = 3 ================================> GAJI DOKTER (dokter_layanan)
            ElseIf ModuleId = 11 And MenuId = 67 And RItem = 3 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    sumber,            tglAwal,                    tglAkhir,                       kontakAwal,                   kontakAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Gaji_Dokter(WebsiteAccessKey & "★M0_Gaji_Dokter★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 67, RITEM = 1 ================================> Komisi Salesman Global
            ElseIf ModuleId = 5 And MenuId = 67 And RItem = 1 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kontakawal,         kontakakhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_komisi_abd(WebsiteAccessKey & "★M0_komisi_abd★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 5, MENUID = 67, RITEM = 2 ================================> Komisi Salesman Detail
            ElseIf ModuleId = 5 And MenuId = 67 And RItem = 2 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kontakawal,         kontakakhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_komisi_abd(WebsiteAccessKey & "★M0_komisi_abd★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 50, RITEM = 8 ================================> Penjualan Per Kota Per Produk
            ElseIf ModuleId = 5 And MenuId = 50 And RItem = 8 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kotaAwal,         kotaAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan(WebsiteAccessKey & "★M0_Penjualan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 50, RITEM = 9 ================================> Penjualan Per Produk Per Kota
            ElseIf ModuleId = 5 And MenuId = 50 And RItem = 9 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kotaAwal,         kotaAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan_Per_Produk_Per_Kota(WebsiteAccessKey & "★M0_Penjualan_Per_Produk_Per_Kota★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 5, MENUID = 50, RITEM = 10 ================================> Penjualan Per Produk Per Propinsi
            ElseIf ModuleId = 5 And MenuId = 50 And RItem = 10 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kotaAwal,         kotaAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan_Per_Produk_Per_Kota(WebsiteAccessKey & "★M0_Penjualan_Per_Produk_Per_Kota★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 5, MENUID = 50, RITEM = 11 ================================> Penjualan Per Propinsi Per Produk
            ElseIf ModuleId = 5 And MenuId = 50 And RItem = 11 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kotaAwal,         kotaAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan_Per_Propinsi_Per_Produk(WebsiteAccessKey & "★M0_Penjualan_Per_Propinsi_Per_Produk★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 50, RITEM = 12 ================================> Penjualan Per Customer Per Produk
            ElseIf ModuleId = 5 And MenuId = 50 And RItem = 12 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kontakAwal,         kontakAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan_Per_Customer_Per_Produk(WebsiteAccessKey & "★M0_Penjualan_Per_Customer_Per_Produk★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 50, RITEM = 13 ================================> Penjualan Per Produk Per Customer
            ElseIf ModuleId = 5 And MenuId = 50 And RItem = 13 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kotaAwal,         kotaAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan_Per_Produk_Per_Kota(WebsiteAccessKey & "★M0_Penjualan_Per_Produk_Per_Kota★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 50, RITEM = 14 ================================> Penjualan Per Customer (Periode Tgl)
            ElseIf ModuleId = 5 And MenuId = 50 And RItem = 14 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kontakAwal,         kontakAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan_Per_Customer_Per_Produk(WebsiteAccessKey & "★M0_Penjualan_Per_Customer_Per_Produk★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 5, MENUID = 50, RITEM = 15 ================================> Penjualan Per Produk (Periode Tgl)
            ElseIf ModuleId = 5 And MenuId = 50 And RItem = 15 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kotaAwal,         kotaAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan_Per_Produk_Per_Kota(WebsiteAccessKey & "★M0_Penjualan_Per_Produk_Per_Kota★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 5, MENUID = 50, RITEM = 16 ================================> Penjualan Per Customer (Periode Bulan)
            ElseIf ModuleId = 5 And MenuId = 50 And RItem = 16 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kotaAwal,         kotaAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan_Per_Customer_Periode_Bulan(WebsiteAccessKey & "★M0_Penjualan_Per_Customer_Periode_Bulan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 5, MENUID = 50, RITEM = 17 ================================> Penjualan Per Produk (Periode Bulan)
            ElseIf ModuleId = 5 And MenuId = 50 And RItem = 17 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kotaAwal,         kotaAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan_Per_Produk_Periode_Bulan(WebsiteAccessKey & "★M0_Penjualan_Per_Produk_Periode_Bulan★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 5, MENUID = 50, RITEM = 18 ================================> Penjualan Per Customer (Nilai) (4 Bulan Terakhir)
            ElseIf ModuleId = 5 And MenuId = 50 And RItem = 18 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kotaAwal,         kotaAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan_Per_Customer_Qty_4BulanTerakhir(WebsiteAccessKey & "★M0_Penjualan_Per_Customer_Qty_4BulanTerakhir★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 5, MENUID = 50, RITEM = 19 ================================> Penjualan Per Customer (Qty) (4 Bulan Terakhir)
            ElseIf ModuleId = 5 And MenuId = 50 And RItem = 19 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kotaAwal,         kotaAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan_Per_Customer_Qty_4BulanTerakhir(WebsiteAccessKey & "★M0_Penjualan_Per_Customer_Qty_4BulanTerakhir★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 5, MENUID = 50, RITEM = 20 ================================> Penjualan Per Produk (Qty) (4 Bulan Terakhir)
            ElseIf ModuleId = 5 And MenuId = 50 And RItem = 20 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kotaAwal,         kotaAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan_Per_Produk_Qty_4BulanTerakhir(WebsiteAccessKey & "★M0_Penjualan_Per_Produk_Qty_4BulanTerakhir★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 50, RITEM = 21 ================================> Penjualan Per Produk (Nilai) (4 Bulan Terakhir)
            ElseIf ModuleId = 5 And MenuId = 50 And RItem = 21 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kotaAwal,         kotaAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan_Per_Produk_Qty_4BulanTerakhir(WebsiteAccessKey & "★M0_Penjualan_Per_Produk_Qty_4BulanTerakhir★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 5, MENUID = 50, RITEM = 25 ================================> Penjualan Per Salesman Per Produk 
            ElseIf ModuleId = 5 And MenuId = 50 And RItem = 25 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                   kontakAwal                 kontakAKhir                          kotaAwal,         kotaAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Penjualan_Per_Salesman_Per_Produk(WebsiteAccessKey & "★M0_Penjualan_Per_Salesman_Per_Produk★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 12, MENUID = 77, RITEM = 1 ================================> STOCK OUT
            ElseIf ModuleId = 12 And MenuId = 77 And RItem = 1 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    kategoriPosAwal,         kategoriPosAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_StockOut(WebsiteAccessKey & "★M0_StockOut★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 36, RITEM = 7 ================================> KARTU STOK REKAP (DETAIL)
            ElseIf ModuleId = 3 And MenuId = 36 And RItem = 7 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy,  gudangAwal,   gudangAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "bkode", sptField, "", sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_RekapKartuStok(WebsiteAccessKey & "★M0_RekapKartuStok★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 36, RITEM = 8 ================================> KARTU STOK REKAP (GLOBAL)
            ElseIf ModuleId = 3 And MenuId = 36 And RItem = 8 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy,  gudangAwal,   gudangAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "bkode", sptField, "", sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_RekapKartuStok(WebsiteAccessKey & "★M0_RekapKartuStok★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 3, MENUID = 36, RITEM = 9 ================================> INVENTORY ACTUAL BALANCE SHEET
            ElseIf ModuleId = 3 And MenuId = 36 And RItem = 9 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,            OrderBy,  gudangAwal,   gudangAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "bkode", sptField, "", sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_RekapKartuStok(WebsiteAccessKey & "★M0_RekapKartuStok★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 104, RITEM = 2 ================================> CASH FLOW (AKUN LAWAN)
            ElseIf ModuleId = 2 And MenuId = 104 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,                    matauang
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_CashFlow(WebsiteAccessKey & "★M0_CashFlow★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 12, MENUID = 8, RITEM = 37 ================================> PENJUALAN PER CUSTOMER
            ElseIf ModuleId = 12 And MenuId = 8 And RItem = 37 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,                      norekAwal,                     norekAkhir,                    matauang
                detail = String.Concat(paramDetailSplit(0), paramDetailSplit(1), paramDetailSplit(2), paramDetailSplit(3), paramDetailSplit(4), paramDetailSplit(5), paramDetailSplit(6), paramDetailSplit(7), paramDetailSplit(8), paramDetailSplit(9))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_label_barcode(WebsiteAccessKey & "★M0_label_barcode★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 5, MENUID = 71, RITEM = 1 ================================> Perhitungan Insentive
            ElseIf ModuleId = 5 And MenuId = 71 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :      tglAwal,                       tglAkhir,         salesmanawal,         salesmanakhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.m0_Perhitungan_Insentive(WebsiteAccessKey & "★m0_Perhitungan_Insentive★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 13, MENUID = 22, RITEM = 1 ====================================> BUKU BESAR DETAIL PER SISWA
            ElseIf ModuleId = 13 And MenuId = 22 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 12 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                    norekAkhir,     matauang,  perTanggal,                    kontakAwal,                   kontakAkhir,                  thnAkademik                      fakultas                     programStudi                      kelasAwal                      KelasAkhir                   KategoriSiswa            ordeyBy
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, 0, sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, "kkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M13_StudentGeneralLedger(WebsiteAccessKey & "★M13_StudentGeneralLedger★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 13, MENUID = 22, RITEM = 2 ================================> REKAP PER SISWA
            ElseIf ModuleId = 13 And MenuId = 22 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 12 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                     norekAwal,                 norekAkhir,costCenterAwal,costCenterAkhir,         area,kontakCategory,customerCategory,supplierCategory,             kontakAwal,                   kontakAkhir,salesmanCategory,  salesman,                      matauang,                 orderBy
                'detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, "", sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, "bpkontakkode")
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, 0, sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, "bpkontakkode")

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M13_StudentSummary(WebsiteAccessKey & "★M13_StudentSummary★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 5, MENUID = 72, RITEM = 1 ================================> Laporan Omzet Dan Tagihan
            ElseIf ModuleId = 5 And MenuId = 72 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 2 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir
                'detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1))
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Omzet_Tagihan4(WebsiteAccessKey & "★M0_Omzet_Tagihan4★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 146, RITEM = 1 ================================> Laporan Harian
            ElseIf ModuleId = 2 And MenuId = 146 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 3 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                     tglAwalPJ,                  tglAkhirPJ
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_HarianAlamindo(WebsiteAccessKey & "★M0_HarianAlamindo★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 147, RITEM = 1 ================================> STOCK COVER MONTH
            ElseIf ModuleId = 2 And MenuId = 147 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                 katBarangAwal,                katBarangAkhir,                kodeBarangAwal,               kodeBarangAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_CoverMonth(WebsiteAccessKey & "★M0_CoverMonth★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 57, RITEM = 9 ================================> Komisi Salesman Limoplast (Detail)
            ElseIf ModuleId = 5 And MenuId = 57 And RItem = 9 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                 tglAkhir,                salesmanAwal,                salesmanAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_KomisiLimo(WebsiteAccessKey & "★M0_KomisiLimo★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 12, MENUID = 83, RITEM = 8 ================================> Top 10 best seller per store
            ElseIf ModuleId = 12 And MenuId = 83 And RItem = 8 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                 tglAkhir,                lokasiAwal,                lokasiAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.m0_best_seller_per_store(WebsiteAccessKey & "★m0_best_seller_per_store★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 50, RITEM = 1 ================================> REKAP BARANG KONSINYASI
            ElseIf ModuleId = 3 And MenuId = 50 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,                    gudangAwal,                   gudangAkhir,           orderBy,               notransaksiAwal,              notransaksiAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "bkode", sptField, paramDetailSplit(6), sptField, paramDetailSplit(7))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_RekapBarangKonsinyasi(WebsiteAccessKey & "★M0_RekapBarangKonsinyasi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 74, RITEM = 1 ================================> WEEK COVER
            ElseIf ModuleId = 5 And MenuId = 74 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,           orderBy,                  kategoriAwal,                 kategoriAkhir,                    gudangAwal,                   gudangAkhir,                       vSatuan,                   vPergudang
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "bkode", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_StockCoverWeek(WebsiteAccessKey & "★M0_StockCoverWeek★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 74, RITEM = 2 ================================> WEEK COVER
            ElseIf ModuleId = 5 And MenuId = 74 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,           orderBy,                  kategoriAwal,                 kategoriAkhir,                    gudangAwal,                   gudangAkhir,                       vSatuan,                   vPergudang
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "bkode", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_StockCoverWeek(WebsiteAccessKey & "★M0_StockCoverWeek★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 74, RITEM = 3 ================================> WEEK COVER
            ElseIf ModuleId = 5 And MenuId = 74 And RItem = 3 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    barangAwal,                   barangAkhir,           orderBy,                  kategoriAwal,                 kategoriAkhir,                    gudangAwal,                   gudangAkhir,                       vSatuan,                   vPergudang
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, "bkode", sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_StockCoverWeek(WebsiteAccessKey & "★M0_StockCoverWeek★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 12, MENUID = 73, RITEM = 17 ================================> LABEL BARCODE
            ElseIf ModuleId = 12 And MenuId = 73 And RItem = 17 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 8 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,                    notransaksiawal,                   notransaksiAkhir,           kategoriAwal,                  kategoriAkhir,                 barangAwal,                    barangAkhir                   gudangAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_label_barcode(WebsiteAccessKey & "★M0_label_barcode★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 148, RITEM = 1 ================================> LAPORAN HPP PRODUKSI
            ElseIf ModuleId = 2 And MenuId = 148 And RItem = 1 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 2 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_HppProduksi(WebsiteAccessKey & "★M0_HppProduksi★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 5, MENUID = 81, RITEM = 1 ================================> Daily Sales old
            ElseIf ModuleId = 5 And MenuId = 81 And RItem = 1 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 13 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                   tglAwal,                      tglAkhir,                    salesToday,                 salesmanAwal,                salesmanAkhir,               salesTargetBR,                     daysInBR,                       daysUntilBR,                 salesTargetBW,                     daysInBW,                      daysUntilBW,                 gudangAwal,                     gudangAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.m0_daily_sales(WebsiteAccessKey & "★m0_daily_sales★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 81, RITEM = 2 ================================> Daily Sales old
            ElseIf ModuleId = 5 And MenuId = 81 And RItem = 2 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 13 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                   tglAwal,                      tglAkhir,                    salesToday,                 salesmanAwal,                salesmanAkhir,               salesTargetBR,                     daysInBR,                       daysUntilBR,                 salesTargetBW,                     daysInBW,                      daysUntilBW,                 gudangAwal,                     gudangAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.m0_daily_salesnew(WebsiteAccessKey & "★m0_daily_salesnew★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                '    'MODULEID = 6, MENUID = 4, RITEM = 14 ================================> Machine Production Planning
                'ElseIf ModuleId = 6 And MenuId = 4 And RItem = 14 Then
                '    'CEK PARAMETER REPORT
                '    If paramDetailSplit.Length <> 4 Then
                '        'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                '        s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                '    End If

                '    'mapping detail :                  tglAwal,                      tglAkhir,                     mesinAwal,                    mesinAkhir
                '    detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))

                '    'PANGGIL FUNGSI REPORT PROGRESS
                '    strRptProgress = clsRptProgress.M0_MachinePdPlanning(WebsiteAccessKey & "★M0_MachinePdPlanning★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 149, RITEM = 1 ================================> Costing
            ElseIf ModuleId = 2 And MenuId = 149 And RItem = 1 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 5 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    jenis,                       tglAwal,                      tglAkhir,                       rmloss,                         pdqty
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Costing(WebsiteAccessKey & "★M0_Costing★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 2, MENUID = 149, RITEM = 2 ================================> Material Used
            ElseIf ModuleId = 2 And MenuId = 149 And RItem = 2 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 2 And paramDetailSplit.Length <> 3 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    tglAwal,                      tglAkhir
                If paramDetailSplit.Length > 2 Then
                    detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2))
                Else
                    detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1))
                End If


                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_MaterialUsed(WebsiteAccessKey & "★M0_MaterialUsed★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 149, RITEM = 3 ================================> Material Costing
            ElseIf ModuleId = 2 And MenuId = 149 And RItem = 3 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 3 And paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    divisi,                        tglAwal,                      tglAkhir
                If paramDetailSplit.Length > 3 Then
                    detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))
                Else
                    detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2))
                End If


                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_MaterialCosting(WebsiteAccessKey & "★M0_MaterialCosting★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 149, RITEM = 4 ================================> Material Costing 2025 Detail
            ElseIf ModuleId = 2 And MenuId = 149 And RItem = 4 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 3 And paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    divisi,                        tglAwal,                      tglAkhir
                If paramDetailSplit.Length > 3 Then
                    detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))
                Else
                    detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2))
                End If


                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_MaterialCosting2025(WebsiteAccessKey & "★M0_MaterialCosting★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)



                'MODULEID = 2, MENUID = 149, RITEM = 5 ================================> Material Costing 2025 Global
            ElseIf ModuleId = 2 And MenuId = 149 And RItem = 5 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 3 And paramDetailSplit.Length <> 4 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                    divisi,                        tglAwal,                      tglAkhir
                If paramDetailSplit.Length > 3 Then
                    detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3))
                Else
                    detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2))
                End If


                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_MaterialCosting2025(WebsiteAccessKey & "★M0_MaterialCosting★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)



                'MODULEID = 2, MENUID = 44, RITEM = 3 ================================> Daily Bank
            ElseIf ModuleId = 2 And MenuId = 44 And RItem = 3 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 2 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                      tgl,                            tgl2
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_DailyBank(WebsiteAccessKey & "★M0_DailyBank★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 2, MENUID = 150, RITEM = 1 ================================> Sales and Purchases
            ElseIf ModuleId = 2 And MenuId = 150 And RItem = 1 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 1 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                      tgl
                detail = String.Concat(paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_SalesPurchase(WebsiteAccessKey & "★M0_SalesPurchase★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 55, RITEM = 1 ================================> Daily Available Stock
            ElseIf ModuleId = 3 And MenuId = 55 And RItem = 1 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 21 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes(paramDetailSplit.Length & " Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                      tgl,                         brmax1,                     brmax2,                         brmax3,                         brmax4,                         brmax5,                     brmax6,                         brmax7,                         brmax8,                         bwmax1,                     bwmax2,                         bwmax3,                         bwmax4,                         bwmax5,                         bwmax6,                          bwmax7,                        bwmax8                 brmax9                           bwmax9,                     not priced,                     Minimal Stock
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12), sptField, paramDetailSplit(13), sptField, paramDetailSplit(14), sptField, paramDetailSplit(15), sptField, paramDetailSplit(16), sptField, paramDetailSplit(17), sptField, paramDetailSplit(18), sptField, paramDetailSplit(19), sptField, paramDetailSplit(20))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_DailyAvailableStock(WebsiteAccessKey & "★M0_DailyAvailableStock★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 55, RITEM = 2 ================================> Raw Material Stock Grouping (Grouping RM)
            ElseIf ModuleId = 3 And MenuId = 55 And RItem = 2 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 2 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                 divisiAwal,                divisiAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(1))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_GroupingRM(WebsiteAccessKey & "★M0_CoverMonth★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 55, RITEM = 3 ================================> Daily Rekon Bahan Maklon
            ElseIf ModuleId = 3 And MenuId = 55 And RItem = 3 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 1 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                      tgl,                         brmax1,                     brmax2,                         brmax3,                         brmax4,                         brmax5,                     brmax6,                         brmax7,                         brmax8,                         bwmax1,                     bwmax2,                         bwmax3,                         bwmax4,                         bwmax5,                         bwmax6,                          bwmax7,                        bwmax8                 brmax9                           bwmax9
                detail = String.Concat(paramDetailSplit(0), sptField, 0, sptField, 0, sptField, 0, sptField, 0, sptField, 0, sptField, 0, sptField, 0, sptField, 0, sptField, 0, sptField, 0, sptField, 0, sptField, 0, sptField, 0, sptField, 0, sptField, 0, sptField, 0, sptField, 0, sptField, 0)

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_DailyRekonBahanMaklon(WebsiteAccessKey & "★M0_DailyRekonBahanMaklon★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                '    'MODULEID = 3, MENUID = 55, RITEM = 4 ================================> Hedging Daily Available Stock
                'ElseIf ModuleId = 3 And MenuId = 55 And RItem = 4 Then
                '    'CEK PARAMETER REPORT
                '    If paramDetailSplit.Length <> 21 Then
                '        'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                '        s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                '    End If

                '    'mapping detail :                      tgl,                         brmax1,                     brmax2,                         brmax3,                         brmax4,                         brmax5,                     brmax6,                         brmax7,                         brmax8,                         bwmax1,                     bwmax2,                         bwmax3,                         bwmax4,                         bwmax5,                         bwmax6,                          bwmax7,                        bwmax8                 brmax9           bwmax9,         not priced br,                     Minimal Stock,              hedging,         not priced bw
                '    detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(3), sptField, paramDetailSplit(5), sptField, paramDetailSplit(7), sptField, paramDetailSplit(9), sptField, paramDetailSplit(11), sptField, paramDetailSplit(13), sptField, paramDetailSplit(15), sptField, paramDetailSplit(2), sptField, paramDetailSplit(4), sptField, paramDetailSplit(6), sptField, paramDetailSplit(8), sptField, paramDetailSplit(10), sptField, paramDetailSplit(12), sptField, paramDetailSplit(14), sptField, paramDetailSplit(16), sptField, 0, sptField, 0, sptField, paramDetailSplit(17), sptField, paramDetailSplit(19), sptField, paramDetailSplit(20), sptField, paramDetailSplit(18))

                '    'PANGGIL FUNGSI REPORT PROGRESS
                '    strRptProgress = clsRptProgress.M0_HedgingDailyAvailableStock(WebsiteAccessKey & "★M0_HedgingDailyAvailableStock★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 3, MENUID = 55, RITEM = 4 ================================> Hedging Daily Available Stock
            ElseIf ModuleId = 3 And MenuId = 55 And RItem = 4 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 22 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                ''mapping detail :                      tgl,                         brmax1,                     brmax2,                         brmax3,                         brmax4,                         brmax5,                     brmax6,                         brmax7,                         brmax8,                         bwmax1,                     bwmax2,                         bwmax3,                         bwmax4,                         bwmax5,                         bwmax6,                          bwmax7,                        bwmax8                 brmax9           bwmax9,       not priced br,                     Minimal Stock,                     hedging,                not priced bw
                'detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(3), sptField, paramDetailSplit(5), sptField, paramDetailSplit(7), sptField, paramDetailSplit(9), sptField, paramDetailSplit(11), sptField, paramDetailSplit(13), sptField, paramDetailSplit(15), sptField, paramDetailSplit(2), sptField, paramDetailSplit(4), sptField, paramDetailSplit(6), sptField, paramDetailSplit(8), sptField, paramDetailSplit(10), sptField, paramDetailSplit(12), sptField, paramDetailSplit(14), sptField, paramDetailSplit(16), sptField, 0, sptField, 0, sptField, paramDetailSplit(17), sptField, paramDetailSplit(19), sptField, paramDetailSplit(20), sptField, paramDetailSplit(18))

                'mapping detail :                      tgl,                         brmax1,                     brmax2,                         brmax3,                         brmax4,                         brmax5,                     brmax6,                         brmax7,                         brmax8,                         bwmax1,                     bwmax2,                         bwmax3,                         bwmax4,                         bwmax5,                         bwmax6,                          bwmax7,                        bwmax8                 brmax9           bwmax9,       not priced br,                     Minimal Stock,                     hedging,                not priced bw,                %faktor
                detail = String.Concat(paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(4), sptField, paramDetailSplit(6), sptField, paramDetailSplit(8), sptField, paramDetailSplit(10), sptField, paramDetailSplit(12), sptField, paramDetailSplit(14), sptField, paramDetailSplit(16), sptField, paramDetailSplit(3), sptField, paramDetailSplit(5), sptField, paramDetailSplit(7), sptField, paramDetailSplit(9), sptField, paramDetailSplit(11), sptField, paramDetailSplit(13), sptField, paramDetailSplit(15), sptField, paramDetailSplit(17), sptField, 0, sptField, 0, sptField, paramDetailSplit(18), sptField, paramDetailSplit(20), sptField, paramDetailSplit(21), sptField, paramDetailSplit(19), sptField, paramDetailSplit(0))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_HedgingDailyAvailableStock(WebsiteAccessKey & "★M0_HedgingDailyAvailableStock★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)

                'MODULEID = 5, MENUID = 84, RITEM = 1 ================================> Order Tracking
            ElseIf ModuleId = 5 And MenuId = 84 And RItem = 1 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 17 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                      tgl,                         brmax1,                     brmax2,                         brmax3,                         brmax4,                         brmax5,                     brmax6,                         brmax7,                         brmax8,                         bwmax1,                     bwmax2,                         bwmax3,                         bwmax4,                         bwmax5,                         bwmax6,                          bwmax7,                        bwmax8
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11), sptField, paramDetailSplit(12), sptField, paramDetailSplit(13), sptField, paramDetailSplit(14), sptField, paramDetailSplit(15), sptField, paramDetailSplit(16))

                'PANGGIL FUNGSI REPORT PROGRESS
                'strRptProgress = clsRptProgress.M0_OrderTracking(WebsiteAccessKey & "★M0_DailyAvailableStock★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)
                strRptProgress = clsRptProgress.M0_DailyAvailableStock(WebsiteAccessKey & "★M0_DailyAvailableStock★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 6, MENUID = 8, RITEM = 24 ================================> Laporan Abu
            ElseIf ModuleId = 6 And MenuId = 8 And RItem = 24 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 2 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                 tglAwal,                      tglAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_Abu(WebsiteAccessKey & "★M0_Abu★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 6, MENUID = 8, RITEM = 25 ================================> Production Backorder Report Detail (New)
            ElseIf ModuleId = 6 And MenuId = 8 And RItem = 25 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 13 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                   tglAwal,                    tglAkhir,                   customerAwal,               customerAkhir,                      mesinAwal,                  mesinAkhir,                         soAwal,                         soAkhir,                     filterBo,                  salesmanAwal,                   salesmanAkhir,                      divisi,                     excKontak
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(12), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_BOProductionDetail(WebsiteAccessKey & "★M0_BOProductionDetail★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 6, MENUID = 8, RITEM = 26 ================================> Production Backorder Report Global (New)
            ElseIf ModuleId = 6 And MenuId = 8 And RItem = 26 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 13 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                   tglAwal,                    tglAkhir,                   customerAwal,               customerAkhir,                      mesinAwal,                  mesinAkhir,                         soAwal,                         soAkhir,                     filterBo,                  salesmanAwal,                   salesmanAkhir,                      divisi,                     excKontak
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, paramDetailSplit(12), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_BOProductionGlobal(WebsiteAccessKey & "★M0_BOProductionGlobal★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 4, RITEM = 60 ================================> Index Delivery Sales Order
            ElseIf ModuleId = 5 And MenuId = 4 And RItem = 60 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 10 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                   tglAwal,                    tglAkhir,                   customerAwal,               customerAkhir,      mesinAwal,  mesinAkhir,                         soAwal,                         soAkhir,    filterBo,                  salesmanAwal,                   salesmanAkhir,                      divisi,                     excKontak
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, "", sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_IndexSO(WebsiteAccessKey & "★M0_IndexSO★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 4, RITEM = 62 ================================> Index Delivery Sales Order Item
            ElseIf ModuleId = 5 And MenuId = 4 And RItem = 62 Then
                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 12 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                   tglAwal,                    tglAkhir,                   customerAwal,               customerAkhir,      mesinAwal,  mesinAkhir,                         soAwal,                         soAkhir,    filterBo,                  salesmanAwal,                   salesmanAkhir,                      divisi,                     excKontak,                   itemAwal,                       itemAkhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, "", sptField, "", sptField, paramDetailSplit(6), sptField, paramDetailSplit(7), sptField, "", sptField, paramDetailSplit(4), sptField, paramDetailSplit(5), sptField, paramDetailSplit(8), sptField, paramDetailSplit(9), sptField, paramDetailSplit(10), sptField, paramDetailSplit(11))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_IndexSOItem(WebsiteAccessKey & "★M0_IndexSOItem★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'MODULEID = 5, MENUID = 10, RITEM = 103 ================================> EXPORT FAKTUR PAJAK
            ElseIf ModuleId = 5 And MenuId = 10 And RItem = 103 Then

                'CEK PARAMETER REPORT
                If paramDetailSplit.Length <> 6 Then
                    'KIRIM PROGRESS GAGAL, s(0) = isSuccess, s(1) = errMessage
                    s(0) = 0 : s(1) = FixQuotes("Invalid detail transaction data parameter." & " - " & FixQuotes(Query)) : GoTo selesai
                End If

                'mapping detail :                  tglAwal,                      tglAkhir,              kodeCustomerAwal,             kodeCustomerAkhir,                 sinotransaksi,             sinotransaksiakhir
                detail = String.Concat(paramDetailSplit(0), sptField, paramDetailSplit(1), sptField, paramDetailSplit(2), sptField, paramDetailSplit(3), sptField, paramDetailSplit(4), sptField, paramDetailSplit(5))

                'PANGGIL FUNGSI REPORT PROGRESS
                strRptProgress = clsRptProgress.M0_FakturPajak(WebsiteAccessKey & "★M0_FakturPajak★0△0△△△dd/MM/yyyy△dd/MM/yyyy H:mm:ss★" & userId & "★0★" & Utama & sptSubParam & detail)


                'JIKA MODULEID, MENUID, RITEM TIDAK TESEDIA ==========================> KIRIM INFORMASI PROSES GAGAL
            Else
                'Status Failed
                s(0) = 0 : s(1) = "Invalid Report Packet - " & FixQuotes(Query) & " " & ModuleId & " " & MenuId & " " & RItem : GoTo selesai
            End If

            '//CEK KEMBALIAN DARI FUNGSI REPORT YANG DIPANGGIL
            'FORMAT KEMBALIAN = NamaFungsi△isSuccess△errMessage△errStep△idtransaksi★0△0△0△0△0★ ----> M0_BankHarian_AkunLawan△1△△0△0★0△0△0△0△0★
            Dim rsRptProgress() As String = strRptProgress.Split(sptSubParam)
            If rsRptProgress(1) <> 1 Then
                'JIKA ISSUCCESS <> 1 MAKA KIRIM INFORMASI PROSES GAGAL
                'Status Success
                's(0) = 0 : s(1) = FixQuotes(rsRptProgress(2)) & " - " & FixQuotes(Query) : GoTo selesai
                s(0) = 0 : s(1) = FixQuotes(rsRptProgress(2)) : GoTo selesai
            End If

            '//KIRIM INFORMASI SEDANG RENDER REPORT DENGAN PROGRESS
            'Send("Rendering" & sptField & idMsmq & sptField)

        Catch ex As Exception
            SimpanLogToFile("[Agent] Error F_ReportProgress : Step : " + xstep.ToString + " Desc : " + Err.Description)
            s(0) = 0 : s(1) = "[Agent] Error F_ReportProgress : Step : " + xstep.ToString + " Desc TEST2 : " + Err.Description
            If ErrDescription <> "" Then
                s(1) = "[Agent] Error F_ReportProgress : Step : " + xstep.ToString + " Desc TEST: " + ErrDescription
            End If
            GoTo selesai
        End Try

        s(0) = 1 : s(1) = ""

selesai:
        Return s

    End Function

End Class