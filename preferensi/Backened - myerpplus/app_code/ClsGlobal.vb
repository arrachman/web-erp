'Imports System.Web
'Imports System.Web.Services
'Imports System.Web.Services.Protocols
'Imports AsModuleMySQL.CommonFunction
'Imports System.Web.Script.Services

Imports System.Data
Imports Microsoft.VisualBasic
Imports Enyim.Caching.Configuration
Imports Enyim.Caching
Imports Enyim.Caching.Memcached
Imports System.Security.Cryptography

Public Class RsHasilWsSearch
    Public success, isPaging, isNext, isPrevious, countPage, countRow As Integer
    Public errmessage, data As String
End Class
Public Class RsHasil
    Public success As Boolean
    Public errmessage, target As String
    Public errstep As Integer
End Class
Public Class RsHasilBi
    Public success As Boolean
    Public chart As String
    Public table As String
    Public errmessage, target As String
    Public errstep As Integer
End Class
Public Class RsHasilId
    Inherits RsHasil
    Public idtransaksi As String
End Class
Public Class RsValidKey
    Public success As Boolean
    Public errmessage As String
End Class
Public Class RsPaging
    Public isPaging, isNext, isPrev As Boolean
    Public curPage, prevPage, nextPage As String
    Public countPage, countRow As Integer
End Class
Public Class RsCPaging
    Inherits RsPaging
    Public dt As New DataTable
End Class

Public Class Result
    Inherits RsHasilId
    Public paging As New RsPaging
    Public SearchResult As String
End Class

Public Class wsResult
    Public result As String
    Public paging As String
    Public searchResult As String
End Class

Public Class ClsSecurity


#Region "Generate ID"
    ' Define default min and max password lengths.
    Private Shared DEFAULT_MIN_PASSWORD_LENGTH As Integer = 8
    Private Shared DEFAULT_MAX_PASSWORD_LENGTH As Integer = 10

    ' Define supported password characters divided into groups.
    ' You can add (or remove) characters to (from) these groups.
    Private Shared PASSWORD_CHARS_LCASE As String = "abcdwfghijklmnopqrstuvqxyz"
    Private Shared PASSWORD_CHARS_UCASE As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
    Private Shared PASSWORD_CHARS_NUMERIC As String = "0123456789"
    Private Shared PASSWORD_CHARS_SPECIAL As String = "0123456789"

    Public Function ValidateKey(ByVal key As String) As RsValidKey
        '///Perlu dibenahi dan dipasang dg benar
        Dim vk As New RsValidKey
        vk.success = True
        vk.errmessage = "Website access key is invalid !."
        Return vk
    End Function


    Public Function Generate() As String
        Return Generate(DEFAULT_MIN_PASSWORD_LENGTH, DEFAULT_MAX_PASSWORD_LENGTH)
    End Function

    Public Function Generate(ByVal length As Integer) As String
        Return Generate(length, length)
    End Function


    Private Function Generate(ByVal minLength As Integer, ByVal maxLength As Integer) As String
        ' Make sure that input parameters are valid.
        If minLength <= 0 OrElse maxLength <= 0 OrElse minLength > maxLength Then
            Return Nothing
        End If
        Dim charGroups As Char()() = New Char()() {PASSWORD_CHARS_LCASE.ToCharArray(), PASSWORD_CHARS_UCASE.ToCharArray(), PASSWORD_CHARS_NUMERIC.ToCharArray(), PASSWORD_CHARS_SPECIAL.ToCharArray()}

        Dim charsLeftInGroup As Integer() = New Integer(charGroups.Length - 1) {}

        ' Initially, all characters in each group are not used.
        For i As Integer = 0 To charsLeftInGroup.Length - 1
            charsLeftInGroup(i) = charGroups(i).Length
        Next

        Dim leftGroupsOrder As Integer() = New Integer(charGroups.Length - 1) {}

        For i As Integer = 0 To leftGroupsOrder.Length - 1
            leftGroupsOrder(i) = i
        Next
        Dim randomBytes As Byte() = New Byte(3) {}

        Dim rng As New RNGCryptoServiceProvider()
        rng.GetBytes(randomBytes)

        Dim seed As Integer = (randomBytes(0) And &H7F) << 24 Or randomBytes(1) << 16 Or randomBytes(2) << 8 Or randomBytes(3)

        Dim random As New Random(seed)

        Dim password As Char() = Nothing

        If minLength < maxLength Then
            password = New Char(random.[Next](minLength, maxLength + 1) - 1) {}
        Else
            password = New Char(minLength - 1) {}
        End If

        Dim nextCharIdx As Integer

        Dim nextGroupIdx As Integer

        Dim nextLeftGroupsOrderIdx As Integer

        Dim lastCharIdx As Integer

        Dim lastLeftGroupsOrderIdx As Integer = leftGroupsOrder.Length - 1

        For i As Integer = 0 To password.Length - 1
            If lastLeftGroupsOrderIdx = 0 Then
                nextLeftGroupsOrderIdx = 0
            Else
                nextLeftGroupsOrderIdx = random.[Next](0, lastLeftGroupsOrderIdx)
            End If

            nextGroupIdx = leftGroupsOrder(nextLeftGroupsOrderIdx)

            lastCharIdx = charsLeftInGroup(nextGroupIdx) - 1

            If lastCharIdx = 0 Then
                nextCharIdx = 0
            Else
                nextCharIdx = random.[Next](0, lastCharIdx + 1)
            End If

            ' Add this character to the password.
            password(i) = charGroups(nextGroupIdx)(nextCharIdx)

            If lastCharIdx = 0 Then
                charsLeftInGroup(nextGroupIdx) = charGroups(nextGroupIdx).Length
            Else
                If lastCharIdx <> nextCharIdx Then
                    Dim temp As Char = charGroups(nextGroupIdx)(lastCharIdx)
                    charGroups(nextGroupIdx)(lastCharIdx) = charGroups(nextGroupIdx)(nextCharIdx)
                    charGroups(nextGroupIdx)(nextCharIdx) = temp
                End If
                charsLeftInGroup(nextGroupIdx) -= 1
            End If

            If lastLeftGroupsOrderIdx = 0 Then
                lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1
            Else
                If lastLeftGroupsOrderIdx <> nextLeftGroupsOrderIdx Then
                    Dim temp As Integer = leftGroupsOrder(lastLeftGroupsOrderIdx)
                    leftGroupsOrder(lastLeftGroupsOrderIdx) = leftGroupsOrder(nextLeftGroupsOrderIdx)
                    leftGroupsOrder(nextLeftGroupsOrderIdx) = temp
                End If
                lastLeftGroupsOrderIdx -= 1
            End If
        Next

        Return New String(password)
    End Function
