﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。  
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.P = New System.Windows.Forms.PictureBox()
        Me.TB1 = New System.Windows.Forms.TextBox()
        CType(Me.P, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'P
        '
        Me.P.Location = New System.Drawing.Point(120, 53)
        Me.P.Name = "P"
        Me.P.Size = New System.Drawing.Size(593, 329)
        Me.P.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.P.TabIndex = 0
        Me.P.TabStop = False
        '
        'TB1
        '
        Me.TB1.AcceptsReturn = True
        Me.TB1.AcceptsTab = True
        Me.TB1.Location = New System.Drawing.Point(230, 168)
        Me.TB1.Multiline = True
        Me.TB1.Name = "TB1"
        Me.TB1.Size = New System.Drawing.Size(414, 122)
        Me.TB1.TabIndex = 1
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(12.0!, 24.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.TB1)
        Me.Controls.Add(Me.P)
        Me.Name = "Form1"
        Me.Text = "Form1"
        CType(Me.P, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents P As PictureBox
    Friend WithEvents TB1 As TextBox
End Class
