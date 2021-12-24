Imports System.IO
Imports System.Text.RegularExpressions

Public Class Form1
    ''' <summary>
    ''' default window index
    ''' <para>If multiple windows show, this indicate the index of note shown</para>
    ''' </summary>
    Public WindowIndex As Integer = 0

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Visible = False
        TB1.Visible = False
        If WindowIndex = 0 Then
            'load settings
            Initialize()
        End If

        'load UI
        ApplyUISetting(AppliedSetting)

        Me.Visible = True

    End Sub


    Public Sub ApplyUISetting(style As ST_Setting)

        Me.ClientSize = style.WindowSize
        Me.P.Size = style.WindowSize
        Me.P.Location = New Point(0, 0)
        Me.TB1.Size = New Size(style.WindowSize.Width - 4, style.WindowSize.Height - style.TopBarHeight - 4)
        Me.TB1.Location = New Point(2, 2 + style.TopBarHeight)

        Dim bitmap As New Bitmap(style.WindowSize.Width, style.WindowSize.Height)
        Dim g As Graphics = Graphics.FromImage(bitmap)
        With g
            .Clear(style.BackGroundColor)
            .FillRectangle(New SolidBrush(style.BackGroundDarkColor), New RectangleF(0, 0, style.WindowSize.Width, style.TopBarHeight))
        End With

        'load text
        DrawNote(NotesBuffer(WindowIndex), g)

        g.Dispose()
        If P.Image IsNot Nothing Then P.Image.Dispose()
        Me.P.Image = bitmap

    End Sub

    ''' <summary>
    ''' draw note on the sticky window using GDI+
    ''' </summary>
    ''' <param name="note"></param>
    ''' <param name="g"></param>
    Public Sub DrawNote(note As String, g As Graphics)

        Dim tmpFont As New Font("Microsoft YaHei", 16)
        'parse note
        Dim lines As String() = Regex.Split(note, vbCrLf)
        If lines.Count > 0 Then
            For i = 0 To lines.Count - 1
                Dim line As String = lines(i)
                Dim lineBrush As SolidBrush = Nothing
                Dim lineContent As String = vbNullString
                If line.Contains("::") Then
                    Dim args As String() = Regex.Split(line, "::")
                    lineContent = args(1)
                    Dim argList As New List(Of String)
                    If args(0).Contains(",") Then
                        Dim args2 As String() = Regex.Split(args(0), ",")
                        argList.AddRange(args2)
                    Else
                        argList.Add(args(0))
                    End If

                    For Each seg As String In argList
                        If seg = "c_red" Then
                            lineBrush = Brushes.Red
                        ElseIf seg = "c_blue" Then
                            lineBrush = Brushes.Blue
                        ElseIf seg = "c_green" Then
                            lineBrush = Brushes.Green
                        Else
                            lineBrush = Brushes.Black
                        End If

                    Next

                Else
                    lineBrush = Brushes.Black
                    lineContent = line
                End If

                With g
                    .DrawString(lineContent, tmpFont, lineBrush, New PointF(3, 3 + 22 * i))
                End With
            Next
        End If

        'draw


        tmpFont.Dispose()

    End Sub

    Private Sub P_MouseClick(sender As Object, e As MouseEventArgs) Handles P.MouseClick
        If e.Button = MouseButtons.Right Then
            If TB1.Visible Then
                ApplyTextBox()
            Else
                PullTextBox()
            End If
            TB1.Visible = Not TB1.Visible
        End If
    End Sub

    Public Sub ApplyTextBox()
        NotesBuffer(WindowIndex) = TB1.Text
        SaveAllNotes()

        Dim bitmap As New Bitmap(AppliedSetting.WindowSize.Width, AppliedSetting.WindowSize.Height)
        Dim g As Graphics = Graphics.FromImage(bitmap)
        With g
            .Clear(AppliedSetting.BackGroundColor)
            .FillRectangle(New SolidBrush(AppliedSetting.BackGroundDarkColor), New RectangleF(0, 0, AppliedSetting.WindowSize.Width, AppliedSetting.TopBarHeight))
        End With
        DrawNote(NotesBuffer(WindowIndex), g)
        g.Dispose()
        If P.Image IsNot Nothing Then P.Image.Dispose()
        Me.P.Image = bitmap

    End Sub

    Public Sub PullTextBox()
        TB1.Text = NotesBuffer(WindowIndex)
    End Sub

    Private Sub TB1_KeyDown(sender As Object, e As KeyEventArgs) Handles TB1.KeyDown
        If e.KeyCode = Keys.Escape Then
            ApplyTextBox()
            TB1.Visible = False
        End If
    End Sub
End Class