#End Region

#Region "MD5"
    Public Function MD5CalcString(ByVal strData As String) As String

        Dim objMD5 As New System.Security.Cryptography.MD5CryptoServiceProvider
        Dim arrData() As Byte
        Dim arrHash() As Byte

        ' first convert the string to bytes (using UTF8 encoding for unicode characters)
        arrData = System.Text.Encoding.UTF8.GetBytes(strData)

        ' hash contents of this byte array
        arrHash = objMD5.ComputeHash(arrData)

        ' thanks objects
        objMD5 = Nothing

        ' return formatted hash
        Return ByteArrayToString(arrHash)

    End Function

    ' utility function to convert a byte array into a hex string
    Private Function ByteArrayToString(ByVal arrInput() As Byte) As String

        Dim strOutput As New System.Text.StringBuilder(arrInput.Length)

        For i As Integer = 0 To arrInput.Length - 1
            strOutput.Append(arrInput(i).ToString("X2"))
        Next

        Return strOutput.ToString().ToLower

    End Function
#End Region

    'Public Function ValidateKey(ByVal key As String) As RsValidKey
    '    '///Perlu dibenahi dan dipasang dg benar

    '    Dim vk As New RsValidKey
    '    vk.errmessage = "Invalid Website Access Key."
    '    If IsNothing(AsMemcached.GetCache("myerpplus-" & key)) Then
    '        vk.success = False
    '    Else
    '        vk.success = True
    '    End If

    '    'Dim vk As New RsValidKey
    '    'vk.success = True
    '    'vk.errmessage = "Website access key is invalid !."
    '    Return vk
    'End Function

    Public Function ApaBisaAkses(ByVal ModuleID As Integer, ByVal MenuID As Integer, ByVal AksesID As Integer) As Boolean
        'AksesID : 1=Insert/Update, 2=Delete, 3=GetData, 4=Report
        '///Perlu dibenahi dan dipasang dg benar
        Return True
    End Function

End Class

