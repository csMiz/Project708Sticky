Imports System.IO
Imports System.Numerics
Imports System.Text.RegularExpressions

Public Class Form1
    ''' <summary>
    ''' default window index
    ''' <para>If multiple windows show, this indicate the index of note shown</para>
    ''' </summary>
    Public WindowIndex As Integer = 0

    Public GlobalBitmap As Bitmap = Nothing
    Public GlobalCanvas As Graphics = Nothing

    Private TopBarDragFlag As Boolean = False
    Private TopBarDragLocation As Point

    Private MarkerDragFlag As Boolean = False
    Private MarkerDragLocation As Point
    Private MarkerPen As New SolidBrush(Color.FromArgb(151, 15, 15, 15))

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
        Dim screenRect As Rectangle = System.Windows.Forms.Screen.GetWorkingArea(Me)
        Dim tmpLoc As New Point(screenRect.Width - style.WindowSize.Width, screenRect.Height - style.WindowSize.Height - (style.WindowSize.Height + 4) * (NotesBuffer.Count - 1 - WindowIndex))
        If tmpLoc.X < 0 Then tmpLoc.X = 0
        If tmpLoc.Y < 0 Then tmpLoc.Y = 0
        Me.Location = tmpLoc
        Me.P.Size = style.WindowSize
        Me.P.Location = New Point(0, 0)
        Me.TB1.Size = New Size(style.WindowSize.Width - 4, style.WindowSize.Height - style.TopBarHeight - 4)
        Me.TB1.Location = New Point(2, 2 + style.TopBarHeight)

        Dim g As Graphics = GlobalCanvas
        If GlobalCanvas Is Nothing Then
            GlobalBitmap = New Bitmap(style.WindowSize.Width, style.WindowSize.Height)
            g = Graphics.FromImage(GlobalBitmap)
            GlobalCanvas = g
        End If

        With g
            .Clear(style.BackGroundColor)
            .FillRectangle(New SolidBrush(style.TopBarColor), New RectangleF(0, 0, style.WindowSize.Width, style.TopBarHeight))
        End With

        'load text
        DrawNote(NotesBuffer(WindowIndex), g)

        'g.Dispose()
        'If P.Image IsNot Nothing Then P.Image.Dispose()
        Me.P.Image = GlobalBitmap

    End Sub

    ''' <summary>
    ''' draw note on the sticky window using GDI+
    ''' </summary>
    ''' <param name="note"></param>
    ''' <param name="g"></param>
    Public Sub DrawNote(note As String, g As Graphics)

        Dim tmpFont As New Font("Microsoft YaHei", 16)
        Dim tmpFont2 As New Font("Arial Black", 12)

        'draw topbar
        Dim brushTopBar As New SolidBrush(Color.FromArgb(255, 175, 170, 100))
        Dim penTopBar As New Pen(brushTopBar.Color)
        With g
            Dim titleString As String = "NOTE"
            .DrawString(titleString, tmpFont2, brushTopBar, New PointF(6, 6))
            .FillEllipse(brushTopBar, New RectangleF(Me.Width - 24, 12, 12, 12))
        End With
        brushTopBar.Dispose()
        penTopBar.Dispose()

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
                            lineBrush = New SolidBrush(Color.Red)
                        ElseIf seg = "c_blue" Then
                            lineBrush = New SolidBrush(Color.Blue)
                        ElseIf seg = "c_green" Then
                            lineBrush = New SolidBrush(Color.Green)
                        Else
                            lineBrush = New SolidBrush(Color.FromArgb(255, 37, 37, 37))
                        End If

                    Next

                Else
                    lineBrush = New SolidBrush(Color.FromArgb(255, 37, 37, 37))
                    lineContent = line
                End If

                With g
                    .DrawString(lineContent, tmpFont, lineBrush, New PointF(3, 3 + AppliedSetting.TopBarHeight + 24 * i))
                End With
                lineBrush.Dispose()
            Next
        End If

        tmpFont.Dispose()
        tmpFont2.Dispose()

    End Sub

    Public Sub DrawMarker(newLocation As Point, Optional penWidth As Single = 5.5)
        'Dim dist As Point = newLocation - MarkerDragLocation
        'Dim len As Single = Math.Sqrt(dist.X * dist.X + dist.Y * dist.Y)

        With GlobalCanvas
            .FillEllipse(MarkerPen, New RectangleF(newLocation.X - penWidth, newLocation.Y - penWidth, penWidth * 2, penWidth * 2))
        End With
        P.Image = GlobalBitmap
    End Sub

    Private Sub P_MouseClick(sender As Object, e As MouseEventArgs) Handles P.MouseClick
        If e.Button = MouseButtons.Right Then
            If TB1.Visible Then
                ApplyTextBox()
            Else
                PullTextBox()
            End If
            TB1.Enabled = Not TB1.Enabled
            TB1.Visible = Not TB1.Visible

        ElseIf e.Button = MouseButtons.Left Then
            If e.X > Me.Width - 36 AndAlso e.Y < AppliedSetting.TopBarHeight Then
                Me.Close()
            End If
        End If
    End Sub

    Public Sub ApplyTextBox()
        NotesBuffer(WindowIndex) = TB1.Text
        SaveAllNotes()

        Dim g As Graphics = GlobalCanvas
        With g
            .Clear(AppliedSetting.BackGroundColor)
            .FillRectangle(New SolidBrush(AppliedSetting.TopBarColor), New RectangleF(0, 0, AppliedSetting.WindowSize.Width, AppliedSetting.TopBarHeight))
        End With
        DrawNote(NotesBuffer(WindowIndex), g)
        'g.Dispose()
        'If P.Image IsNot Nothing Then P.Image.Dispose()
        Me.P.Image = GlobalBitmap

    End Sub

    Public Sub PullTextBox()
        TB1.Text = NotesBuffer(WindowIndex)
    End Sub

    Private Sub TB1_KeyDown(sender As Object, e As KeyEventArgs) Handles TB1.KeyDown
        If e.KeyCode = Keys.Escape Then
            ApplyTextBox()
            TB1.Enabled = False
            TB1.Visible = False
        End If
    End Sub

    Private Sub P_MouseDown(sender As Object, e As MouseEventArgs) Handles P.MouseDown
        If e.Button = MouseButtons.Left Then
            If e.Y < AppliedSetting.TopBarHeight Then
                TopBarDragLocation = e.Location
                TopBarDragFlag = True
            Else
                MarkerDragLocation = e.Location
                MarkerDragFlag = True
            End If
        End If
    End Sub

    Private Sub P_MouseMove(sender As Object, e As MouseEventArgs) Handles P.MouseMove
        If TopBarDragFlag Then
            Me.Location += e.Location - TopBarDragLocation
        ElseIf MarkerDragFlag Then
            DrawMarker(e.Location, 1.5)
            MarkerDragLocation = e.Location
        End If
    End Sub

    Private Sub P_MouseUp(sender As Object, e As MouseEventArgs) Handles P.MouseUp
        TopBarDragFlag = False
        MarkerDragFlag = False
    End Sub

    Private Sub Form1_KeyDown(sender As Object, e As KeyEventArgs) Handles Me.KeyDown, P.KeyDown, TB1.KeyDown
        If TB1.Enabled Then Return
        If e.Control AndAlso e.KeyCode = Keys.N Then
            ' new window
            NotesBuffer.Add("")
            IndexGeneratorFlag += 1
            Dim subForm As New Form1() With {.WindowIndex = IndexGeneratorFlag - 1}
            subForm.Show()
            subForm.Location = New Point(0, 0)
        End If
    End Sub

End Class
