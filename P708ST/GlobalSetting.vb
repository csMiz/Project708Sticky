Imports System.IO
Imports System.Text.RegularExpressions

Module GlobalSetting

    Public AppliedSetting As ST_Setting = Nothing

    Public NotesBuffer As List(Of String) = Nothing
    Public IndexGeneratorFlag As Integer = 0


    ''' <summary>
    ''' initialize the program and deal with existing notes which need to be displaced.
    ''' </summary>
    Public Sub Initialize()
        'initialize variables
        'load applied settings
        Dim appliedSettingFile As FileStream = Nothing
        Dim createNewFileFlag As Boolean = False
        Try
            appliedSettingFile = New FileStream(Application.StartupPath & "\as.log", FileMode.Open)
        Catch ex As Exception
            If ex.GetType = GetType(FileNotFoundException) Then
                createNewFileFlag = True
                'create default setting file
                appliedSettingFile = New FileStream(Application.StartupPath & "\as.log", FileMode.Create)
            End If
        End Try
        If createNewFileFlag Then
            AppliedSetting = New ST_Setting
            AppliedSetting.SaveToFile(appliedSettingFile)
        Else
            AppliedSetting = New ST_Setting(False)
            AppliedSetting.LoadFromFile(appliedSettingFile)
        End If
        appliedSettingFile.Close()
        appliedSettingFile.Dispose()

        'read notes from local files
        NotesBuffer = GetAllSavedNotes()
        IndexGeneratorFlag = NotesBuffer.Count
        If NotesBuffer.Count > 1 Then
            For i = 1 To NotesBuffer.Count - 1
                Dim subForm As New Form1 With {.WindowIndex = i}
                subForm.Show()
            Next
        End If


    End Sub

    ''' <summary>
    ''' get all saved notes from default file path
    ''' </summary>
    Public Function GetAllSavedNotes() As List(Of String)
        Dim startArg As String = Microsoft.VisualBasic.Command
        If startArg = "-new" Then
            Dim r As New List(Of String)
            r.Add("右键此处添加便签")
            Return r
        ElseIf startArg <> "" Then
            MsgBox(startArg)
        End If

        Dim result As New List(Of String)
        Dim plainNotes As String = vbNullString
        Dim noteFile As FileStream = Nothing
        Dim createNewFileFlag As Boolean = False
        Try
            noteFile = New FileStream(Application.StartupPath & "\note.txt", FileMode.Open)
        Catch ex As Exception
            If ex.GetType = GetType(FileNotFoundException) Then
                createNewFileFlag = True
                'create default setting file
                noteFile = New FileStream(Application.StartupPath & "\note.txt", FileMode.Create)
            End If
        End Try
        If createNewFileFlag Then
            plainNotes = ""
        Else
            Using sr As New StreamReader(noteFile)
                plainNotes = sr.ReadToEnd
            End Using
        End If
        noteFile.Close()
        noteFile.Dispose()

        'divide notes
        If Not (plainNotes = "") Then
            Dim seg As String() = Regex.Split(plainNotes, "<note_sep>")
            result.AddRange(seg)
        End If

        If result.Count = 0 Then
            End
        End If

        For i = result.Count - 1 To 0 Step -1
            If result(i).Length = 0 Then
                result.RemoveAt(i)
            End If
        Next

        Return result
    End Function


    ''' <summary>
    ''' save all notes to default file path
    ''' </summary>
    Public Sub SaveAllNotes()
        Dim saveText As String = ""
        If NotesBuffer.Count Then
            If NotesBuffer.Count > 1 Then
                For i = 0 To NotesBuffer.Count - 1
                    If NotesBuffer(i).Length > 0 Then
                        saveText = saveText & NotesBuffer(i) & "<note_sep>"
                    End If
                Next
            Else
                saveText = NotesBuffer.First
            End If

        End If

        Dim noteFile As FileStream = New FileStream(Application.StartupPath & "\note.txt", FileMode.Truncate)
        noteFile.Position = 0
        Using sw As New StreamWriter(noteFile)
            sw.Write(saveText)
        End Using
        noteFile.Close()
        noteFile.Dispose()

    End Sub

End Module
