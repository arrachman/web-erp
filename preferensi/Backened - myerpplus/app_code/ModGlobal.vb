Imports Microsoft.VisualBasic
Imports System.Web
Imports System.Web.Services
'Imports System.Web.Services.Protocols
'Imports System.Web.Script.Services
Imports System.Data
Imports AsModuleMySQL.CommonFunction
Imports System.Globalization
Imports System.Messaging
Imports System.Data.OleDb
Imports System.IO
Imports System
Imports System.Diagnostics
Imports System.ComponentModel
Imports System.Management
Imports MySql.Data.MySqlClient

Imports System.IO.Compression

Public Module ModGlobal
    'Public Server As List(Of String) = New List(Of String) From {"1:11211"}
    Public Server As List(Of String) = New List(Of String) From {"127.0.0.1:11211"}
    'Public AsMemcached As New ClsMemcached(Server)

    Public AppId As String = "7Wu5h23C66PwiZ01X59l"
    Public AppSecret As String = "afd31f64ed70fbe9e81fb5425bacfd46"

    'PEMISAH PARAMETER          : ★
    'PEMISAH UTAMA DAN DETAIL   : △
    'PEMISAH ROW                : ▲
    'PEMISAH FIELD              : ▼
    'PEMISAH DATA LOGIN		    : Θ
    'PEMISAH SETTING            : |

    Public sptParam As String = "★"
    Public sptSubParam As String = "△"
    Public sptRow As String = "▲"
    Public sptField As String = "▼"
    Public sptLogin As String = "Θ"
    Public sptSetting As String = "|"

    Public spt1 As String = "|"
    Public spt2 As String = "~"

    Public dirMsmq As String = ".\PRIVATE$\ToolsQueue"
    Public dirMsmqUserLogin As String = ".\PRIVATE$\UserLogin"

    Public UrlWS As String = HttpContext.Current.Request.Url.AbsoluteUri

    Public FormatTanggal As String = "yyyy-MM-dd" 'Variabel utk FormatTanggal Global

    Public Function ValidateKey(ByVal key As String) As RsValidKey
        '///Perlu dibenahi dan dipasang dg benar
        Dim vk As New RsValidKey, sql As String = ""
        vk.errmessage = "Invalid Website Access Key."

        If FixQuotes(key) = "store138e99318fb7cbd9f2230bdf86166f1d" Then
            vk.success = True
            Return vk
        End If

        'CEK MEMCACHED
        'If IsNothing(AsMemcached.GetCache("myerpplus-" & key)) Then
        'JIKA GAGAL MAKA CEK TABEL USERLOGIN
        sql = "SELECT ulid FROM m0_userlogin WHERE ulid = '" & FixQuotes(key) & "'"
        Dim dtUser As DataTable = AsDataTableAmbilDariDB(sql)
        If dtUser.Rows.Count = 0 Then
            vk.success = False
        Else
            vk.success = True
        End If
        'Else
        'vk.success = True
        'End If

        'Dim vk As New RsValidKey
        'vk.success = True
        'vk.errmessage = "Website access key is invalid !."
        Return vk
    End Function

    Public Function AmbilDataLama(ByVal key As String, Optional ByVal filter As String = Nothing, Optional ByVal sort As String = Nothing, Optional ByVal AmbilKeDb As Boolean = False, Optional ByVal strField As String = Nothing, Optional ByVal strFieldType As String = Nothing, Optional ByVal pageNumber As Integer = 0, Optional ByVal itemLimit As Integer = 0, Optional ByRef Pg As RsPaging = Nothing, Optional ByVal Relasi As String = Nothing, Optional ByVal koneksidb As Integer = 0, Optional ByVal groupby As String = "", Optional ByVal strSqlm As String = Nothing) As DataTable
        Dim dt As New DataTable
        Dim sKey() As String = key.Split("-")           'Sample : dbase-nmtable -> (northwind-categories)
        Dim jmlSplitkey As Integer = sKey.Length
        Dim param As String = sKey(1)                   'Sample : skey(1) = nmtablenya : 'categories'
        Dim sTable() As String = param.Split("~")       'Split table
        Dim JmlsplitTable As Integer = sTable.Length    'Jika jumlah 2, brati relasi antar table

        Dim isPaging As Boolean = pageNumber > 0
        Dim sql As String = ""
        Dim jmlData As Double

        If AmbilKeDb Then
            'CekConnection()
            Dim rowStart = (pageNumber - 1) * itemLimit
            Dim Limit As String = ""
            If pageNumber > 0 Then
                Limit = " limit " & rowStart & "," & itemLimit
            End If

            ''Original from mas nawi : filter & sort
            'If Len(filter) = 0 And Len(sort) = 0 Then
            '    sql = "select * from " & sTable(0) & Limit
            'ElseIf Len(filter) > 0 And Len(sort) = 0 Then
            '    sql = "select * from " & sTable(0) & " where " & filter & Limit
            'ElseIf Len(filter) = 0 And Len(sort) > 0 Then
            '    sql = "select * from " & sTable(0) & " order by " & sort & Limit
            'ElseIf Len(filter) > 0 And Len(sort) > 0 Then
            '    sql = "select * from " & sTable(0) & " where " & filter & " order by " & sort & Limit
            'End If
            ''----------------------------------------------------------------------------------------

            'Upgraded by huda : sql manual by 9 feb 2013 -------------------------------------------------------------------------------
            If (strSqlm = Nothing) Then
                'Upgraded by Afidz : filter , sort & group by 6 feb 2013
                If Len(filter) = 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=0 & sort=0 & groupby=0
                    sql = "select * from " & sTable(0) & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=1 & sort=0 & groupby=0
                    sql = "select * from " & sTable(0) & " where " & filter & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=0 & sort=1 & groupby=0
                    sql = "select * from " & sTable(0) & " order by " & sort & Limit
                ElseIf Len(filter) = 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=0 & sort=0 & groupby=1
                    sql = "select * from " & sTable(0) & " group by " & groupby & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=1 & sort=1 & groupby=0
                    sql = "select * from " & sTable(0) & " where " & filter & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=1 & sort=0 & groupby=1
                    sql = "select * from " & sTable(0) & " where " & filter & " group by " & groupby & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=0 & sort=1 & groupby=1
                    sql = "select * from " & sTable(0) & " group by " & groupby & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=1 & sort=1 & groupby=1
                    sql = "select * from " & sTable(0) & " where " & filter & " group by " & groupby & " order by " & sort & Limit
                End If
                '----------------------------------------------------------------------------------------
            Else
                If Len(filter) = 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=0 & sort=0 & groupby=0
                    sql = strSqlm & " " & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=1 & sort=0 & groupby=0
                    sql = strSqlm & " " & " where " & filter & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=0 & sort=1 & groupby=0
                    sql = strSqlm & " " & " order by " & sort & Limit
                ElseIf Len(filter) = 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=0 & sort=0 & groupby=1
                    sql = strSqlm & " " & " group by " & groupby & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=1 & sort=1 & groupby=0
                    sql = strSqlm & " " & " where " & filter & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=1 & sort=0 & groupby=1
                    sql = strSqlm & " " & " where " & filter & " group by " & groupby & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=0 & sort=1 & groupby=1
                    sql = strSqlm & " " & " group by " & groupby & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=1 & sort=1 & groupby=1
                    sql = strSqlm & " " & " where " & filter & " group by " & groupby & " order by " & sort & Limit
                End If
            End If
            '-------------------------------------------------------------------------------------------------------------------------
            'Tulis(sql)
            'Edit by huda 8 des 2012 ------------------------------------------------
            If koneksidb = 0 Then
                ' jika menggunakan koneksi default
                dt = AsDataTableAmbilDariDB(sql)
            Else
                ' jika menggunakan koneksi lain
                dt = AsDataTableAmbilDariDB(sql, koneksidb)
            End If

            'AsMemcached.SetCache(key, dt)
            'END Edit ----------------------------------------------------------------

            'Original from mas nawi 
            'jmlData = AsDCount("0", sTable(0), filter)
            '-------------------------------------------

            'Upgraded by Afidz 12 feb 2013
            Dim dt2 As New DataTable
            If (strSqlm = Nothing) Then
                sql = "select 0 from " & sTable(0)
                If Len(filter) > 0 Then sql &= " where " & filter
                If Len(groupby) > 0 Then sql &= " group by " & groupby
                dt2 = AsDataTableAmbilDariDB(sql)
                jmlData = dt2.Rows.Count
            Else
                'Upgraded by huda 13 feb 2013
                If Len(filter) > 0 Then strSqlm &= " where " & filter
                If Len(groupby) > 0 Then strSqlm &= " group by " & groupby
                dt2 = AsDataTableAmbilDariDB(strSqlm)
                jmlData = dt2.Rows.Count
                '-----------------------------
            End If
            '------------------------------------------------------------------------------------------------------

            If itemLimit = 0 Then itemLimit = jmlData
        Else
            'Added by Afidz. 1 des 2012
            Dim rowStart = (pageNumber - 1) * itemLimit
            Dim Limit As String = ""
            If pageNumber > 0 Then
                Limit = " limit " & rowStart & "," & itemLimit
            End If
            '----------------------------------------

            'If AsMemcached.IsExist(key) Then
            '    dt = CType(AsMemcached.GetCache(key), DataTable)
            'Else
            If (jmlSplitkey > 2) Then
                Dim strSql As String = ""
                Dim strTable As String = ""                             'Tampung nama tabel
                Dim sField() As String = strField.Split("~")            'Split field
                Dim sFieldType() As String = strFieldType.Split("~")    'Split field type
                Dim CIdx As Integer = 0

                For i = 2 To (jmlSplitkey - 1)
                    If (sFieldType(CIdx) = "String") Then
                        strSql += IIf(Len(strSql) = 0, sField(CIdx) & " = '" & sKey(i) & "'", " and " & sField(CIdx) & " = '" & sKey(i) & "'")
                    Else
                        strSql += IIf(Len(strSql) = 0, sField(CIdx) & " = " & sKey(i), " and " & sField(CIdx) & " = " & sKey(i))
                    End If
                    CIdx += 1
                Next


                If (JmlsplitTable >= 2) Then    'Jika jmlsplit table lebih dari 2, berati relasi antar table...
                    sql = "select * from " & sTable(0) & " inner join " & sTable(1) & " on " & Relasi & " and " & strSql
                Else
                    sql = "select * from " & sTable(0) & " where " & strSql
                End If
            Else
                If (strSqlm = Nothing) Then
                    sql = "select * from " & param
                Else
                    sql = strSqlm & " " & param
                End If
            End If
            'Tulis(sql)
            'Edit by huda 8 des 2012 ------------------------------------------------
            If koneksidb = 0 Then
                ' jika menggunakan koneksi default
                dt = AsDataTableAmbilDariDB(sql)
            Else
                ' jika menggunakan koneksi lain
                dt = AsDataTableAmbilDariDB(sql, koneksidb)
            End If
            'END Edit ----------------------------------------------------------------
            '    AsMemcached.SetCache(key, dt)
            'End If

            jmlData = dt.Rows.Count

            'Original from mas nawi
            'If Len(filter) > 0 Or Len(sort) > 0 Then
            '    If itemLimit = 0 Then itemLimit = jmlData

            'dt = AsDataTableFilterLimit(dt, filter, sort, pageNumber, itemLimit)

            'End If
            '-----------------------------

            'Upgraded by Afidz 1 des 2012
            If itemLimit = 0 Then itemLimit = jmlData

            If Len(filter) = 0 And Len(sort) = 0 Then
                dt = AsDataTableFilterLimit(dt, "", , rowStart, itemLimit)
            ElseIf Len(filter) > 0 And Len(sort) = 0 Then
                dt = AsDataTableFilterLimit(dt, filter, , rowStart, itemLimit)
            ElseIf Len(filter) = 0 And Len(sort) > 0 Then
                dt = AsDataTableFilterLimit(dt, "", sort, rowStart, itemLimit)
            ElseIf Len(filter) > 0 And Len(sort) > 0 Then
                dt = AsDataTableFilterLimit(dt, filter, sort, rowStart, itemLimit)
            End If
            '------------------------------------------------------------------------


        End If

        If isPaging Then
            With Pg
                .countRow = jmlData
                .countPage = Math.Ceiling(jmlData / itemLimit)
                .curPage = pageNumber
                .isNext = pageNumber < .countPage
                .isPaging = isPaging
                .isPrev = pageNumber > 1
                .nextPage = IIf(pageNumber < .countPage, pageNumber + 1, 1)
                .prevPage = IIf(pageNumber > 1, pageNumber - 1, 1)
            End With
        Else
            With Pg
                .countRow = jmlData
                .countPage = 0
                .curPage = 0
                .isNext = False
                .isPaging = False
                .isPrev = False
                .nextPage = False
                .prevPage = False
            End With
        End If
        Return dt
    End Function

    Public Function f_coba2(ByVal key As String, Optional ByVal filter As String = Nothing, Optional ByVal sort As String = Nothing, Optional ByVal AmbilKeDb As Boolean = False, Optional ByVal strField As String = Nothing, Optional ByVal strFieldType As String = Nothing, Optional ByVal pageNumber As Integer = 0, Optional ByVal itemLimit As Integer = 0, Optional ByRef Pg As RsPaging = Nothing, Optional ByVal Relasi As String = Nothing, Optional ByVal koneksidb As Integer = 0, Optional ByVal groupby As String = "", Optional ByVal strSqlm As String = Nothing) As String
        Dim dt As New DataTable
        Dim sKey() As String = key.Split("-")           'Sample : dbase-nmtable -> (northwind-categories)
        Dim jmlSplitkey As Integer = sKey.Length
        Dim param As String = sKey(1)                   'Sample : skey(1) = nmtablenya : 'categories'
        Dim sTable() As String = param.Split("~")       'Split table
        Dim JmlsplitTable As Integer = sTable.Length    'Jika jumlah 2, brati relasi antar table

        Dim isPaging As Boolean = pageNumber <> 0, isNext As Boolean = False
        Dim sql As String = ""
        Dim jmlData As Double

        'JIKA AMBIL KE DATABASE ================================================================================
        If AmbilKeDb Then

            Dim rowStart As Integer = 0
            Dim Limit As String = ""

            'LIMIT LAST PAGE
            If pageNumber = -1 Then
                Dim sqldata As String = ""
                'HITUNG PAGE NUMBER = jmldata/itemlimit
                Dim dtlastpage As DataTable
                If (strSqlm = Nothing) Then
                    sqldata = "select 0 from " & sTable(0)
                    If Len(filter) > 0 Then sqldata &= " where " & filter
                    If Len(groupby) > 0 Then sqldata &= " group by " & groupby
                    dtlastpage = AsDataTableAmbilDariDB(sqldata)
                    pageNumber = Math.Ceiling((dtlastpage.Rows.Count) / itemLimit)
                Else
                    sqldata = strSqlm
                    If Len(filter) > 0 Then sqldata &= " where " & filter
                    If Len(groupby) > 0 Then sqldata &= " group by " & groupby
                    dtlastpage = AsDataTableAmbilDariDB(sqldata)
                    pageNumber = Math.Ceiling((dtlastpage.Rows.Count) / itemLimit)
                End If

                rowStart = (pageNumber - 1) * itemLimit
                Limit = " limit " & rowStart & "," & itemLimit + 1

                'LIMIT SESUAI PAGENUMBER
            ElseIf pageNumber > 0 Then
                rowStart = (pageNumber - 1) * itemLimit
                Limit = " limit " & rowStart & "," & itemLimit + 1
            End If


            'AMBIL KE DB LANGSUNG DARI NAMA TABEL
            If (strSqlm = Nothing) Then
                If Len(filter) = 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=0 & sort=0 & groupby=0
                    sql = "select * from " & sTable(0) & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=1 & sort=0 & groupby=0
                    sql = "select * from " & sTable(0) & " where " & filter & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=0 & sort=1 & groupby=0
                    sql = "select * from " & sTable(0) & " order by " & sort & Limit
                ElseIf Len(filter) = 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=0 & sort=0 & groupby=1
                    sql = "select * from " & sTable(0) & " group by " & groupby & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=1 & sort=1 & groupby=0
                    sql = "select * from " & sTable(0) & " where " & filter & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=1 & sort=0 & groupby=1
                    sql = "select * from " & sTable(0) & " where " & filter & " group by " & groupby & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=0 & sort=1 & groupby=1
                    sql = "select * from " & sTable(0) & " group by " & groupby & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=1 & sort=1 & groupby=1
                    sql = "select * from " & sTable(0) & " where " & filter & " group by " & groupby & " order by " & sort & Limit
                End If

                'AMBIL KE DB MENGGUNAKAN QUERY
            Else
                If Len(filter) = 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=0 & sort=0 & groupby=0
                    sql = strSqlm & " " & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=1 & sort=0 & groupby=0
                    sql = strSqlm & " " & " where " & filter & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=0 & sort=1 & groupby=0
                    sql = strSqlm & " " & " order by " & sort & Limit
                ElseIf Len(filter) = 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=0 & sort=0 & groupby=1
                    sql = strSqlm & " " & " group by " & groupby & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=1 & sort=1 & groupby=0
                    sql = strSqlm & " " & " where " & filter & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=1 & sort=0 & groupby=1
                    sql = strSqlm & " " & " where " & filter & " group by " & groupby & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=0 & sort=1 & groupby=1
                    sql = strSqlm & " " & " group by " & groupby & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=1 & sort=1 & groupby=1
                    sql = strSqlm & " " & " where " & filter & " group by " & groupby & " order by " & sort & Limit
                End If
            End If

            'SEETING KONEKSI DB
            If koneksidb = 0 Then
                ' jika menggunakan koneksi default
                dt = AsDataTableAmbilDariDB(sql)
                If dt.Rows.Count > itemLimit Then isNext = True Else isNext = False
                If isPaging Then dt = AsDataTableFilterLimit(dt, "", "", 0, itemLimit)
            Else
                ' jika menggunakan koneksi lain
                dt = AsDataTableAmbilDariDB(sql, koneksidb)
                If dt.Rows.Count > itemLimit Then isNext = True Else isNext = False
                If isPaging Then dt = AsDataTableFilterLimit(dt, "", "", 0, itemLimit)
            End If

            'JIKA TIDAK AMBIL KE DATABASE ==========================================================================
        Else

            Dim rowStart = (pageNumber - 1) * itemLimit
            Dim Limit As String = ""
            If pageNumber > 0 Then
                Limit = " limit " & rowStart & "," & itemLimit
            End If
            '----------------------------------------

            'If AsMemcached.IsExist(key) Then
            '    dt = CType(AsMemcached.GetCache(key), DataTable)
            'Else
            If (jmlSplitkey > 2) Then
                Dim strSql As String = ""
                Dim strTable As String = ""                             'Tampung nama tabel
                Dim sField() As String = strField.Split("~")            'Split field
                Dim sFieldType() As String = strFieldType.Split("~")    'Split field type
                Dim CIdx As Integer = 0

                For i = 2 To (jmlSplitkey - 1)
                    If (sFieldType(CIdx) = "String") Then
                        strSql += IIf(Len(strSql) = 0, sField(CIdx) & " = '" & sKey(i) & "'", " and " & sField(CIdx) & " = '" & sKey(i) & "'")
                    Else
                        strSql += IIf(Len(strSql) = 0, sField(CIdx) & " = " & sKey(i), " and " & sField(CIdx) & " = " & sKey(i))
                    End If
                    CIdx += 1
                Next

                If (JmlsplitTable >= 2) Then    'Jika jmlsplit table lebih dari 2, berati relasi antar table...
                    sql = "select * from " & sTable(0) & " inner join " & sTable(1) & " on " & Relasi & " and " & strSql
                Else
                    sql = "select * from " & sTable(0) & " where " & strSql
                End If
            Else
                If (strSqlm = Nothing) Then
                    sql = "select * from " & param
                Else
                    sql = strSqlm & " " & param
                End If
            End If

            If koneksidb = 0 Then
                ' jika menggunakan koneksi default
                dt = AsDataTableAmbilDariDB(sql)
            Else
                ' jika menggunakan koneksi lain
                dt = AsDataTableAmbilDariDB(sql, koneksidb)
            End If
            '    AsMemcached.SetCache(key, dt)
            'End If


            jmlData = dt.Rows.Count

            If itemLimit = 0 Then itemLimit = jmlData

            If Len(filter) = 0 And Len(sort) = 0 Then
                dt = AsDataTableFilterLimit(dt, "", , rowStart, itemLimit)
            ElseIf Len(filter) > 0 And Len(sort) = 0 Then
                dt = AsDataTableFilterLimit(dt, filter, , rowStart, itemLimit)
            ElseIf Len(filter) = 0 And Len(sort) > 0 Then
                dt = AsDataTableFilterLimit(dt, "", sort, rowStart, itemLimit)
            ElseIf Len(filter) > 0 And Len(sort) > 0 Then
                dt = AsDataTableFilterLimit(dt, filter, sort, rowStart, itemLimit)
            End If
            '------------------------------------------------------------------------
        End If

        If isPaging Then
            With Pg
                .countRow = 0
                .countPage = pageNumber ' dijadikan curPage
                .curPage = pageNumber
                .isNext = isNext
                .isPaging = isPaging
                .isPrev = pageNumber > 1
                .nextPage = True
                .prevPage = True
            End With
        Else
            With Pg
                .countRow = 0
                .countPage = 0
                .curPage = 0
                .isNext = False
                .isPaging = isPaging
                .isPrev = False
                .nextPage = False
                .prevPage = False
            End With
        End If
        Return "@"
    End Function

    Public Function AmbilDataAsMemcached(ByVal key As String, Optional ByVal filter As String = Nothing, Optional ByVal sort As String = Nothing, Optional ByVal AmbilKeDb As Boolean = False, Optional ByVal strField As String = Nothing, Optional ByVal strFieldType As String = Nothing, Optional ByVal pageNumber As Integer = 0, Optional ByVal itemLimit As Integer = 0, Optional ByRef Pg As RsPaging = Nothing, Optional ByVal Relasi As String = Nothing, Optional ByVal koneksidb As Integer = 0, Optional ByVal groupby As String = "", Optional ByVal strSqlm As String = Nothing) As DataTable
        Dim dt As New DataTable
        Dim sKey() As String = key.Split("-")           'Sample : dbase-nmtable -> (northwind-categories)
        Dim jmlSplitkey As Integer = sKey.Length
        Dim param As String = sKey(1)                   'Sample : skey(1) = nmtablenya : 'categories'
        Dim sTable() As String = param.Split("~")       'Split table
        Dim JmlsplitTable As Integer = sTable.Length    'Jika jumlah 2, brati relasi antar table

        Dim isPaging As Boolean = pageNumber <> 0, isNext As Boolean = False
        Dim sql As String = ""
        Dim jmlData As Double

        'JIKA AMBIL KE DATABASE ================================================================================
        If AmbilKeDb Then

            Dim rowStart As Integer = 0
            Dim Limit As String = ""

            'LIMIT LAST PAGE
            If pageNumber = -1 Then
                Dim sqldata As String = ""
                'HITUNG PAGE NUMBER = jmldata/itemlimit
                Dim dtlastpage As DataTable
                If (strSqlm = Nothing) Then
                    sqldata = "select 0 from " & sTable(0)
                    If Len(filter) > 0 Then sqldata &= " where " & filter
                    If Len(groupby) > 0 Then sqldata &= " group by " & groupby
                    dtlastpage = AsDataTableAmbilDariDB(sqldata)
                    pageNumber = Math.Ceiling((dtlastpage.Rows.Count) / itemLimit)
                Else
                    sqldata = strSqlm
                    If Len(filter) > 0 Then sqldata &= " where " & filter
                    If Len(groupby) > 0 Then sqldata &= " group by " & groupby
                    dtlastpage = AsDataTableAmbilDariDB(sqldata)
                    pageNumber = Math.Ceiling((dtlastpage.Rows.Count) / itemLimit)
                End If

                rowStart = (pageNumber - 1) * itemLimit
                Limit = " limit " & rowStart & "," & itemLimit + 1

                'LIMIT SESUAI PAGENUMBER
            ElseIf pageNumber > 0 Then
                rowStart = (pageNumber - 1) * itemLimit
                Limit = " limit " & rowStart & "," & itemLimit + 1
            End If


            'AMBIL KE DB LANGSUNG DARI NAMA TABEL
            If (strSqlm = Nothing) Then
                If Len(filter) = 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=0 & sort=0 & groupby=0
                    sql = "select * from " & sTable(0) & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=1 & sort=0 & groupby=0
                    sql = "select * from " & sTable(0) & " where " & filter & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=0 & sort=1 & groupby=0
                    sql = "select * from " & sTable(0) & " order by " & sort & Limit
                ElseIf Len(filter) = 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=0 & sort=0 & groupby=1
                    sql = "select * from " & sTable(0) & " group by " & groupby & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=1 & sort=1 & groupby=0
                    sql = "select * from " & sTable(0) & " where " & filter & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=1 & sort=0 & groupby=1
                    sql = "select * from " & sTable(0) & " where " & filter & " group by " & groupby & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=0 & sort=1 & groupby=1
                    sql = "select * from " & sTable(0) & " group by " & groupby & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=1 & sort=1 & groupby=1
                    sql = "select * from " & sTable(0) & " where " & filter & " group by " & groupby & " order by " & sort & Limit
                End If

                'AMBIL KE DB MENGGUNAKAN QUERY
            Else
                If Len(filter) = 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=0 & sort=0 & groupby=0
                    sql = strSqlm & " " & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=1 & sort=0 & groupby=0
                    sql = strSqlm & " " & " where " & filter & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=0 & sort=1 & groupby=0
                    sql = strSqlm & " " & " order by " & sort & Limit
                ElseIf Len(filter) = 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=0 & sort=0 & groupby=1
                    sql = strSqlm & " " & " group by " & groupby & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=1 & sort=1 & groupby=0
                    sql = strSqlm & " " & " where " & filter & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=1 & sort=0 & groupby=1
                    sql = strSqlm & " " & " where " & filter & " group by " & groupby & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=0 & sort=1 & groupby=1
                    sql = strSqlm & " " & " group by " & groupby & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=1 & sort=1 & groupby=1
                    sql = strSqlm & " " & " where " & filter & " group by " & groupby & " order by " & sort & Limit
                End If
            End If

            'SEETING KONEKSI DB
            If koneksidb = 0 Then
                ' jika menggunakan koneksi default
                dt = AsDataTableAmbilDariDB(sql)
                If dt.Rows.Count > itemLimit Then isNext = True Else isNext = False
                If isPaging Then dt = AsDataTableFilterLimit(dt, "", "", 0, itemLimit)
            Else
                ' jika menggunakan koneksi lain
                dt = AsDataTableAmbilDariDB(sql, koneksidb)
                If dt.Rows.Count > itemLimit Then isNext = True Else isNext = False
                If isPaging Then dt = AsDataTableFilterLimit(dt, "", "", 0, itemLimit)
            End If

            'JIKA TIDAK AMBIL KE DATABASE ==========================================================================
        Else

            Dim rowStart = (pageNumber - 1) * itemLimit
            Dim Limit As String = ""
            If pageNumber > 0 Then
                Limit = " limit " & rowStart & "," & itemLimit
            End If
            '----------------------------------------

            'If AsMemcached.IsExist(key) Then
            '    dt = CType(AsMemcached.GetCache(key), DataTable)
            'Else
            If (jmlSplitkey > 2) Then
                Dim strSql As String = ""
                Dim strTable As String = ""                             'Tampung nama tabel
                Dim sField() As String = strField.Split("~")            'Split field
                Dim sFieldType() As String = strFieldType.Split("~")    'Split field type
                Dim CIdx As Integer = 0

                For i = 2 To (jmlSplitkey - 1)
                    If (sFieldType(CIdx) = "String") Then
                        strSql += IIf(Len(strSql) = 0, sField(CIdx) & " = '" & sKey(i) & "'", " and " & sField(CIdx) & " = '" & sKey(i) & "'")
                    Else
                        strSql += IIf(Len(strSql) = 0, sField(CIdx) & " = " & sKey(i), " and " & sField(CIdx) & " = " & sKey(i))
                    End If
                    CIdx += 1
                Next

                If (JmlsplitTable >= 2) Then    'Jika jmlsplit table lebih dari 2, berati relasi antar table...
                    sql = "select * from " & sTable(0) & " inner join " & sTable(1) & " on " & Relasi & " and " & strSql
                Else
                    sql = "select * from " & sTable(0) & " where " & strSql
                End If
            Else
                If (strSqlm = Nothing) Then
                    sql = "select * from " & param
                Else
                    sql = strSqlm & " " & param
                End If
            End If

            If koneksidb = 0 Then
                ' jika menggunakan koneksi default
                dt = AsDataTableAmbilDariDB(sql)
            Else
                ' jika menggunakan koneksi lain
                dt = AsDataTableAmbilDariDB(sql, koneksidb)
            End If
            'AsMemcached.SetCache(key, dt)
            'End If

            jmlData = dt.Rows.Count

            If itemLimit = 0 Then itemLimit = jmlData

            If Len(filter) = 0 And Len(sort) = 0 Then
                dt = AsDataTableFilterLimit(dt, "", , rowStart, itemLimit)
            ElseIf Len(filter) > 0 And Len(sort) = 0 Then
                dt = AsDataTableFilterLimit(dt, filter, , rowStart, itemLimit)
            ElseIf Len(filter) = 0 And Len(sort) > 0 Then
                dt = AsDataTableFilterLimit(dt, "", sort, rowStart, itemLimit)
            ElseIf Len(filter) > 0 And Len(sort) > 0 Then
                dt = AsDataTableFilterLimit(dt, filter, sort, rowStart, itemLimit)
            End If
            '------------------------------------------------------------------------
        End If

        If isPaging Then
            With Pg
                .countRow = 0
                .countPage = pageNumber ' dijadikan curPage
                .curPage = pageNumber
                .isNext = isNext
                .isPaging = isPaging
                .isPrev = pageNumber > 1
                .nextPage = True
                .prevPage = True
            End With
        Else
            With Pg
                .countRow = 0
                .countPage = 0
                .curPage = 0
                .isNext = False
                .isPaging = isPaging
                .isPrev = False
                .nextPage = False
                .prevPage = False
            End With
        End If

        'Split akhir char sptLogin untuk ambil data double dengan koma banyak di belakang
        Dim datarow As String = ""
        Dim datapemisah As String = "#$"
        For i = 0 To dt.Rows.Count - 1
            For j = 0 To dt.Columns.Count - 1
                If Not IsDBNull(dt.Rows(i)(j)) Then
                    datarow = dt.Rows(i)(j)
                    If datarow.IndexOf(datapemisah) = datarow.Count - datapemisah.Count Then
                        dt.Rows(i)(j) = datarow.Replace(datapemisah, "")
                    End If
                End If
            Next
        Next

        Return dt
    End Function

    Public Function AmbilData(ByVal key As String, Optional ByVal filter As String = Nothing, Optional ByVal sort As String = Nothing, Optional ByVal AmbilKeDb As Boolean = False, Optional ByVal strField As String = Nothing, Optional ByVal strFieldType As String = Nothing, Optional ByVal pageNumber As Integer = 0, Optional ByVal itemLimit As Integer = 0, Optional ByRef Pg As RsPaging = Nothing, Optional ByVal Relasi As String = Nothing, Optional ByVal koneksidb As Integer = 0, Optional ByVal groupby As String = "", Optional ByVal strSqlm As String = Nothing) As DataTable
        Dim dt As New DataTable
        Dim sKey() As String = key.Split("-")           'Sample : dbase-nmtable -> (northwind-categories)
        Dim jmlSplitkey As Integer = sKey.Length
        Dim param As String = sKey(1)                   'Sample : skey(1) = nmtablenya : 'categories'
        Dim sTable() As String = param.Split("~")       'Split table
        Dim JmlsplitTable As Integer = sTable.Length    'Jika jumlah 2, brati relasi antar table

        Dim isPaging As Boolean = pageNumber <> 0, isNext As Boolean = False
        Dim sql As String = ""
        Dim jmlData As Double

        'JIKA AMBIL KE DATABASE ================================================================================
        If AmbilKeDb Then

            Dim rowStart As Integer = 0
            Dim Limit As String = ""

            'LIMIT LAST PAGE
            If pageNumber = -1 Then
                Dim sqldata As String = ""
                'HITUNG PAGE NUMBER = jmldata/itemlimit
                Dim dtlastpage As DataTable
                If (strSqlm = Nothing) Then
                    sqldata = "select 0 from " & sTable(0)
                    If Len(filter) > 0 Then sqldata &= " where " & filter
                    If Len(groupby) > 0 Then sqldata &= " group by " & groupby
                    dtlastpage = AsDataTableAmbilDariDBCon(sqldata, Con1)
                    pageNumber = Math.Ceiling((dtlastpage.Rows.Count) / itemLimit)
                Else
                    sqldata = strSqlm
                    If Len(filter) > 0 Then sqldata &= " where " & filter
                    If Len(groupby) > 0 Then sqldata &= " group by " & groupby
                    dtlastpage = AsDataTableAmbilDariDBCon(sqldata, Con1)
                    pageNumber = Math.Ceiling((dtlastpage.Rows.Count) / itemLimit)
                End If

                rowStart = (pageNumber - 1) * itemLimit
                Limit = " limit " & rowStart & "," & itemLimit + 1

                'LIMIT SESUAI PAGENUMBER
            ElseIf pageNumber > 0 Then
                rowStart = (pageNumber - 1) * itemLimit
                Limit = " limit " & rowStart & "," & itemLimit + 1
            End If


            'AMBIL KE DB LANGSUNG DARI NAMA TABEL
            If (strSqlm = Nothing) Then
                If Len(filter) = 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=0 & sort=0 & groupby=0
                    sql = "select * from " & sTable(0) & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=1 & sort=0 & groupby=0
                    sql = "select * from " & sTable(0) & " where " & filter & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=0 & sort=1 & groupby=0
                    sql = "select * from " & sTable(0) & " order by " & sort & Limit
                ElseIf Len(filter) = 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=0 & sort=0 & groupby=1
                    sql = "select * from " & sTable(0) & " group by " & groupby & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=1 & sort=1 & groupby=0
                    sql = "select * from " & sTable(0) & " where " & filter & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=1 & sort=0 & groupby=1
                    sql = "select * from " & sTable(0) & " where " & filter & " group by " & groupby & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=0 & sort=1 & groupby=1
                    sql = "select * from " & sTable(0) & " group by " & groupby & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=1 & sort=1 & groupby=1
                    sql = "select * from " & sTable(0) & " where " & filter & " group by " & groupby & " order by " & sort & Limit
                End If

                'AMBIL KE DB MENGGUNAKAN QUERY
            Else
                If Len(filter) = 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=0 & sort=0 & groupby=0
                    sql = strSqlm & " " & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=1 & sort=0 & groupby=0
                    sql = strSqlm & " " & " where " & filter & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=0 & sort=1 & groupby=0
                    sql = strSqlm & " " & " order by " & sort & Limit
                ElseIf Len(filter) = 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=0 & sort=0 & groupby=1
                    sql = strSqlm & " " & " group by " & groupby & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=1 & sort=1 & groupby=0
                    sql = strSqlm & " " & " where " & filter & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=1 & sort=0 & groupby=1
                    sql = strSqlm & " " & " where " & filter & " group by " & groupby & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=0 & sort=1 & groupby=1
                    sql = strSqlm & " " & " group by " & groupby & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=1 & sort=1 & groupby=1
                    sql = strSqlm & " " & " where " & filter & " group by " & groupby & " order by " & sort & Limit
                End If
            End If

            'SEETING KONEKSI DB
            If koneksidb = 0 Then
                ' jika menggunakan koneksi default
                dt = AsDataTableAmbilDariDBCon(sql, Con1)
                If dt.Rows.Count > itemLimit Then isNext = True Else isNext = False
                If isPaging Then dt = AsDataTableFilterLimit(dt, "", "", 0, itemLimit)
            Else
                ' jika menggunakan koneksi lain
                dt = AsDataTableAmbilDariDBCon(sql, Con1)
                If dt.Rows.Count > itemLimit Then isNext = True Else isNext = False
                If isPaging Then dt = AsDataTableFilterLimit(dt, "", "", 0, itemLimit)
            End If

            'JIKA TIDAK AMBIL KE DATABASE ==========================================================================
        Else

            Dim rowStart = (pageNumber - 1) * itemLimit
            Dim Limit As String = ""
            If pageNumber > 0 Then
                Limit = " limit " & rowStart & "," & itemLimit
            End If
            '----------------------------------------

            'If AsMemcached.IsExist(key) Then
            '    dt = CType(AsMemcached.GetCache(key), DataTable)
            'Else
            If (jmlSplitkey > 2) Then
                Dim strSql As String = ""
                Dim strTable As String = ""                             'Tampung nama tabel
                Dim sField() As String = strField.Split("~")            'Split field
                Dim sFieldType() As String = strFieldType.Split("~")    'Split field type
                Dim CIdx As Integer = 0

                For i = 2 To (jmlSplitkey - 1)
                    If (sFieldType(CIdx) = "String") Then
                        strSql += IIf(Len(strSql) = 0, sField(CIdx) & " = '" & sKey(i) & "'", " and " & sField(CIdx) & " = '" & sKey(i) & "'")
                    Else
                        strSql += IIf(Len(strSql) = 0, sField(CIdx) & " = " & sKey(i), " and " & sField(CIdx) & " = " & sKey(i))
                    End If
                    CIdx += 1
                Next

                If (JmlsplitTable >= 2) Then    'Jika jmlsplit table lebih dari 2, berati relasi antar table...
                    sql = "select * from " & sTable(0) & " inner join " & sTable(1) & " on " & Relasi & " and " & strSql
                Else
                    sql = "select * from " & sTable(0) & " where " & strSql
                End If
            Else
                If (strSqlm = Nothing) Then
                    sql = "select * from " & param
                Else
                    sql = strSqlm & " " & param
                End If
            End If

            If koneksidb = 0 Then
                ' jika menggunakan koneksi default
                dt = AsDataTableAmbilDariDBCon(sql, Con1)
            Else
                ' jika menggunakan koneksi lain
                dt = AsDataTableAmbilDariDBCon(sql, Con1)
            End If
            '    AsMemcached.SetCache(key, dt)
            'End If

            jmlData = dt.Rows.Count

            If itemLimit = 0 Then itemLimit = jmlData

            If Len(filter) = 0 And Len(sort) = 0 Then
                dt = AsDataTableFilterLimit(dt, "", , rowStart, itemLimit)
            ElseIf Len(filter) > 0 And Len(sort) = 0 Then
                dt = AsDataTableFilterLimit(dt, filter, , rowStart, itemLimit)
            ElseIf Len(filter) = 0 And Len(sort) > 0 Then
                dt = AsDataTableFilterLimit(dt, "", sort, rowStart, itemLimit)
            ElseIf Len(filter) > 0 And Len(sort) > 0 Then
                dt = AsDataTableFilterLimit(dt, filter, sort, rowStart, itemLimit)
            End If
            '------------------------------------------------------------------------
        End If

        If isPaging Then
            With Pg
                .countRow = 0
                .countPage = pageNumber ' dijadikan curPage
                .curPage = pageNumber
                .isNext = isNext
                .isPaging = isPaging
                .isPrev = pageNumber > 1
                .nextPage = True
                .prevPage = True
            End With
        Else
            With Pg
                .countRow = 0
                .countPage = 0
                .curPage = 0
                .isNext = False
                .isPaging = isPaging
                .isPrev = False
                .nextPage = False
                .prevPage = False
            End With
        End If

        'Split akhir char sptLogin untuk ambil data double dengan koma banyak di belakang
        Dim datarow As String = ""
        Dim datapemisah As String = "#$"
        For i = 0 To dt.Rows.Count - 1
            For j = 0 To dt.Columns.Count - 1
                If Not IsDBNull(dt.Rows(i)(j)) Then
                    datarow = dt.Rows(i)(j)
                    If datarow.IndexOf(datapemisah) = datarow.Count - datapemisah.Count Then
                        dt.Rows(i)(j) = datarow.Replace(datapemisah, "")
                    End If
                End If
            Next
        Next

        Return dt
    End Function

    Public Function AmbilData1(ByVal key As String, Optional ByVal filter As String = Nothing, Optional ByVal sort As String = Nothing, Optional ByVal AmbilKeDb As Boolean = False, Optional ByVal strField As String = Nothing, Optional ByVal strFieldType As String = Nothing, Optional ByVal pageNumber As Integer = 0, Optional ByVal itemLimit As Integer = 0, Optional ByRef Pg As RsPaging = Nothing, Optional ByVal Relasi As String = Nothing, Optional ByVal koneksidb As Integer = 0, Optional ByVal groupby As String = "", Optional ByVal strSqlm As String = Nothing) As DataTable
        Dim dt As New DataTable
        Dim sKey() As String = key.Split("-")           'Sample : dbase-nmtable -> (northwind-categories)
        Dim jmlSplitkey As Integer = sKey.Length
        Dim param As String = sKey(1)                   'Sample : skey(1) = nmtablenya : 'categories'
        Dim sTable() As String = param.Split("~")       'Split table
        Dim JmlsplitTable As Integer = sTable.Length    'Jika jumlah 2, brati relasi antar table

        Dim isPaging As Boolean = pageNumber <> 0, isNext As Boolean = False
        Dim sql As String = ""
        Dim jmlData As Double

        'JIKA AMBIL KE DATABASE ================================================================================
        If AmbilKeDb Then

            Dim rowStart As Integer = 0
            Dim Limit As String = ""

            'LIMIT LAST PAGE
            If pageNumber = -1 Then
                Dim sqldata As String = ""
                'HITUNG PAGE NUMBER = jmldata/itemlimit
                Dim dtlastpage As DataTable
                If (strSqlm = Nothing) Then
                    sqldata = "select 0 from " & sTable(0)
                    If Len(filter) > 0 Then sqldata &= " where " & filter
                    If Len(groupby) > 0 Then sqldata &= " group by " & groupby
                    dtlastpage = AsDataTableAmbilDariDB(sqldata)
                    pageNumber = Math.Ceiling((dtlastpage.Rows.Count) / itemLimit)
                Else
                    sqldata = strSqlm
                    If Len(filter) > 0 Then sqldata &= " where " & filter
                    If Len(groupby) > 0 Then sqldata &= " group by " & groupby
                    dtlastpage = AsDataTableAmbilDariDB(sqldata)
                    pageNumber = Math.Ceiling((dtlastpage.Rows.Count) / itemLimit)
                End If

                rowStart = (pageNumber - 1) * itemLimit
                Limit = " limit " & rowStart & "," & itemLimit + 1

                'LIMIT SESUAI PAGENUMBER
            ElseIf pageNumber > 0 Then
                rowStart = (pageNumber - 1) * itemLimit
                Limit = " limit " & rowStart & "," & itemLimit + 1
            End If


            'AMBIL KE DB LANGSUNG DARI NAMA TABEL
            If (strSqlm = Nothing) Then
                If Len(filter) = 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=0 & sort=0 & groupby=0
                    sql = "select * from " & sTable(0) & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=1 & sort=0 & groupby=0
                    sql = "select * from " & sTable(0) & " where " & filter & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=0 & sort=1 & groupby=0
                    sql = "select * from " & sTable(0) & " order by " & sort & Limit
                ElseIf Len(filter) = 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=0 & sort=0 & groupby=1
                    sql = "select * from " & sTable(0) & " group by " & groupby & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=1 & sort=1 & groupby=0
                    sql = "select * from " & sTable(0) & " where " & filter & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=1 & sort=0 & groupby=1
                    sql = "select * from " & sTable(0) & " where " & filter & " group by " & groupby & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=0 & sort=1 & groupby=1
                    sql = "select * from " & sTable(0) & " group by " & groupby & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=1 & sort=1 & groupby=1
                    sql = "select * from " & sTable(0) & " where " & filter & " group by " & groupby & " order by " & sort & Limit
                End If

                'AMBIL KE DB MENGGUNAKAN QUERY
            Else
                If Len(filter) = 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=0 & sort=0 & groupby=0
                    sql = strSqlm & " " & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=1 & sort=0 & groupby=0
                    sql = strSqlm & " " & " where " & filter & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=0 & sort=1 & groupby=0
                    sql = strSqlm & " " & " order by " & sort & Limit
                ElseIf Len(filter) = 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=0 & sort=0 & groupby=1
                    sql = strSqlm & " " & " group by " & groupby & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=1 & sort=1 & groupby=0
                    sql = strSqlm & " " & " where " & filter & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=1 & sort=0 & groupby=1
                    sql = strSqlm & " " & " where " & filter & " group by " & groupby & Limit
                ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=0 & sort=1 & groupby=1
                    sql = strSqlm & " " & " group by " & groupby & " order by " & sort & Limit
                ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=1 & sort=1 & groupby=1
                    sql = strSqlm & " " & " where " & filter & " group by " & groupby & " order by " & sort & Limit
                End If
            End If

            'SEETING KONEKSI DB
            If koneksidb = 0 Then
                ' jika menggunakan koneksi default
                dt = AsDataTableAmbilDariDB(sql)
                If dt.Rows.Count > itemLimit Then isNext = True Else isNext = False
                If isPaging Then dt = AsDataTableFilterLimit(dt, "", "", 0, itemLimit)
            Else
                ' jika menggunakan koneksi lain
                dt = AsDataTableAmbilDariDB(sql, koneksidb)
                If dt.Rows.Count > itemLimit Then isNext = True Else isNext = False
                If isPaging Then dt = AsDataTableFilterLimit(dt, "", "", 0, itemLimit)
            End If

            'JIKA TIDAK AMBIL KE DATABASE ==========================================================================
        Else

            Dim rowStart = (pageNumber - 1) * itemLimit
            Dim Limit As String = ""
            If pageNumber > 0 Then
                Limit = " limit " & rowStart & "," & itemLimit
            End If
            '----------------------------------------

            'If AsMemcached.IsExist(key) Then
            '    dt = CType(AsMemcached.GetCache(key), DataTable)
            'Else
            If (jmlSplitkey > 2) Then
                Dim strSql As String = ""
                Dim strTable As String = ""                             'Tampung nama tabel
                Dim sField() As String = strField.Split("~")            'Split field
                Dim sFieldType() As String = strFieldType.Split("~")    'Split field type
                Dim CIdx As Integer = 0

                For i = 2 To (jmlSplitkey - 1)
                    If (sFieldType(CIdx) = "String") Then
                        strSql += IIf(Len(strSql) = 0, sField(CIdx) & " = '" & sKey(i) & "'", " and " & sField(CIdx) & " = '" & sKey(i) & "'")
                    Else
                        strSql += IIf(Len(strSql) = 0, sField(CIdx) & " = " & sKey(i), " and " & sField(CIdx) & " = " & sKey(i))
                    End If
                    CIdx += 1
                Next

                If (JmlsplitTable >= 2) Then    'Jika jmlsplit table lebih dari 2, berati relasi antar table...
                    sql = "select * from " & sTable(0) & " inner join " & sTable(1) & " on " & Relasi & " and " & strSql
                Else
                    sql = "select * from " & sTable(0) & " where " & strSql
                End If
            Else
                If (strSqlm = Nothing) Then
                    sql = "select * from " & param
                Else
                    sql = strSqlm & " " & param
                End If
            End If

            If koneksidb = 0 Then
                ' jika menggunakan koneksi default
                dt = AsDataTableAmbilDariDB(sql)
            Else
                ' jika menggunakan koneksi lain
                dt = AsDataTableAmbilDariDB(sql, koneksidb)
            End If
            '    AsMemcached.SetCache(key, dt)
            'End If

            jmlData = dt.Rows.Count

            If itemLimit = 0 Then itemLimit = jmlData

            If Len(filter) = 0 And Len(sort) = 0 Then
                dt = AsDataTableFilterLimit(dt, "", , rowStart, itemLimit)
            ElseIf Len(filter) > 0 And Len(sort) = 0 Then
                dt = AsDataTableFilterLimit(dt, filter, , rowStart, itemLimit)
            ElseIf Len(filter) = 0 And Len(sort) > 0 Then
                dt = AsDataTableFilterLimit(dt, "", sort, rowStart, itemLimit)
            ElseIf Len(filter) > 0 And Len(sort) > 0 Then
                dt = AsDataTableFilterLimit(dt, filter, sort, rowStart, itemLimit)
            End If
            '------------------------------------------------------------------------
        End If

        If isPaging Then
            With Pg
                .countRow = 0
                .countPage = pageNumber ' dijadikan curPage
                .curPage = pageNumber
                .isNext = isNext
                .isPaging = isPaging
                .isPrev = pageNumber > 1
                .nextPage = True
                .prevPage = True
            End With
        Else
            With Pg
                .countRow = 0
                .countPage = 0
                .curPage = 0
                .isNext = False
                .isPaging = isPaging
                .isPrev = False
                .nextPage = False
                .prevPage = False
            End With
        End If

        'Split akhir char sptLogin untuk ambil data double dengan koma banyak di belakang
        Dim datarow As String = ""
        Dim datapemisah As String = "#$"
        For i = 0 To dt.Rows.Count - 1
            For j = 0 To dt.Columns.Count - 1
                If Not IsDBNull(dt.Rows(i)(j)) Then
                    datarow = dt.Rows(i)(j)
                    If datarow.IndexOf(datapemisah) = datarow.Count - datapemisah.Count Then
                        dt.Rows(i)(j) = datarow.Replace(datapemisah, "")
                    End If
                End If
            Next
        Next

        Return dt
    End Function

    'Mengambil data dari fungsi di database, ~AFIDZ 30 OKT 2012
    Public Function AmbilDataFungsi(ByVal NamaFungsi As String, ByVal ParamFungsi As String, Optional ByRef Pg As RsPaging = Nothing, Optional ByVal pageNumber As Integer = 0, Optional ByVal itemLimit As Integer = 0) As DataTable
        'Format parameter fungsi di database harus di akhiri 2 parameter terakhir utk pagenumber dan itemlimit.
        'contoh : namafungsimu(param1,param2,param3,....,pagenumber,itemlimit)

        'Variabel ParamFungsi di atas berisi parameter ke1,ke2,ke3...dst;  2 parameter terakhir fungsi databasemu sisanya di isikan oleh pageNumber dan itemLimit

        Dim dt As New DataTable

        Dim sParamPaket() As String = ParamFungsi.Split("~")

        Dim isPaging As Boolean = pageNumber > 0
        Dim sql1, sql2 As New StringBuilder
        Dim jmlData As Double
        Dim temp_dt As New DataTable

        sql1.Append("call " & NamaFungsi & " (")
        sql2.Append("call " & NamaFungsi & " (")

        'Perulangan bedasarkan banyaknya parameter
        For i As Integer = 0 To sParamPaket.Length - 1 Step 1
            If i > 0 Then
                sql1.Append(", '" & sParamPaket(i) & "' ")
                sql2.Append(", '" & sParamPaket(i) & "' ")
            Else
                sql1.Append("'" & sParamPaket(i) & "'")
                sql2.Append("'" & sParamPaket(i) & "'")
            End If
        Next

        sql1.Append(",0,0 )")
        sql2.Append("," & pageNumber & "," & itemLimit & " )")

        temp_dt = AsDataTableAmbilDariDB(sql1.ToString)
        dt = AsDataTableAmbilDariDB(sql2.ToString)

        jmlData = temp_dt.Rows.Count

        If itemLimit = 0 Then itemLimit = jmlData

        If isPaging Then
            With Pg
                .countPage = Math.Ceiling(jmlData / itemLimit)
                .curPage = pageNumber
                .isNext = pageNumber < .countPage
                .isPaging = isPaging
                .isPrev = pageNumber > 1
                .nextPage = IIf(pageNumber < .countPage, pageNumber + 1, 1)
                .prevPage = IIf(pageNumber > 1, pageNumber - 1, 1)
            End With
        End If

        Return dt
    End Function

    Public Function FxDB(ByVal Param As Object, ByVal DefaultVal As Object) As String
        If IsDBNull(Param) Then
            Return DefaultVal
        Else
            ''Cek jika Formattgl=true , maka format tgl
            'If FormatTgl Then
            '    Dim formattgl1 As String = "dd/MM/yyyy"
            '    Dim formattgl2 As String = "dd/MM/yyyy hh:mm:ss"

            '    'Jika tipe date, maka format tanggal
            '    If (IsDate(Param)) Then
            '        'Jika panjang param>10 maka ksh formattgl2
            '        Param = AsFormatTanggal(Param, formattgl1)
            '    End If
            'End If

            Return Param
        End If
    End Function

    Public Function FxCurrency(ByVal nilai As Double) As String
        ' Creates a CultureInfo for English in United Kingdom
        Dim gb As New CultureInfo("en-GB")
        Return nilai.ToString("c", gb).Replace("£", "")
    End Function

    Public Function Tulis(ByVal text As String) As Boolean
        HttpContext.Current.Response.Write(text)
        Return True
    End Function

    Public Sub CloseConnection(Optional ByVal Koneksi As Long = 1)
        If Koneksi = 1 Then
            If Con1.State = ConnectionState.Open Then Con1.Close()
        ElseIf Koneksi = 2 Then
            If Con2.State = ConnectionState.Open Then Con2.Close()
        Else
            If Con3.State = ConnectionState.Open Then Con3.Close()
        End If
    End Sub

    Public Function FixQuotes(ByVal text As String) As String
        text = text.Replace("'", "''")
        Return text
    End Function

    Public Function FixDouble(ByVal text As String) As String
        text = text.Replace(",", ".")   'Replace koma menjadi titik
        Return text
    End Function

    '###Tambah function (nawi) : 2013-04-18
    Public Sub CekConnection(Optional ByVal Koneksi As Long = 1)
        If AsKoneksiKeDB() = False Then
            If Koneksi = 1 Then
                Con1.Open()
            ElseIf Koneksi = 2 Then
                Con2.Open()
            Else
                Con3.Open()
            End If
        End If
    End Sub

    'Get Struktur DataTable utk Jurnal
    Public Function GetDTJurnal() As DataTable
        Dim dtjurnal As New DataTable
        'Buat struktur datatable jurnal
        AsDataTableTambahField(dtjurnal, "noakun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "namaakun", AsEnumTypeData.AsString)
        AsDataTableTambahField(dtjurnal, "nominal", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "nominalvalas", AsEnumTypeData.AsDouble)
        AsDataTableTambahField(dtjurnal, "debitkredit", AsEnumTypeData.AsBoolean) 'true=debit,false=kredit
        AsDataTableTambahField(dtjurnal, "catatan", AsEnumTypeData.AsString)
        Return dtjurnal
        '-----------------------
    End Function

    Public Function PostWsSearch(ByVal WebsiteAccessKey As String, ByVal Paket As String, ByVal PageNumber As Integer, ByVal ItemLimit As Integer, ByVal Filter As String, ByVal Sorting As String, ByVal FormatTgl As String, ByVal FormatTglWaktu As String, Optional ByVal IdTransaksi As String = "0", Optional ByVal userId As String = "0") As String
        Dim hasil As String = String.Concat(WebsiteAccessKey, sptParam, Paket, sptParam, PageNumber, sptSubParam, ItemLimit, sptSubParam, Filter, sptSubParam, Sorting, sptSubParam, FormatTgl, sptSubParam, FormatTglWaktu, sptParam, IdTransaksi, sptParam, userId, sptParam)
        Return hasil
    End Function

    Public Function PostWsTerkait(ByVal WebsiteAccessKey As String, ByVal Paket As String, ByVal PageNumber As Integer, ByVal ItemLimit As Integer, ByVal Filter As String, ByVal Sorting As String, ByVal FormatTgl As String, ByVal FormatTglWaktu As String, ByVal IdTransaksi As String) As String
        Dim hasil As String = String.Concat(WebsiteAccessKey, sptParam, Paket, sptParam, PageNumber, sptSubParam, ItemLimit, sptSubParam, Filter, sptSubParam, Sorting, sptSubParam, FormatTgl, sptSubParam, FormatTglWaktu, sptParam, 0, sptParam, 0, sptParam, IdTransaksi)
        Return hasil
    End Function

    Public Function GetWsSearch(ByVal param As String) As RsHasilWsSearch
        Dim hasil As New RsHasilWsSearch
        Dim splitParam() As String
        Dim splitResult(5) As String
        Dim splitPaging(5) As String

        splitParam = param.Split(sptParam)

        splitResult = param.Split(sptSubParam)
        hasil.success = Val(splitResult(1))
        hasil.errmessage = splitResult(2).ToString

        splitPaging = splitParam(1).Split(sptSubParam)
        hasil.isPaging = Val(splitPaging(0))
        hasil.isNext = Val(splitPaging(1))
        hasil.isPrevious = Val(splitPaging(2))
        hasil.countPage = Val(splitPaging(3))
        hasil.countRow = Val(splitPaging(4))

        If (splitParam.Length = 4) Then
            hasil.data = String.Concat(splitParam(2).ToString, sptParam, splitParam(3).ToString)
        Else
            hasil.data = splitParam(2).ToString
        End If

        Return hasil
    End Function

    Public Function ReplaceMapping(ByVal text As String) As String
        text = text.Replace(" ", "")
        text = text.Replace(",", sptField)
        Return text
    End Function

    Public Function CreateSHAHash(ByVal Password As String, ByVal Keys As String) As String
        Dim HashTool As New System.Security.Cryptography.SHA512Managed()
        Dim PasswordAsByte As [Byte]() = System.Text.Encoding.UTF8.GetBytes(String.Concat(Password, Keys))
        Dim EncryptedBytes As [Byte]() = HashTool.ComputeHash(PasswordAsByte)
        HashTool.Clear()
        Return Convert.ToBase64String(EncryptedBytes)
    End Function

    Public Function RandomString(ByVal size As Integer) As String
        Dim nilai As Char() = New Char(size - 1) {}
        Dim _rng As Random = New Random()
        Dim _chars As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"

        For i As Integer = 0 To size - 1
            nilai(i) = _chars(_rng.[Next](_chars.Length))
        Next
        Return New String(nilai)
    End Function

    Public Function CreateJson(ByVal id As String, ByVal content As DataTable, ByVal namaKolom As String()) As String
        Dim hasil As String = "{""id"":""" & id & """, ""property"":["

        If content.Rows.Count > 0 Then
            For i = 1 To content.Rows.Count
                If i - 1 = 0 Then
                    hasil = hasil & "{"
                Else
                    hasil = hasil & ", {"
                End If

                For j = 1 To namaKolom.Length
                    If j - 1 = 0 Then
                        hasil = hasil & """" & namaKolom(j - 1) & """:""" & content(i - 1)(0) & """"
                    Else
                        hasil = hasil & ", """ & namaKolom(j - 1) & """:""" & content(i - 1)(1) & """"
                    End If
                Next
                hasil = hasil & "}"
            Next

        End If
        hasil = hasil & "]}"

        Return hasil
    End Function

    Public Function M2_Accounting_PeriodeCheck(ByVal tglAwal As String, ByVal tglAkhir As String) As String
        On Error GoTo selesai
        Dim success As Integer = 0, errmessage As String = "", filter As String = ""

        'CEK TIPE DATA =============================================
        If (IsDate(tglAwal) = False) Then
            errmessage = "tglAwal required date." : GoTo selesai
        Else
            tglAwal = AsFormatTanggal(tglAwal)
        End If
        If (IsDate(tglAkhir) = False) Then
            errmessage = "tglAkhir required date." : GoTo selesai
        Else
            tglAkhir = AsFormatTanggal(tglAkhir)
        End If
        'END OF CEK TIPE DATA ======================================

        'BUAT FILTER ===============================================
        '   'jika tahun berbeda
        If Not Year(tglAwal).Equals(Year(tglAkhir)) Then
            filter = "((aptahun = '" & Year(tglAwal) & "' AND apbulan >= '" & Month(tglAwal) & "') or (aptahun > '" & Year(tglAwal) & "' AND aptahun < '" & Year(tglAkhir) & "') or (aptahun = '" & Year(tglAkhir) & "' AND apbulan <= '" & Month(tglAkhir) & "'))"
            'jika tahun sama
        ElseIf Year(tglAwal).Equals(Year(tglAkhir)) Then
            '   'jika bulan sama
            If Month(tglAwal).Equals(Month(tglAkhir)) Then
                filter = "((aptahun = '" & Year(tglAwal) & "') AND (apbulan = '" & Month(tglAwal) & "'))"
                'jika bulan beda
            Else
                filter = "((aptahun = '" & Year(tglAwal) & "') AND (apbulan BETWEEN '" & Month(tglAwal) & "' AND '" & Month(tglAkhir) & "'))"
            End If
        End If
        'END OF BUAT FILTER ========================================


        'CEK PERIODE AKUNTANSI SUDAH TUTUP/BELUM
        Dim dt As DataTable = AsDataTableAmbilDariDB("SELECT aptahun, apbulan FROM m2_accounting_period WHERE " & filter & " AND aptutupperiode = '1'")
        If dt.Rows.Count > 0 Then success = 0 : errmessage = "Accounting Periode : Year = '" & dt.Rows(0)(0) & "', Month = '" & dt.Rows(0)(1) & "' has closed." : GoTo selesai

        success = 1
selesai:
        Return String.Concat(success, sptSubParam, errmessage)
    End Function

    Public Function ValidasiBatchSerial20241128(ByVal dtdetail As DataTable, ByRef dtbatch As DataTable, ByRef dtserial As DataTable, ByVal ftbarang As String, ByVal fieldJmlBarang As String, ByVal jenismutasi As Double) As String
        Dim errmessage As String = "", sql As String = ""

        Dim dtbatchBaru As New DataTable, dtserialBaru As New DataTable
        Dim dtval As New DataTable, dtbarang As New DataTable, dtLookup As New DataTable
        Dim jmlbarang As Double = 0, jmlnomor As Double = 0, urutan As Double = 0
        Dim kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuanbarang As String = ""

        'CEK VARIBEL
        If Len(fieldJmlBarang) = 0 Then errmessage = "Field jmlbarang can't be empty." : GoTo selesai
        If Len(ftbarang) = 0 Then errmessage = "Filter barang can't be empty." : GoTo selesai

        'AMBIL DTBATCH DAN SERIAL SESUAI JENISMUTASINYA
        dtbatchBaru = AsDataTableFilterSortDt(dtbatch, "nbtjenismutasi = '" & jenismutasi & "'")
        dtserialBaru = AsDataTableFilterSortDt(dtserial, "nstjenismutasi = '" & jenismutasi & "'")

        'BUAT FILTER DT BATCH DAN SERIAL
        Dim ftCekBatch As String = "(nbtjenismutasi = '" & jenismutasi & "')"
        Dim ftCekSerial As String = "(nstjenismutasi = '" & jenismutasi & "')"

        '1. AMBIL BARANG BATCH DAN SERIAL
        dtbarang = AsDataTableAmbilDariDB("SELECT bid, bkode, bsatuan, bbatch, bserial FROM m1_item WHERE (bbatch = 1 OR bserial = 1) AND (" & ftbarang & ")")

        '2. CEK NO BATCH DAN SERIAL
        If dtbarang.Rows.Count > 0 Then
            '2.1 CEK NO BATCH
            dtval = AsDataTableFilterSortDt(dtbarang, "bbatch = 1")
            If dtval.Rows.Count > 0 Then
                For Each dr As DataRow In dtval.Rows
                    'AMBIL JMLBARANG DARI DETAIL
                    jmlbarang = AsDataTableDSum(dtdetail, fieldJmlBarang, "idbarang = '" & dr("bid") & "'")

                    'AMBIL JMLBARANG DARI BATCH
                    jmlnomor = AsDataTableDSum(dtbatchBaru, "nbtjml", "nbtjenismutasi = '" & jenismutasi & "' AND nbtidbarang = '" & dr("bid") & "'")

                    'BANDINGKAN JMLBARANG DETAIL DAN BATCH
                    If jmlbarang <> jmlnomor Then
                        dtLookup = AsDataTableFilterLimit(dtdetail, "idbarang = '" & dr("bid") & "'", , , 1)
                        urutan = dtLookup.Rows(0)("urutan")
                        kodebarang = dr("bkode")
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuanbarang = dr("bsatuan")
                        errmessage = "No. Batch for Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " is not equal to the number of items in detail transactions, it must be " & jmlbarang & " " & satuanbarang & " | jmlbarang : " & jmlbarang & " jmlbatch : " & jmlnomor : GoTo selesai
                    End If

                    'BUAT FILTER UNTUK CEK DATA BATCH YG TIDAK SESUAI DENGAN DATA BARANG
                    ftCekBatch = IIf(Len(ftCekBatch.ToString) = 0, "", ftCekBatch & " AND ")
                    ftCekBatch = String.Concat(ftCekBatch, "(nbtidbarang <> '" & dr("bid") & "')")
                Next

                ''CEK DATA BATCH YG TIDAK SESUAI DENGAN DATA BARANG
                'dtval = AsDataTableFilterSortDt(dtbatchBaru, ftCekBatch)
                'If dtval.Rows.Count > 0 Then
                '    errmessage = "No. Batch : " & dtval(0)("nbtkode") & ", doesn't match with item in detail transactions." : GoTo selesai
                'End If

                'HAPUS DATA BATCH YG TIDAK SESUAI DENGAN DATA BARANG
                AsDataTableDeleteData(dtbatch, ftCekBatch)

            ElseIf (dtbatchBaru.Rows.Count > 0) Then
                'errmessage = "Batch Item not found." : GoTo selesai
                'JIKA TERDAPAT DATA BATCH TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA BATCH
                AsDataTableDeleteData(dtbatch, ftCekBatch)

            End If

            '2.2 CEK NO SERIAL
            dtval = AsDataTableFilterSortDt(dtbarang, "bserial = 1")
            If dtval.Rows.Count > 0 Then
                For Each dr As DataRow In dtval.Rows
                    'AMBIL JMLBARANG DARI DETAIL
                    jmlbarang = AsDataTableDSum(dtdetail, fieldJmlBarang, "idbarang = '" & dr("bid") & "'")

                    'AMBIL JMLBARANG DARI SERIAL
                    jmlnomor = AsDataTableDSum(dtserialBaru, "nstjml", "nstjenismutasi = '" & jenismutasi & "' AND nstidbarang = '" & dr("bid") & "'")

                    'BANDINGKAN JMLBARANG DETAIL DAN SERIAL
                    If jmlbarang <> jmlnomor Then
                        dtLookup = AsDataTableFilterLimit(dtdetail, "idbarang = '" & dr("bid") & "'", , , 1)
                        urutan = dtLookup.Rows(0)("urutan")
                        kodebarang = dr("bkode")
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuanbarang = dr("bsatuan")
                        errmessage = "No. Serial for Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " is not equal to the number of items in detail transactions, it must be " & jmlbarang & " " & satuanbarang : GoTo selesai
                    End If

                    'BUAT FILTER UNTUK CEK DATA SERIAL YG TIDAK SESUAI DENGAN DATA BARANG
                    ftCekSerial = IIf(Len(ftCekSerial.ToString) = 0, "", ftCekSerial & " AND ")
                    ftCekSerial = String.Concat(ftCekSerial, "(nstidbarang <> '" & dr("bid") & "')")
                Next

                ''CEK DATA SERIAL YG TIDAK SESUAI DENGAN DATA BARANG
                'dtval = AsDataTableFilterSortDt(dtserialBaru, ftCekSerial)
                'If dtval.Rows.Count > 0 Then
                '    errmessage = "No. Serial : " & dtval(0)("nstkode") & ", doesn't match with item in detail transactions." : GoTo selesai
                'End If

                'HAPUS DATA SERIAL YG TIDAK SESUAI DENGAN DATA BARANG
                AsDataTableDeleteData(dtserial, ftCekSerial)

            ElseIf (dtserialBaru.Rows.Count > 0) Then
                'errmessage = "Serial Item not found." : GoTo selesai
                'JIKA TERDAPAT DATA SERIAL TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA SERIAL
                AsDataTableDeleteData(dtserial, ftCekSerial)

            End If


        ElseIf (dtbatchBaru.Rows.Count > 0 Or dtserialBaru.Rows.Count > 0) Then
            'errmessage = "Batch Item not found." : GoTo selesai
            If dtbatchBaru.Rows.Count > 0 Then
                'JIKA TERDAPAT DATA BATCH TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA BATCH
                AsDataTableDeleteData(dtbatch, ftCekBatch)
            End If
            If dtserialBaru.Rows.Count > 0 Then
                'JIKA TERDAPAT DATA SERIAL TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA SERIAL
                AsDataTableDeleteData(dtserial, ftCekSerial)
            End If

        End If

selesai:
        Return errmessage
    End Function

    Public Function ValidasiBatchSerial(ByVal dtdetail As DataTable, ByRef dtbatch As DataTable, ByRef dtserial As DataTable, ByVal ftbarang As String, ByVal fieldJmlBarang As String, ByVal jenismutasi As Double) As String
        Dim errmessage As String = "", sql As String = ""

        Dim dtbatchBaru As New DataTable, dtserialBaru As New DataTable
        Dim dtval As New DataTable, dtbarang As New DataTable, dtLookup As New DataTable
        Dim jmlbarang As Double = 0, jmlnomor As Double = 0, urutan As Double = 0
        Dim kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuanbarang As String = ""

        'CEK VARIBEL
        If Len(fieldJmlBarang) = 0 Then errmessage = "Field jmlbarang can't be empty." : GoTo selesai
        If Len(ftbarang) = 0 Then errmessage = "Filter barang can't be empty." : GoTo selesai

        'AMBIL DTBATCH DAN SERIAL SESUAI JENISMUTASINYA
        dtbatchBaru = AsDataTableFilterSortDt(dtbatch, "nbtjenismutasi = '" & jenismutasi & "'")
        dtserialBaru = AsDataTableFilterSortDt(dtserial, "nstjenismutasi = '" & jenismutasi & "'")

        'BUAT FILTER DT BATCH DAN SERIAL
        Dim ftCekBatch As String = "(nbtjenismutasi = '" & jenismutasi & "')"
        Dim ftCekSerial As String = "(nstjenismutasi = '" & jenismutasi & "')"

        '1. AMBIL BARANG BATCH DAN SERIAL
        dtbarang = AsDataTableAmbilDariDB("SELECT bid, bkode, bsatuan, bbatch, bserial FROM m1_item WHERE (bbatch = 1 OR bserial = 1) AND (" & ftbarang & ")")

        '2. CEK NO BATCH DAN SERIAL
        If dtbarang.Rows.Count > 0 Then
            '2.1 CEK NO BATCH
            dtval = AsDataTableFilterSortDt(dtbarang, "bbatch = 1")
            If dtval.Rows.Count > 0 Then
                For Each dr As DataRow In dtval.Rows

                    'AMBIL JMLBARANG DARI DETAIL
                    jmlbarang = AsDataTableDSum(dtdetail, fieldJmlBarang, "idbarang = '" & dr("bid") & "'")

                    'AMBIL JMLBARANG DARI BATCH
                    jmlnomor = AsDataTableDSum(dtbatchBaru, "nbtjml", "nbtjenismutasi = '" & jenismutasi & "' AND nbtidbarang = '" & dr("bid") & "'")

                    'BANDINGKAN JMLBARANG DETAIL DAN BATCH
                    If Math.Round(jmlbarang, 5) <> Math.Round(jmlnomor, 5) Then
                        dtLookup = AsDataTableFilterLimit(dtdetail, "idbarang = '" & dr("bid") & "'", , , 1)
                        urutan = dtLookup.Rows(0)("urutan")
                        kodebarang = dr("bkode")
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuanbarang = dr("bsatuan")
                        errmessage = "No. Batch for Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " is not equal to the number of items in detail transactions, it must be " & jmlbarang & " " & satuanbarang & " " & jmlnomor : GoTo selesai
                    End If



                    'BUAT FILTER UNTUK CEK DATA BATCH YG TIDAK SESUAI DENGAN DATA BARANG
                    ftCekBatch = IIf(Len(ftCekBatch.ToString) = 0, "", ftCekBatch & " AND ")
                    ftCekBatch = String.Concat(ftCekBatch, "(nbtidbarang <> '" & dr("bid") & "')")
                Next

                ''CEK DATA BATCH YG TIDAK SESUAI DENGAN DATA BARANG
                'dtval = AsDataTableFilterSortDt(dtbatchBaru, ftCekBatch)
                'If dtval.Rows.Count > 0 Then
                '    errmessage = "No. Batch : " & dtval(0)("nbtkode") & ", doesn't match with item in detail transactions." : GoTo selesai
                'End If

                'HAPUS DATA BATCH YG TIDAK SESUAI DENGAN DATA BARANG
                AsDataTableDeleteData(dtbatch, ftCekBatch)
                dtbatchBaru = AsDataTableFilterSortDt(dtbatch, "nbtjenismutasi = '" & jenismutasi & "'")
                'VALIDASI CEK NOMOR BATCH HARUS SESUAI NOMOR SO, STATUS SO <> 4, STATUS SO <> 7/CLOSE
                If jenismutasi = 1 Then
                    Dim dtSO As DataTable = AsDataTableAmbilDariDB("SELECT sonotransaksi, sostatus FROM m5_so")
                    If dtSO.Rows.Count > 0 Then
                        If dtbatchBaru.Rows.Count > 0 Then
                            Dim vNoSO As String = ""
                            For Each drB As DataRow In dtbatchBaru.Rows
                                vNoSO = AsDataTableDLookup(dtSO, "sostatus", "sonotransaksi = '" & FixQuotes(drB("nbtkode")) & "'", "xxx")
                                If vNoSO.Equals("4") Or vNoSO.Equals("7") Then
                                    errmessage = "Batch for SO no '" & drB("nbtkode") & "' has been Closed/Completed." : GoTo selesai
                                ElseIf vNoSO.Equals("xxx") Then
                                    errmessage = "Batch for SO no '" & drB("nbtkode") & "' doesn't exist in Data SO." : GoTo selesai
                                End If
                            Next
                        End If
                    Else
                        errmessage = "Data SO for No Batch not found." : GoTo selesai
                    End If

                    If dtbatchBaru.Rows.Count > 0 Then
                        Dim vNoSO As String = ""
                        Dim dtSODetail As DataTable
                        For Each drB As DataRow In dtbatchBaru.Rows
                            dtSODetail = AsDataTableAmbilDariDB("SELECT sonotransaksi, sostatus FROM m5_so join m5_so_detail on soid = idso where idbarang = '" & drB("nbtidbarang") & "' and sonotransaksi = '" & FixQuotes(drB("nbtkode")) & "' and sostatus in(2,3,4,7)")
                            If dtSODetail.Rows.Count = 0 Then
                                Dim dtBarangx As DataTable = AsDataTableAmbilDariDB("SELECT bkode, bnama, btipe from m1_item where bid = '" & drB("nbtidbarang") & "'")

                                errmessage = "Item :" & dtBarangx.Rows(0)("bkode") & " | " & dtBarangx.Rows(0)("btipe") & " | " & dtBarangx.Rows(0)("bnama") & " doesn't exists/yet in SO " & drB("nbtkode") & "" : GoTo selesai
                            End If

                        Next

                    End If
                End If

            ElseIf (dtbatchBaru.Rows.Count > 0) Then
                'errmessage = "Batch Item not found." : GoTo selesai
                'JIKA TERDAPAT DATA BATCH TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA BATCH
                AsDataTableDeleteData(dtbatch, ftCekBatch)

            End If

            '2.2 CEK NO SERIAL
            dtval = AsDataTableFilterSortDt(dtbarang, "bserial = 1")
            If dtval.Rows.Count > 0 Then
                For Each dr As DataRow In dtval.Rows
                    'AMBIL JMLBARANG DARI DETAIL
                    jmlbarang = AsDataTableDSum(dtdetail, fieldJmlBarang, "idbarang = '" & dr("bid") & "'")

                    'AMBIL JMLBARANG DARI SERIAL
                    jmlnomor = AsDataTableDSum(dtserialBaru, "nstjml", "nstjenismutasi = '" & jenismutasi & "' AND nstidbarang = '" & dr("bid") & "'")

                    'BANDINGKAN JMLBARANG DETAIL DAN SERIAL
                    If Math.Round(jmlbarang, 5) <> Math.Round(jmlnomor, 5) Then
                        dtLookup = AsDataTableFilterLimit(dtdetail, "idbarang = '" & dr("bid") & "'", , , 1)
                        urutan = dtLookup.Rows(0)("urutan")
                        kodebarang = dr("bkode")
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuanbarang = dr("bsatuan")
                        errmessage = "No. Serial for Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " is not equal to the number of items in detail transactions, it must be " & jmlbarang & " " & satuanbarang : GoTo selesai
                    End If

                    'BUAT FILTER UNTUK CEK DATA SERIAL YG TIDAK SESUAI DENGAN DATA BARANG
                    ftCekSerial = IIf(Len(ftCekSerial.ToString) = 0, "", ftCekSerial & " AND ")
                    ftCekSerial = String.Concat(ftCekSerial, "(nstidbarang <> '" & dr("bid") & "')")
                Next

                ''CEK DATA SERIAL YG TIDAK SESUAI DENGAN DATA BARANG
                'dtval = AsDataTableFilterSortDt(dtserialBaru, ftCekSerial)
                'If dtval.Rows.Count > 0 Then
                '    errmessage = "No. Serial : " & dtval(0)("nstkode") & ", doesn't match with item in detail transactions." : GoTo selesai
                'End If

                'HAPUS DATA SERIAL YG TIDAK SESUAI DENGAN DATA BARANG
                AsDataTableDeleteData(dtserial, ftCekSerial)

            ElseIf (dtserialBaru.Rows.Count > 0) Then
                'errmessage = "Serial Item not found." : GoTo selesai
                'JIKA TERDAPAT DATA SERIAL TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA SERIAL
                AsDataTableDeleteData(dtserial, ftCekSerial)

            End If


        ElseIf (dtbatchBaru.Rows.Count > 0 Or dtserialBaru.Rows.Count > 0) Then
            'errmessage = "Batch Item not found." : GoTo selesai
            If dtbatchBaru.Rows.Count > 0 Then
                'JIKA TERDAPAT DATA BATCH TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA BATCH
                AsDataTableDeleteData(dtbatch, ftCekBatch)
            End If
            If dtserialBaru.Rows.Count > 0 Then
                'JIKA TERDAPAT DATA SERIAL TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA SERIAL
                AsDataTableDeleteData(dtserial, ftCekSerial)
            End If

        End If

selesai:
        Return errmessage
    End Function
	
	
	    Public Function ValidasiBatchSerialSR(ByVal dtdetail As DataTable, ByRef dtbatch As DataTable, ByRef dtserial As DataTable, ByVal ftbarang As String, ByVal fieldJmlBarang As String, ByVal jenismutasi As Double) As String
        Dim errmessage As String = "", sql As String = ""

        Dim dtbatchBaru As New DataTable, dtserialBaru As New DataTable
        Dim dtval As New DataTable, dtbarang As New DataTable, dtLookup As New DataTable
        Dim jmlbarang As Double = 0, jmlnomor As Double = 0, urutan As Double = 0
        Dim kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuanbarang As String = ""

        'CEK VARIBEL
        If Len(fieldJmlBarang) = 0 Then errmessage = "Field jmlbarang can't be empty." : GoTo selesai
        If Len(ftbarang) = 0 Then errmessage = "Filter barang can't be empty." : GoTo selesai

        'AMBIL DTBATCH DAN SERIAL SESUAI JENISMUTASINYA
        dtbatchBaru = AsDataTableFilterSortDt(dtbatch, "nbtjenismutasi = '" & jenismutasi & "'")
        dtserialBaru = AsDataTableFilterSortDt(dtserial, "nstjenismutasi = '" & jenismutasi & "'")

        'BUAT FILTER DT BATCH DAN SERIAL
        Dim ftCekBatch As String = "(nbtjenismutasi = '" & jenismutasi & "')"
        Dim ftCekSerial As String = "(nstjenismutasi = '" & jenismutasi & "')"

        '1. AMBIL BARANG BATCH DAN SERIAL
        dtbarang = AsDataTableAmbilDariDB("SELECT bid, bkode, bsatuan, bbatch, bserial FROM m1_item WHERE (bbatch = 1 OR bserial = 1) AND (" & ftbarang & ")")

        '2. CEK NO BATCH DAN SERIAL
        If dtbarang.Rows.Count > 0 Then
            '2.1 CEK NO BATCH
            dtval = AsDataTableFilterSortDt(dtbarang, "bbatch = 1")
            If dtval.Rows.Count > 0 Then
                For Each dr As DataRow In dtval.Rows

                    'AMBIL JMLBARANG DARI DETAIL
                    jmlbarang = AsDataTableDSum(dtdetail, fieldJmlBarang, "idbarang = '" & dr("bid") & "'")

                    'AMBIL JMLBARANG DARI BATCH
                    jmlnomor = AsDataTableDSum(dtbatchBaru, "nbtjml", "nbtjenismutasi = '" & jenismutasi & "' AND nbtidbarang = '" & dr("bid") & "'")

                    'BANDINGKAN JMLBARANG DETAIL DAN BATCH
                    If Math.Round(jmlbarang, 5) <> Math.Round(jmlnomor, 5) Then
                        dtLookup = AsDataTableFilterLimit(dtdetail, "idbarang = '" & dr("bid") & "'", , , 1)
                        urutan = dtLookup.Rows(0)("urutan")
                        kodebarang = dr("bkode")
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuanbarang = dr("bsatuan")
                        errmessage = "No. Batch for Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " is not equal to the number of items in detail transactions, it must be " & jmlbarang & " " & satuanbarang & " " & jmlnomor : GoTo selesai
                    End If



                    'BUAT FILTER UNTUK CEK DATA BATCH YG TIDAK SESUAI DENGAN DATA BARANG
                    ftCekBatch = IIf(Len(ftCekBatch.ToString) = 0, "", ftCekBatch & " AND ")
                    ftCekBatch = String.Concat(ftCekBatch, "(nbtidbarang <> '" & dr("bid") & "')")
                Next

                ''CEK DATA BATCH YG TIDAK SESUAI DENGAN DATA BARANG
                'dtval = AsDataTableFilterSortDt(dtbatchBaru, ftCekBatch)
                'If dtval.Rows.Count > 0 Then
                '    errmessage = "No. Batch : " & dtval(0)("nbtkode") & ", doesn't match with item in detail transactions." : GoTo selesai
                'End If

                'HAPUS DATA BATCH YG TIDAK SESUAI DENGAN DATA BARANG
                AsDataTableDeleteData(dtbatch, ftCekBatch)
                dtbatchBaru = AsDataTableFilterSortDt(dtbatch, "nbtjenismutasi = '" & jenismutasi & "'")
                'VALIDASI CEK NOMOR BATCH HARUS SESUAI NOMOR SO, STATUS SO <> 4, STATUS SO <> 7/CLOSE
                If jenismutasi = 1 Then
                    'Dim dtSO As DataTable = AsDataTableAmbilDariDB("SELECT sonotransaksi, sostatus FROM m5_so")
                    'If dtSO.Rows.Count > 0 Then
                        'If dtbatchBaru.Rows.Count > 0 Then
                        '    Dim vNoSO As String = ""
                        '    For Each drB As DataRow In dtbatchBaru.Rows
                        '        vNoSO = AsDataTableDLookup(dtSO, "sostatus", "sonotransaksi = '" & FixQuotes(drB("nbtkode")) & "'", "xxx")
                        '        If vNoSO.Equals("4") Or vNoSO.Equals("7") Then
                        '            errmessage = "Batch for SO no '" & drB("nbtkode") & "' has been Closed/Completed." : GoTo selesai
                        '        ElseIf vNoSO.Equals("xxx") Then
                        '            errmessage = "Batch for SO no '" & drB("nbtkode") & "' doesn't exist in Data SO." : GoTo selesai
                        '        End If
                        '    Next
                        'End If
                    'Else
                    '    errmessage = "Data SO for No Batch not found." : GoTo selesai
                    'End If

                    If dtbatchBaru.Rows.Count > 0 Then
                        Dim vNoSO As String = ""
                        Dim dtSODetail As DataTable
                        For Each drB As DataRow In dtbatchBaru.Rows
                            dtSODetail = AsDataTableAmbilDariDB("SELECT sonotransaksi, sostatus FROM m5_so join m5_so_detail on soid = idso where idbarang = '" & drB("nbtidbarang") & "' and sonotransaksi = '" & FixQuotes(drB("nbtkode")) & "' and sostatus in(2,3,4,7)")
                            If dtSODetail.Rows.Count = 0 Then
                                Dim dtBarangx As DataTable = AsDataTableAmbilDariDB("SELECT bkode, bnama, btipe from m1_item where bid = '" & drB("nbtidbarang") & "'")

                                errmessage = "Item :" & dtBarangx.Rows(0)("bkode") & " | " & dtBarangx.Rows(0)("btipe") & " | " & dtBarangx.Rows(0)("bnama") & " doesn't exists/yet in SO " & drB("nbtkode") & "" : GoTo selesai
                            End If

                        Next

                    End If
                End If

            ElseIf (dtbatchBaru.Rows.Count > 0) Then
                'errmessage = "Batch Item not found." : GoTo selesai
                'JIKA TERDAPAT DATA BATCH TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA BATCH
                AsDataTableDeleteData(dtbatch, ftCekBatch)

            End If

            '2.2 CEK NO SERIAL
            dtval = AsDataTableFilterSortDt(dtbarang, "bserial = 1")
            If dtval.Rows.Count > 0 Then
                For Each dr As DataRow In dtval.Rows
                    'AMBIL JMLBARANG DARI DETAIL
                    jmlbarang = AsDataTableDSum(dtdetail, fieldJmlBarang, "idbarang = '" & dr("bid") & "'")

                    'AMBIL JMLBARANG DARI SERIAL
                    jmlnomor = AsDataTableDSum(dtserialBaru, "nstjml", "nstjenismutasi = '" & jenismutasi & "' AND nstidbarang = '" & dr("bid") & "'")

                    'BANDINGKAN JMLBARANG DETAIL DAN SERIAL
                    If Math.Round(jmlbarang, 5) <> Math.Round(jmlnomor, 5) Then
                        dtLookup = AsDataTableFilterLimit(dtdetail, "idbarang = '" & dr("bid") & "'", , , 1)
                        urutan = dtLookup.Rows(0)("urutan")
                        kodebarang = dr("bkode")
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuanbarang = dr("bsatuan")
                        errmessage = "No. Serial for Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " is not equal to the number of items in detail transactions, it must be " & jmlbarang & " " & satuanbarang : GoTo selesai
                    End If

                    'BUAT FILTER UNTUK CEK DATA SERIAL YG TIDAK SESUAI DENGAN DATA BARANG
                    ftCekSerial = IIf(Len(ftCekSerial.ToString) = 0, "", ftCekSerial & " AND ")
                    ftCekSerial = String.Concat(ftCekSerial, "(nstidbarang <> '" & dr("bid") & "')")
                Next

                ''CEK DATA SERIAL YG TIDAK SESUAI DENGAN DATA BARANG
                'dtval = AsDataTableFilterSortDt(dtserialBaru, ftCekSerial)
                'If dtval.Rows.Count > 0 Then
                '    errmessage = "No. Serial : " & dtval(0)("nstkode") & ", doesn't match with item in detail transactions." : GoTo selesai
                'End If

                'HAPUS DATA SERIAL YG TIDAK SESUAI DENGAN DATA BARANG
                AsDataTableDeleteData(dtserial, ftCekSerial)

            ElseIf (dtserialBaru.Rows.Count > 0) Then
                'errmessage = "Serial Item not found." : GoTo selesai
                'JIKA TERDAPAT DATA SERIAL TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA SERIAL
                AsDataTableDeleteData(dtserial, ftCekSerial)

            End If


        ElseIf (dtbatchBaru.Rows.Count > 0 Or dtserialBaru.Rows.Count > 0) Then
            'errmessage = "Batch Item not found." : GoTo selesai
            If dtbatchBaru.Rows.Count > 0 Then
                'JIKA TERDAPAT DATA BATCH TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA BATCH
                AsDataTableDeleteData(dtbatch, ftCekBatch)
            End If
            If dtserialBaru.Rows.Count > 0 Then
                'JIKA TERDAPAT DATA SERIAL TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA SERIAL
                AsDataTableDeleteData(dtserial, ftCekSerial)
            End If

        End If

selesai:
        Return errmessage
    End Function
	

    Public Function ValidasiAsset(ByVal dtDetail As DataTable, ByRef dtAsset As DataTable, ByVal ftBarang As String, ByVal fieldJmlBarang As String, ByVal jenismutasi As Double) As String
        Dim errmessage As String = "", sql As String = ""

        Dim dtAssetBaru As New DataTable
        Dim dtVal As New DataTable, dtBarang As New DataTable, dtLookup As New DataTable
        Dim jmlbarang As Double = 0, jmlnomor As Double = 0, urutan As Double = 0
        Dim kodebarang As String = "", tipebarang As String = "", namabarang As String = "", satuanbarang As String = ""

        'CEK VARIBEL
        If Len(fieldJmlBarang) = 0 Then errmessage = "Field jmlbarang can't be empty." : GoTo selesai
        If Len(ftBarang) = 0 Then errmessage = "Filter barang can't be empty." : GoTo selesai

        'AMBIL ASSET SESUAI JENISMUTASINYA
        dtAssetBaru = AsDataTableFilterSortDt(dtAsset, "atjenismutasi = '" & jenismutasi & "'")

        'BUAT FILTER DT ASSET
        Dim ftCekAsset As String = "(atjenismutasi = '" & jenismutasi & "')"

        '1. AMBIL BARANG ASSET
        dtBarang = AsDataTableAmbilDariDB("SELECT bid, bkode, bsatuan, basset FROM m1_item WHERE (basset = 1) AND (" & ftBarang & ")")

        '2. CEK ASSET
        If dtBarang.Rows.Count > 0 Then
            '2.1 CEK ASSET
            dtVal = AsDataTableFilterSortDt(dtBarang, "basset = 1")
            If dtVal.Rows.Count > 0 Then
                For Each dr As DataRow In dtVal.Rows
                    'AMBIL JMLBARANG DARI DETAIL
                    jmlbarang = AsDataTableDSum(dtDetail, fieldJmlBarang, "idbarang = '" & dr("bid") & "'")

                    'AMBIL JMLBARANG DARI ASSET
                    jmlnomor = AsDataTableDSum(dtAssetBaru, "atjml", "atjenismutasi = '" & jenismutasi & "' AND atidbarang = '" & dr("bid") & "'")

                    'BANDINGKAN JMLBARANG DETAIL DAN ASSET
                    If jmlbarang <> jmlnomor Then
                        dtLookup = AsDataTableFilterLimit(dtDetail, "idbarang = '" & dr("bid") & "'", , , 1)
                        urutan = dtLookup.Rows(0)("urutan")
                        kodebarang = dr("bkode")
                        tipebarang = dtLookup.Rows(0)("tipebarang")
                        namabarang = dtLookup.Rows(0)("namabarang")
                        satuanbarang = dr("bsatuan")
                        errmessage = "Asset for Row : " & urutan & " - " & kodebarang & " | " & tipebarang & " | " & namabarang & " is not equal to the number of items in detail transactions, it must be " & jmlbarang & " " & satuanbarang : GoTo selesai
                    End If

                    'BUAT FILTER UNTUK CEK DATA BATCH YG TIDAK SESUAI DENGAN DATA BARANG
                    ftCekAsset = IIf(Len(ftCekAsset.ToString) = 0, "", ftCekAsset & " AND ")
                    ftCekAsset = String.Concat(ftCekAsset, "(atidbarang <> '" & dr("bid") & "')")
                Next

                ''CEK DATA ASSET YG TIDAK SESUAI DENGAN DATA BARANG
                'dtval = AsDataTableFilterSortDt(dtassetBaru, ftCekasset)
                'If dtval.Rows.Count > 0 Then
                '    errmessage = "Asset : " & dtval(0)("atkode") & ", doesn't match with item in detail transactions." : GoTo selesai
                'End If

                'HAPUS DATA ASSET YG TIDAK SESUAI DENGAN DATA BARANG
                AsDataTableDeleteData(dtAsset, ftCekAsset)

            ElseIf (dtAssetBaru.Rows.Count > 0) Then
                'errmessage = "Asset Item not found." : GoTo selesai
                'JIKA TERDAPAT DATA ASSET TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA ASSET
                AsDataTableDeleteData(dtAsset, ftCekAsset)

            End If


        ElseIf (dtAssetBaru.Rows.Count > 0) Then
            'errmessage = "Asset Item not found." : GoTo selesai
            If dtAssetBaru.Rows.Count > 0 Then
                'JIKA TERDAPAT DATA ASSET TAPI DATA DETAIL TIDAK ADA, MAKA HAPUS DATA ASSET
                AsDataTableDeleteData(dtAsset, ftCekAsset)
            End If

        End If

selesai:
        Return errmessage
    End Function

    Public Function ValidasiGudangAsset(ByVal dtAsset As DataTable, ByVal gudang As String, Optional ByVal jenismutasi As Integer = -1) As String
        Dim errmessage As String = "", sql As String = ""
        Dim dtVal As DataTable, dtAssetNew As DataTable

        If jenismutasi <> -1 Then
            dtAssetNew = AsDataTableFilterSortDt(dtAsset, "atjenismutasi = '" & jenismutasi & "'")
        Else
            dtAssetNew = dtAsset
        End If

        If dtAssetNew.Rows.Count > 0 Then
            Dim strValue1 As New StringBuilder
            Dim strValue2 As New StringBuilder
            For Each dr1 As DataRow In dtAssetNew.Rows
                'FILTER EXIST ASET
                strValue1.Append(IIf(Len(strValue1.ToString) = 0, "", " UNION "))
                strValue1.Append("SELECT EXISTS(SELECT 1 FROM m7_asset WHERE aid = '" & dr1("atasetid") & "' LIMIT 1) as rowExists, '" & dr1("atidbarang") & "' as idbarang, bkode, '" & dr1("atkode") & "' as atkode FROM m1_item WHERE bid = '" & dr1("atidbarang") & "'")

                'FILTER GUDANG ASET
                strValue2.Append(IIf(Len(strValue2.ToString) = 0, "", ", "))
                strValue2.Append(FixDouble(dr1("atasetid")))
            Next

            'VALIDASI EXIST ASET
            dtVal = AsDataTableAmbilDariDB(strValue1.ToString) 'ftExistSerial = rowExists, idbarang, bkode, atkode
            dtVal = AsDataTableFilterLimit(dtVal, "rowExists = 0", , , 1)
            If dtVal.Rows.Count > 0 Then
                errmessage = "Item : " & dtVal.Rows(0)("bkode") & " | Asset : " & dtVal.Rows(0)("atkode") & " doesn't exists in Asset data." : GoTo selesai
            End If

            'VALIDASI GUDANG ASET
            sql = "SELECT a.akode, a.anama FROM m7_asset a WHERE a.aid IN(" & strValue2.ToString & ") AND a.agudang <> '" & FixQuotes(gudang) & "' LIMIT 1"
            Dim dtValAset As DataTable = AsDataTableAmbilDariDB(sql)
            If dtValAset.Rows.Count > 0 Then
                errmessage = "Asset " & dtValAset(0)("akode") & " - " & dtValAset(0)("anama") & " doesn't exist in '" & gudang & "' warehouse." : GoTo selesai
            End If
        End If

selesai:
        Return errmessage
    End Function

    Public Function SendMsmq(ByVal msmqPath As String, ByVal tipe As String, ByVal id As String, ByVal sumber As String, ByVal idtransaksi As Integer, ByVal userid As Integer) As String
        Dim hasil As String = ""

        'CEK FOLDER MSMQ
        Try
            If Not MessageQueue.Exists(msmqPath) Then
                hasil = "MSMQ directory doesn't exist." : GoTo selesai
            End If
        Catch ex As Exception
            hasil = ex.Message : GoTo selesai
        End Try

        'KIRIM ANTRIAN MSMQ
        Try
            Dim mymsmq As New MessageQueue(msmqPath)
            mymsmq.Send(String.Concat(tipe, sptField, id, sptField, sumber, sptField, idtransaksi, sptField, userid, sptField, HttpContext.Current.Server.MapPath("~")))
        Catch ex As Exception
            hasil = ex.Message : GoTo selesai
        End Try

selesai:
        Return hasil
    End Function

    Public Function SendMsmqReqJurnalUlang(ByVal data As String) As String
        Dim hasil As String = ""

        'CEK FOLDER MSMQ
        Try
            If Not MessageQueue.Exists(dirMsmq) Then
                hasil = "MSMQ directory doesn't exist." : GoTo selesai
            End If
        Catch ex As Exception
            hasil = ex.Message : GoTo selesai
        End Try

        'KIRIM ANTRIAN MSMQ
        Try
            Dim mymsmq As New MessageQueue(dirMsmq)
            mymsmq.Send(String.Concat(data, sptField, HttpContext.Current.Server.MapPath("~")))
        Catch ex As Exception
            hasil = ex.Message : GoTo selesai
        End Try

selesai:
        Return hasil
    End Function

    Public Function SendMsmqReport(ByVal msmqPath As String, ByVal strValue As String) As String
        Dim hasil As String = ""

        'CEK FOLDER MSMQ
        Try
            If Not MessageQueue.Exists(msmqPath) Then
                MessageQueue.Create(msmqPath)
            End If
        Catch ex As Exception
            hasil = ex.Message : GoTo selesai
        End Try

        'KIRIM ANTRIAN MSMQ
        Try
            Dim mymsmq As New MessageQueue(msmqPath)
            mymsmq.Send(strValue)
        Catch ex As Exception
            hasil = ex.Message : GoTo selesai
        End Try

selesai:
        Return hasil
    End Function

    Public Function SendMsmqLogin(ByVal msmqPath As String, ByVal tipe As String, ByVal id As String, ByVal userid As String, ByVal AppCode As String) As String
        Dim hasil As String = ""

        msmqPath = ".\PRIVATE$\UserLogin"

        'CEK FOLDER MSMQ
        Try
            If Not MessageQueue.Exists(msmqPath) Then
                hasil = "MSMQ directory doesn't exist." : GoTo selesai
            End If
        Catch ex As Exception
            hasil = ex.Message : GoTo selesai
        End Try

        'KIRIM ANTRIAN MSMQ
        Try
            Dim mymsmq As New MessageQueue(msmqPath)
            mymsmq.Send(String.Concat(tipe, sptField, id, sptField, userid, sptField, HttpContext.Current.Server.MapPath("~")))

        Catch ex As Exception
            hasil = ex.Message : GoTo selesai
        End Try
selesai:
        Return hasil
    End Function

    Public Function F_Diskon(ByVal jml As Double, ByVal harga As Double, ByVal diskon As String) As Double
        Dim vjumlahdiskon As Double = 0, total As Double = jml * harga
        Dim diskonSplit() As String = diskon.Split("+")
        If diskonSplit.Length > 0 Then
            For i = 1 To diskonSplit.Length
                'jumlahdiskon = jumlahdiskon + (diskon% pada posisi looping / 100) * (total - jumlahdiskon)
                vjumlahdiskon = vjumlahdiskon + (Double.Parse(diskonSplit(i - 1)) / 100) * (total - vjumlahdiskon)
            Next
        End If
        Return vjumlahdiskon
    End Function

    Public Function ValidasiMatauangCOA(ByVal DtMain As DataTable, ByVal MainCurrencyField As String, ByVal MainArrayField As String, ByVal DtDetail As DataTable, ByVal DetailArrayField As String, Optional ByVal PemisahArray As String = "~", Optional ByVal MainArrayFieldMessage As String = "", Optional ByVal DetailArrayFieldMessage As String = "", Optional ByVal DetailUrutanField As String = "urutan") As String
        Dim ErrMessage As String = "", DtCoa As New DataTable, DtValidasi As New DataTable
        Dim Filter As String = "", Sql As String = "", CurrField As String = "", CurrFieldMessage As String = "", Norek As String = ""
        Dim valNorek As String = "", valNama As String = "", valMatauang As String = "", valUrutan As String = ""

        'SET FIELD UTAMA ===================================================
        Dim vMain() As String = Split(MainArrayField, PemisahArray)

        'SET FIELD MESSAGE UTAMA
        Dim vMainMessage() As String
        If Len(MainArrayFieldMessage) <> 0 Then
            vMainMessage = Split(MainArrayFieldMessage, PemisahArray)
        Else
            vMainMessage = vMain
        End If

        'VALIDASI JML FIELD UTAMA DAN FIELD MESSAGE UTAMA
        If vMain.Length <> vMainMessage.Length Then
            ErrMessage = "Invalid MainArrayFieldMessage." : GoTo selesai
        End If
        'END OF SET FIELD UTAMA ============================================


        'SET FIELD DETAIL ==================================================
        Dim vDetail() As String = Split(DetailArrayField, PemisahArray)

        'SET FIELD MESSAGE DETAIL
        Dim vDetailMessage() As String
        If Len(DetailArrayFieldMessage) <> 0 Then
            vDetailMessage = Split(DetailArrayFieldMessage, PemisahArray)
        Else
            vDetailMessage = vDetail
        End If

        'VALIDASI JML FIELD DETAIL DAN FIELD MESSAGE DETAIL
        If vDetail.Length <> vDetailMessage.Length Then
            ErrMessage = "Invalid DetailArrayFieldMessage." : GoTo selesai
        End If
        'END OF SET FIELD DETAIL ===========================================


        'AMBIL MATAUANG FUNGSIONAL =========================================
        Dim dtMatauang As DataTable = AsDataTableAmbilDariDB("SELECT skode, snilai FROM m0_setting WHERE smodule = 0 AND sgrup = 'accounting' AND (skode = 'MataUangFungsional' OR skode = 'Kurs')")
        Dim uangFungsional As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'MataUangFungsional'", "Not found")
        If uangFungsional = "Not found" Then
            ErrMessage = "Setting Functional Currency not found." : GoTo selesai
        End If
        Dim kursFungsional As String = AsDataTableDLookup(dtMatauang, "snilai", "skode = 'Kurs'", "Not found")
        If kursFungsional = "Not found" Then
            ErrMessage = "Setting Exchange Rate Functional Currency not found." : GoTo selesai
        End If
        'END OF AMBIL MATAUANG FUNGSIONAL ==================================


        'VALIDASI MATAUANG COA =============================================
        If DtMain.Rows.Count > 0 Then
            'SET MATAUANG UTAMA
            Dim uangUtama As String = AsDataTableDLookup(DtMain, MainCurrencyField, "", "Not Found")

            'VALIDASI DATA UTAMA ----------------------------------
            If Len(MainArrayField) > 0 And vMain.Length > 0 Then
                'PERULANGAN SEBANYAK FIELD UTAMA
                For i = 1 To vMain.Length
                    'SET FIELD DAN FIELD MESSAGE
                    CurrField = vMain(i - 1) : CurrFieldMessage = vMainMessage(i - 1)

                    'PERULANGAN SEBANYAK ROW DATA UTAMA
                    For Each dr As DataRow In DtMain.Rows
                        'SET NOREK
                        Norek = dr(CurrField)

                        'SET FILTER COA
                        Filter = IIf(Len(Filter) = 0, "", Filter & " OR ")
                        Filter = String.Concat(Filter, " cnomor = '" & Norek & "' ")
                    Next

                    'VALIDASI KE DATABASE (M1_COA)
                    'AMBIL NOREK YANG MEMILIKI MATAUANG <> MATAUANG FUNGSIONAL DAN <> MATAUANG UTAMA
                    Sql = "SELECT cnomor, cnama, cmatauang FROM m1_coa "
                    Sql &= " WHERE cmatauang <> '" & uangFungsional & "' AND cmatauang <> '" & uangUtama & "' "
                    Sql &= " AND (" & Filter & ") "
                    DtCoa = AsDataTableAmbilDariDB(Sql)

                    'JIKA TERDAPAT DATA, MAKA TAMPILKAN ALERT
                    If DtCoa.Rows.Count > 0 Then
                        'AMBIL NOREK, NAMA DAN MATAUANG DARI M1_COA
                        valNorek = DtCoa.Rows(0)("cnomor")
                        valNama = DtCoa.Rows(0)("cnama")
                        valMatauang = DtCoa.Rows(0)("cmatauang")
                        'ErrMessage = "Main Transaction : Invalid COA Currency for column " & CurrFieldMessage & " on " & valNorek & " - " & valNama & " (" & valMatauang & ")." : GoTo selesai
                        ErrMessage = "Main Transaction : Invalid COA Currency on " & valNorek & " - " & valNama & " (" & valMatauang & ")." : GoTo selesai
                    End If

                    'CLEAR FILTER
                    Filter = ""
                Next
            End If
            'END OF VALIDASI DATA UTAMA ---------------------------


            'VALIDASI DATA DETAIL ---------------------------------
            If DtDetail.Rows.Count > 0 And Len(DetailArrayField) > 0 And vDetail.Length > 0 Then
                'PERULANGAN SEBANYAK FIELD DETAIL
                For i = 1 To vDetail.Length
                    'SET FIELD DAN FIELD MESSAGE
                    CurrField = vDetail(i - 1) : CurrFieldMessage = vDetailMessage(i - 1)

                    'PERULANGAN SEBANYAK ROW DATA DETAIL
                    For Each dr As DataRow In DtDetail.Rows
                        'SET NOREK
                        Norek = dr(CurrField)

                        'SET FILTER COA
                        Filter = IIf(Len(Filter) = 0, "", Filter & " OR ")
                        Filter = String.Concat(Filter, " cnomor = '" & Norek & "' ")
                    Next

                    'VALIDASI KE DATABASE (M1_COA)
                    'AMBIL NOREK YANG MEMILIKI MATAUANG <> MATAUANG FUNGSIONAL DAN <> MATAUANG UTAMA
                    Sql = "SELECT cnomor, cnama, cmatauang FROM m1_coa "
                    Sql &= " WHERE cmatauang <> '" & uangFungsional & "' AND cmatauang <> '" & uangUtama & "' "
                    Sql &= " AND (" & Filter & ") "
                    DtCoa = AsDataTableAmbilDariDB(Sql)

                    'JIKA TERDAPAT DATA, MAKA TAMPILKAN ALERT
                    If DtCoa.Rows.Count > 0 Then
                        'AMBIL NOREK, NAMA DAN MATAUANG DARI M1_COA
                        valNorek = DtCoa.Rows(0)("cnomor")
                        valNama = DtCoa.Rows(0)("cnama")
                        valMatauang = DtCoa.Rows(0)("cmatauang")
                        'AMBIL URUTAN DARI DATA DETAIL
                        valUrutan = AsDataTableDLookup(DtDetail, DetailUrutanField, CurrField & " = '" & valNorek & "'")
                        'ErrMessage = "Detail Row - " & valUrutan & " : Invalid COA Currency for column " & CurrFieldMessage & " on " & valNorek & " - " & valNama & " (" & valMatauang & ")." : GoTo selesai
                        ErrMessage = "Detail Row - " & valUrutan & " : Invalid COA Currency on " & valNorek & " - " & valNama & " (" & valMatauang & ")." : GoTo selesai
                    End If

                    'CLEAR FILTER
                    Filter = ""
                Next
            End If
            'END OF VALIDASI DATA DETAIL --------------------------

        Else
            ErrMessage = "Main transaction not found." : GoTo selesai
        End If
        'END OF VALIDASI MATAUANG COA ======================================

selesai:
        Return ErrMessage
    End Function

    Public Function HakAksesGiro(ByVal ModuleId As Integer, ByVal MenuId As Integer, ByVal UserId As Integer) As String
        Dim ErrMessage As String = "", sql As String = "", akses As Integer = 0
        Dim dt As New DataTable

        'CEK HAK AKSES APPROVED GIRO KELUAR =======================================
        sql = "SELECT ur.userid, ur.role, rm.rmmoduleid, rm.rmmenuid, rm.rmrole, rm.rmakses, rm.rmfavourite FROM m0_user_role ur JOIN m0_role_menu rm ON ur.role = rm.rmrole WHERE ur.userid = '" & FixDouble(UserId) & "' AND rm.rmmoduleid = '" & FixDouble(ModuleId) & "' AND rmmenuid = '" & FixDouble(MenuId) & "'"
        dt = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count > 0 Then
            'AMBIL AKSES SIMPAN APRROVED ---> HAK AKSES APPROVED ADA DI KARAKTER KE 9 (INDEKS 8)
            akses = Double.Parse(dt.Rows(0)("rmakses").ToString.ElementAt(8))

            'JIKA AKSES APPROVED <> 1 MAKA ALERT TIDAK BISA SIMPAN APPROVED
            If akses <> 1 Then
                ErrMessage = "This role doesn't have permission for giro out approval." : GoTo selesai
            End If
        Else
            ErrMessage = "This role doesn't have permission for this menu." : GoTo selesai
        End If
        'END OF CEK HAK AKSES APPROVED GIRO KELUAR ================================

selesai:
        Return ErrMessage
    End Function

    Public Function GetSettingNominal(ByRef digitGroup As String, ByRef pemisahDesimal As String, ByRef digitDesimal As Integer) As String
        Dim ErrMessage As String = "", sql As String = ""
        Dim dtFnominal As New DataTable
        Dim FNominal(3) As String ' Digit grup | Pemisah Desimal | Digit Desimal

        'AMBIL SETTING FORMAT NOMINAL ==============================================
        sql = "SELECT snilai FROM m0_setting WHERE smodule= 0 AND sgrup = 'company' AND skode = 'ValidasiNominal'"
        dtFnominal = AsDataTableAmbilDariDB(sql)

        If dtFnominal.Rows.Count > 0 Then
            FNominal = dtFnominal.Rows(0)("snilai").ToString.Split(sptSetting)

            'SET DIGIT GROUP
            If Len(FNominal(0)) > 0 Then
                digitGroup = FNominal(0)
            Else
                ErrMessage = "Setting for Digit Group in Nominal Format not found." : GoTo selesai
            End If

            'SET PEMISAH DESIMAL
            If Len(FNominal(1)) > 0 Then
                pemisahDesimal = FNominal(1)
            Else
                ErrMessage = "Setting for Decimal Splitter in Nominal Format not found." : GoTo selesai
            End If

            'SET DIGIT DESIMAL
            If Len(FNominal(2)) > 0 Then
                digitDesimal = Double.Parse(FNominal(2))
            Else
                ErrMessage = "Setting for Digit Decimal in Nominal Format not found." : GoTo selesai
            End If

        Else
            ErrMessage = "Setting for Nominal Format not found." : GoTo selesai
        End If
        'END OF AMBIL SETTING FORMAT NOMINAL =======================================

selesai:
        Return ErrMessage
    End Function

    '<WebMethod()>
    Public Function M0_DeleteNotransaksi(ByVal cabang As String, ByVal lokasi As String, ByVal kodetabel As String, ByVal tgl As String, ByVal notransaksi As String, Optional ByVal sumber As String = "", Optional ByVal smodule As Integer = 0, Optional ByVal matauang As String = "", Optional ByVal userid As Integer = 0) As String

        Dim dt As DataTable
        Dim sqlambil As String = "", sql As String = "", withCabang As String = "1", withLokasi As String = "1", resetBulan As String = "1"
        Dim success As Integer = 0, jmldigit As Double = 0, noberikutnya As Double = 0
        Dim errmessage As String = "", rsSetting As String = "", mukodenotransaksi As String = ""
        Dim sgrup As String = IIf(smodule = 0, "accounting", "options")

        Try

            'AMBIL SETTING, PAKAI CABANG ATAU TIDAK
            rsSetting = F_getSetting(smodule, sgrup, sumber & "NoTransactionCabang")
            If Len(rsSetting) > 0 Then withCabang = rsSetting
            If withCabang <> 1 Then cabang = "--"

            'AMBIL SETTING, PAKAI LOKASI ATAU TIDAK
            rsSetting = F_getSetting(smodule, sgrup, sumber & "NoTransactionLokasi")
            If Len(rsSetting) > 0 Then withLokasi = rsSetting
            If withLokasi <> 1 Then lokasi = "--"

            'AMBIL SETTING, RESET PERBULAN ATAU PERTAHUN
            rsSetting = F_getSetting(smodule, sgrup, sumber & "NoTransactionPeriode")
            If Len(rsSetting) > 0 Then resetBulan = rsSetting

            If withLokasi = 1 Then
                If kodetabel = "SQ" Then
                    'AMBIL LOKASI USER
                    sqlambil = "SELECT ulokasi FROM m0_user WHERE userid = '" & userid & "'"
                    dt = AsDataTableAmbilDariDB(sqlambil)
                    If dt.Rows.Count > 0 Then
                        lokasi = dt.Rows(0)("ulokasi")
                    Else
                        errmessage = "Could not find Location for userid : '" & userid & "'." : GoTo selesai
                    End If

                    'AMBIL KODE TRANSAKSI LOKASI
                    sqlambil = "SELECT lkodetransaksi FROM m1_location WHERE lkode = '" & lokasi & "'"
                    dt = AsDataTableAmbilDariDB(sqlambil)
                    If dt.Rows.Count > 0 Then
                        lokasi = dt.Rows(0)("lkodetransaksi")
                    Else
                        errmessage = "Could not find Transaction Code for '" & lokasi & "' location." : GoTo selesai
                    End If

                Else
                    'AMBIL KODE TRANSAKSI LOKASI
                    sqlambil = "SELECT lkodetransaksi FROM m1_location WHERE lkode = '" & lokasi & "'"
                    dt = AsDataTableAmbilDariDB(sqlambil)
                    If dt.Rows.Count > 0 Then
                        lokasi = dt.Rows(0)("lkodetransaksi")
                    Else
                        errmessage = "Could not find Transaction Code for '" & lokasi & "' location." : GoTo selesai
                    End If
                End If

            End If

            'AMBIL KODE NO TRANSAKSI MATAUANG
            sqlambil = "SELECT c.ckodenotransaksi FROM m1_currency c WHERE c.ckode = '" & matauang & "'"
            dt = AsDataTableAmbilDariDB(sqlambil)
            If (dt.Rows.Count > 0) Then
                mukodenotransaksi = FxDB(dt.Rows(0)(0), "")
            End If

            'FORMAT TGL
            tgl = AsFormatTanggal(tgl)
            If resetBulan <> 1 Then tgl = AsFormatTanggal(tgl, "yyyy-01-dd")

            'AMBIL NOMORBERIKUTNYA DARI M0_NOMOR_NEXT BERDASARKAN :
            'KODETABEL, LOKASI, TAHUN DAN BULAN TRANSAKSI
            sqlambil = "  SELECT noberikutnya FROM m0_nomor_next"
            sqlambil &= " WHERE kodetabel = '" & FixQuotes(kodetabel & mukodenotransaksi) & "'"
            sqlambil &= " AND lokasi = '" & FixQuotes(lokasi) & "'"
            sqlambil &= " AND cabang = '" & FixQuotes(cabang) & "'"
            sqlambil &= " AND tahun = RIGHT(YEAR('" & FixQuotes(tgl) & "'), 2)"
            sqlambil &= " AND bulan = MONTH('" & FixQuotes(tgl) & "')"
            dt = AsDataTableAmbilDariDB(sqlambil)
            If dt.Rows.Count > 0 Then
                noberikutnya = Double.Parse(dt.Rows(0)("noberikutnya"))
            End If


            'AMBIL JMLDIGIT DARI M0_NOMOR BERDASARKAN KODETABEL
            sqlambil = "  SELECT jmldigit FROM m0_nomor"
            sqlambil &= " WHERE kodetabel = '" & FixQuotes(kodetabel) & "'"
            dt = AsDataTableAmbilDariDB(sqlambil)
            dt = AsDataTableAmbilDariDB(sqlambil)
            If dt.Rows.Count > 0 Then
                jmldigit = Double.Parse(dt.Rows(0)("jmldigit"))
            End If


            'JIKA URUTAN NO.TRANSAKSI = NOMORBERIKUTNYA - 1 MAKA UPDATE M0_NOMOR_NEXT
            If Double.Parse(notransaksi.Substring(notransaksi.Length - jmldigit)) = noberikutnya - 1 Then
                sql = "  UPDATE m0_nomor_next SET noberikutnya = noberikutnya - 1"
                sql &= " WHERE kodetabel = '" & FixQuotes(kodetabel & mukodenotransaksi) & "'"
                sql &= " AND lokasi = '" & FixQuotes(lokasi) & "'"
                sql &= " AND cabang = '" & FixQuotes(cabang) & "'"
                sql &= " AND tahun = RIGHT(YEAR('" & FixQuotes(tgl) & "'), 2)"
                sql &= " AND bulan = MONTH('" & FixQuotes(tgl) & "')"
            End If

            success = 1

        Catch ex As Exception

            errmessage = ex.Message

        End Try


selesai:
        Return String.Concat(success, sptSubParam, errmessage, sptSubParam, notransaksi, sptSubParam, sql)
    End Function

    '//FUNGSI UNTUK SET TGL JATUH TEMPO SESUAI HARI JATUH TEMPO TERMIN
    Public Function F_TglJT(ByVal Termin As String, ByVal Tgl As String, ByVal NamaFieldTgl As String) As String

        'FUNGSI INI MENGEMBALIKAN 2 HASIL, ISSUCCESS DAN HASIL
        '1△2014-02-01                   >> 1 = SUCCESS, 2014-02-01 = TGL JATUH TEMPO
        '0△Terms : Tgl Required Date.   >> 0 = GAGAL, Terms : Tgl Required Date. = ERROR MESSAGE

        Dim isSuccess As Integer = 0, hasil As String = ""

        'VALIDASI TGL
        If IsDate(Tgl) = False Then
            hasil = "Terms : " & NamaFieldTgl & " required date." : GoTo Selesai
        End If

        'PROSES PERHITUNGAN TGL JATUH TEMPO ====================================
        Dim HariJT As Double = 0, TglJT As String = ""

        'CEK TERMIN
        If Len(Termin) > 0 Then
            'JIKA TERMIN DIISI MAKA AMBIL HARI JATUH TEMPO DARI MASTER TERMIN
            Dim sql As String = "SELECT trharijatuhtempo FROM m1_terms WHERE trkode = '" & FixQuotes(Termin) & "'"

            Dim Dt As DataTable = AsDataTableAmbilDariDB(sql)
            If Dt.Rows.Count > 0 Then
                'JIKA MASTER TERMIN DITEMUKAN MAKA SET HARI JATUH TEMPO DARI MASTER TERMIN
                HariJT = Double.Parse(Dt.Rows(0)("trharijatuhtempo"))

            Else
                'JIKA MASTER TERMIN TIDAK DITEMUKAN MAKA SET HARI JATUH TEMPO = 0
                HariJT = 0

            End If

        Else
            'JIKA TERMIN TIDAK DIISI MAKA HARI JATUH TEMPO = 0
            HariJT = 0

        End If

        'SET TGL JATUH TEMPO = TGL + HARI JATUH TEMPO
        TglJT = DateAdd(DateInterval.Day, HariJT, Date.Parse(Tgl))
        hasil = AsFormatTanggal(TglJT)

        isSuccess = 1
        'END OF PROSES PERHITUNGAN TGL JATUH TEMPO =============================


Selesai:
        Return String.Concat(isSuccess, sptSubParam, hasil)

    End Function

    '//FUNGSI UNTUK FORMAT NOMINAL SESUAI SETTING
    Public Function F_Nominal(ByVal Nominal As Double, ByVal JmlDesimalSetting As Boolean) As String

        'FUNGSI INI MENGEMBALIKAN 2 HASIL, ISSUCCESS DAN HASIL
        '1△1.000,25                     >> 1 = SUCCESS, 1.000,25 = NOMINAL YG TERFORMAT
        '0△Nominal setting not found.   >> 0 = GAGAL, Nominal setting not found. = ERROR MESSAGE

        Dim isSuccess As Integer = 0, hasil As String = "", sql As String = ""
        Dim Dt As New DataTable
        Dim FNominal(3) As String, FMinus As String = ""

        'PROSES AMBIL FORMAT DARI SETTING ======================================
        sql = "  SELECT smodule, sgrup, skode, snilai FROM m0_setting"
        sql &= " WHERE (smodule = 0 AND sgrup = 'company' AND skode = 'FormatMinusApp')"
        sql &= " OR (smodule = 0 AND sgrup = 'company' AND skode = 'FormatNominal')"
        Dt = AsDataTableAmbilDariDB(sql)
        If Dt.Rows.Count > 0 Then
            'AMBIL FORMAT NOMINAL ==== Digit grup(0) | Pemisah Desimal(1) | Digit Desimal(2)
            FNominal = AsDataTableDLookup(Dt, "snilai", "smodule = 0 AND sgrup = 'company' AND skode = 'FormatNominal'").Split("|")
            If FNominal.Length <> 3 Then
                hasil = "Invalid Setting for Nominal Format." : GoTo Selesai
            End If

            'AMBIL FORMAT MINUS ====== 0 : (n), 1 : -n, 2 : - n, 3 : n-, 4 : n -
            FMinus = AsDataTableDLookup(Dt, "snilai", "smodule = 0 AND sgrup = 'company' AND skode = 'FormatMinusApp'")
            If FMinus <> 0 And FMinus <> 1 And FMinus <> 2 And FMinus <> 3 And FMinus <> 4 Then
                hasil = "Invalid Setting for Minus Nominal Format." : GoTo Selesai
            End If

        Else
            hasil = "Setting for Nominal Format not found. #1" : GoTo Selesai
        End If
        'END OF PROSES AMBIL FORMAT DARI SETTING ===============================


        'PROSES FORMATTING =====================================================
        'FORMATTING JMLDIGIT DESIMAL
        If JmlDesimalSetting Then
            Dim digit As Integer = Integer.Parse(FNominal(2))
            Nominal = Math.Round(Nominal, digit)
        End If

        Dim ArrNominal(2) As String

        'FORMATTING PEMISAH DESIMAL
        If Nominal.ToString.Contains(".") Then
            ArrNominal = Math.Abs(Nominal).ToString.Split(".")
        Else
            ArrNominal(0) = Math.Abs(Nominal).ToString
            ArrNominal(1) = String.Join("", Enumerable.Repeat("0", Integer.Parse(FNominal(2))))
        End If

        'FORMATTING PEMISAH GRUP
        For i = 1 To ArrNominal(0).Length
            hasil = ArrNominal(0).Substring(ArrNominal(0).Length - i, 1) & hasil
            If i Mod 3 = 0 And i <> ArrNominal(0).Length Then
                hasil = FNominal(0) & hasil
            End If
        Next

        'SET PEMISAH DESIMAL
        If Len(ArrNominal(1)) > 0 Then
            hasil = hasil & FNominal(1) & ArrNominal(1)
        End If

        'FORMATTING MINUS
        If Nominal < 0 Then
            '0 = (n), 1 = -n, 2 = - n, 3 = n-, 4 = n -
            Select Case FMinus
                Case 0 : hasil = "(" & hasil & ")"
                Case 0 : hasil = "-" & hasil
                Case 0 : hasil = "- " & hasil
                Case 0 : hasil = hasil & "-"
                Case 0 : hasil = hasil & " -"
                Case Else : hasil = "Invalid Setting for Minus Nominal Format." : GoTo Selesai
            End Select
        End If

        isSuccess = 1
        'END OF PROSES FORMATTING ==============================================

Selesai:
        Return String.Concat(isSuccess, sptSubParam, hasil)

    End Function

    '//FUNGSI UNTUK CEK HAK AKSES
    Public Function HakAkses(ByVal ModuleId As Integer, ByVal MenuId As Integer, ByVal IndeksAkses As Integer, ByVal UserId As Integer) As String
        'INDEKS HAK AKSES
        '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
        '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid

        Dim ErrMessage As String = "", sql As String = "", akses As Integer = 0
        Dim dt As New DataTable
        Dim arrNamaAkses() As String = {"Insert", "Update/Draft", "Delete", "GetData", "Approved1", "Approved2", "Approved3", "Approved4", "Approved", "Close/Unclose", "Journal", "History", "Setting Grid"}

        'CEK HAK AKSES =======================================
        sql = "SELECT ur.userid, ur.role, rm.rmmoduleid, rm.rmmenuid, rm.rmrole, rm.rmakses, rm.rmfavourite FROM m0_user_role ur JOIN m0_role_menu rm ON ur.role = rm.rmrole WHERE ur.userid = '" & FixDouble(UserId) & "' AND rm.rmmoduleid = '" & FixDouble(ModuleId) & "' AND rmmenuid = '" & FixDouble(MenuId) & "'"
        dt = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count > 0 Then
            'AMBIL AKSES SESUAI INDEKS
            akses = Double.Parse(dt.Rows(0)("rmakses").ToString.ElementAt(IndeksAkses))

            'JIKA AKSES <> 1 MAKA ALERT TIDAK MEMPUNYAI HAK AKSES
            If akses <> 1 Then
                ErrMessage = "This role doesn't have permission to '" & arrNamaAkses(IndeksAkses) & "' this menu." : GoTo selesai
            End If
        Else
            ErrMessage = "This role doesn't have permission to '" & arrNamaAkses(IndeksAkses) & "' this menu." : GoTo selesai
        End If
        'END OF CEK HAK AKSES ================================

selesai:
        Return ErrMessage
    End Function

    '//FUNGSI UNTUK CEK HAK AKSES PENJUALAN DIBAWAH HARGA JUAL
    Public Function HakAksesLowerPrice(ByVal ModuleId As Integer, ByVal MenuId As Integer, ByVal IndeksAkses As Integer, ByVal UserId As Integer, ByVal DtDetail As DataTable, ByVal ftLowerPrice As String) As String
        'INDEKS HAK AKSES
        '0 = Insert, 1 = Update/Draft, 2 = Delete, 3 = GetData, 4 = Approved1, 5 = Approved2, 6 = Approved3, 
        '7 = Approved4, 8 = Approved, 9 = Close/Unclose, 10 = Journal, 11 = History, 12 = Setting Grid

        Dim ErrMessage As String = "", sql As String = "", akses As Integer = 0
        Dim dt As New DataTable
        Dim arrNamaAkses() As String = {"Insert", "Update/Draft", "Delete", "GetData", "Approved1", "Approved2", "Approved3", "Approved4", "Approved", "Close/Unclose", "Journal", "History", "Setting Grid"}

        Dim idbarang As Double = 0, hargaJual As Double = 0, harga As Double = 0, urutan As Integer = 0
        Dim kodeBarang As String = "", tipeBarang As String = "", namaBarang As String = ""

        'CEK PENJUALAN DIWAH HARGA JUAL ======================
        sql = "SELECT bid, bkode, bhargajual1 FROM m1_item WHERE " & ftLowerPrice
        dt = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count > 0 Then
            idbarang = Double.Parse(dt.Rows(0)("bid")) : kodeBarang = dt.Rows(0)("bkode") : hargaJual = Double.Parse(dt.Rows(0)("bhargajual1"))

            'AMBIL NAMA BARANG, HARGA PENJUALAN DAN URUTAN DARI DETAIL TRANSAKSI
            Dim dtLookUp As DataTable = AsDataTableFilterSortDt(DtDetail, "idbarang = '" & idbarang & "' AND harga < " & hargaJual)
            If dtLookUp.Rows.Count > 0 Then
                namaBarang = dtLookUp.Rows(0)("namabarang")
                tipeBarang = dtLookUp.Rows(0)("tipebarang")
                harga = Double.Parse(dtLookUp.Rows(0)("harga"))
                urutan = Integer.Parse(dtLookUp.Rows(0)("urutan"))
            End If

            Dim HargaF(2) As String, HargaJualF(2) As String

            HargaF = F_Nominal(harga, False).Split(sptSubParam)
            HargaJualF = F_Nominal(hargaJual, False).Split(sptSubParam)

            'CEK HAK AKSES -----------------------
            'sql = "SELECT ur.userid, ur.role, rm.rmmoduleid, rm.rmmenuid, rm.rmrole, rm.rmakses, rm.rmfavourite FROM m0_user_role ur JOIN m0_role_menu rm ON ur.role = rm.rmrole WHERE ur.userid = '" & FixDouble(UserId) & "' AND rm.rmmoduleid = '" & FixDouble(ModuleId) & "' AND rmmenuid = '" & FixDouble(MenuId) & "'"
            sql = "SELECT rc.rcakses FROM m0_role_custom rc JOIN m0_user_role ur ON rc.rcrole = ur.role WHERE rc.rcmoduleid = 5 AND rc.rcidpc = 1 AND ur.userid = '" & FixDouble(UserId) & "'"
            dt = AsDataTableAmbilDariDB(sql)
            If dt.Rows.Count > 0 Then
                If Len(dt.Rows(0)(0)) > 0 Then
                    'AMBIL AKSES SESUAI INDEKS
                    'akses = Double.Parse(dt.Rows(0)("rmakses").ToString.ElementAt(IndeksAkses))
                    akses = Double.Parse(dt.Rows(0)("rcakses").ToString)

                    'JIKA AKSES <> 1 MAKA ALERT TIDAK MEMPUNYAI HAK AKSES
                    If akses <> 1 Then
                        GoTo tidakPunyaAkses
                    End If

                Else
                    GoTo tidakPunyaAkses
                End If

            Else
tidakPunyaAkses:
                ErrMessage = "Row : " & urutan & " - " & kodeBarang & " | " & tipeBarang & " | " & namaBarang & " price is less then item's price list (" & HargaF(1) & " < " & HargaJualF(1) & "). This role doesn't have permission to '" & arrNamaAkses(IndeksAkses) & "' this menu." : GoTo selesai
            End If
            'END OF CEK HAK AKSES ----------------

        End If
        'END OF CEK PENJUALAN DIWAH HARGA JUAL ===============


selesai:
        Return ErrMessage
    End Function

    Public Function f_Random(ByVal size As Integer) As String
        Dim nilai As Char() = New Char(size - 1) {}
        Dim _rng As Random = New Random()
        Dim _chars As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"

        For i As Integer = 0 To size - 1
            nilai(i) = _chars(_rng.[Next](_chars.Length))
        Next
        Return New String(nilai)
    End Function

    '//FUNGSI UNTUK PEMBULATAN ANGKA DESIMAL
    Public Function F_Round(ByVal Number As Double) As Double

        Dim hasilDesimal As String = ""
        Dim StrNum As String = Number.ToString.Replace(",", ".")
        Dim sptAngka As String() = StrNum.Split(".")

        If sptAngka.Length > 1 Then
            Dim currNum As Integer = 0, prevNum As Integer = 0

            For i = 1 To sptAngka(1).Length
                If i = 1 Then
                    prevNum = 0
                Else
                    prevNum = Val(sptAngka(1).ElementAt(i - 2))
                End If
                currNum = Val(sptAngka(1).ElementAt(i - 1))

                If Val(prevNum) > 0 And Val(currNum) = 0 Then
                    Exit For
                End If

                hasilDesimal = String.Concat(hasilDesimal, currNum)
            Next
        End If

        hasilDesimal = String.Concat(sptAngka(0), ".", hasilDesimal)

        Return Double.Parse(hasilDesimal)
    End Function

    '//FUNGSI UNTUK PEMBULATAN ANGKA DECIMAL DESIMAL
    Public Function F_RoundDecimal(ByVal Number As Decimal) As Double

        Dim hasilDesimal As String = ""
        Dim StrNum As String = Number.ToString.Replace(",", ".")
        Dim sptAngka As String() = StrNum.Split(".")

        If sptAngka.Length > 1 Then
            Dim currNum As Integer = 0, prevNum As Integer = 0

            For i = 1 To sptAngka(1).Length
                If i = 1 Then
                    prevNum = 0
                Else
                    prevNum = Val(sptAngka(1).ElementAt(i - 2))
                End If
                currNum = Val(sptAngka(1).ElementAt(i - 1))

                If Val(prevNum) > 0 And Val(currNum) = 0 Then
                    Exit For
                End If

                hasilDesimal = String.Concat(hasilDesimal, currNum)
            Next
        End If

        hasilDesimal = String.Concat(sptAngka(0), ".", hasilDesimal)

        Return Double.Parse(hasilDesimal)
    End Function

    '//FUNGSI UNTUK PEMBULATAN 5 ANGKA DI BELAKANG KOMA
    Public Function Rnd(ByVal Number As Decimal) As Decimal
        Return Math.Round(Number, 5, MidpointRounding.AwayFromZero)
    End Function

    '//FUNGSI UNTUK VALIDASI AKUN WAJIB COSTCENTER ATAU TIDAK
    Public Function ValidasiCoaRequiredCostCenter(ByVal strFilter As String, ByVal dtdetail As DataTable) As String
        Dim hasil As String = ""

        'CEK COA WAJIB COST CENTER ==============================
        Dim dtCekCC As DataTable = AsDataTableAmbilDariDB("SELECT cnomor, cnama FROM m1_coa WHERE ccostcenter = 1 AND (" & strFilter & ")")
        Dim dtDetailCC As New DataTable
        If dtCekCC.Rows.Count > 0 Then
            For Each dr1 As DataRow In dtCekCC.Rows
                dtDetailCC = AsDataTableFilterLimit(dtdetail, "norek = '" & dr1("cnomor") & "' AND costcenter = ''", , , 1)
                If dtDetailCC.Rows.Count > 0 Then
                    hasil = "Row " & dtDetailCC(0)("urutan") & " : " & dr1("cnomor") & " " & dr1("cnama") & " - cost center can't be empty." : GoTo selesai
                End If
            Next
        End If
        'END OF CEK COA WAJIB COST CENTER =======================

selesai:
        Return hasil
    End Function

    '//FUNGSI UNTUK MENGAMBIL NAMA SHEET EXCEL
    Public Function GetExcelSheet(ByVal sPath As String) As String

        Dim rsExcelSheet As String = "" '(result★namafile▼sheet1▲namafile▼sheet2▲namafile▼sheet...★mapping)
        Dim rsResult As String = "", rsSheet As String = "", rsMapping As String = ""
        rsMapping = "namafile▼namasheet"

        'PROSES READ FILE EXCEL -------------------------
        'OleDbDataAdapter untuk komunikasi antara DataTable dan OleDb Data Sources
        Dim da As New OleDbDataAdapter

        'OleDbCommand untuk eksekusi SQL query
        Dim cmd As New OleDbCommand

        'OleDbConnection yang digunakan OleDbCommand untuk konek ke excel file
        Dim xlsConn As OleDbConnection

        'Buat koneksi file excel
        xlsConn = New OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & sPath & ";Extended Properties=Excel 12.0")

        Try
            'Buka koneksi excel
            xlsConn.Open()

            Dim namafile As String = Path.GetFileName(sPath)

            Dim dtExcelSheet As DataTable = xlsConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, Nothing)
            dtExcelSheet = AsDataTableFilterSortDt(dtExcelSheet, "TABLE_NAME NOT LIKE '%FilterDatabase%'")
            For Each drSheet As DataRow In dtExcelSheet.Rows
                rsSheet = IIf(Len(rsSheet) > 0, rsSheet & sptRow, rsSheet)
                rsSheet &= namafile & sptField & drSheet("TABLE_NAME")
            Next

        Catch ex As Exception
            rsResult = ex.Message

        Finally
            'Close connection dan set nothing
            xlsConn.Close()
            xlsConn = Nothing

        End Try

selesai:
        rsExcelSheet = rsResult & sptParam & rsSheet & sptParam & rsMapping
        Return rsExcelSheet
        'END OF PROSES READ FILE EXCEL ------------------

    End Function

    '//FUNGSI UNTUK READ FILE EXCEL DAN DITAMPUNG DALAM DATATABLE
    Public Function ReadExcelFile(ByVal sPath As String, ByVal sheetName As String, ByRef dtExcelData As DataTable) As String

        Dim rsReadExcel As String = ""

        'PROSES READ FILE EXCEL -------------------------
        'OleDbDataAdapter untuk komunikasi antara DataTable dan OleDb Data Sources
        Dim da As New OleDbDataAdapter

        'OleDbCommand untuk eksekusi SQL query
        Dim cmd As New OleDbCommand

        'OleDbConnection yang digunakan OleDbCommand untuk konek ke excel file
        Dim xlsConn As OleDbConnection

        'Buat koneksi file excel
        xlsConn = New OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" & sPath & ";Extended Properties=Excel 12.0")

        Try
            'Buka koneksi excel
            xlsConn.Open()

            'Set koneksi
            cmd.Connection = xlsConn
            cmd.CommandType = CommandType.Text

            'Set query untuk ambil data dari excel
            cmd.CommandText = ("select * from [" & sheetName & "]")

            'Set query ke dataadapter
            da.SelectCommand = cmd

            'Isi datatable dengan data dari excel file menggunakan DataAdapter
            da.Fill(dtExcelData)

        Catch ex As Exception
            rsReadExcel = ex.Message

        Finally
            'Close connection dan set nothing
            xlsConn.Close()
            xlsConn = Nothing

        End Try

selesai:
        Return rsReadExcel
        'END OF PROSES READ FILE EXCEL ------------------

    End Function

    '//FUNGSI UNTUK BACA XML
    Public Function F_BacaXML(ByVal dataXML As String, ByVal tagXML As String) As String()
        Dim hasil(2) As String
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Dim rsDataXML() As String = dataXML.Replace("<" + tagXML + ">", "★").Split(CChar("★"))
        If rsDataXML.Length <> 2 Then
            hasil(0) = 0
            hasil(1) = "Tag : '" & tagXML & "' does not found in XML file."
            GoTo selesai

        Else
            hasil(0) = 1
            hasil(1) = rsDataXML(1).Replace("</" + tagXML + ">", "★").Split(CChar("★"))(0)
        End If

selesai:
        Return hasil
    End Function

    '//FUNGSI UNTUK AMBIL NILAI TAG DARI APP.XML
    Public Function F_AppGetValue(ByVal tagXML As String) As String()
        Dim hasil(2) As String 'isSuccess(0), result(1)
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Try

            Dim myPath As String = HttpContext.Current.Server.MapPath("~/") + "app\app.xml"
            Dim sr As StreamReader
            Dim contents As String = ""

            sr = File.OpenText(myPath)
            contents = sr.ReadToEnd()
            sr.Close()

            Dim rsBacaXML() As String = F_BacaXML(contents, tagXML)
            hasil(0) = rsBacaXML(0)
            hasil(1) = rsBacaXML(1)

        Catch ex As Exception
            hasil(0) = 0
            hasil(1) = ex.Message

        End Try

selesai:
        Return hasil
    End Function

    '//FUNGSI UNTUK AMBIL NILAI DARI CONSTR DATABASE DI APP.XML
    Public Function F_ConStrGetValue(ByVal ConStrTag As String, ByVal strCon As String) As String()
        Dim hasil(2) As String 'isSuccess(0), result(1)
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Dim conStr As String = "", currTag As String = ""
        Dim conStrSplit() As String, currTagSplit() As String
        'Dim AppValue() As String = F_AppGetValue("ConStr")
        Dim AppValue() As String = {"1", strCon}
        If AppValue(0) = 1 Then
            conStr = AppValue(1)
            conStrSplit = conStr.Split(";")
            For i As Integer = 0 To conStrSplit.Length - 1
                currTag = conStrSplit(i)
                currTagSplit = currTag.Split("=")
                If currTagSplit(0).ToLower.Equals(ConStrTag.ToLower) Then
                    If currTagSplit.Length <> 2 Then
                        hasil(0) = 0
                        hasil(1) = "Tag : '" & ConStrTag & "' does not found in ConStr file."
                        GoTo selesai

                    Else
                        hasil(0) = 1
                        hasil(1) = currTagSplit(1)
                        GoTo selesai

                    End If

                End If
            Next

        Else
            hasil(0) = AppValue(0)
            hasil(1) = AppValue(1)
            GoTo selesai

        End If

        hasil(0) = 0
        hasil(1) = "Tag : '" & ConStrTag & "' does not found in ConStr file."

selesai:
        Return hasil
    End Function

    '//FUNSI UNTUK AMBIL DIREKTORI PATH BERDASARKAN NAMA SERVICE
    Public Function F_GetServicePath(ByVal ServiceName As String) As String()
        Dim hasil(2) As String 'isSuccess(0), result(1)
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Dim result As String = ""
        Dim resultSplit As String()

        'AMBIL DIREKTORI SERVICE BERDASARKAN NAMA SERVICE
        Dim query As [String] = [String].Format("SELECT PathName FROM Win32_Service WHERE Name = '{0}'", ServiceName)

        'PERULANGAN SEBANYAK DATA YANG DITEMUKAN
        Using mos As New ManagementObjectSearcher(query)
            For Each mo As ManagementObject In mos.[Get]()
                'SET URL PATH SERVICE
                result = mo("PathName").ToString()
            Next
        End Using

        'PATH DISPLIT DENGAN TANDA PETIK DUA
        If Len(result) > 0 Then
            resultSplit = result.Split(Chr(34))

            'AMBIL DIREKTORI PATH TANPA NAMA FILE SERVICENYA
            If resultSplit.Length > 1 Then
                result = resultSplit(1).Substring(0, resultSplit(1).LastIndexOf("\") + 1)
            Else
                result = resultSplit(0).Substring(0, resultSplit(0).LastIndexOf("\") + 1)
            End If

            hasil(0) = 1
            hasil(1) = result
            GoTo selesai
        End If

        hasil(0) = 0
        hasil(1) = "Service Name : '" & ServiceName & "' does not found in services list."

selesai:
        Return hasil
    End Function

    '//FUNGSI UNTUK DUMP DATABASE SQL => UNTUK KEBUTUHAN APLIKASI POS OFFLINE
    Public Function F_DumpSQLAsFile(ByVal websiteAccessKey As String, ByVal userid As String, ByVal strCon As String) As String()
        ' Uses the mysqldump.exe program to make a backup of the database.
        ' This is an in-out stream operation.  The data is piped in via the 
        ' process's standard output and sent to a filestream to be written 
        ' to disk.

        Dim hasil(2) As String 'isSuccess(0), result(1)
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Dim Security As New ClsSecurity
        Dim fileName As String = "", filePath As String = ""
        'Dim sr As StreamReader
        Dim contents As String = "", dirRoot As String = "", namaFile As String = ""

        Try

            Dim DBUser As String = "", DBPassword As String = "", DBServer As String = "", DBPort As String = "", DBDatabase As String = ""
            Dim conStrValue() As String
            Dim ServiceDBValue() As String, ServiceDB As String = ""
            Dim pathServiceDBValue() As String, pathServiceDB As String = ""

            'SET FILENAME DAN FILEPATH
            dirRoot = HttpContext.Current.Server.MapPath("~/")
            fileName = Security.MD5CalcString(userid & websiteAccessKey & Now) & ".sql"
            filePath = HttpContext.Current.Server.MapPath("~/") & "files\db\"
            namaFile = fileName.Replace(".sql", "")

            'DAFTAR TABEL YANG DI DUMP (tabel1 tabel2 tabel3 tabeldst)
            Dim strTable As String = "m0_role m0_role_custom m0_role_menu m0_role_report m0_selling_rate m0_setting m0_setting_location m0_user m0_user_branch m0_user_location m0_user_role m0_user_warehouse m1_area m1_bank m1_branch m1_coa m1_cogs_fifo_in m1_cogs_fifo_out m1_cogs_special_in m1_cogs_special_out m1_contact m1_contact_attention m1_contact_category m1_contact_point m1_cost_center m1_currency m1_customer_category m1_division m1_files m1_item m1_item_assembly m1_item_category m1_item_location m1_item_location_warehouse m1_item_stock_warehouse m1_item_type m1_location m1_no_batch_in m1_no_batch_out m1_no_batch_transaction m1_no_serial_in m1_no_serial_out m1_no_serial_transaction m1_project m1_region m1_salesman_category m1_selling_point m1_subdivision m1_supplier_category m1_tax m1_terms m1_unit m1_warehouse m1_type_sa m_12_area m_12_area_category m_12_pos_additional_item m_12_pos_additional_item_detail m_12_pos_bonus_item m_12_pos_bonus_item_detail m_12_pos_category m_12_pos_category_setting m_12_pos_discount_category_customer m_12_pos_discount_category_item m_12_pos_discount_item m_12_pos_item m_12_pos_point_category_item m_12_pos_point_item m_12_pos_point_transaction m_12_pos_setting m_12_pos_substitution_item m_12_pos_substitution_item_detail m_12_pos_voucher_in m_12_pos_voucher_out m1_item_permission"

            'AMBIL SERVICE DB -> MYSQL (DARI APP.XML)
            ServiceDBValue = F_AppGetValue("SqlServiceName")
            If ServiceDBValue(0) = 1 Then
                ServiceDB = ServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = ServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL PATH SERVICE MYSQL -> UNTUK PANGGIL mysqldump
            pathServiceDBValue = F_GetServicePath(ServiceDB)
            If pathServiceDBValue(0) = 1 Then
                pathServiceDB = pathServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = pathServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL NILAI STRCON DARI APP.XML
            'USER
            conStrValue = F_ConStrGetValue("Uid", strCon)
            If conStrValue(0) = 1 Then
                DBUser = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PASSWORD
            conStrValue = F_ConStrGetValue("Pwd", strCon)
            If conStrValue(0) = 1 Then
                DBPassword = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'SERVER
            conStrValue = F_ConStrGetValue("Server", strCon)
            If conStrValue(0) = 1 Then
                DBServer = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PORT
            conStrValue = F_ConStrGetValue("Port", strCon)
            If conStrValue(0) = 1 Then
                DBPort = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'DATABASE
            conStrValue = F_ConStrGetValue("Database", strCon)
            If conStrValue(0) = 1 Then
                DBDatabase = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If


            'PROSES DUMP DATABASE
            Dim myProcess As Process = New Process()

            'Dim strOptions As String = String.Format(" --user={0} --password={1} --host={2} --port={3} --add-drop-database --add-drop-table --extended-insert --databases {4}", DBUser, DBPassword, DBServer, DBPort, DBDatabase & " " & strTable)
            Dim strOptions As String = String.Format(" --user={0} --password={1} --extended-insert=FALSE {4}", DBUser, DBPassword, DBServer, DBPort, DBDatabase & " " & strTable)

            With myProcess
                .StartInfo.UseShellExecute = False
                .StartInfo.CreateNoWindow = True
                .StartInfo.RedirectStandardInput = True
                .StartInfo.RedirectStandardOutput = True
                .StartInfo.RedirectStandardError = True
                .StartInfo.FileName = pathServiceDB & "mysqldump.exe"
                .StartInfo.Arguments = strOptions
                .Start()

            End With

            If (Not System.IO.Directory.Exists(filePath)) Then
                System.IO.Directory.CreateDirectory(filePath)
            End If
            filePath = filePath & fileName

            Dim sOut As System.IO.StreamReader = myProcess.StandardOutput
            Dim sToFile As New System.IO.StreamWriter(filePath)
            Dim line As String

            'TAMBAHKAN QUERY SET AUTOCOMMIT = 0
            sToFile.WriteLine("SET foreign_key_checks = 0;SET UNIQUE_CHECKS = 0;SET AUTOCOMMIT = 0;")

            ' Read and display the lines from the file until the end 
            ' of the file is reached.
            Do
                line = sOut.ReadLine()
                sToFile.WriteLine(line)
            Loop Until line Is Nothing

            'TAMBAHKAN QUERY SET AUTOCOMMIT = 0
            sToFile.WriteLine("SET foreign_key_checks = 1;SET UNIQUE_CHECKS = 1;SET AUTOCOMMIT = 1;COMMIT;")

            sOut.Close()
            sToFile.Close()

            myProcess.Close()

            Dim dirCmd As String = Environment.SystemDirectory + "\"

            'CEK FILE EXISTS
            If (File.Exists(filePath)) Then

                Shell(String.Format(dirCmd + "cmd.exe /k {0} & {1}", dirRoot & "Bin\7zr a " & dirRoot & "files\db\" & namaFile & ".7z " & dirRoot & "files\db\" & fileName & " -mx -mf=BCJ2", "exit"))

                'File.Delete(filePath)
                System.Threading.Thread.Sleep(30000)

                hasil(0) = 1
                hasil(1) = namaFile & ".7z"

            Else
                hasil(0) = 0
                hasil(1) = "'" & fileName & "' file doesn't exists." : GoTo selesai

            End If

        Catch ex As Exception
            hasil(0) = 0
            hasil(1) = "Dump database failed : " & (ex.Message)
            GoTo selesai

        End Try

selesai:
        Return hasil

    End Function

    '//FUNGSI UNTUK DUMP DATABASE SQL => UNTUK KEBUTUHAN APLIKASI POS OFFLINE
    Public Function F_DumpSQLAsFilePOSOld(ByVal websiteAccessKey As String, ByVal userid As String, ByVal strCon As String, ByVal catPos As String) As String()
        ' Uses the mysqldump.exe program to make a backup of the database.
        ' This is an in-out stream operation.  The data is piped in via the 
        ' process's standard output and sent to a filestream to be written 
        ' to disk.

        Dim hasil(2) As String 'isSuccess(0), result(1)
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Dim Security As New ClsSecurity
        Dim fileName As String = "", filePath As String = ""
        'Dim sr As StreamReader
        Dim contents As String = "", dirRoot As String = "", namaFile As String = ""

        Try

            Dim DBUser As String = "", DBPassword As String = "", DBServer As String = "", DBPort As String = "", DBDatabase As String = ""
            Dim conStrValue() As String
            Dim ServiceDBValue() As String, ServiceDB As String = ""
            Dim pathServiceDBValue() As String, pathServiceDB As String = ""

            'SET FILENAME DAN FILEPATH
            dirRoot = HttpContext.Current.Server.MapPath("~/")
            fileName = Security.MD5CalcString(userid & websiteAccessKey & Now) & ".sql"
            filePath = HttpContext.Current.Server.MapPath("~/") & "files\db\"
            namaFile = fileName.Replace(".sql", "")

            'AMBIL SERVICE DB -> MYSQL (DARI APP.XML)
            ServiceDBValue = F_AppGetValue("SqlServiceName")
            If ServiceDBValue(0) = 1 Then
                ServiceDB = ServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = ServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL PATH SERVICE MYSQL -> UNTUK PANGGIL mysqldump
            pathServiceDBValue = F_GetServicePath(ServiceDB)
            If pathServiceDBValue(0) = 1 Then
                pathServiceDB = pathServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = pathServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL NILAI STRCON DARI APP.XML
            'USER
            conStrValue = F_ConStrGetValue("Uid", strCon)
            If conStrValue(0) = 1 Then
                DBUser = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PASSWORD
            conStrValue = F_ConStrGetValue("Pwd", strCon)
            If conStrValue(0) = 1 Then
                DBPassword = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'SERVER
            conStrValue = F_ConStrGetValue("Server", strCon)
            If conStrValue(0) = 1 Then
                DBServer = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PORT
            conStrValue = F_ConStrGetValue("Port", strCon)
            If conStrValue(0) = 1 Then
                DBPort = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'DATABASE
            conStrValue = F_ConStrGetValue("Database", strCon)
            If conStrValue(0) = 1 Then
                DBDatabase = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If


            'CREATE FILE
            If (Not System.IO.Directory.Exists(filePath)) Then
                System.IO.Directory.CreateDirectory(filePath)
            End If
            filePath = filePath & fileName

            'TULIS FILE
            Dim sToFile As New System.IO.StreamWriter(filePath)
            Dim line As String
            Dim sOut As System.IO.StreamReader
            Dim strTable As String = "", strOptions As String = ""

            'TAMBAHKAN QUERY SET AUTOCOMMIT = 0
            sToFile.WriteLine("SET foreign_key_checks = 0;SET UNIQUE_CHECKS = 0;SET AUTOCOMMIT = 0;")


            '----------------------------------------------------------------
            'DAFTAR TABEL GLOBAL YANG DI DUMP (tabel1 tabel2 tabel3 tabeldst)
            strTable = "m0_role m0_role_custom m0_role_menu m0_role_report m0_selling_rate m0_setting m0_setting_location m0_user m0_user_branch m0_user_location m0_user_role m0_user_warehouse m1_area m1_bank m1_branch m1_coa m1_cogs_fifo_in m1_cogs_fifo_out m1_cogs_special_in m1_cogs_special_out m1_contact m1_contact_attention m1_contact_category m1_contact_point m1_cost_center m1_currency m1_customer_category m1_division m1_files m1_item m1_item_assembly m1_item_category m1_item_location m1_item_location_warehouse m1_item_permission m1_item_stock_warehouse m1_item_type m1_location m1_no_batch_in m1_no_batch_out m1_no_batch_transaction m1_no_serial_in m1_no_serial_out m1_no_serial_transaction m1_project m1_region m1_salesman_category m1_selling_point m1_subdivision m1_supplier_category m1_tax m1_terms m1_type_sa m1_unit m1_warehouse m_12_area m_12_area_category m_12_pos_additional_item m_12_pos_additional_item_detail m_12_pos_bonus_item m_12_pos_bonus_item_detail m_12_pos_category m_12_pos_category_setting m_12_pos_discount_category_customer m_12_pos_discount_category_item m_12_pos_discount_item m_12_pos_point_category_item m_12_pos_point_item m_12_pos_point_transaction m_12_pos_setting m_12_pos_substitution_item m_12_pos_substitution_item_detail m_12_pos_voucher_in m_12_pos_voucher_out"

            'SYNTAX DUMP SQL
            strOptions = String.Format(" --user={0} --password={1} --extended-insert=FALSE {4}", DBUser, DBPassword, DBServer, DBPort, DBDatabase & " " & strTable)

            'PROSES DUMP DATABASE
            Dim myProcess As Process = New Process()
            With myProcess
                .StartInfo.UseShellExecute = False
                .StartInfo.CreateNoWindow = True
                .StartInfo.RedirectStandardInput = True
                .StartInfo.RedirectStandardOutput = True
                .StartInfo.RedirectStandardError = True
                .StartInfo.FileName = pathServiceDB & "mysqldump.exe"
                .StartInfo.Arguments = strOptions
                .Start()
            End With

            'AMBIL HASIL DUMP SQL
            sOut = myProcess.StandardOutput
            myProcess.Close()

            'TULIS HASIL DUMP KE FILE
            Do
                line = sOut.ReadLine()
                sToFile.WriteLine(line)
            Loop Until line Is Nothing
            sOut.Close()
            '----------------------------------------------------------------


            '----------------------------------------------------------------
            'DAFTAR TABEL POS ITEM YANG DI DUMP (PER KATEGORI POS)
            strTable = "m_12_pos_item --where=" & Chr(34) & "pikategori='" & catPos & "'" & Chr(34) & ""

            'SYNTAX DUMP SQL
            strOptions = String.Format(" --user={0} --password={1} --extended-insert=FALSE {4}", DBUser, DBPassword, DBServer, DBPort, DBDatabase & " " & strTable)

            'PROSES DUMP DATABASE
            With myProcess
                .StartInfo.UseShellExecute = False
                .StartInfo.CreateNoWindow = True
                .StartInfo.RedirectStandardInput = True
                .StartInfo.RedirectStandardOutput = True
                .StartInfo.RedirectStandardError = True
                .StartInfo.FileName = pathServiceDB & "mysqldump.exe"
                .StartInfo.Arguments = strOptions
                .Start()
            End With

            'AMBIL HASIL DUMP SQL
            sOut = myProcess.StandardOutput
            myProcess.Close()

            'TULIS HASIL DUMP KE FILE
            Do
                line = sOut.ReadLine()
                sToFile.WriteLine(line)
            Loop Until line Is Nothing
            sOut.Close()
            '----------------------------------------------------------------


            'TAMBAHKAN QUERY SET AUTOCOMMIT = 0
            sToFile.WriteLine("SET foreign_key_checks = 1;SET UNIQUE_CHECKS = 1;SET AUTOCOMMIT = 1;COMMIT;")

            'SELESAI TULIS FILE
            sToFile.Close()


            'COMPRESS FILE
            Dim dirCmd As String = Environment.SystemDirectory + "\"

            'CEK FILE EXISTS
            If (File.Exists(filePath)) Then

                Shell(String.Format(dirCmd + "cmd.exe /k {0} & {1}", dirRoot & "Bin\7zr a " & dirRoot & "files\db\" & namaFile & ".7z " & dirRoot & "files\db\" & fileName & " -mx -mf=BCJ2", "exit"))

                'File.Delete(filePath)
                System.Threading.Thread.Sleep(10000)

                hasil(0) = 1
                hasil(1) = namaFile & ".7z"

            Else
                hasil(0) = 0
                hasil(1) = "'" & fileName & "' file doesn't exists." : GoTo selesai

            End If

        Catch ex As Exception
            hasil(0) = 0
            hasil(1) = "Dump database failed : " & (ex.Message)
            GoTo selesai

        End Try

selesai:
        Return hasil

    End Function

    '//FUNGSI UNTUK DUMP DATABASE SQL => UNTUK KEBUTUHAN APLIKASI POS OFFLINE
    Public Function F_DumpSQLAsFilePOS(ByVal websiteAccessKey As String, ByVal userid As String, ByVal strCon As String, ByVal catPos As String) As String()
        ' Uses the mysqldump.exe program to make a backup of the database.
        ' This is an in-out stream operation.  The data is piped in via the 
        ' process's standard output and sent to a filestream to be written 
        ' to disk.

        Dim hasil(2) As String 'isSuccess(0), result(1)
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Dim Security As New ClsSecurity
        Dim fileName As String = "", filePath As String = ""
        'Dim sr As StreamReader
        Dim contents As String = "", dirRoot As String = "", namaFile As String = ""

        Try

            Dim DBUser As String = "", DBPassword As String = "", DBServer As String = "", DBPort As String = "", DBDatabase As String = ""
            Dim conStrValue() As String
            Dim ServiceDBValue() As String, ServiceDB As String = ""
            Dim pathServiceDBValue() As String, pathServiceDB As String = ""

            'SET FILENAME DAN FILEPATH
            dirRoot = HttpContext.Current.Server.MapPath("~/")
            fileName = Security.MD5CalcString(userid & websiteAccessKey & Now) & ".sql"
            filePath = HttpContext.Current.Server.MapPath("~/") & "files\db\"
            namaFile = fileName.Replace(".sql", "")

            'AMBIL SERVICE DB -> MYSQL (DARI APP.XML)
            ServiceDBValue = F_AppGetValue("SqlServiceName")
            If ServiceDBValue(0) = 1 Then
                ServiceDB = ServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = ServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL PATH SERVICE MYSQL -> UNTUK PANGGIL mysqldump
            pathServiceDBValue = F_GetServicePath(ServiceDB)
            If pathServiceDBValue(0) = 1 Then
                pathServiceDB = pathServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = pathServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL NILAI STRCON DARI APP.XML
            'USER
            conStrValue = F_ConStrGetValue("Uid", strCon)
            If conStrValue(0) = 1 Then
                DBUser = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PASSWORD
            conStrValue = F_ConStrGetValue("Pwd", strCon)
            If conStrValue(0) = 1 Then
                DBPassword = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'SERVER
            conStrValue = F_ConStrGetValue("Server", strCon)
            If conStrValue(0) = 1 Then
                DBServer = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PORT
            conStrValue = F_ConStrGetValue("Port", strCon)
            If conStrValue(0) = 1 Then
                DBPort = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'DATABASE
            conStrValue = F_ConStrGetValue("Database", strCon)
            If conStrValue(0) = 1 Then
                DBDatabase = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If


            'CREATE FILE
            If (Not System.IO.Directory.Exists(filePath)) Then
                System.IO.Directory.CreateDirectory(filePath)
            End If
            filePath = filePath & fileName

            'TULIS FILE
            Dim sToFile As New System.IO.StreamWriter(filePath)
            Dim line As String
            Dim sOut As System.IO.StreamReader
            Dim strTable As String = "", strOptions As String = ""

            'TAMBAHKAN QUERY SET AUTOCOMMIT = 0
            sToFile.WriteLine("SET foreign_key_checks = 0;SET UNIQUE_CHECKS = 0;SET AUTOCOMMIT = 0;")


            '----------------------------------------------------------------
            'DAFTAR TABEL GLOBAL YANG DI DUMP (tabel1 tabel2 tabel3 tabeldst)
            strTable = "m0_report m0_role m0_role_custom m0_role_menu m0_role_report m0_selling_rate m0_setting m0_setting_location m0_user m0_user_branch m0_user_location m0_user_role m0_user_warehouse m1_area m1_bank m1_branch m1_coa m1_cogs_fifo_in m1_cogs_fifo_out m1_cogs_special_in m1_cogs_special_out m1_contact m1_contact_attention m1_contact_category m1_contact_point m1_cost_center m1_currency m1_customer_category m1_division m1_files m1_item m1_item_assembly m1_item_category m1_item_location m1_item_location_warehouse m1_item_permission m1_item_stock_warehouse m1_item_type m1_location m1_no_batch_in m1_no_batch_out m1_no_batch_transaction m1_no_serial_in m1_no_serial_out m1_no_serial_transaction m1_project m1_region m1_salesman_category m1_selling_point m1_subdivision m1_supplier_category m1_tax m1_terms m1_type_sa m1_unit m1_warehouse m_12_area m_12_area_category m_12_pos_additional_item m_12_pos_additional_item_detail m_12_pos_bonus_item m_12_pos_bonus_item_detail m_12_pos_category m_12_pos_category_setting m_12_pos_discount_category_customer m_12_pos_discount_category_item m_12_pos_point_category_item m_12_pos_point_item m_12_pos_point_transaction m_12_pos_setting m_12_pos_substitution_item m_12_pos_substitution_item_detail m_12_pos_voucher_in m_12_pos_voucher_out"

            'SYNTAX DUMP SQL
            strOptions = String.Format(" --user={0} --password={1} --extended-insert=FALSE {4}", DBUser, DBPassword, DBServer, DBPort, DBDatabase & " " & strTable)

            'PROSES DUMP DATABASE
            Dim myProcess As Process = New Process()
            With myProcess
                .StartInfo.UseShellExecute = False
                .StartInfo.CreateNoWindow = True
                .StartInfo.RedirectStandardInput = True
                .StartInfo.RedirectStandardOutput = True
                .StartInfo.RedirectStandardError = True
                .StartInfo.FileName = pathServiceDB & "mysqldump.exe"
                .StartInfo.Arguments = strOptions
                .Start()
            End With

            'AMBIL HASIL DUMP SQL
            sOut = myProcess.StandardOutput
            myProcess.Close()

            'TULIS HASIL DUMP KE FILE
            Do
                line = sOut.ReadLine()
                sToFile.WriteLine(line)
            Loop Until line Is Nothing
            sOut.Close()
            '----------------------------------------------------------------


            '----------------------------------------------------------------
            'DAFTAR TABEL POS ITEM YANG DI DUMP (PER KATEGORI POS)
            strTable = "m_12_pos_item --where=" & Chr(34) & "pikategori='" & catPos & "'" & Chr(34) & ""

            'SYNTAX DUMP SQL
            strOptions = String.Format(" --user={0} --password={1} --extended-insert=FALSE {4}", DBUser, DBPassword, DBServer, DBPort, DBDatabase & " " & strTable)

            'PROSES DUMP DATABASE
            With myProcess
                .StartInfo.UseShellExecute = False
                .StartInfo.CreateNoWindow = True
                .StartInfo.RedirectStandardInput = True
                .StartInfo.RedirectStandardOutput = True
                .StartInfo.RedirectStandardError = True
                .StartInfo.FileName = pathServiceDB & "mysqldump.exe"
                .StartInfo.Arguments = strOptions
                .Start()
            End With

            'AMBIL HASIL DUMP SQL
            sOut = myProcess.StandardOutput
            myProcess.Close()

            'TULIS HASIL DUMP KE FILE
            Do
                line = sOut.ReadLine()
                sToFile.WriteLine(line)
            Loop Until line Is Nothing
            sOut.Close()
            '----------------------------------------------------------------


            '----------------------------------------------------------------
            'DAFTAR TABEL m_12_pos_discount_item YANG DI DUMP (PER KATEGORI POS)
            strTable = "m_12_pos_discount_item --where=" & Chr(34) & "dikategori='" & catPos & "'" & Chr(34) & ""

            'SYNTAX DUMP SQL
            strOptions = String.Format(" --user={0} --password={1} --extended-insert=FALSE {4}", DBUser, DBPassword, DBServer, DBPort, DBDatabase & " " & strTable)

            'PROSES DUMP DATABASE
            With myProcess
                .StartInfo.UseShellExecute = False
                .StartInfo.CreateNoWindow = True
                .StartInfo.RedirectStandardInput = True
                .StartInfo.RedirectStandardOutput = True
                .StartInfo.RedirectStandardError = True
                .StartInfo.FileName = pathServiceDB & "mysqldump.exe"
                .StartInfo.Arguments = strOptions
                .Start()
            End With

            'AMBIL HASIL DUMP SQL
            sOut = myProcess.StandardOutput
            myProcess.Close()

            'TULIS HASIL DUMP KE FILE
            Do
                line = sOut.ReadLine()
                sToFile.WriteLine(line)
            Loop Until line Is Nothing
            sOut.Close()
            '----------------------------------------------------------------


            'TAMBAHKAN QUERY SET AUTOCOMMIT = 0
            sToFile.WriteLine("SET foreign_key_checks = 1;SET UNIQUE_CHECKS = 1;SET AUTOCOMMIT = 1;COMMIT;")

            'SELESAI TULIS FILE
            sToFile.Close()


            'COMPRESS FILE
            Dim dirCmd As String = Environment.SystemDirectory + "\"

            'CEK FILE EXISTS
            If (File.Exists(filePath)) Then

                Shell(String.Format(dirCmd + "cmd.exe /k {0} & {1}", dirRoot & "Bin\7zr a " & dirRoot & "files\db\" & namaFile & ".7z " & dirRoot & "files\db\" & fileName & " -mx -mf=BCJ2", "exit"))

                'File.Delete(filePath)
                System.Threading.Thread.Sleep(10000)

                hasil(0) = 1
                hasil(1) = namaFile & ".7z"

            Else
                hasil(0) = 0
                hasil(1) = "'" & fileName & "' file doesn't exists." : GoTo selesai

            End If

        Catch ex As Exception
            hasil(0) = 0
            hasil(1) = "Dump database failed : " & (ex.Message)
            GoTo selesai

        End Try

selesai:
        Return hasil

    End Function

    '//FUNGSI UNTUK DUMP DATABASE SQL => UNTUK KEBUTUHAN APLIKASI POS OFFLINE
    Public Function F_DumpSQLAsString(ByVal websiteAccessKey As String, ByVal userid As String, ByVal strCon As String) As String()
        ' Uses the mysqldump.exe program to make a backup of the database.
        ' This is an in-out stream operation.  The data is piped in via the 
        ' process's standard output and sent to a filestream to be written 
        ' to disk.

        Dim hasil(2) As String 'isSuccess(0), result(1)
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Dim Security As New ClsSecurity
        Dim strValue As New StringBuilder

        Try

            Dim DBUser As String = "", DBPassword As String = "", DBServer As String = "", DBPort As String = "", DBDatabase As String = ""
            Dim conStrValue() As String
            Dim ServiceDBValue() As String, ServiceDB As String = ""
            Dim pathServiceDBValue() As String, pathServiceDB As String = ""

            'DAFTAR TABEL YANG DI DUMP (tabel1 tabel2 tabel3 tabeldst)
            Dim strTable As String = "m0_role m0_role_custom m0_role_menu m0_role_report m0_selling_rate m0_setting m0_setting_location m0_user m0_user_branch m0_user_location m0_user_role m0_user_warehouse m1_area m1_bank m1_branch m1_coa m1_cogs_fifo_in m1_cogs_fifo_out m1_cogs_special_in m1_cogs_special_out m1_contact m1_contact_attention m1_contact_category m1_contact_point m1_cost_center m1_currency m1_customer_category m1_division m1_files m1_item m1_item_assembly m1_item_category m1_item_location m1_item_location_warehouse m1_item_stock_warehouse m1_item_type m1_location m1_no_batch_in m1_no_batch_out m1_no_batch_transaction m1_no_serial_in m1_no_serial_out m1_no_serial_transaction m1_project m1_region m1_salesman_category m1_selling_point m1_subdivision m1_supplier_category m1_tax m1_terms m1_unit m1_warehouse m1_type_sa m_12_area m_12_area_category m_12_pos_additional_item m_12_pos_additional_item_detail m_12_pos_bonus_item m_12_pos_bonus_item_detail m_12_pos_category m_12_pos_category_setting m_12_pos_discount_category_customer m_12_pos_discount_category_item m_12_pos_discount_item m_12_pos_item m_12_pos_point_category_item m_12_pos_point_item m_12_pos_point_transaction m_12_pos_setting m_12_pos_substitution_item m_12_pos_substitution_item_detail m_12_pos_voucher_in m_12_pos_voucher_out m1_item_permission"

            'AMBIL SERVICE DB -> MYSQL (DARI APP.XML)
            ServiceDBValue = F_AppGetValue("SqlServiceName")
            If ServiceDBValue(0) = 1 Then
                ServiceDB = ServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = ServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL PATH SERVICE MYSQL -> UNTUK PANGGIL mysqldump
            pathServiceDBValue = F_GetServicePath(ServiceDB)
            If pathServiceDBValue(0) = 1 Then
                pathServiceDB = pathServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = pathServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL NILAI STRCON DARI APP.XML
            'USER
            conStrValue = F_ConStrGetValue("Uid", strCon)
            If conStrValue(0) = 1 Then
                DBUser = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PASSWORD
            conStrValue = F_ConStrGetValue("Pwd", strCon)
            If conStrValue(0) = 1 Then
                DBPassword = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'SERVER
            conStrValue = F_ConStrGetValue("Server", strCon)
            If conStrValue(0) = 1 Then
                DBServer = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PORT
            conStrValue = F_ConStrGetValue("Port", strCon)
            If conStrValue(0) = 1 Then
                DBPort = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'DATABASE
            conStrValue = F_ConStrGetValue("Database", strCon)
            If conStrValue(0) = 1 Then
                DBDatabase = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If


            'PROSES DUMP DATABASE
            Dim myProcess As Process = New Process()

            'Dim strOptions As String = String.Format(" --user={0} --password={1} --host={2} --port={3} --add-drop-database --add-drop-table --extended-insert --databases {4}", DBUser, DBPassword, DBServer, DBPort, DBDatabase & " " & strTable)
            Dim strOptions As String = String.Format(" --user={0} --password={1} --extended-insert=FALSE {4}", DBUser, DBPassword, DBServer, DBPort, DBDatabase & " " & strTable)

            With myProcess
                .StartInfo.UseShellExecute = False
                .StartInfo.CreateNoWindow = True
                .StartInfo.RedirectStandardInput = True
                .StartInfo.RedirectStandardOutput = True
                .StartInfo.RedirectStandardError = True
                .StartInfo.FileName = pathServiceDB & "mysqldump.exe"
                .StartInfo.Arguments = strOptions
                .Start()

            End With


            Dim sOut As System.IO.StreamReader = myProcess.StandardOutput
            Dim line As String

            'TAMBAHKAN QUERY SET AUTOCOMMIT = 0
            strValue.AppendLine("SET foreign_key_checks = 0;SET UNIQUE_CHECKS = 0;SET AUTOCOMMIT = 0;")

            ' Read and display the lines from the file until the end 
            ' of the file is reached.
            Do
                line = sOut.ReadLine()
                strValue.AppendLine(line)
            Loop Until line Is Nothing

            'TAMBAHKAN QUERY SET AUTOCOMMIT = 0
            strValue.AppendLine("SET foreign_key_checks = 1;SET UNIQUE_CHECKS = 1;SET AUTOCOMMIT = 1;COMMIT;")

            sOut.Close()
            myProcess.Close()

            hasil(0) = 1
            hasil(1) = strValue.ToString

        Catch ex As Exception
            hasil(0) = 0
            hasil(1) = "Dump database failed : " & (ex.Message)
            GoTo selesai

        End Try

selesai:
        Return hasil

    End Function

    '//FUNGSI UNTUK EXECUTE DATABASE SQL => UNTUK KEBUTUHAN APLIKASI POS OFFLINE
    Public Function F_ExecuteSQL(ByVal websiteAccessKey As String, ByVal userid As String, ByVal fileName As String, ByVal strCon As String) As String()
        ' Uses the mysqlimport.exe program to execute a backup of the database.

        Dim hasil(2) As String 'isSuccess(0), result(1)
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Dim Security As New ClsSecurity
        Dim filePath As String = HttpContext.Current.Server.MapPath("~/") & "files\db\"

        Try

            Dim DBUser As String = "", DBPassword As String = "", DBServer As String = "", DBPort As String = "", DBDatabase As String = ""
            Dim conStrValue() As String
            Dim ServiceDBValue() As String, ServiceDB As String = ""
            Dim pathServiceDBValue() As String, pathServiceDB As String = ""

            'AMBIL SERVICE DB -> MYSQL (DARI APP.XML)
            ServiceDBValue = F_AppGetValue("SqlServiceName")
            If ServiceDBValue(0) = 1 Then
                ServiceDB = ServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = ServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL PATH SERVICE MYSQL -> UNTUK PANGGIL mysqldump
            pathServiceDBValue = F_GetServicePath(ServiceDB)
            If pathServiceDBValue(0) = 1 Then
                pathServiceDB = pathServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = pathServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL NILAI STRCON DARI APP.XML
            'USER
            conStrValue = F_ConStrGetValue("Uid", strCon)
            If conStrValue(0) = 1 Then
                DBUser = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PASSWORD
            conStrValue = F_ConStrGetValue("Pwd", strCon)
            If conStrValue(0) = 1 Then
                DBPassword = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'SERVER
            conStrValue = F_ConStrGetValue("Server", strCon)
            If conStrValue(0) = 1 Then
                DBServer = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PORT
            conStrValue = F_ConStrGetValue("Port", strCon)
            If conStrValue(0) = 1 Then
                DBPort = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'DATABASE
            conStrValue = F_ConStrGetValue("Database", strCon)
            If conStrValue(0) = 1 Then
                DBDatabase = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If

            'CEK FILE EXISTS
            If (Not File.Exists(filePath & fileName)) Then
                hasil(0) = 0
                hasil(1) = "'" & fileName & "' file doesn't exists." : GoTo selesai
            End If

            'PROSES EXECUTE SQL
            Dim myProcess As New Process()
            myProcess.StartInfo.FileName = "cmd.exe"
            myProcess.StartInfo.UseShellExecute = False
            myProcess.StartInfo.CreateNoWindow = True
            myProcess.StartInfo.WorkingDirectory = pathServiceDB
            myProcess.StartInfo.RedirectStandardInput = True
            myProcess.StartInfo.RedirectStandardOutput = True
            myProcess.StartInfo.RedirectStandardError = True
            myProcess.Start()

            Dim myStreamWriter As StreamWriter = myProcess.StandardInput
            Dim mystreamreader As StreamReader = myProcess.StandardOutput
            myStreamWriter.WriteLine("mysql -u " & DBUser & " -p" & DBPassword & " " & DBDatabase & " < " & filePath & fileName & " ")
            myStreamWriter.Close()
            myProcess.WaitForExit()
            myProcess.Close()

            hasil(0) = 1
            hasil(1) = ""

        Catch ex As Exception
            hasil(0) = 0
            hasil(1) = "Execute database failed : " & (ex.Message)
            GoTo selesai

        End Try

selesai:
        Return hasil

    End Function



    '//FUNGSI UNTUK DUMP DATABASE SQL
    Public Function F_DumpAllSQLAsFile(ByVal websiteAccessKey As String, ByVal userid As String, ByVal strCon As String) As String()
        ' Uses the mysqldump.exe program to make a backup of the database.
        ' This is an in-out stream operation.  The data is piped in via the 
        ' process's standard output and sent to a filestream to be written 
        ' to disk.

        Dim hasil(2) As String 'isSuccess(0), result(1)
        hasil(0) = 0
        hasil(1) = "Processing " & System.Reflection.MethodBase.GetCurrentMethod.Name & " Failed."

        Dim Security As New ClsSecurity
        Dim fileName As String = "", filePath As String = ""
        Dim sr As StreamReader
        Dim contents As String = ""

        Try

            Dim DBUser As String = "", DBPassword As String = "", DBServer As String = "", DBPort As String = "", DBDatabase As String = ""
            Dim conStrValue() As String
            Dim ServiceDBValue() As String, ServiceDB As String = ""
            Dim pathServiceDBValue() As String, pathServiceDB As String = ""

            'SET FILENAME DAN FILEPATH
            fileName = Security.MD5CalcString(userid & websiteAccessKey & Now) & ".sql"
            filePath = HttpContext.Current.Server.MapPath("~/") & "files\db\"

            'DAFTAR TABEL YANG DI DUMP (tabel1 tabel2 tabel3 tabeldst)
            Dim strTable As String = "m0_role m0_role_custom m0_role_menu m0_role_report m0_selling_rate m0_setting m0_setting_location m0_user m0_user_branch m0_user_location m0_user_role m0_user_warehouse m1_area m1_bank m1_branch m1_coa m1_cogs_fifo_in m1_cogs_fifo_out m1_cogs_special_in m1_cogs_special_out m1_contact m1_contact_attention m1_contact_category m1_contact_point m1_cost_center m1_currency m1_customer_category m1_division m1_files m1_item m1_item_assembly m1_item_category m1_item_location m1_item_location_warehouse m1_item_stock_warehouse m1_item_type m1_location m1_no_batch_in m1_no_batch_out m1_no_batch_transaction m1_no_serial_in m1_no_serial_out m1_no_serial_transaction m1_project m1_region m1_salesman_category m1_selling_point m1_subdivision m1_supplier_category m1_tax m1_terms m1_unit m1_warehouse m1_type_sa m_12_area m_12_area_category m_12_pos_additional_item m_12_pos_additional_item_detail m_12_pos_bonus_item m_12_pos_bonus_item_detail m_12_pos_category m_12_pos_category_setting m_12_pos_discount_category_customer m_12_pos_discount_category_item m_12_pos_discount_item m_12_pos_item m_12_pos_point_category_item m_12_pos_point_item m_12_pos_point_transaction m_12_pos_setting m_12_pos_substitution_item m_12_pos_substitution_item_detail m_12_pos_voucher_in m_12_pos_voucher_out m1_item_permission"

            'AMBIL SERVICE DB -> MYSQL (DARI APP.XML)
            ServiceDBValue = F_AppGetValue("SqlServiceName")
            If ServiceDBValue(0) = 1 Then
                ServiceDB = ServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = ServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL PATH SERVICE MYSQL -> UNTUK PANGGIL mysqldump
            pathServiceDBValue = F_GetServicePath(ServiceDB)
            If pathServiceDBValue(0) = 1 Then
                pathServiceDB = pathServiceDBValue(1)
            Else
                hasil(0) = 0
                hasil(1) = pathServiceDBValue(1) : GoTo selesai
            End If

            'AMBIL NILAI STRCON DARI APP.XML
            'USER
            conStrValue = F_ConStrGetValue("Uid", strCon)
            If conStrValue(0) = 1 Then
                DBUser = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PASSWORD
            conStrValue = F_ConStrGetValue("Pwd", strCon)
            If conStrValue(0) = 1 Then
                DBPassword = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'SERVER
            conStrValue = F_ConStrGetValue("Server", strCon)
            If conStrValue(0) = 1 Then
                DBServer = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'PORT
            conStrValue = F_ConStrGetValue("Port", strCon)
            If conStrValue(0) = 1 Then
                DBPort = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If
            'DATABASE
            conStrValue = F_ConStrGetValue("Database", strCon)
            If conStrValue(0) = 1 Then
                DBDatabase = conStrValue(1)
            Else
                hasil(0) = 0
                hasil(1) = conStrValue(1) : GoTo selesai
            End If


            'PROSES DUMP DATABASE
            Dim myProcess As Process = New Process()

            'Dim strOptions As String = String.Format(" --user={0} --password={1} --host={2} --port={3} --add-drop-database --add-drop-table --extended-insert --databases {4}", DBUser, DBPassword, DBServer, DBPort, DBDatabase & " " & strTable)
            Dim strOptions As String = String.Format(" --user={0} --password={1} --extended-insert=FALSE {4}", DBUser, DBPassword, DBServer, DBPort, "myerpplus-kosong-terbaru" & " " & "")

            With myProcess
                .StartInfo.UseShellExecute = False
                .StartInfo.CreateNoWindow = True
                .StartInfo.RedirectStandardInput = True
                .StartInfo.RedirectStandardOutput = True
                .StartInfo.RedirectStandardError = True
                .StartInfo.FileName = pathServiceDB & "mysqldump.exe"
                .StartInfo.Arguments = strOptions
                .Start()

            End With

            If (Not System.IO.Directory.Exists(filePath)) Then
                System.IO.Directory.CreateDirectory(filePath)
            End If
            filePath = filePath & fileName

            Dim sOut As System.IO.StreamReader = myProcess.StandardOutput
            Dim sToFile As New System.IO.StreamWriter(filePath)
            Dim line As String

            'TAMBAHKAN QUERY SET AUTOCOMMIT = 0
            sToFile.WriteLine("SET foreign_key_checks = 0;SET UNIQUE_CHECKS = 0;SET AUTOCOMMIT = 0;")

            ' Read and display the lines from the file until the end 
            ' of the file is reached.
            Do
                line = sOut.ReadLine()
                sToFile.WriteLine(line)
            Loop Until line Is Nothing

            'TAMBAHKAN QUERY SET AUTOCOMMIT = 0
            sToFile.WriteLine("SET foreign_key_checks = 1;SET UNIQUE_CHECKS = 1;SET AUTOCOMMIT = 1;COMMIT;")

            sOut.Close()
            sToFile.Close()

            myProcess.Close()


            'CEK FILE EXISTS
            If (File.Exists(filePath)) Then
                sr = File.OpenText(filePath)
                contents = sr.ReadToEnd()
                sr.Close()

                hasil(0) = 1
                hasil(1) = contents

            Else
                hasil(0) = 0
                hasil(1) = "'" & fileName & "' file doesn't exists." : GoTo selesai

            End If

            'Catch ex As MySqlException
            '    hasil(0) = 0
            '    hasil(1) = "Dump database failed : " & (ex.Message)
            '    GoTo selesai

        Catch ex As Exception
            hasil(0) = 0
            hasil(1) = "Dump database failed : " & (ex.Message)
            GoTo selesai

        End Try

selesai:
        Return hasil

    End Function

    'FUNGSI UNTUK AMBIL SETTING
    Public Function F_getSetting(ByVal sModule As Integer, ByVal sGrup As String, ByVal sKode As String) As String
        Dim sql As String = "", hasil As String = ""

        sql = "SELECT snilai FROM m0_setting WHERE smodule = '" & sModule & "' AND sgrup = '" & sGrup & "' AND skode = '" & sKode & "'"
        Dim dtSetting As DataTable = AsDataTableAmbilDariDB(sql)
        If dtSetting.Rows.Count > 0 Then
            If Len(FxDB(dtSetting.Rows(0)("snilai"), "")) > 0 Then
                hasil = FxDB(dtSetting.Rows(0)("snilai"), "")
            End If
        End If

        Return hasil
    End Function

    '//FUNGSI UNTUK CEK HAK AKSES CUSTOM
    Public Function HakAksesCustom(ByVal ModuleId As Integer, ByVal Id As Integer, ByVal Kode As String, ByVal UserId As Integer) As String
        Dim ErrMessage As String = "", sql As String = "", akses As Integer = 0
        Dim dt As New DataTable

        'CEK HAK AKSES =================================
        sql = "SELECT rc.rcakses FROM m0_role_custom rc JOIN m0_user_role ur ON rc.rcrole = ur.role WHERE rc.rcmoduleid = '" & FixDouble(ModuleId) & "' AND rc.rcidpc = '" & FixDouble(Id) & "' AND ur.userid = '" & FixDouble(UserId) & "'"
        dt = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count > 0 Then
            If Len(dt.Rows(0)(0)) > 0 Then
                akses = Double.Parse(dt.Rows(0)("rcakses").ToString)

                'JIKA AKSES <> 1 MAKA ALERT TIDAK MEMPUNYAI HAK AKSES
                If akses <> 1 Then
                    GoTo tidakPunyaAkses
                End If

            Else
                GoTo tidakPunyaAkses
            End If

        Else
tidakPunyaAkses:
            ErrMessage = "This role doesn't have permission for '" & Kode & "' menu." : GoTo selesai
        End If
        'END OF CEK HAK AKSES ==========================

selesai:
        Return ErrMessage
    End Function

    '// AsDataTableAmbilDariDB Koneksi Khusus
    Public Function AsDataTableAmbilDariDBCon(ByVal StrSQL As String, ByVal ConX As MySqlConnection) As DataTable
        On Error GoTo Salah
        Dim xStep As Long = 0 : ErrNumber = 0 : ErrStep = 0 : ErrDescription = ""
        'Dim ConX As MySqlConnection = New MySqlConnection

        xStep = 1 'Buat Koneksi
        'ConX.ConnectionString = strCon
        'ConX.Open()

Ulang:
        Dim da1 As MySqlDataAdapter = New MySqlDataAdapter(StrSQL, ConX)
        da1.SelectCommand.CommandTimeout = 31536000

        xStep = 4 'Set datatable
        Dim dt1 As DataTable = New DataTable()
        da1.Fill(dt1)

        ''Tutup Koneksi
        'ConX.Close()

        Return dt1
        Exit Function
Salah:
        'ErrNumber = Err.Number : ErrDescription = Err.Description : ErrSource = Err.Source : ErrLine = Err.Erl : ErrStep = xStep
        'If ErrNumber = 5 Then
        '    If InStr(ErrDescription, "Connection which must be closed first") > 0 Then
        '        Err.Clear()
        '        ConX = New MySqlConnection(strCon)
        '        ConX.Open()
        '        GoTo Ulang
        '    End If
        'End If

        'AsPesanKesalahan("AsDataTableAmbilDariDB", ErrNumber, ErrDescription, ErrSource, ErrLine, ErrStep)
        Return dt1
    End Function

    Public Sub SimpanLogWsToFile(ByVal modul As Double, ByVal Sumber As String, ByVal StrVal As String)
        If Len(StrVal) > 0 Then
            Try
                Dim myPath As String = HttpContext.Current.Server.MapPath("~/") & "app_code\ws\m" & modul & "\m" & modul & "_" & Sumber & "Log.txt"

                If File.Exists(myPath) = False Then
                    File.Create(myPath).Dispose()
                End If

                Dim streamWriter As StreamWriter = New StreamWriter(myPath, True)
                With streamWriter
                    .Write(Now & " : " & StrVal & vbCrLf)
                    .Flush()
                    .Dispose()
                    .Close()
                End With

            Catch ex As Exception

            End Try
        End If
    End Sub

    'FUNGSI RANDOM CHAR
    Public Function F_RandomChar(ByVal sChar As Integer, ByVal sNumber As Integer) As String
        Dim validchars As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        Dim validnumbers As String = "1234567890"
        Dim randomChar As Char = "", hasil As String = ""
        Dim idx As Integer = 0

        Dim sb As New StringBuilder()
        Dim rand As New Random()

        For i As Integer = 1 To sChar
            idx = rand.Next(0, validchars.Length)
            randomChar = validchars(idx)
            sb.Append(randomChar)
        Next i

        For i As Integer = 1 To sNumber
            idx = rand.Next(0, validnumbers.Length)
            randomChar = validnumbers(idx)
            sb.Append(randomChar)
        Next i

        hasil = sb.ToString()

        Return hasil
    End Function

    Public Function AmbilDataNewest(ByVal key As String, Optional ByVal filter As String = Nothing, Optional ByVal sort As String = Nothing, Optional ByVal strField As String = Nothing, Optional ByVal strFieldType As String = Nothing, Optional ByVal pageNumber As Integer = 0, Optional ByVal itemLimit As Integer = 0, Optional ByRef Pg As RsPaging = Nothing, Optional ByVal Relasi As String = Nothing, Optional ByVal koneksidb As Integer = 0, Optional ByVal groupby As String = "", Optional ByVal strSqlm As String = Nothing) As DataTable
        Dim dt As New DataTable
        Dim sKey() As String = key.Split("-")           'Sample : dbase-nmtable -> (northwind-categories)
        Dim jmlSplitkey As Integer = sKey.Length
        Dim param As String = sKey(1)                   'Sample : skey(1) = nmtablenya : 'categories'
        Dim sTable() As String = param.Split("~")       'Split table
        Dim JmlsplitTable As Integer = sTable.Length    'Jika jumlah 2, brati relasi antar table

        Dim isPaging As Boolean = pageNumber <> 0, isNext As Boolean = False
        Dim sql As String = ""


        'JIKA AMBIL KE DATABASE ================================================================================
        Dim rowStart As Integer = 0
        Dim Limit As String = ""

        'LIMIT LAST PAGE
        If pageNumber = -1 Then
            Dim sqldata As String = ""
            'HITUNG PAGE NUMBER = jmldata/itemlimit
            Dim dtlastpage As DataTable
            If (strSqlm = Nothing) Then
                sqldata = "select 0 from " & sTable(0)
                If Len(filter) > 0 Then sqldata &= " where " & filter
                If Len(groupby) > 0 Then sqldata &= " group by " & groupby
                dtlastpage = AsDataTableAmbilDariDB(sqldata)
                pageNumber = Math.Ceiling((dtlastpage.Rows.Count) / itemLimit)
            Else
                sqldata = strSqlm
                If Len(filter) > 0 Then sqldata &= " where " & filter
                If Len(groupby) > 0 Then sqldata &= " group by " & groupby
                dtlastpage = AsDataTableAmbilDariDB(sqldata)
                pageNumber = Math.Ceiling((dtlastpage.Rows.Count) / itemLimit)
            End If

            rowStart = (pageNumber - 1) * itemLimit
            Limit = " limit " & rowStart & "," & itemLimit + 1

            'LIMIT SESUAI PAGENUMBER
        ElseIf pageNumber > 0 Then
            rowStart = (pageNumber - 1) * itemLimit
            Limit = " limit " & rowStart & "," & itemLimit + 1
        End If


        'AMBIL KE DB LANGSUNG DARI NAMA TABEL
        If (strSqlm = Nothing) Then
            If Len(filter) = 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=0 & sort=0 & groupby=0
                sql = "select * from " & sTable(0) & Limit
            ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=1 & sort=0 & groupby=0
                sql = "select * from " & sTable(0) & " where " & filter & Limit
            ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=0 & sort=1 & groupby=0
                sql = "select * from " & sTable(0) & " order by " & sort & Limit
            ElseIf Len(filter) = 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=0 & sort=0 & groupby=1
                sql = "select * from " & sTable(0) & " group by " & groupby & Limit
            ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=1 & sort=1 & groupby=0
                sql = "select * from " & sTable(0) & " where " & filter & " order by " & sort & Limit
            ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=1 & sort=0 & groupby=1
                sql = "select * from " & sTable(0) & " where " & filter & " group by " & groupby & Limit
            ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=0 & sort=1 & groupby=1
                sql = "select * from " & sTable(0) & " group by " & groupby & " order by " & sort & Limit
            ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=1 & sort=1 & groupby=1
                sql = "select * from " & sTable(0) & " where " & filter & " group by " & groupby & " order by " & sort & Limit
            End If

            'AMBIL KE DB MENGGUNAKAN QUERY
        Else
            If Len(filter) = 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=0 & sort=0 & groupby=0
                sql = strSqlm & " " & Limit
            ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) = 0 Then 'filter=1 & sort=0 & groupby=0
                sql = strSqlm & " " & " where " & filter & Limit
            ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=0 & sort=1 & groupby=0
                sql = strSqlm & " " & " order by " & sort & Limit
            ElseIf Len(filter) = 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=0 & sort=0 & groupby=1
                sql = strSqlm & " " & " group by " & groupby & Limit
            ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) = 0 Then 'filter=1 & sort=1 & groupby=0
                sql = strSqlm & " " & " where " & filter & " order by " & sort & Limit
            ElseIf Len(filter) > 0 And Len(sort) = 0 And Len(groupby) > 0 Then 'filter=1 & sort=0 & groupby=1
                sql = strSqlm & " " & " where " & filter & " group by " & groupby & Limit
            ElseIf Len(filter) = 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=0 & sort=1 & groupby=1
                sql = strSqlm & " " & " group by " & groupby & " order by " & sort & Limit
            ElseIf Len(filter) > 0 And Len(sort) > 0 And Len(groupby) > 0 Then 'filter=1 & sort=1 & groupby=1
                sql = strSqlm & " " & " where " & filter & " group by " & groupby & " order by " & sort & Limit
            End If
        End If

        'AMBIL KE DB
        dt = AsDataTableAmbilDariDB(sql)
        If dt.Rows.Count > itemLimit Then isNext = True Else isNext = False
        If isPaging Then dt = AsDataTableFilterLimit(dt, "", "", 0, itemLimit)

        'PAGING
        If isPaging Then
            With Pg
                .countRow = 0
                .countPage = pageNumber ' dijadikan curPage
                .curPage = pageNumber
                .isNext = isNext
                .isPaging = isPaging
                .isPrev = pageNumber > 1
                .nextPage = True
                .prevPage = True
            End With
        Else
            With Pg
                .countRow = 0
                .countPage = 0
                .curPage = 0
                .isNext = False
                .isPaging = isPaging
                .isPrev = False
                .nextPage = False
                .prevPage = False
            End With
        End If

        Return dt
    End Function


End Module
