Imports System.Net
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports Newtonsoft.Json


Public Class Form1

    Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        NumericUpDown1.Value = 1
        ListView1.View = 1
        ListView2.View = 1


        Dim url As String = "https://platforma.enspirion.pl/services/energa/?pass=mFffGMVLcDNQRdeubJ3qFH2tvpb2zA2KDxFx4epD9XSc3BF2GB"
        Dim webClient As New WebClient
        Dim rawJSON As String

        rawJSON = webClient.DownloadString(url)
        Dim zestaw As Root = JsonConvert.DeserializeObject(Of Root)(rawJSON)

        Console.WriteLine(zestaw.value_interval)
        Console.WriteLine(zestaw.value_unit)
        Console.WriteLine(zestaw.timestamp_timezone)
        Console.WriteLine("licznik pomiarów:" & zestaw.data.Count)

        Dim zestaw2 As DataItem
        ' Dim zestaw3 As Latest_valuesItem
        Dim listaDane(2)
        ' Dim listaDaneCzas(2)

        For i = 0 To zestaw.data.Count - 1
            zestaw2 = zestaw.data(i)
            'Console.WriteLine(i & "," & zestaw2.name & ", " & zestaw2.mac & ", " & zestaw2.detector_id)
            'ListView1.Items.Add(i + 1 & "," & zestaw2.name & ", " & zestaw2.mac & ", " & zestaw2.detector_id)
            'ListView1.Items.Add(zestaw.data(i).name)
            ListView2.Items.Add(New ListViewItem(New String() {i + 1, zestaw2.name, zestaw2.mac, zestaw2.detector_id}))

            For j = 0 To zestaw2.latest_values.Count - 1
                'zestaw3 = zestaw2.latest_values(j)
                'Console.WriteLine("     " & j + 1 & "," & zestaw3.timestamp & ", " & zestaw3.value)
                ' ListView1.Items.Add("     " & j + 1 & "," & zestaw3.timestamp & ", " & zestaw3.value)
                'ListView1.Items.Add("     " & zestaw.data(i).latest_values(j).timestamp & ", " & zestaw.data(i).latest_values(j).value)
                ' ListView1.Items.Add(New ListViewItem(New String() {i + 1, zestaw.data(i).name, zestaw.data(i).mac, zestaw.data(i).detector_id, zestaw.data(i).latest_values(j).timestamp, zestaw.data(i).latest_values(j).value}))
                listaDane(j) = ""
                listaDane(j) = zestaw.data(i).latest_values(j).value

            Next

            ListView1.Items.Add(New ListViewItem(New String() {i + 1, zestaw.data(i).name, zestaw.data(i).mac, zestaw.data(i).detector_id, listaDane(0), listaDane(1), listaDane(2)}))

        Next

        ListView1.Columns(4).Text = zestaw.data(0).latest_values(0).timestamp
        ListView1.Columns(5).Text = zestaw.data(0).latest_values(1).timestamp
        ListView1.Columns(6).Text = zestaw.data(0).latest_values(2).timestamp

        ListView1.Items.Add("koniec")





        'Dim DataSet As DataSet = JsonConvert.DeserializeObject(Of DataSet)(rawJSON)
        'Dim DataTable As DataTable = DataSet.Tables("data")
        'Console.WriteLine(DataTable.Rows.Count)

        'For Each row As DataRow In DataTable.Rows
        '    Console.WriteLine(row("id") & " - " + row("item"))
        'Next


    End Sub

    Private Sub NumericUpDown1_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown1.ValueChanged

        ListView1.View = NumericUpDown1.Value
        ListView2.View = NumericUpDown1.Value
    End Sub


End Class

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
