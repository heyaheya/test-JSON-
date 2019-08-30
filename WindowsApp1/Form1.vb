Imports System.Net
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports Newtonsoft.Json
Imports System.IO

Imports MySql.Data.MySqlClient



Public Class Form1

    Public status_logu As Integer = 1

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

                If zestaw2.latest_values.Count > 0 Then

                End If

            Next

            ListView1.Items.Add(New ListViewItem(New String() {i + 1, zestaw.data(i).name, zestaw.data(i).mac, zestaw.data(i).detector_id, listaDane(0), listaDane(1), listaDane(2)}))

        Next

        ListView1.Columns(4).Text = zestaw.data(0).latest_values(0).timestamp
        ListView1.Columns(5).Text = zestaw.data(0).latest_values(1).timestamp
        ListView1.Columns(6).Text = zestaw.data(0).latest_values(2).timestamp

        ListView1.Items.Add("Koniec parsowania.")
        ListView1.Items.Add("Rozpoczęcie zapisu na bazę")

        Dim dt As DataTable = New DataTable()
        ' Zapis_danych_do_bazy(dt)


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





    Function Zapis_danych_do_bazy(dt As DataTable) As Integer
        'połączenie z bazą 
        'Dim server As String = "127.0.0.1"
        Dim server As String = "eobx-s-00224"
        'Dim server As String = "eobx-n-00982"


        '--------------
        'do zrobienia f-cja przełączająca BD
        '----------------

        Dim myConnectionString As String = "server=" & server & ";" _
                & "uid=test;" _
                & "pwd=test;" _
                & "database=mm"


        Dim conn As New MySql.Data.MySqlClient.MySqlConnection(myConnectionString)
        Dim myInsertQuery As String
        Dim FormulaID As Integer
        Dim DataTime As Date
        Dim Volume As Long
        Dim Status As Long
        Dim cmd As New MySqlCommand
        Dim myAdapter As New MySqlDataAdapter

        Try
            WriteToFile2("Połączono z bazą " & server)
            'zapis danych 
            Dim row As DataRow
            row = dt.Rows(1)
            'wyliczenie ilosći danych na podst. daty 
            Dim ilosc_danych As Long

            Dim d As DateTime
            d = row(3)
            ilosc_danych = d.Hour * 12 + (Math.Ceiling(d.Minute / 5)) - 1

            Status = 1
            'zapis do bazy
            For Each row1 As DataRow In dt.Rows
                FormulaID = Poierz_dane_id(myConnectionString, row1(0))
                If FormulaID > 0 Then
                    WriteToFile2("Pobrano id=" & FormulaID & " dla formuly " & row1(0), 2)
                    For i As Integer = 6 To ilosc_danych + 5
                        Volume = row1(i)
                        DataTime = CDate(dt.Columns(i).ToString).ToString("yyyy-MM-dd HH:mm:ss")

                        WriteToFile2("Volume=" & Volume.ToString, 2)
                        WriteToFile2("DataTime=" & DataTime.ToString, 2)

                        WriteToFile2("DataTime 2=" & Format(CDate(dt.Columns(i).ToString), "yyyy-MM-dd HH:mm:ss"), 2)

                        cmd.Connection = conn
                        myAdapter.SelectCommand = cmd
                        conn.Open()
                        myInsertQuery = String.Format("REPLACE into energy5 (`FormulaID`, `DataTime`, `Volume`,`Status`,`DataCzasZapisu`) values('" & FormulaID & "', '" & Format(DataTime, "yyyy-MM-dd HH:mm:ss") & "', '" & Volume & "', '" & Status & "', '" & Format(Now(), "yyyy-MM-dd HH:mm:ss") & "')")

                        WriteToFile2("sql= " & myInsertQuery, 2)

                        cmd.CommandText = myInsertQuery
                        cmd.ExecuteNonQuery()

                        '---------------------
                        'do usuniecia
                        'If FormulaID = 2 Then
                        '	DoKonsoli("FormulaID ID:" & FormulaID & ", wolumen:" & Volume & ", dataczas:" & DataTime, Color.Purple)
                        'End If
                        '---------------------

                        'zamkniecie polaczenia
                        conn.Close()
                    Next

                    WriteToFile2("Zapisano dane dla ID=" & FormulaID & " Nazwa formuly " & row1(0), 2)
                    WriteToFile2("Ostatnie zapisane dane: czas(" & DataTime & ")	wartosc(" & Volume & ")	status(" & Status & ")", 2)
                Else
                    WriteToFile2("Nie znalezniono ID formuły dla: " & row1(0), 1)
                    'GdyNieznalazlFormuly(row1(0))
                End If
            Next
            Return ilosc_danych
        Catch ex As MySql.Data.MySqlClient.MySqlException
            Select Case ex.Number
                Case 0
                    WriteToFile2("Brak połączenia z serwerem bazodanowym.", 1)
                Case 1045
                    WriteToFile2("Błąd logowania. Błędny login lub hasło.", 1)
                Case Else
                    WriteToFile2("Brak połaczenia z bazą. Błąd: " & ex.Message, 1)
            End Select
            Return 0
        End Try
    End Function

    Private Function Poierz_dane_id(connString As String, nazwa As String) As Integer
        'Dim connString As String = "server=REMOVED;Port=REMOVED; user id=REMOVED; password=REMOVED; database=REMOVED"
        Dim sqlQuery As String = "SELECT id, nazwa_z_pliku FROM id_formuly WHERE nazwa_z_pliku = @uname"


        Using sqlConn As New MySqlConnection(connString)
            Using sqlComm As New MySqlCommand()
                With sqlComm
                    .Connection = sqlConn
                    .CommandText = sqlQuery
                    .CommandType = CommandType.Text
                    .Parameters.AddWithValue("@uname", nazwa)
                End With
                Try
                    sqlConn.Open()
                    Dim sqlReader As MySqlDataReader = sqlComm.ExecuteReader()
                    sqlReader.Read()
                    'While sqlReader.Read()
                    'Label1.Text = sqlReader("id").ToString()
                    'Label2.Text = sqlReader("nazwa_z_pliku").ToString()
                    'End While
                    Return CInt(sqlReader("id"))
                Catch ex As MySqlException
                    Return 0
                    WriteToFile2("Błąd w funkcji pobierania id dla nazwy formuły o numerze: " & ex.ToString(), 1)

                Finally
                    sqlConn.Close()
                End Try
            End Using
        End Using
    End Function



    Public Sub WriteToFile2(text As String, Optional poziom As Integer = 1)
        'Dim path As String = "C:\temp\" & DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").ToString & "_ServiceLog.txt"if 
        'Dim text2 As String = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").ToString

        'If Form1.status_logu >= poziom Then

        '    Dim path As String = "C:\temp\" + DateTime.Now.ToString("yyyyMMdd").ToString + "_Podczyt_Miernik_mocy_log.txt"
        '    text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").ToString & " " & text
        '    Using writer As New StreamWriter(path, True)
        '        writer.WriteLine(text)
        '        writer.Close()
        '    End Using

        'End If

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

