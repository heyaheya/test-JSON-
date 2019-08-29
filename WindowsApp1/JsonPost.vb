'Install Newtonsoft.json
'-----------------------
'
'PM> Install-Package Newtonsoft.Json -Version 6.0.8

'Sample Usage
'------------

'Dim jsonPost As New JsonPost("http://192.168.254.104:8000")
'Dim dictData As New Dictionary(Of String, Object)
'dictData.Add("test_key", "test_value")
'jsonPost.postData(dictData)

Imports Newtonsoft.Json
Imports System.Net
Imports System.Text
Public Class JsonPost

    Private urlToPost As String = ""

    Public Sub New(ByVal urlToPost As String)
        Me.urlToPost = urlToPost
    End Sub

    Public Function postData(ByVal dictData As Dictionary(Of String, Object)) As Boolean
        Dim webClient As New WebClient()
        Dim resByte As Byte()
        Dim resString As String
        Dim reqString() As Byte

        Try
            webClient.Headers("content-type") = "application/json"
            reqString = Encoding.Default.GetBytes(JsonConvert.SerializeObject(dictData, Formatting.Indented))
            resByte = webClient.UploadData(Me.urlToPost, "post", reqString)
            resString = Encoding.Default.GetString(resByte)
            Console.WriteLine(resString)
            webClient.Dispose()
            Return True
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
        Return False
    End Function

End Class

Public Class json1



        Public Class Latest_valuesItem
            Public Property timestamp As String
            Public Property value As Double
        End Class

        Public Class DataItem
            Public Property name As String
            Public Property mac As String
            Public Property detector_id As Integer
            Public Property latest_values As List(Of Latest_valuesItem)
        End Class

        Public Class Root
            Public Property value_interval As String
            Public Property value_unit As String
            Public Property timestamp_timezone As String
            Public Property data As List(Of DataItem)
        End Class

    End Class
