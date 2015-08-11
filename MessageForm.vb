Imports System.Threading

Public Class MessageForm
    'hi  *999
    Dim tmWaiting As Timers.Timer = New Timers.Timer
    Dim Switch As Boolean = False

    Private Sub MessageForm_Load(sender As Object, e As EventArgs) Handles Me.Load

        Me.TopMost = True
        tmWaiting = New Timers.Timer
        tmWaiting.AutoReset = False
        AddHandler tmWaiting.Elapsed, AddressOf tmWaiting_Tick

        lblMsg.Text = "Processing"
        tmWaiting.Interval = 500
        Me.Visible = False

    End Sub

    Public Function iniProcess(ByVal total As Integer, ByVal pstep As Integer)

        pb_Msgform.Maximum = total
        pb_Msgform.Step = pstep

        Return 1

    End Function

    Public Function displayMsg(ByVal msg As String)



        lblMsg.Text = msg & "..."
        'Switch = True

        'tmWaiting.Enabled = True
        Return 1


    End Function

    Public Function runstep()


        'tmWait.Start()
        'While True
        '    '  MsgBox("A")
        '    If asswitch = True Then
        '        Exit While
        '    End If
        'End While
        pb_Msgform.PerformStep()
        tmWaiting.Enabled = True

        While True
            If Switch Then
                'MsgBox("i")
                Switch = False
                Exit While
            End If
        End While
        MsgBox("HI")

        If pb_Msgform.Value = pb_Msgform.Maximum Then
            MsgBox("finish")
            Me.Visible = False
            pb_Msgform.Value = 0
        End If

        Return 0
    End Function

    Private Sub tmWaiting_Tick()

        Switch = True
        tmWaiting.Enabled = False
        ' MsgBox("I run")
        'If Switch Then

        '    Me.Visible = False
        '    tmWaiting.Enabled = False

        'End If

    End Sub

    Private Sub MessageForm_Shown(sender As Object, e As EventArgs) Handles Me.Shown

    End Sub
    Public Sub Exe_Show()
        Me.Visible = True

    End Sub
End Class
