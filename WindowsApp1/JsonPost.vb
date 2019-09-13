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

#Disable Warning IDE0044 ' Dodaj modyfikator tylko do odczytu
    Private urlToPost As String = ""
#Enable Warning IDE0044 ' Dodaj modyfikator tylko do odczytu

    Public Sub New(ByVal urlToPost As String)
        Me.urlToPost = urlToPost
    End Sub

    Public Function PostData(ByVal dictData As Dictionary(Of String, Object)) As Boolean
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

