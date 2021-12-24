' -----------------------------------------
' Copyright (c) 2019 All Rights Reserved.
' 
' Filename: ST_Setting
' Author: Miz
' Date: 2019/6/18 13:53:46
' -----------------------------------------

Imports System.IO
Imports System.Text.RegularExpressions
''' <summary>
''' An instance of a style-setting class
''' </summary>
Public Class ST_Setting
    ''' <summary>
    ''' Style name
    ''' </summary>
    Public SettingPatternName As String
    ''' <summary>
    ''' The window's size
    ''' <para>If the window is too small, contents might be unavailable</para>
    ''' </summary>
    Public WindowSize As Size

    Public TopBarHeight As Integer
    ''' <summary>
    ''' uses GDI+ to fill the whole background
    ''' </summary>
    Public BackGroundColor As Color

    Public ReadOnly Property BackGroundDarkColor As Color
        Get
            Return Color.FromArgb(255, BackGroundColor.R * 0.9, BackGroundColor.G * 0.9, BackGroundColor.B * 0.9)
        End Get
    End Property


    ''' <summary>
    ''' create a new style
    ''' <para>fill in default settings</para>
    ''' </summary>
    Public Sub New(Optional defaultSettings = True)
        If defaultSettings Then
            Me.WindowSize = New Size(300, 336)
            Me.TopBarHeight = 36
            Me.BackGroundColor = Color.WhiteSmoke
        End If
    End Sub

    Public Function LoadFromFile(fileStream As FileStream) As Integer
        Dim fileContent As String = ""
        Using sr As StreamReader = New StreamReader(fileStream)
            fileContent = sr.ReadToEnd
        End Using
        Dim lines() As String = Regex.Split(fileContent, vbCrLf)
        For Each line As String In lines
            Dim args() As String = Regex.Split(line, ":")
            Select Case args(0)
                Case "sx"
                    Me.WindowSize.Width = CInt(args(1))
                Case "sy"
                    Me.WindowSize.Height = CInt(args(1))
                Case "bgc"
                    Dim rgb() As String = Regex.Split(args(1), ",")
                    Me.BackGroundColor = Color.FromArgb(255, CInt(rgb(0)), CInt(rgb(1)), CInt(rgb(2)))
                Case "tbh"
                    Me.TopBarHeight = CInt(args(1))

            End Select
        Next
        Return 1
    End Function

    Public Function SaveToFile(fileStream As FileStream) As Integer
        Dim line_sx As String = "sx:"
        line_sx = line_sx & Me.WindowSize.Width
        Dim line_sy As String = "sy:"
        line_sy = line_sy & Me.WindowSize.Height
		dim line_bgc as String = "bgc:"
		line_bgc = line_bgc & me.BackGroundColor.R & "," & me.BackGroundColor.G & "," & me.BackGroundColor.B
		dim line_tbh as String = "tbh:"
        line_tbh = line_tbh & Me.TopBarHeight

        Using sw As StreamWriter = New StreamWriter(fileStream)
            sw.WriteLine(line_sx)
            sw.WriteLine(line_sy)
			sw.WriteLine(line_bgc)
			sw.WriteLine(line_tbh)

        End Using
		return 1
    End Function

End Class
