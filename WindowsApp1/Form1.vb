﻿Imports System.Net
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports Newtonsoft.Json
Imports System.IO

Imports MySql.Data.MySqlClient


'zapis aby przywrocic poprawną wersje na 982


Public Class Form1

    Public status_logu As Integer = 1

    Dim server As String = "eobx-s-00224"

    Public myConnectionString As String = "server=" & server & ";" _
            & "uid=test;" _
            & "pwd=test;" _
            & "database=mm"

    Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim dtListaObiektow As DataTable = New DataTable()
        Dim dtDaneDoZapisu As DataTable = New DataTable()
        Dim dtBrakDanych As DataTable = New DataTable()
        Dim row As DataRow
        Dim row2 As DataRow
        Dim row3 As DataRow

        For i = 1 To 5
            'test_str = Mid(s, 2, Len(s) - 2)
            dtListaObiektow.Columns.Add(New DataColumn(i.ToString, GetType(String)))
            dtDaneDoZapisu.Columns.Add(New DataColumn(i.ToString, GetType(String)))
            dtBrakDanych.Columns.Add(New DataColumn(i.ToString, GetType(String)))
        Next

        Dim url As String = "https://platforma.enspirion.pl/services/energa/?pass=mFffGMVLcDNQRdeubJ3qFH2tvpb2zA2KDxFx4epD9XSc3BF2GB"
        Dim webClient As New WebClient
        Dim rawJSON As String
        Dim zestaw As Root

        zestaw = Nothing


        With DataGridView1
            '.AutoGenerateColumns = True
            .ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
            .AutoResizeColumns()
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
            .Dock = DockStyle.Fill
        End With

        With DataGridView2
            '.AutoGenerateColumns = True
            .ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
            .AutoResizeColumns()
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
            .Dock = DockStyle.Fill
        End With

        With DataGridView3
            '.AutoGenerateColumns = True
            .ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
            .DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
            .AutoResizeColumns()
            .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
            .Dock = DockStyle.Fill
        End With


        'Dim stempel_czasowy As DateTime = Now()
        Try
            Using WC As New Net.WebClient
                rawJSON = webClient.DownloadString(url)

                If rawJSON.Length > 0 Then
                    zestaw = JsonConvert.DeserializeObject(Of Root)(rawJSON)
                    WriteToFile2("połączenie ok")
                Else
                    WriteToFile2("bład połączenia")
                End If
            End Using

        Catch ex As Exception
            WriteToFile2(ex.InnerException.Message)
        End Try

        'WriteToFile2("pobieranie i parsowanie danych trwało:" & Format(stempel_czasowy - Now(), "yyyy-MM-dd HH:mm:ss"))
        WriteToFile2(zestaw.Value_interval)
        WriteToFile2(zestaw.Value_unit)
        WriteToFile2(zestaw.Timestamp_timezone)
        WriteToFile2("licznik pomiarów:" & zestaw.Data.Count)

        Dim zestaw2 As DataItem
        'Dim zestaw3 As Latest_valuesItem
        'Dim listaDane(2)
        'Dim listaDaneCzas(2)
        Dim rowArrayListaObiektow As Object() = New Object(4) {}
        Dim rowArrayDane As Object() = New Object(4) {}
        Dim rowArrayBrakDanych As Object() = New Object(4) {}

        Dim ID_obiekt_BD As Integer

        For i = 0 To zestaw.Data.Count - 1
            zestaw2 = zestaw.Data(i)
            ID_obiekt_BD = Nothing
            ID_obiekt_BD = Pobierz_dane_id(myConnectionString, zestaw2.Mac & "-" & zestaw2.Detector_id)
            If ID_obiekt_BD > 0 Then
                ID_obiekt_BD = ID_obiekt_BD
            End If

            rowArrayListaObiektow(0) = i
            rowArrayListaObiektow(1) = zestaw2.Name
            rowArrayListaObiektow(2) = zestaw2.Mac
            rowArrayListaObiektow(3) = zestaw2.Detector_id
            rowArrayListaObiektow(4) = ID_obiekt_BD
            row = dtListaObiektow.NewRow()
            row.ItemArray = rowArrayListaObiektow
            dtListaObiektow.Rows.Add(row)

            For j = 0 To zestaw2.Latest_values.Count - 1
                If zestaw2.Latest_values.Count <> 3 Then
                    WriteToFile2(" niepełne dane dla: " & zestaw2.Name & ", " & zestaw2.Mac & "-" & zestaw2.Detector_id & ", ilosc danych=" & zestaw2.Latest_values.Count)
                End If

                'gdy są dane
                If zestaw2.Latest_values.Count > 0 Then
                    rowArrayDane(0) = zestaw.Data(i).Name
                    rowArrayDane(1) = zestaw.Data(i).Mac
                    rowArrayDane(2) = zestaw.Data(i).Detector_id
                    rowArrayDane(3) = zestaw.Data(i).Latest_values(j).Value
                    rowArrayDane(4) = zestaw.Data(i).Latest_values(j).Timestamp
                    row2 = dtDaneDoZapisu.NewRow()
                    row2.ItemArray = rowArrayDane
                    dtDaneDoZapisu.Rows.Add(row2)
                Else
                    rowArrayBrakDanych(0) = zestaw.Data(i).Name
                    rowArrayBrakDanych(1) = zestaw.Data(i).Mac
                    rowArrayBrakDanych(2) = zestaw.Data(i).Detector_id
                    row3 = dtDaneDoZapisu.NewRow()
                    row3.ItemArray = rowArrayBrakDanych
                    dtBrakDanych.Rows.Add(row3)
                End If
            Next




        Next

        'nie uruchamia  się przy trybie bez Form1
        'DataGridView1.DataSource = dtListaObiektow
        'DataGridView2.DataSource = dtDaneDoZapisu
        'DataGridView3.DataSource = dtBrakDanych

        Zapis_danych_do_bazy(dtDaneDoZapisu)



    End Sub





    Function Zapis_danych_do_bazy(dt As DataTable) As Integer

        Dim conn As New MySql.Data.MySqlClient.MySqlConnection(myConnectionString)
        Dim myInsertQuery As String
        Dim FormulaID As Integer
        Dim DataTime As Date
        Dim Volume As Decimal
        Dim Status As Long
        Dim cmd As New MySqlCommand
        Dim myAdapter As New MySqlDataAdapter

        Try
            'zapis danych 
            Dim ilosc_danych As Long
            Dim i As Integer

            ilosc_danych = dt.Rows.Count
            i = 0
            Status = 1

            'zapis do bazy
            For Each row1 As DataRow In dt.Rows

                FormulaID = Pobierz_dane_id(myConnectionString, row1(1) & "-" & row1(2))

                If FormulaID > 0 Then

                    Volume = row1(3)
                    DataTime = CDate(row1(4).ToString).ToString("yyyy-MM-dd HH:mm:ss")

                    cmd.Connection = conn
                    myAdapter.SelectCommand = cmd
                    conn.Open()
                    myInsertQuery = String.Format("REPLACE into energy1 (`FormulaID`, `DataTime`, `Volume`,`Status`,`DataCzasZapisu`) values('" & FormulaID & "', '" & Format(DataTime, "yyyy-MM-dd HH:mm:ss") & "', '" & Replace(Volume.ToString, ",", ".") & "', '" & Status & "', '" & Format(Now(), "yyyy-MM-dd HH:mm:ss") & "')")

                    cmd.CommandText = myInsertQuery
                    cmd.ExecuteNonQuery()

                    'zamkniecie polaczenia
                    conn.Close()

                    'WriteToFile2(FormulaID & ", " & Format(DataTime, "yyyy-MM-dd HH:mm:ss") & ", " & Replace(Volume.ToString, ",", "."))
                Else
                    WriteToFile2("Nie znalezniono ID formuły dla: " & row1(0))
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

    Private Function Pobierz_dane_id(connString As String, mac As String) As Integer
        Dim sqlQuery As String = "SELECT id, nazwa_z_pliku, mac, ID_ERGH FROM id_formuly_1 WHERE mac = @uname"
        Using sqlConn As New MySqlConnection(connString)
            Using sqlComm As New MySqlCommand()
                With sqlComm
                    .Connection = sqlConn
                    .CommandText = sqlQuery
                    .CommandType = CommandType.Text
                    .Parameters.AddWithValue("@uname", mac)
                End With
                Try
                    sqlConn.Open()
                    Dim sqlReader As MySqlDataReader = sqlComm.ExecuteReader()
                    sqlReader.Read()
                    If sqlReader.HasRows Then
                        Return CInt(sqlReader("id"))
                    Else

                        Return 0

                    End If
                Catch ex As MySqlException
                    Return 0
                    WriteToFile2("Błąd w funkcji pobierania id dla nazwy formuły o numerze: " & ex.ToString(), 1)
                Finally
                    sqlConn.Close()
                End Try
            End Using
        End Using
    End Function

    Public Sub WriteToFile(text As String, Optional i As Integer = 1)
        'Dim path As String = "C:\temp\" & DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").ToString & "_ServiceLog.txt"if 
        'Dim text2 As String = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").ToString
        Dim path As String = "C:\temp\" + DateTime.Now.ToString("yyyyMMdd").ToString + "_Podczyt_Miernik_mocy_1_min_log.txt"
        text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").ToString & " " & text
        Using writer As New StreamWriter(path, True)
            writer.WriteLine(text)
            writer.Close()
        End Using

        ListView1.Items.Add(New ListViewItem(text))
        ListView1.Items(ListView1.Items.Count - 1).EnsureVisible()

    End Sub

    Public Sub WriteToFile2(text As String, Optional poziom As Integer = 1)
        'Dim path As String = "C:\temp\" & DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").ToString & "_ServiceLog.txt"if 
        'Dim text2 As String = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss").ToString

        If status_logu >= poziom Then

            Dim path As String = "C:\temp\" + DateTime.Now.ToString("yyyyMMdd").ToString + "_Podczyt_Miernik_mocy_1_min_log.txt"
            text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").ToString & " " & text
            Using writer As New StreamWriter(path, True)
                writer.WriteLine(text)
                writer.Close()
            End Using

        End If

    End Sub

End Class

Public Class Latest_valuesItem
    Public Property Timestamp As String
    Public Property Value As Double
End Class

Public Class DataItem
    Public Property Name As String
    Public Property Mac As String
    Public Property Detector_id As Integer
    Public Property Latest_values As List(Of Latest_valuesItem)
End Class

Public Class Root
    Public Property Value_interval As String
    Public Property Value_unit As String
    Public Property Timestamp_timezone As String
    Public Property Data As List(Of DataItem)
End Class

