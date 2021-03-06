Imports System.IO
Imports System.Numerics
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions

Public Class Form1

    ' Add support for stylus pen, require Windows7+
    <DllImport("user32")>
    Private Shared Function GetPointerPenInfo(ByVal pointerId As Integer, ByRef penInfo As POINTER_PEN_INFO) As Boolean
    End Function

    Private Structure POINTER_PEN_INFO
        Dim pointerInfo As POINTER_INFO
        Dim penFlags As UInt32
        Dim penMask As UInt32
        Dim pressure As UInt32    ' 1024 pressure level supprted
        Dim rotation As UInt32
        Dim tiltX As Int32
        Dim tiltY As Int32
    End Structure

    Private Structure POINTER_INFO
        Dim pointerType As Int32    ' 3 - stylus pen
        Dim pointerId As UInt32
        Dim frameId As UInt32
        Dim pointerFlags As UInt32
        Dim sourceDevice As Long    ' Cautious: for x64 app, it is Int64, but for x86 app, it is Int32
        Dim hwndTarget As Long    ' Cautious: for x64 app, it is Int64, but for x86 app, it is Int32
        Dim ptPixelLocation As POINTAPI    ' screen coord
        Dim ptHimetricLocation As POINTAPI
        Dim ptPixelLocationRaw As POINTAPI
        Dim ptHimetricLocationRaw As POINTAPI
        Dim dwTime As UInt32
        Dim historyCount As UInt32
        Dim InputData As Int32
        Dim dwKeyStates As UInt32
        Dim PerformanceCount As UInt64
        Dim ButtonChangeType As Int32
    End Structure

    Private Structure POINTAPI
        Dim x As Integer
        Dim y As Integer
    End Structure

    Private Const WM_POINTERUPDATE = &H245

    ''' <summary>
    ''' default window index
    ''' <para>If multiple windows show, this indicate the index of note shown</para>
    ''' </summary>
    Public WindowIndex As Integer = 0

    Public GlobalBitmap As Bitmap = Nothing
    Public GlobalCanvas As Graphics = Nothing

    Private TopBarDragFlag As Boolean = False
    Private TopBarDragLocation As Point


    Public MouseDrawingEnable As Integer = 0

    Private MarkerDragFlag As Boolean = False
    Private MarkerDragLocation As Point
    Private MarkerPen As New SolidBrush(Color.FromArgb(151, 15, 15, 15))
    Private MarkerVelocity As Single = 0.0F
    Private MarkerLastPressure As Integer = 0

    Protected Overrides Sub WndProc(ByRef m As Message)
        MyBase.WndProc(m)

        If Me.MouseDrawingEnable = 0 Then
            If (m.Msg = WM_POINTERUPDATE) Then
                Dim ptid As UInt32 = m.WParam.ToInt32 And &HFFFF
                Dim info As POINTER_PEN_INFO
                Dim info_status As Boolean
                info_status = GetPointerPenInfo(ptid, info)
                If info_status AndAlso info.pointerInfo.pointerType = 3 Then    ' is pen input
                    Dim windowPos As Point = Me.Location
                    Dim x As Integer = info.pointerInfo.ptPixelLocation.x - windowPos.X
                    Dim y As Integer = info.pointerInfo.ptPixelLocation.y - windowPos.Y

                    If MarkerDragLocation.X > 0 AndAlso MarkerDragLocation.Y > 0 Then
                        DrawMarker_Pressure(x, y, info.pressure)
                    End If
                    MarkerDragLocation = New Point(x, y)
                    MarkerLastPressure = info.pressure
                End If

            End If
        End If

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Visible = False
        TB1.Visible = False
        If WindowIndex = 0 Then
            'load settings
            Initialize()
        End If

        'load UI
        ApplyUISetting(AppliedSetting)

        P.Visible = False

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
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
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
        'Me.P.Image = GlobalBitmap
        Me.BackgroundImage = GlobalBitmap

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
                Dim lineBrush As SolidBrush = New SolidBrush(Color.FromArgb(255, 37, 37, 37))
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
                            lineBrush.Dispose()
                            lineBrush = New SolidBrush(Color.Red)
                        ElseIf seg = "c_blue" Then
                            lineBrush.Dispose()
                            lineBrush = New SolidBrush(Color.Blue)
                        ElseIf seg = "c_green" Then
                            lineBrush.Dispose()
                            lineBrush = New SolidBrush(Color.Green)
                        ElseIf seg = "fs_xs" Then
                            tmpFont.Dispose()
                            tmpFont = New Font("Microsoft YaHei", 8)
                        ElseIf seg = "fs_s" Then
                            tmpFont.Dispose()
                            tmpFont = New Font("Microsoft YaHei", 12)
                        ElseIf seg = "fs_m" Then
                            tmpFont.Dispose()
                            tmpFont = New Font("Microsoft YaHei", 16)
                        ElseIf seg = "fs_l" Then
                            tmpFont.Dispose()
                            tmpFont = New Font("Microsoft YaHei", 20)
                        Else

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
        Dim dist As Point = newLocation - MarkerDragLocation
        Dim len As Single = Math.Sqrt(dist.X * dist.X + dist.Y * dist.Y)
        Dim moreDotCount As Integer = CInt(Math.Floor(len * 1.0F)) + 1
        If moreDotCount > 0 Then
            Dim interval As New PointF(dist.X / (moreDotCount), dist.Y / (moreDotCount))
            Dim v_interval As Single = (len - MarkerVelocity) / moreDotCount

            With GlobalCanvas
                For i = 1 To moreDotCount
                    Dim tmpCenter As New PointF(MarkerDragLocation.X + interval.X * i, MarkerDragLocation.Y + interval.Y * i)
                    Dim tmpVelocity As Single = MarkerVelocity + i * v_interval
                    Dim tmpWidth As Single = 1.0F - 0.1F * tmpVelocity
                    If tmpWidth < 0.5F Then tmpWidth = 0.5F
                    tmpWidth *= penWidth
                    .FillEllipse(MarkerPen, New RectangleF(tmpCenter.X - tmpWidth, tmpCenter.Y - tmpWidth, tmpWidth * 2, tmpWidth * 2))
                Next
            End With
            'P.Image = GlobalBitmap
            Me.BackgroundImage = Nothing    ' HACK: 强制刷新，考虑双缓冲？
            Me.BackgroundImage = GlobalBitmap
            MarkerVelocity = len
        Else
            MarkerVelocity = 0.0F
        End If

    End Sub

    Public Sub DrawMarker_Pressure(x As Integer, y As Integer, pressure As Integer)
        If pressure = 0 Then Return

        Dim newLocation As New Point(x, y)
        Dim newWidth As Single = pressure * 5.5F / 1024.0F
        Dim lastWidth As Single = MarkerLastPressure * 5.5F / 1024.0F

        Dim dist As Point = newLocation - MarkerDragLocation
        Dim len As Single = Math.Sqrt(dist.X * dist.X + dist.Y * dist.Y)
        Dim moreDotCount As Integer = CInt(Math.Floor(len * 1.0F)) + 1
        If moreDotCount > 0 Then
            Dim interval As New PointF(dist.X / (moreDotCount), dist.Y / (moreDotCount))
            Dim intervalWidth As Single = (newWidth - lastWidth) / moreDotCount

            With GlobalCanvas
                For i = 1 To moreDotCount
                    Dim tmpCenter As New PointF(MarkerDragLocation.X + interval.X * i, MarkerDragLocation.Y + interval.Y * i)
                    Dim tmpWidth As Single = lastWidth + intervalWidth * i
                    .FillEllipse(MarkerPen, New RectangleF(tmpCenter.X - tmpWidth, tmpCenter.Y - tmpWidth, tmpWidth * 2, tmpWidth * 2))
                Next
            End With
            'P.Image = GlobalBitmap
            Me.BackgroundImage = Nothing    ' HACK: 强制刷新，考虑双缓冲？
            Me.BackgroundImage = GlobalBitmap
        End If

    End Sub

    Private Sub P_MouseClick(sender As Object, e As MouseEventArgs) Handles P.MouseClick, Me.MouseClick
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
        'Me.P.Image = GlobalBitmap
        Me.BackgroundImage = GlobalBitmap

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

    Private Sub P_MouseDown(sender As Object, e As MouseEventArgs) Handles P.MouseDown, Me.MouseDown
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

    Private Sub P_MouseMove(sender As Object, e As MouseEventArgs) Handles P.MouseMove, Me.MouseMove
        If TopBarDragFlag Then
            Me.Location += e.Location - TopBarDragLocation
        ElseIf MarkerDragFlag Then
            If Me.MouseDrawingEnable Then
                DrawMarker(e.Location, 1.5F)
            End If
            MarkerDragLocation = e.Location
            End If
    End Sub

    Private Sub P_MouseUp(sender As Object, e As MouseEventArgs) Handles P.MouseUp, Me.MouseUp
        TopBarDragFlag = False
        MarkerDragFlag = False
        MarkerVelocity = 0.0F
    End Sub

    Private Sub P_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles P.MouseDoubleClick, Me.MouseDoubleClick
        Me.MouseDrawingEnable = 1 - Me.MouseDrawingEnable
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