Public Class ClsMemcached

    '// Ini List Server nya
    Sub New()
    End Sub

    Sub New(ByVal ServerIP As List(Of String))
        Dim i As Integer = 1
        For Each Server As String In ServerIP
            SetServer(i) = Server
            i += 1
        Next
    End Sub

    Sub New(ByVal ServerIP As String)
        SetServer = ServerIP
    End Sub

    Private _MemcachedServer(2) As MemcachedClientConfiguration '// 0-1
    Private _MaxServerIndex As Integer = 1

    '// Ser server 
    Public WriteOnly Property SetServer(Optional ByVal ServerId As Integer = 1) As String
        Set(ByVal value As String)
            Try
                '// Maximal  
                If _MaxServerIndex < ServerId Then
                    _MaxServerIndex = ServerId
                    ReDim Preserve _MemcachedServer(ServerId)
                End If

                _MemcachedServer(ServerId) = New MemcachedClientConfiguration()
                _MemcachedServer(ServerId).AddServer(value)

                _MemcachedServer(ServerId).SocketPool.ReceiveTimeout = New TimeSpan(60, 60, 60)
                _MemcachedServer(ServerId).SocketPool.ConnectionTimeout = New TimeSpan(60, 60, 60)
                _MemcachedServer(ServerId).SocketPool.DeadTimeout = New TimeSpan(10, 10, 10)

                _MemcachedServer(ServerId).Authentication.Parameters("Username") = ""

                _MemcachedServer(ServerId).Protocol = MemcachedProtocol.Text
            Catch ex As Exception

            End Try
        End Set
    End Property

    '// Set Distcached
    Public Function SetCache(ByVal Key As String, ByVal ObjVal As Object, Optional ByVal AliveFor As TimeSpan = Nothing, Optional ByVal ServerId As Long = 1) As Boolean
        If Not IsNothing(_MemcachedServer(ServerId)) Then
            Dim mcc As MemcachedClientConfiguration
            mcc = _MemcachedServer(ServerId)
            Using client As New MemcachedClient(mcc)
                Try
                    If IsNothing(AliveFor) = False Then
                        Return client.Store(StoreMode.[Set], Key, ObjVal, AliveFor)
                    Else
                        Return client.Store(StoreMode.[Set], Key, ObjVal)
                    End If

                Catch ex As Exception
                    Return False
                End Try
            End Using
        Else
            Return False
        End If
    End Function


    '// Get Distcached
    Public ReadOnly Property GetCache(ByVal Key As String, Optional ByVal ServerId As Long = 1) As Object
        Get
            Dim mcc As MemcachedClientConfiguration
            mcc = _MemcachedServer(ServerId)
            Using client As New MemcachedClient(mcc)
                Try
                    Return client.[Get](Key)
                Catch ex As Exception
                    Return Nothing
                End Try
            End Using
        End Get
    End Property

    '// Get Multiple Cached
    Public ReadOnly Property GetCache(ByVal Key As List(Of String), Optional ByVal ServerId As Long = 1) As Object
        Get
            Dim mcc As MemcachedClientConfiguration
            mcc = _MemcachedServer(ServerId)
            Using client As New MemcachedClient(mcc)
                Return client.Get(Key)
            End Using
        End Get
    End Property

    '// Delete Cachedd
    Public Function Remove(ByVal Key As String, Optional ByVal ServerId As Integer = 1) As Boolean
        Dim mcc As MemcachedClientConfiguration
        mcc = _MemcachedServer(ServerId)
        If Not IsNothing(mcc) Then
            Using client As New MemcachedClient(mcc)
                Return client.Remove(Key)
            End Using
        Else
            Return False
        End If
    End Function

    '// Remove ALL
    Public Function RemoveAll(Optional ByVal ServerId As Integer = 1) As Boolean
        Dim mcc As MemcachedClientConfiguration
        mcc = _MemcachedServer(ServerId)
        If Not IsNothing(mcc) Then
            Using client As New MemcachedClient(mcc)
                Try
                    client.FlushAll()
                    Return (True)
                Catch ex As Exception
                    Return False
                End Try
            End Using
        Else
            Return False
        End If
    End Function

    Public Function IsExist(ByVal Key As String, Optional ByVal ServerId As Integer = 1) As Boolean
        Dim mcc As MemcachedClientConfiguration
        mcc = _MemcachedServer(ServerId)
        If Not IsNothing(mcc) Then
            Using client As New MemcachedClient(mcc)
                Try
                    Return client.TryGet(Key, New Object)
                Catch ex As Exception
                    Return False
                End Try
            End Using
        Else
            Return False
        End If
    End Function


End Class