Public Class Setting

    Dim INI_Path As String
    Dim rswitch As Boolean = True

    Dim ClientName As String
    Dim INI_TrackSoftwaveName As String
    Dim INI_MailSrvIP As String
    Dim INI_ClientName As String
    Dim INI_SrvIp As String
    Dim INI_TrackOption As String

    Private Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" ( _
        ByVal lpApplicationName As String, _
        ByVal lpKeyName As String, ByVal lpString As String, _
        ByVal lpFilename As String) As Integer

    Private Sub Setting_Disposed(sender As Object, e As EventArgs) Handles Me.Disposed

        'MsgBox("Dispose Over")

    End Sub

    Private Sub Setting_Load(sender As Object, e As EventArgs) Handles Me.Load

        INI_Path = Main.INI_Path
        INI_ClientName = Main.INI_ClientName
        INI_TrackSoftwaveName = Main.INI_TrackedSoftwaveName
        INI_MailSrvIP = Main.INI_MailSrv_IP
        INI_SrvIp = Main.INI_SrvIP
        INI_TrackOption = Main.INI_TrackOption

        cbSrvName.Items.Add("NVR-A")
        cbSrvName.Items.Add("NVR-B")
        cbSrvName.Items.Add("NVR-C")
        cbSrvName.Items.Add("NVR-D")
        cbSrvName.Items.Add("NVR-E")
        cbSrvName.Items.Add("NVR-F")
        cbSrvName.Items.Add("NVR-G")
        cbSrvName.Items.Add("NVR-H")
        cbSrvName.Items.Add("NVR-I")
        cbSrvName.Items.Add("NVR-J")
        cbSrvName.Items.Add("NVR-K")
        cbSrvName.Items.Add("NVR-L")
        cbSrvName.Items.Add("NVR-M")
        cbSrvName.Items.Add("NVR-N")
        cbSrvName.Items.Add("NVR-Z")

        Try
            cbSrvName.SelectedItem = GetIniInfo("Configuration", INI_ClientName, INI_Path)
            txtTraSoftName.Text = GetIniInfo("Configuration", INI_TrackSoftwaveName, INI_Path)

            txtSrvIP.Text = GetIniInfo("Network", INI_SrvIp, INI_Path)
            txtMailIP.Text = GetIniInfo("Network", INI_MailSrvIP, INI_Path)
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try


        If Main.SW_SoftwavePerformanceTrack Then
            cbxNVRInspect.Checked = True
        Else
            cbxNVRInspect.Checked = False
        End If

        If Main.SW_RecordingDet Then
            cbxRecordingDet.Checked = True
        Else
            cbxRecordingDet.Checked = False
        End If

        If Main.SW_SoftwaveDet Then
            cbxSoftwaveDet.Checked = True
        Else
            cbxSoftwaveDet.Checked = False
        End If

        If Main.SW_SendEmailAlert Then
            cbxMailReport.Checked = True
        Else
            cbxMailReport.Checked = False
        End If

    End Sub

    Private Sub btnSetup_Click(sender As Object, e As EventArgs) Handles btnSetup.Click

        INI_ClientName = Main.INI_ClientName
        INI_TrackSoftwaveName = Main.INI_TrackedSoftwaveName
        INI_MailSrvIP = Main.INI_MailSrv_IP
        INI_ClientName = Main.INI_ClientName
        INI_SrvIp = Main.INI_SrvIP
        INI_TrackOption = Main.INI_TrackOption

        Main.ClientName = cbSrvName.SelectedItem
        ClientName = Main.ClientName


        Try
            If rswitch Then
                rswitch = False
                WritePrivateProfileString("Configuration", INI_TrackSoftwaveName, txtTraSoftName.Text, INI_Path)
                rswitch = True
            End If

        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try


        Try
            If rswitch Then
                rswitch = False
                WritePrivateProfileString("Network", INI_MailSrvIP, txtMailIP.Text, INI_Path)
                rswitch = True
            End If

        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try


        Try
            If rswitch Then
                rswitch = False
                WritePrivateProfileString("Configuration", INI_ClientName, ClientName, INI_Path)
                rswitch = True
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try

        Try
            If rswitch Then
                rswitch = False
                WritePrivateProfileString("Network", INI_SrvIp, txtSrvIP.Text, INI_Path)
                rswitch = True
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try

        Dim TrackOption As Integer = 0

        If cbxSoftwaveDet.Checked Then
            TrackOption += 1
            Main.SW_SoftwaveDet = True

        Else

            Main.SW_SoftwaveDet = False

        End If
        If cbxRecordingDet.Checked Then
            TrackOption += 2
            Main.SW_RecordingDet = True

        Else

            Main.SW_RecordingDet = False

        End If
        If cbxNVRInspect.Checked Then
            TrackOption += 4
            Main.SW_SoftwavePerformanceTrack = True

        Else

            Main.SW_SoftwavePerformanceTrack = False

        End If

        If cbxMailReport.Checked Then
            TrackOption += 8
            Main.SW_SendEmailAlert = True

        Else

            Main.SW_SendEmailAlert = False

        End If
        Try
            If rswitch Then
                rswitch = False
                WritePrivateProfileString("Configuration", INI_TrackOption, TrackOption, INI_Path)
                rswitch = True
            End If
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
        If MsgBox("Do you want to reloading ?", MsgBoxStyle.YesNo, "Wanning") = MsgBoxResult.Yes Then
            Main.ReloadFile()

        End If

        Me.Dispose()
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()

    End Sub

    Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" _
           (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpDefault As String, _
            ByVal lpReturnedString As String, ByRef nSize As Integer, ByVal lpFileName As String) As Integer
    Public Function GetIniInfo(ByVal pSection As String, ByVal pKey As String, ByVal AppPath As String)

        Return GetIniString(pSection, pKey, AppPath)

        'Return Nothing

    End Function

    Public Function GetIniString(ByVal lkSection As String, ByVal lkKey As String, ByVal lkIniPath As String) As String

        Dim lngRtn As Long
        Dim strRtn As String
        GetIniString = ""
        strRtn = New String(Chr(20), 255)
        Dim fi As New System.IO.FileInfo(lkIniPath)
        If Not fi.Exists Then

            Throw New Exception("設定檔不存在(" & lkIniPath & ")")

        End If

        lngRtn = GetPrivateProfileString(lkSection, lkKey, vbNullString, strRtn, Len(strRtn), lkIniPath)
        If lngRtn <> 0 Then

            'GetIniString = Left$(strRtn, InStr(strRtn, Chr(0)) - 1)

            GetIniString = strRtn.Substring(0, InStr(strRtn, Chr(0)) - 1)

        End If

    End Function

    Private Sub Setting_Shown(sender As Object, e As EventArgs) Handles Me.Shown

        INI_ClientName = Main.INI_ClientName
        INI_TrackSoftwaveName = Main.INI_TrackedSoftwaveName
        INI_MailSrvIP = Main.INI_MailSrv_IP
        INI_ClientName = Main.INI_ClientName
        INI_SrvIp = Main.INI_SrvIP
        INI_Path = Main.INI_Path

        Try
            cbSrvName.SelectedItem = GetIniInfo("Configuration", INI_ClientName, INI_Path)
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try

        ' cbSrvName.SelectedItem = GetIniInfo("Configuration", "SrvName", Main.INIpath)
        txtTraSoftName.Text = GetIniInfo("Configuration", INI_TrackSoftwaveName, INI_Path)

        txtSrvIP.Text = GetIniInfo("Network", INI_SrvIp, INI_Path)
        txtMailIP.Text = GetIniInfo("Network", INI_MailSrvIP, INI_Path)
        If Main.SW_SoftwavePerformanceTrack Then

            cbxNVRInspect.Checked = True

        Else

            cbxNVRInspect.Checked = False

        End If

        If Main.SW_RecordingDet Then

            cbxRecordingDet.Checked = True

        Else

            cbxRecordingDet.Checked = False

        End If

        If Main.SW_SoftwaveDet Then
            cbxSoftwaveDet.Checked = True
        Else
            cbxSoftwaveDet.Checked = False
        End If

        If Main.SW_SendEmailAlert Then
            cbxMailReport.Checked = True
        Else
            cbxMailReport.Checked = False
        End If

    End Sub

End Class
