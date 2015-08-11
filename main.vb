Imports System.Runtime.InteropServices
Imports Microsoft.Win32
Imports System.IO
Imports System.Windows.Forms
Imports System.Net.Sockets
Imports System.Net
Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic
Imports System.Xml
Imports System.Management
Imports System.Management.Instrumentation
Imports System.Data
Imports System.Data.Sql
Imports System.Data.SqlClient
Imports System.Collections
Imports System.Diagnostics
Imports System.ComponentModel
Imports System.Net.Mail



Public Class Main
    'A Section
#Region "Win API"
    'Set Windows supoort function
    '**************************************
    '/* This function be used to read *.ini file. */
    Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" _
           (ByVal lpApplicationName As String, ByVal lpKeyName As String, ByVal lpDefault As String, _
            ByVal lpReturnedString As String, ByRef nSize As Integer, ByVal lpFileName As String) As Integer

    Declare Function ShowWindowA Lib "user32" Alias "ShowWindow" _
        (ByVal hwnd As IntPtr, ByVal nCmdShow As Integer) As Boolean

    Private Declare Function NtQuerySystemInformation Lib "ntdll" _
       (ByVal dwInfoType As Long, ByVal lpStructure As Long, _
       ByVal dwSize As Long, ByVal dwReserved As Long) As Long
    Public Enum WindowShowStyle As UInteger
        ''' <summary>Hides the window and activates another window.</summary>
        ''' <remarks>See SW_HIDE</remarks>
        Hide = 0
        '''<summary>Activates and displays a window. If the window is minimized 
        ''' or maximized, the system restores it to its original size and 
        ''' position. An application should specify this flag when displaying 
        ''' the window for the first time.</summary>
        ''' <remarks>See SW_SHOWNORMAL</remarks>
        ShowNormal = 1
        ''' <summary>Activates the window and displays it as a minimized window.</summary>
        ''' <remarks>See SW_SHOWMINIMIZED</remarks>
        ShowMinimized = 2
        ''' <summary>Activates the window and displays it as a maximized window.</summary>
        ''' <remarks>See SW_SHOWMAXIMIZED</remarks>
        ShowMaximized = 3
        ''' <summary>Maximizes the specified window.</summary>
        ''' <remarks>See SW_MAXIMIZE</remarks>
        Maximize = 3
        ''' <summary>Displays a window in its most recent size and position. 
        ''' This value is similar to "ShowNormal", except the window is not 
        ''' actived.</summary>
        ''' <remarks>See SW_SHOWNOACTIVATE</remarks>
        ShowNormalNoActivate = 4
        ''' <summary>Activates the window and displays it in its current size 
        ''' and position.</summary>
        ''' <remarks>See SW_SHOW</remarks>
        Show = 5
        ''' <summary>Minimizes the specified window and activates the next 
        ''' top-level window in the Z order.</summary>
        ''' <remarks>See SW_MINIMIZE</remarks>
        Minimize = 6
        '''   <summary>Displays the window as a minimized window. This value is 
        '''   similar to "ShowMinimized", except the window is not activated.</summary>
        ''' <remarks>See SW_SHOWMINNOACTIVE</remarks>
        ShowMinNoActivate = 7
        ''' <summary>Displays the window in its current size and position. This 
        ''' value is similar to "Show", except the window is not activated.</summary>
        ''' <remarks>See SW_SHOWNA</remarks>
        ShowNoActivate = 8
        ''' <summary>Activates and displays the window. If the window is 
        ''' minimized or maximized, the system restores it to its original size 
        ''' and position. An application should specify this flag when restoring 
        ''' a minimized window.</summary>
        ''' <remarks>See SW_RESTORE</remarks>
        Restore = 9
        ''' <summary>Sets the show state based on the SW_ value specified in the 
        ''' STARTUPINFO structure passed to the CreateProcess function by the 
        ''' program that started the application.</summary>
        ''' <remarks>See SW_SHOWDEFAULT</remarks>
        ShowDefault = 10
        ''' <summary>Windows 2000/XP: Minimizes a window, even if the thread 
        ''' that owns the window is hung. This flag should only be used when 
        ''' minimizing windows from a different thread.</summary>
        ''' <remarks>See SW_FORCEMINIMIZE</remarks>
        ForceMinimized = 11

    End Enum
    '<DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
    'Private Shared Function ShowWindow(ByVal hwnd As IntPtr, ByVal nCmdShow As Integer) As Boolean

#End Region
   
#Region "初始變數之定義"

    'System Menu
#Region "System Menu"
    'Program Manul Define
    '***************************************
    Dim Menu_Icon As ContextMenu = New ContextMenu
    'Friend WithEvents Menu_Item As System.Windows.Forms.MenuItem = New MenuItem
    'Friend WithEvents NotifyIcon_Main As System.Windows.Forms.NotifyIcon
    Dim NotifyIco_System As NotifyIcon = New NotifyIcon()
    '***************************************
#End Region

    'variable
#Region "System Variable"

    Dim VersionType As String = "Beta"
    Dim Path_Startup As String = Application.StartupPath
    Dim Version As String = Me.ProductVersion
    Dim CPU_Quantity As Integer = Environment.ProcessorCount

    'Timer interval
    Dim tmPrgChkTime As Double
    Dim tmRecChkTime As Double
    Dim tmHeartBeatTime As Double
    Dim tmResourceInspectTime As Double
    Dim tmLightControlerTime As Double

    Dim SoftNVRIAPath As String
    Dim SoftNVRPrsID As Integer

    'NVR Channel Quantity
    Dim chnMaxNub As Integer

    Dim SendFileBuffer_byte As Long
    'ProAlex Name for instance NVR-A
    Public ClientName As String = "Unknow"

    'System Status Inspect
    Dim OS_CPU_Usage As String
    Dim OS_Mem_Free As String
    Dim NVR_CPU_Usage As String
    Dim NVR_Mem_Usage As String

    Dim StateName As String

    Dim TrackSoftwaveName As String
    Dim TrackOption As Integer

    Dim B_FeatureOption(8) As String
    Dim S_FeatureOption As String

#End Region

    'Process
#Region "Process"

    Dim procsnvr() As Process

#End Region

    'Thread
#Region "Thread"

    Dim t_Instruction As Thread

#End Region

    'Timers
#Region "Timers"

    Dim tmPrgChk As Timers.Timer
    Dim tmRecChk As Timers.Timer
    Dim tmHeartBeat As Timers.Timer
    Dim tmResourceInspect As Timers.Timer
    Dim tmLightControler As Timers.Timer

#End Region
   
    'Socket Define
#Region "Network Socket IP and Port"

    Dim Skt_rTcp_SendFileToSrv As Socket ' Row Socket
    Dim Skt_UDP_Cli_Listen_WaitingOrder As UdpClient
    Dim Skt_UDP_cli_Send_MsgToSrv As New UdpClient
    Dim Skt_TCP_Cli_Send_MsgToMailSrv As TcpClient

    Dim NStream_SendMailToMailSrv As NetworkStream
    'New System.Net.Sockets.Socket(Net.Sockets.AddressFamily.InterNetwork, Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
    Public Ipnp_MsgSrv_ListenMsg As IPEndPoint
    Dim Ipnp_MsgSrv_ListenFile As IPEndPoint
    Dim Ipnp_MailSrv_ListenMsgAndSend As IPEndPoint


    Dim srvipadd As Net.IPAddress
    Dim srvipport As Integer
    Dim MailSrvIP As IPAddress
    Dim MailSrvMsgPort As Integer

    Dim FStream_SendFile As FileStream
    Dim BReader As BinaryReader

    'Server IP & Port
    Dim SrvRecFilePort As Integer
    Dim cliPort As String
    Dim SrvMsgPort As String
    Dim SrvIP As String

#End Region

    'System.ini config
#Region "System.ini Variable "

    Public INI_Path As String = Application.StartupPath & "\System.ini"
    Public INI_TrackOption As String = "TrackOption"
    Public INI_ClientName As String = "ClientName"
    Public INI_TrackedSoftwaveName As String = "TrackedSoftwaveName"
    Public INI_MailSrv_IP As String = "MailSrvIP"
    Public INI_SrvIP As String = "SrvIP"

    Dim INI_MailSrv_Port As String = "MailSrvPort"
    Dim INI_SrvPort As String = "SrvMsgPort"
    Dim INI_CliPort As String = "CltIntPort"
    Dim INI_SrvRecFilePort As String = "SrvRecFilePort"

    Dim INI_SendFileBuffer As String = "SendFileBuffer"

    Dim INI_tmPrgChk As String = "ProgramCheckFrequency"
    Dim INI_tmRecChk As String = "RecordFileCheckFrequency"
    Dim INI_tmHeartbeat As String = "HeartbeatFrequency"
    Dim INI_tmResourceInspect As String = "ResourceInspectFrequency"
    Dim INI_tmLightFrequency As String = "LightFrequency"

    Dim INI_SoftNVRPath As String = "SoftNVRIAPath"


#End Region

    'All the switch 
#Region "IOKey and Switch"

    Dim StateSW_LightControl As Boolean
    Dim StateSW_btnStartMonitor As Boolean = True

    Dim State_System As Boolean = True
    Dim State_CheckerLight As Boolean = False



    Dim IOkey_SendMsg As Boolean = True
    Dim IOKey_SendMail As Boolean
    Dim IOKey_XMLWrite As Boolean = True




    Dim SW_TestMode As Boolean = False
    Public SW_SoftwavePerformanceTrack As Boolean = False 'just performance
    Public SW_SoftwaveDet As Boolean = False 'Detecting Switch
    Public SW_RecordingDet As Boolean = False ' Detecting Recorded Status Switch
    Public SW_SendEmailAlert As Boolean = False

#End Region

    'Net State
#Region "Net State"
    Dim isResponsed_ServerACK As Boolean = 0
#End Region

    'Form Define
#Region "Form"
    Dim FrmSetting As Setting = New Setting()
    Dim MsgForm As MessageForm = New MessageForm()
#End Region

    'Hash
#Region "Hash Variable"
    ' Dim hs_cherrortimes As Hashtable
    Dim hs_NoRecComboCounter As Hashtable
    Dim hs_NoRecCounter As Hashtable
    Dim hs_NoRecSend_key As Hashtable
    Dim hs_RecStatus As Hashtable
    Dim hs_FileQCounter As Hashtable

#End Region

    'Timer's counter
#Region "Counter"

    Dim start_MonitorRecChk_Counter As Long = 0
    Dim start_MonitorPrgChk_Counter As Long = 0
    Dim PrgNoRepCounter As Integer
    Dim PrgNoRepComboCounter As Integer
    Dim PrgStopCounter As Integer
    Dim PrgStopComboCounter As Integer

#End Region

    'XML
#Region "XML Variable"

    Dim Path_XmlDetectFile As String = Path_Startup & "\log\" & datedealtoday() & "_DetectLog.xml"
    Dim Path_XmlLogFile As String = Path_Startup & "\log\" & datedealtoday() & "_Log.xml"

    Dim xDoc As XmlDocument
    Dim xElement As XmlElement
    Dim Root As XmlElement
    Dim xChildElement As XmlElement
    Dim nNode As XmlNode

#End Region

    'DataSet
#Region "DataSet"

    Public dataSset As DataSet
    Dim dataTab As DataTable
    Dim datacl As DataColumn
    Dim datarow As DataRow

#End Region

    'Stack
#Region "Stack"

    Dim BufStack As Stack = New Stack

#End Region

    'PerformanceCounter
#Region "PerformanceCounter"

    Dim PerCou As PerformanceCounter '= New PerformanceCounter("Processor", "% Processor Time", "_Total") for System
    Dim PerCou_App As PerformanceCounter '= New PerformanceCounter("Process", "% Processor Time", "SoftNVRIA") 'CPU
    Dim RamUsage As PerformanceCounter '= New PerformanceCounter("Process", "Working Set - Private", "SoftNVRIA") 'Memoery
    'Dim pf2 As PerformanceCounter = New PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total")

#End Region

#End Region


    Private Sub Main_Load(sender As Object, e As EventArgs) Handles Me.Load
        MsgForm = New MessageForm()


        Me.Text = "ProAlex Monitor v" & Version & " " & VersionType
        'Received Args
        'For Each g In My.Application.CommandLineArgs

        '    MsgBox(g)

        'Next

        '#@#@#@##@#@#@##@#@#@##@#@#@#  Test Area #@#@#@##@#@#@##@#@#@##@#@#@#



        'GetMAC()
        'MsgBox(testfile())
        'MsgBox(testmulti())
        'Check_Ping()
        'trymulticast()
        '#@#@#@##@#@#@##@#@#@##@#@#@##@#@#@##@#@#@##@#@#@##@#@#@##@#@#@##@#@#@#





        '1.
        Load_Menu()


        '2.
        Load_INIParameter()


        '3.
        Initialize()


        '4.
        Set_BaseParameter()


        '5.
        Startup_BaseThread()

        '6.
        Startup_BaseTimer()

    End Sub

    'Startup Step 1
    Private Function Load_Menu()

        Try

            NotifyIco_System.Icon = New Icon("Make.ico")
            NotifyIco_System.Visible = True
            NotifyIco_System.Text = "ProAlex"

            Menu_Icon.MenuItems.Add("Setting", AddressOf Show_FrmSetting)
            Menu_Icon.MenuItems.Add("Show Controller", AddressOf display)
            Menu_Icon.MenuItems.Add("Start Monitor", AddressOf StartMonitor)
            Menu_Icon.MenuItems.Add("StopMonitor", AddressOf StopMonitor)
            Menu_Icon.MenuItems.Add("Exit", AddressOf shutdown)

            NotifyIco_System.ContextMenu = Menu_Icon
            Return 0
        Catch ex As Exception

            MsgBox("Load_Menu Error:" & ex.ToString)

            Return -1
        End Try

    End Function
    'Startup Step 2
    Private Function Load_INIParameter()

        Try
            ClientName = GetIniInfo("Configuration", INI_ClientName, INI_Path)
            TrackOption = CInt(GetIniInfo("Configuration", INI_TrackOption, INI_Path))
            TrackSoftwaveName = GetIniInfo("Configuration", INI_TrackedSoftwaveName, INI_Path)
            MailSrvIP = IPAddress.Parse(GetIniInfo("Network", INI_MailSrv_IP, INI_Path))
            MailSrvMsgPort = GetIniInfo("Network", INI_MailSrv_Port, INI_Path)
            Ipnp_MailSrv_ListenMsgAndSend = New IPEndPoint(MailSrvIP, MailSrvMsgPort)

            SendFileBuffer_byte = GetIniInfo("File", INI_SendFileBuffer, INI_Path) * 2 ^ 20 ' MB
            SrvIP = GetIniInfo("Network", INI_SrvIP, INI_Path)

            srvipadd = Net.IPAddress.Parse(GetIniInfo("Network", INI_SrvIP, INI_Path))
            srvipport = GetIniInfo("Network", INI_SrvPort, INI_Path)

            Ipnp_MsgSrv_ListenMsg = New IPEndPoint(srvipadd, srvipport)

            cliPort = GetIniInfo("Network", INI_CliPort, INI_Path)
            SrvRecFilePort = GetIniInfo("Network", INI_SrvRecFilePort, INI_Path)
            SoftNVRIAPath = GetIniInfo("Configuration", INI_SoftNVRPath, INI_Path)
            chnMaxNub = GetIniInfo(ClientName, "channel", INI_Path)

            tmPrgChkTime = CDbl(GetIniInfo("Configuration", INI_tmPrgChk, INI_Path))
            tmRecChkTime = CDbl(GetIniInfo("Configuration", INI_tmRecChk, INI_Path))
            tmResourceInspectTime = CDbl(GetIniInfo("Configuration", INI_tmResourceInspect, INI_Path))
            tmHeartBeatTime = CDbl(GetIniInfo("Configuration", INI_tmHeartbeat, INI_Path))
            tmLightControlerTime = CDbl(GetIniInfo("Configuration", INI_tmLightFrequency, INI_Path))
            Return 0
        Catch ex As Exception

            MsgBox("Load_INIParameter Error : " & ex.ToString)
            Return -1

        End Try

    End Function
    'Startup Step 3
    Private Function Initialize(Optional ByVal situ As Integer = 0)

        Try
            MsgForm.Exe_Show()


            MsgForm.displayMsg("Initializing")
            MsgForm.iniProcess(7, 1)

            Initilize_XML()
            MsgForm.runstep()
            MsgForm.displayMsg("Initializing XML Accomplish")

            Initialize_Hash()
            MsgForm.runstep()
            MsgForm.displayMsg("Initializing HashTable Accomplish")

            Initialize_Timer()
            MsgForm.runstep()
            MsgForm.displayMsg("Initializing Timer Accomplish")

            Initialize_Socket()
            MsgForm.runstep()
            MsgForm.displayMsg("Initializing Socket Accomplish")

            Initialize_DealWithTrackOption()
            MsgForm.runstep()
            MsgForm.displayMsg("Initializing TrackOption Accomplish")

            Initialize_DataTableAndDataSet(chnMaxNub)
            MsgForm.runstep()
            MsgForm.displayMsg("Initializing DataTable And DataSet Accomplish")

            Initialize_PerformanceCounter()
            MsgForm.runstep()
            MsgForm.displayMsg("Initializing PerformanceCounter Accomplish")

            MsgForm.displayMsg("initialize completely")

            Return 0
        Catch ex As Exception

            MsgBox("Initialize Error:" & ex.ToString)
            Return -1
        End Try

    End Function
    'Startup Step 3-1
    Private Function Initilize_XML()

        If Not IO.Directory.Exists(Path_Startup & "\log") Then

            Directory.CreateDirectory(Path_Startup & "\log")

        End If

        If Not IO.Directory.Exists(Path_Startup & "\State") Then

            Directory.CreateDirectory(Path_Startup & "\State")

        End If

        If Not File.Exists(Path_XmlDetectFile) Then

            xDoc = New XmlDocument

            Try

                Root = xDoc.CreateElement("root")
                Root.SetAttribute("DateAndTime", Now)
                xDoc.AppendChild(Root)

                xChildElement = xDoc.CreateElement("Record-Event")
                xChildElement.SetAttribute("ProAlexMonitor", "v" & Version)
                xChildElement.InnerText = "New_Detect_Log"
                Root.AppendChild(xChildElement)

                'Test Create node from loop

                'For i = 1 To 30000
                '    xChildElement = xdoc.CreateElement("A" & i)
                '    xChildElement.SetAttribute("XYZ", "GYZ")
                '    xChildElement.InnerText = i
                '    root.AppendChild(xChildElement)
                'Next
                ' xElement.SetAttribute("department", "HR")
                'xElement2 = xdoc.CreateElement("gy2")
                'xElement.SetAttribute("department", "Engineer")
                xDoc.Save(Path_XmlDetectFile)
                Return 0
            Catch ex As Exception

                MsgBox("xmll() Error: " & ex.ToString)
                Return -1
            End Try

        End If

        If Not File.Exists(Path_XmlLogFile) Then

            xDoc = New XmlDocument

            Try

                Root = xDoc.CreateElement("root")
                Root.SetAttribute("DateAndTime", Now)
                xDoc.AppendChild(Root)

                xChildElement = xDoc.CreateElement("Record-Event")
                xChildElement.SetAttribute("ProAlexMonitor", "v" & Version)
                xChildElement.InnerText = "New_Even_Log"
                Root.AppendChild(xChildElement)

                'Test Create node from loop

                'For i = 1 To 30000
                '    xChildElement = xdoc.CreateElement("A" & i)
                '    xChildElement.SetAttribute("XYZ", "GYZ")
                '    xChildElement.InnerText = i
                '    root.AppendChild(xChildElement)
                'Next
                ' xElement.SetAttribute("department", "HR")
                'xElement2 = xdoc.CreateElement("gy2")
                'xElement.SetAttribute("department", "Engineer")
                xDoc.Save(Path_XmlLogFile)
                Return 0
            Catch ex As Exception

                MsgBox("xmll() Error: " & ex.ToString)
                Return -1
            End Try

        End If


        Return 1
    End Function
    'Startup Step 3-2
    Private Function Initialize_Hash()

        Try
            ' hs_chdata = New Hashtable
            hs_NoRecCounter = New Hashtable
            hs_NoRecSend_key = New Hashtable
            hs_RecStatus = New Hashtable
            hs_NoRecComboCounter = New Hashtable
            hs_FileQCounter = New Hashtable


            For i = 0 To 36
                ' hs_chdata.Add(i, 0)
                hs_NoRecCounter.Add(i, 0)
                hs_NoRecSend_key.Add(i, 1)
                hs_RecStatus.Add(i, 0)
                hs_NoRecComboCounter.Add(i, 0)
                hs_FileQCounter.Add(i, 0)
            Next

            Return 0

        Catch ex As Exception

            dealwithproblem(ex.ToString)
            Return -1

        End Try

    End Function
    'Startup Step 3-3
    Sub tmStart()
        MsgBox("Success")
    End Sub

    Private Function Initialize_Timer()

        Try
      
            tmPrgChk = New Timers.Timer
            tmRecChk = New Timers.Timer
            tmHeartBeat = New Timers.Timer
            tmResourceInspect = New Timers.Timer
            tmLightControler = New Timers.Timer

            tmPrgChk.Interval = tmPrgChkTime * 10 ^ 3
            tmRecChk.Interval = tmRecChkTime * 10 ^ 3
            tmHeartBeat.Interval = tmHeartBeatTime * 10 ^ 3
            tmResourceInspect.Interval = tmResourceInspectTime * 10 ^ 3
            tmLightControler.Interval = tmLightControlerTime * 10 ^ 3

            tmPrgChk.AutoReset = True
            tmRecChk.AutoReset = True
            tmHeartBeat.AutoReset = True
            tmResourceInspect.AutoReset = True
            tmLightControler.AutoReset = True

            AddHandler tmPrgChk.Elapsed, AddressOf tmPrgChk_Exec
            AddHandler tmRecChk.Elapsed, AddressOf tmRecChk_Exec
            AddHandler tmHeartBeat.Elapsed, AddressOf tmHeartBeat_Exec
            AddHandler tmResourceInspect.Elapsed, AddressOf tmResourceInspect_Exec
            AddHandler tmLightControler.Elapsed, AddressOf tmLightControler_Exec
            Return 0
        Catch ex As Exception

            dealwithproblem("Initialize_Timer Error:" & ex.ToString)
            Return -1

        End Try


    End Function
    'Startup Step 3-4
    Private Function Initialize_Socket()

        Try

            skt_rTcp_SendFileToSrv = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            Ipnp_MsgSrv_ListenFile = New IPEndPoint(IPAddress.Parse(SrvIP), SrvRecFilePort)
            Return 0
        Catch ex As Exception

            dealwithproblem("Initialize_Socket Error:" & ex.ToString)
            Return -1

        End Try


    End Function
    'Startup Step 3-5
    Private Function Initialize_DealWithTrackOption()

        Dim B_FeatureOption(8) As String
        Dim DWNumber As Integer = TrackOption
        Try
            For i = 7 To 0 Step -1

                If DWNumber \ (2 ^ i) > 0 Then

                    B_FeatureOption(i) = True
                    DWNumber -= (2 ^ i)
                    S_FeatureOption &= 1

                Else

                    B_FeatureOption(i) = False
                    S_FeatureOption &= 0

                End If
            Next

            SW_SoftwaveDet = B_FeatureOption(0)
            SW_RecordingDet = B_FeatureOption(1)
            SW_SoftwavePerformanceTrack = B_FeatureOption(2)
            SW_SendEmailAlert = B_FeatureOption(3)
            Return 0

        Catch ex As Exception

            dealwithproblem("Initialize_DealWithTrackOption Error:" & ex.ToString)
            Return -1

        End Try

    End Function
    'Startup Step 3-6
    Private Function Initialize_DataTableAndDataSet(ByVal channal As Integer)

        Try

            dataSset = New DataSet("GGYY")
            dataTab = dataSset.Tables.Add("Counter")

            datacl = New DataColumn()
            datacl.DataType = System.Type.GetType("System.Double")
            datacl.ColumnName = "channel_id"
            datacl.ReadOnly = True
            datacl.Unique = True

            dataTab.Columns.Add(datacl)

            datacl = New DataColumn()
            datacl.DataType = System.Type.GetType("System.Double")
            datacl.ColumnName = "channel_NoRecordingCounter"
            datacl.ReadOnly = False
            datacl.Unique = False

            dataTab.Columns.Add(datacl)

            datacl = New DataColumn()
            datacl.DataType = System.Type.GetType("System.Double")
            datacl.ColumnName = "channel_NoRecordingComboCounter"
            datacl.ReadOnly = False
            datacl.Unique = False

            dataTab.Columns.Add(datacl)

            datacl = New DataColumn()
            datacl.DataType = System.Type.GetType("System.Double")
            datacl.ColumnName = "channel_FileNumber"
            datacl.ReadOnly = False
            datacl.Unique = False

            dataTab.Columns.Add(datacl)

            Dim PrimaryKeyColumns(0) As DataColumn
            PrimaryKeyColumns(0) = dataTab.Columns("channel_id")
            dataTab.PrimaryKey = PrimaryKeyColumns


            'dataSset.Tables.Add(dataTab)

            For i = 1 To channal
                datarow = dataTab.NewRow()
                datarow("channel_id") = i
                datarow("channel_NoRecordingCounter") = 0
                datarow("channel_NoRecordingComboCounter") = 0
                datarow("channel_FileNumber") = 0
                dataTab.Rows.Add(datarow)

            Next i

            ' DataGridView1.DataSource = Nothing
            DataGridView1.DataSource = dataSset.Tables(0)

            Return 0

        Catch ex As Exception

            dealwithproblem("Initialize_DataTableAndDataSet Error:" & ex.ToString)
            Return -1
        End Try



    End Function
    'Startup Step 3-7
    Private Function Initialize_PerformanceCounter()
        Try
            PerCou = New PerformanceCounter("Processor", "% Processor Time", "_Total")
            PerCou_App = New PerformanceCounter("Process", "% Processor Time", TrackSoftwaveName) 'CPU
            RamUsage = New PerformanceCounter("Process", "Working Set - Private", TrackSoftwaveName) 'Memoery
            'pf2 = New PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total")
        Catch ex As Exception
            dealwithproblem("Initialize_PerformanceCounter Error:" & ex.ToString)
            Return 0
        End Try


        Return -1
    End Function
    'Startup Step 4
    Private Function Set_BaseParameter()

        Try
            txtPrgTimer.Text = 0
            txtRecTimer.Text = 0

            CheckForIllegalCrossThreadCalls = False

            Me.KeyPreview = True
            Me.ShowInTaskbar = True

            IOKey_SendMail = True

            If SW_TestMode = True Then

                Panel1.Visible = False
                Me.Size = New System.Drawing.Size(600, 400)
                SW_TestMode = False

            Else

                Panel1.Visible = True
                Me.Size = New System.Drawing.Size(766, 462)
                SW_TestMode = True

            End If
        Catch ex As Exception

            dealwithproblem("Set_BaseParameter Error:" & ex.ToString)
            Return 0

        End Try

        Return -1

    End Function
    'Startup Step 5
    Private Function Startup_BaseThread()

        Try

            t_Instruction = New Thread(AddressOf listen)
            t_Instruction.Name = "GG"
            t_Instruction.SetApartmentState(ApartmentState.STA)
            t_Instruction.IsBackground = True
            t_Instruction.Start()

            sendmsg("Online")

        Catch ex As Exception

            MsgBox("Startup_BaseThread Error:" & ex.ToString)
            Return 0

        End Try

        Return -1

    End Function
    'Startup Step 6
    Private Function Startup_BaseTimer()

        Try

            tmHeartBeat.Start()
            tmLightControler.Start()

        Catch ex As Exception

            MsgBox("Startup_BaseThread Error:" & ex.ToString)

            Return 0

        End Try

        Return -1

    End Function

    Public Sub ReloadFile()

        Try

            MessageForm.displayMsg("Reloaded")

            Load_INIParameter()
            Initialize()

            MessageForm.displayMsg("Reloaded Successful")

        Catch ex As Exception

            dealwithproblem("Reload fault", True)

        End Try

    End Sub

    'first test 8 layer Number

    Sub ExchBinary(ByVal Numb As Integer)

        ' Dim binub As String
        Dim sq As Stack = New Stack()
        Dim s As Integer = Numb / 2 ^ 8


        sq.Push(s)

    End Sub

    Private Sub Main_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing

        If e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
            Me.Hide()
        End If

    End Sub

    Private Sub Main_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        ' MsgBox(Application.StartupPath)
        'Me.Visible = False

    End Sub
    '*****************************

    'Key Press event
    '*****************************
    Private Sub Main_KeyPress(sender As Object, e As KeyPressEventArgs) Handles Me.KeyPress
        'ASCII 
        '32 is space
        '48 is 0
        '65 is A
        '97 is a
        '9 is tab
        '27 is esc
        '13 is enter
        If e.KeyChar = Microsoft.VisualBasic.ChrW(13) Then

            sendmsg(txtCommon.Text)

        End If

    End Sub
    '****************************

    'manu function
    '****************************
#Region "Menul Function"

    'When Program Close, this is will do something

    Sub shutdown()

        sendmsg("Offline")

        Skt_UDP_cli_Send_MsgToSrv.Close()

        skt_rTcp_SendFileToSrv.Close()

        NotifyIco_System.Dispose()

        Me.Dispose()

        'MsgBox(Me.Disposing())
        'Me.Close()
    End Sub

    Function StartMonitor()

        Try
            StartToMailSrvSkt()

            txtDebug.Text += "StartToMailSrvSkt.Start() - Completed" & vbCrLf
            tmPrgChk.Enabled = True
            If SW_SoftwaveDet Then

                tmPrgChk.Enabled = True

                txtDebug.Text += "tmPrgChk.Start() - Completed" & vbCrLf
            End If

            If SW_RecordingDet Then

                tmRecChk.Enabled = True
                txtDebug.Text += " tmRecChk.Start() - Completed" & vbCrLf

            End If


            tmResourceInspect.Enabled = True
            txtDebug.Text += " tmResourceInspect.Start() - Completed" & vbCrLf

            lightRunStatus.BackColor = Color.GreenYellow

            State_CheckerLight = True

            TSMI_StartMonitor.Text = "Stop Monitor"
            TSMI_StartMonitor.BackColor = Color.GreenYellow


            TSMI_ReloadSettingFile.Enabled = False
            TSMI_SettingForm.Enabled = False
            TSMI_Exit.Enabled = False


        Catch ex As Exception
            txtDebug.Text += "Start Monitor Error" & ex.ToString & vbCrLf
            dealwithproblem("Start Monitor Error" & ex.ToString, False)
            Return 1
        End Try
        Return -1
    End Function

    Function StopMonitor()

        Try
            StopToMailSrvSkt()
            txtDebug.Text += "StopToMailSrvSkt.Stop() - Completed" & vbCrLf

            If SW_RecordingDet Then
                tmRecChk.Stop()

                txtDebug.Text += "tmRecChk.Stop() - Completed" & vbCrLf
            End If

            If SW_SoftwaveDet Then
                tmPrgChk.Stop()

                txtDebug.Text += "tmPrgChk.Stop() - Completed" & vbCrLf

            End If



            tmResourceInspect.Stop()

            txtDebug.Text += "tmResourceInspect.Stop() - Completed" & vbCrLf
            lightRunStatus.BackColor = Color.Red
            State_CheckerLight = False
            TSMI_StartMonitor.Text = "Start Monitor"
            TSMI_StartMonitor.BackColor = Color.Red

            TSMI_ReloadSettingFile.Enabled = True
            TSMI_SettingForm.Enabled = True
            TSMI_Exit.Enabled = True

            Return 0

        Catch ex As Exception

            txtDebug.Text += "Stop Monitor Error" & ex.ToString & vbCrLf
            dealwithproblem("Stop Monitor Error" & ex.ToString, False)
            Return -1

        End Try



    End Function

    Sub display()

        Me.Visible = True

    End Sub

    Sub disapper()

        MsgBox("From will hide")

    End Sub

    Sub Show_FrmSetting()

        FrmSetting.Show()

    End Sub

#End Region
    '****************************

    'XML can be a module.
    'XML Function
    'This is sub function. if have anything want to be recording, please use dealwithproblem().
    '&&&&&&&&*********&&&&&&&&*********&&&&&&&&*********&&&&&&&&*********&&&&&&&&*********&&&&&&&&*********
    Public Sub XML_Add_Log(ByVal Channel As Integer, ByVal Message As String, ByVal Counter As Integer, ByVal ComBoCounter As Integer, Optional ByVal Option_M As Integer = 0)

        ' MsgBox(Channel & Message & Counter)

        While True

            If IOKey_XMLWrite = True Then

                IOKey_XMLWrite = False
               
                Try

                    Initilize_XML()
                    Dim xxnelement As XmlElement
                    xDoc = New XmlDocument()
                    Dim Disk_E_Space As String
                    Dim Space_Number As Integer

                    Space_Number = My.Computer.FileSystem.GetDriveInfo("E:\").TotalFreeSpace / 1024 / 1024
                    Disk_E_Space = Space_Number.ToString & " MB"

                    'Detecting Message

                    If Option_M = 1 Then

                        xDoc.Load(Path_XmlDetectFile)
                        ' nNode = xDoc.SelectSingleNode("root")
                        nNode = xDoc.DocumentElement

                        xxnelement = xDoc.CreateElement("Record-Event")
                        xxnelement.SetAttribute("Channel", Channel)
                        xxnelement.SetAttribute("Time", Now)
                        xxnelement.SetAttribute("Fail_Counter", Counter)
                        xxnelement.SetAttribute("Fail_ComboCounter", ComBoCounter)
                        xxnelement.SetAttribute("OS_CPU_Usage", OS_CPU_Usage)
                        xxnelement.SetAttribute("OS_FreeMemory", OS_Mem_Free)
                        xxnelement.SetAttribute("Disk_E_Space", Disk_E_Space)

                        If SW_SoftwavePerformanceTrack Then

                            xxnelement.SetAttribute("Softwave_CPU_Usage", txtNvrCPUUsage.Text)
                            xxnelement.SetAttribute("Softwave_Mem_Usage", txtNvrMemUsage.Text)

                        End If

                        nNode.AppendChild(xxnelement)
                        xDoc.Save(Path_XmlDetectFile)

                        'Log Message
                    ElseIf Option_M = 0 Then

                        xDoc.Load(Path_XmlLogFile)
                        ' nNode = xDoc.SelectSingleNode("root")
                        nNode = xDoc.DocumentElement
                        xxnelement = xDoc.CreateElement("System-Status")
                        xxnelement.SetAttribute("Message", "Report")
                        xxnelement.SetAttribute("Time", Now)
                        xxnelement.SetAttribute("Disk_E_Space", Disk_E_Space)
                        xxnelement.InnerText = Message

                        nNode.AppendChild(xxnelement)
                        xDoc.Save(Path_XmlLogFile)

                    End If

                Catch ex As Exception

                    MsgBox(ex.ToString)

                Finally

                    IOKey_XMLWrite = True

                End Try

                Exit While


            End If

        End While

    End Sub
    '&&&&&&&&*********&&&&&&&&*********&&&&&&&&*********&&&&&&&&*********&&&&&&&&*********&&&&&&&&*********

    'DataSet Function
    'renew the display.
    ' #@#@#@#@##@#@#@#@##@#@#@#@##@#@#@#@##@#@#@#@##@#@#@#@##@#@#@#@##@#@#@#@##@#@#@#@##@#@#@#@#
    Public Sub updatecounter(ByVal channel As Integer, Optional ByVal Rcounter As Integer = -1, Optional ByVal CRcounter As Integer = -1, Optional ByVal FNCounter As Integer = -1)

        Dim customerRow() As Data.DataRow
        customerRow = dataSset.Tables("Counter").Select("channel_id = '" & channel & "'")

        If Not Rcounter = -1 Then
            customerRow(0)("channel_NoRecordingCounter") = Rcounter

        End If

        If Not CRcounter = -1 Then

            customerRow(0)("channel_NoRecordingComboCounter") = CRcounter

        End If

        If Not FNCounter = -1 Then

            customerRow(0)("channel_FileNumber") = FNCounter

        End If

    End Sub
    '#@#@#@#@##@#@#@#@##@#@#@#@##@#@#@#@##@#@#@#@##@#@#@#@##@#@#@#@##@#@#@#@##@#@#@#@##@#@#@#@#

    Public Function RefreshPerforCounter()

        Try

            PerCou_App = New PerformanceCounter("Process", "% Processor Time", TrackSoftwaveName)  'CPU
            RamUsage = New PerformanceCounter("Process", "Working Set - Private", TrackSoftwaveName) 'Memoery

        Catch ex As Exception

            dealwithproblem(ex.ToString)

        End Try


        Return 1
    End Function
    'Get .ini function
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

    'btn click
    '****************************

    'Test Send txtMsg to Server
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        sendmsg(txtCommon.Text)

    End Sub
    'Test Timer(tmResourceInspect) Unstop to change value on Interval
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        '
        'For Each aa In hs_NoRecCounter.Values
        '    MsgBox(aa)
        'Next
        'tcpclient_SendMsgToServer()

    End Sub
    'Test Update file to Server by clientself
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click

        'Update file to Server by clientself

        OFD_A.ShowDialog()
        txtCommon.Text = OFD_A.FileName
        Try
            Dim s() As String = OFD_A.SafeFileName.ToString.Split(".")
            'MsgBox("S001@" & s(0) & "@" & s(1) & "@" & OFD_A.OpenFile.Length)
            sendmsg("S001@" & s(0) & "@" & s(1) & "@" & OFD_A.OpenFile.Length)
        Catch ex As Exception
            dealwithproblem("testError :" & ex.ToString)
        End Try

        Dim CliThread As Thread
        CliThread = New Thread(AddressOf Waiting_ServerX_ACK)
        CliThread.IsBackground = True
        CliThread.Start()


    End Sub
    'Test Open SoftNVRIAPath Program
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click

        MsgBox(SoftNVRIAPath)
        Dim s As Integer
        s = Process.Start(SoftNVRIAPath).Id
        MsgBox(s)

    End Sub
    'Test netKernel_SktMsg("1~@0")
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click

        netKernel_SktMsg("1~@0")
    End Sub
    'Test netKernel_SktMsg("1~@1")
    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        netKernel_SktMsg("1~@1")
    End Sub
    'Test  netKernel_SktMsg("1~@2")
    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        netKernel_SktMsg("1~@2")
    End Sub
    'Test netKernel_SktMsg("1~@3")
    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        netKernel_SktMsg("1~@3")
    End Sub
    'Test netKernel_SktMsg("1~@4")
    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        netKernel_SktMsg("1~@4")
    End Sub
    'Test Shell Shutdown
    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click

        'MsgBox(datedealtominute)
        Shell("cmd /c shutdown -r ")

    End Sub
    'Test Write Registry
    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click

        Try
            Write_Registry("run", "ProAlexMonitor", "Ya")
            MsgBox("Write Success")

        Catch ex As Exception

            MsgBox(ex.ToString)

        End Try


    End Sub
    'Test Reading Registry
    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click

        ReadRegistry()

    End Sub
    'The e-mail function detection is successful.
    Private Sub Button13_Click(sender As Object, e As EventArgs) Handles Button13.Click
        ' ''Try
        ' ''    procsnvr = Process.GetProcessesByName("Line")
        ' ''Catch ex As Exception

        ' ''End Try

        '' ''Hide All Windows
        ' ''For Each p As Process In Process.GetProcesses()  'fetch all process
        ' ''    If (ComboBox1.Items.Contains(p.ProcessName) = False) Then
        ' ''        ComboBox1.Items.Add(p.ProcessName)
        ' ''    End If
        ' ''    ShowWindowA(p.MainWindowHandle, 9)
        ' ''Next


        ' _iniFileTra()


        Dim mail As New MailMessage()
        Dim cred As New NetworkCredential("alex", "highboy5780")
        '收件者
        mail.[To].Add("alex.lin@hwacom.com")
        mail.Subject = "ProAlex"
        '寄件者
        mail.From = New System.Net.Mail.MailAddress("alex@tcbrt.com")
        mail.IsBodyHtml = True
        mail.Body = "ProAlex Message Testing"
        '設定SMTP
        'Dim smtp As New SmtpClient("172.16.3.21")
        Dim smtp As New SmtpClient("172.16.8.30")
        smtp.UseDefaultCredentials = False
        smtp.EnableSsl = False
        'smtp.EnableSsl = True
        smtp.Credentials = cred
        smtp.Port = 25
        '送出Mail
        Try
            smtp.Send(mail)
        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try

    End Sub

    Private Sub Button14_Click(sender As Object, e As EventArgs) Handles Button14.Click
    
        'Shell("at " & Now.Hour & ":" & Now.Minute + 1 & " cmd /c c:\user\rexwalker\documents\upgrade.bat")
        'MsgBox("at " & Now.Hour & ":" & Now.Minute + 1 & " cmd /c cp " & Path_Startup & "\1.txt " & Path_Startup & "\2.txt ")
    End Sub

    Private Sub Button15_Click(sender As Object, e As EventArgs) Handles Button15.Click
        For i = 0 To 36
            MsgBox(hs_RecStatus(i))
        Next
        MsgBox(Hex(2 ^ 36))
    End Sub

    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click

        About.Visible = True

    End Sub

    Private Sub TSMI_ReloadSettingFile_Click(sender As Object, e As EventArgs) Handles TSMI_ReloadSettingFile.Click

        ReloadFile()

    End Sub

    Private Sub STMI_SettingForm_Click(sender As Object, e As EventArgs) Handles TSMI_SettingForm.Click

        Setting.Visible = True

    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles TSMI_Exit.Click

        shutdown()

    End Sub
    '****************************************************************************************************

    Sub Waiting_ServerX_ACK()

        While True

            If isResponsed_ServerACK = True Then
                isResponsed_ServerACK = False
                Exit While
            End If

        End While
        Sub_SendFileToSrv(OFD_A.FileName)
    End Sub
    'Send message to Server
    Sub sendmsg(ByRef msg As String)

        While True

            If IOkey_SendMsg = True Then

                IOkey_SendMsg = False

                Dim bb() As Byte = Encoding.Default.GetBytes(_FindMyIP() & ":" & ClientName & ":" & msg)

                Try

                    Skt_UDP_cli_Send_MsgToSrv.Send(bb, bb.Length, Ipnp_MsgSrv_ListenMsg)

                Catch ex As Exception

                    dealwithproblem(" sendmsg() Error:Server Offline & " & ex.ToString, False)
                    Skt_UDP_cli_Send_MsgToSrv.Close()

                End Try

                IOkey_SendMsg = True
                Exit While

            End If
        End While




    End Sub

    Sub listen()

        Dim byt(16) As Byte
        Dim ep As New IPEndPoint(IPAddress.Any, 7999)
        Skt_UDP_Cli_Listen_WaitingOrder = New UdpClient(7999)

        While True

            Try

                byt = Skt_UDP_Cli_Listen_WaitingOrder.Receive(ep)

                netKernel_SktMsg(Encoding.Default.GetString(byt))

            Catch ex As Exception

                MsgBox(ex.ToString)

                dealwithproblem("listen() Error : " & ex.ToString, 1)

            End Try

        End While

    End Sub

    '%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@
    '
    '                           meassage process
    '
    '    if it is recieved any order message, all the process are dealt with there. 
    '
    'format: X        ~@           SS
    '       type  split_signal  message
    'X = 1 Order
    'X = 2 Send file to server
    'X = 3 heartbeat to server
    '
    '
    '%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@%$#@

    Function netKernel_SktMsg(ByVal data As String)

        'debug display the received message
        txtReceiver.Text = data

        Dim spgst() As String = Split(data, "~@")
        'if message is formatted, spgst(0) is message type spgst(1) is message airtical. 

        Dim typ As String = Mid(spgst(0), 1, 1)


        Select Case typ
            Case 0

            Case 1  'Control message

                netKernel_SktMsg_sub1(spgst(1))

            Case 2  'Update ini

                netKernel_SktMsg_sub2(spgst(1))

            Case 3  'heartbeat

                netKernel_SktMsg_sub3(spgst(1))

        End Select

        Return ""

    End Function

    Sub netKernel_SktMsg_sub1(ByVal cmd As String)
        '* recommend to Authenticate Server Identity and Log the command
        'Command Type

        Dim typ As String = Mid(cmd, 1, 1)

        Select Case typ

            Case 0 'Order Shutdown ProAlexMonitor

                Me.Close()
                End

            Case 1

                Dim pss() As Process
                pss = Process.GetProcessesByName("SoftNVRIA")

                Try

                    If pss(0).HasExited = False Then


                        sendmsg(pss(0).ProcessName & " be Kill ")
                        pss(0).Kill()

                    End If

                Catch ex As Exception

                    dealwithproblem("netKernel_SktMsg_sub1() Error: execute Order ERROR： Kill SoftNVRIA error" & ex.ToString)

                End Try

            Case 2


                Try
                    SoftNVRPrsID = Process.Start(SoftNVRIAPath).Id

                Catch ex As Exception

                    dealwithproblem("netKernel_SktMsg_sub1() Error: execute Order ERROR： Kill SoftNVRIA error" & ex.ToString)

                End Try

            Case 3

                Dim spaces As String
                Dim sssss As Integer

                sssss = My.Computer.FileSystem.GetDriveInfo("E:\").TotalFreeSpace / 1024 / 1024
                spaces = sssss.ToString & " MB"


                sendmsg("R- D Free Space = " & spaces)


            Case 4

                Cmd_StopMonitor()

            Case 5

                Cmd_StartMonitor()


            Case 6
                'Reload Load_INIParameter()
                If StateSW_btnStartMonitor = False Then

                    Try

                        ReloadFile()

                    Catch ex As Exception

                        dealwithproblem("Reload INI Error :" & ex.ToString)

                    End Try
                Else

                    sendmsg("Client Monitor is running. Please Stop it and try again.")

                End If


        End Select

    End Sub

    'Need to be fix.
    Sub netKernel_SktMsg_sub2(ByVal cmd As String)

        'Tra File


        Dim typ As String = Mid(cmd, 1, 1)

        Select Case typ

            Case 0

                Sub_SendFileToSrv("System.ini")


            Case 1

                Sub_SendFileToSrv(".\log\" & datedealtoday() & "_Log.xml")

            Case 2

                Sub_SendFileToSrv(StateName)

            Case 3

                Dim ssaas() As String
                ssaas = cmd.Split(":")
                Sub_SendFileToSrv(ssaas(1))

            Case 4

                isResponsed_ServerACK = True

            Case 5

                Dim filess() As String
                filess = IO.Directory.GetFiles(".\log\")
                For Each file In filess
                    MsgBox(file)
                    If file Like "*" & cmd.Split(":")(1) & "*" Then
                        MsgBox(file)
                        Sub_SendFileToSrv(file)
                    End If
                Next

            Case 6
                ' cmd.Split(":")(1) = Channel Number
                ' cmd.Split(":")(2) = File Time
                Dim chn As Integer = cmd.Split(":")(1)
                Dim chnName As String
                If chn < 10 Then
                    chnName = "0" & chn
                Else
                    chnName = chn
                End If
                Dim filess() As String
                filess = IO.Directory.GetFiles("E:\MoxaRecordData\" & cmd.Split(":")(2) & "\CH00" & chnName)
                '  MsgBox("Start Search" & cmd.Split(":")(2) & cmd.Split(":")(3))
                For Each file In filess

                    If file Like "*" & cmd.Split(":")(2) & cmd.Split(":")(3) & "*" Then

                        'MsgBox(file)

                        Sub_SendFileToSrv(file)
                    End If
                Next

        End Select

    End Sub

    Sub netKernel_SktMsg_sub3(ByVal cmd As String)

        'Heart beat  

        Dim typ As String = Mid(cmd, 1, 1)

        Select Case typ

            Case 0

                sendmsg("heartbeat")

            Case 1

                Try

                    tmLightControler.Stop()

                Catch ex As Exception

                    MsgBox(ex.ToString)

                End Try

            Case 2

                Try

                    tmLightControler.Start()

                Catch ex As Exception

                    MsgBox(ex.ToString)

                End Try

        End Select

    End Sub

    Sub Cmd_StartMonitor()

        Try

            StateSW_btnStartMonitor = True
            btnMonitorbeClick()

        Catch ex As Exception

            MsgBox(ex.ToString)
            dealwithproblem("netKernel_SktMsg_sub1() Error: execute Order ERROR： Kill SoftNVRIA error" & ex.ToString)

        End Try
    End Sub

    Sub Cmd_StopMonitor()

        Try

            StateSW_btnStartMonitor = False
            btnMonitorbeClick()

        Catch ex As Exception

            dealwithproblem("netKernel_SktMsg_sub1() Error: execute Order ERROR： Kill SoftNVRIA error" & ex.ToString)

        End Try
    End Sub

    'Main Timer trick
    '***************************

    'Private Sub tmPrgChk_Tick(sender As Object, e As EventArgs) Handles tmPrgChk.Tick

    '    tmPrgChk_Exec()

    'End Sub

    Sub tmPrgChk_Exec()

        start_MonitorPrgChk_Counter += 1
        txtPrgTimer.Text = start_MonitorPrgChk_Counter

        '  sendmsg(_chkPrgAlive("SoftNVRIA"))
        sendmsg(_chkPrgAlive(TrackSoftwaveName))

    End Sub

    'Private Sub tmResourceInspect_Tick(sender As Object, e As EventArgs) Handles tmResourceInspect.Tick

    '    tmResourceInspect_Exec()

    'End Sub

    Sub tmResourceInspect_Exec()

        OS_CPU_Usage = PerCou.NextValue.ToString("#,##0.0") & " %"
        OS_Mem_Free = (My.Computer.Info.AvailablePhysicalMemory / 2 ^ 20).ToString("#,##0.0") & " MB"
        txtTCPUUsage.Text = OS_CPU_Usage
        txtTMemUsage.Text = OS_Mem_Free
        Try
            If SW_SoftwavePerformanceTrack Then
                NVR_Mem_Usage = (RamUsage.NextValue / 2 ^ 20).ToString("#,##0.0") & " MB"
                NVR_CPU_Usage = (PerCou_App.NextValue / CPU_Quantity).ToString("#,##0.0") & " %"
                txtNvrCPUUsage.Text = NVR_CPU_Usage
                txtNvrMemUsage.Text = NVR_Mem_Usage
            End If

        Catch ex As Exception
            SW_SoftwavePerformanceTrack = False
            dealwithproblem(ex.ToString)

        End Try
        'Log
        XML_Add_Log(0, "", 0, 0, 1)

    End Sub

    'Private Sub tmHeartBeat_Tick(sender As Object, e As EventArgs) Handles tmHeartBeat.Tick

    '    tmHeartBeat_Exec()

    'End Sub

    Sub tmHeartBeat_Exec()

        sendmsg("online")
        'txtReceiver.Text = pf2.NextValue & "%"
        chk_XMLFileName()

    End Sub

    'Private Sub tmRecChk_Tick(sender As Object, e As EventArgs) Handles tmRecChk.Tick



    'End Sub

    Sub tmRecChk_Exec()

        '依據System.ini tmRecChkms執行一次

        start_MonitorRecChk_Counter += 1
        txtRecTimer.Text = start_MonitorRecChk_Counter

        ' MsgBox(start_MonitorRecChk_Counter)
        'sendmsg(_ChkRecord())

        Dim state As Boolean = True

        Dim NoRecCounter As Integer

        For i = 1 To chnMaxNub Step 1

            '***&&&&^^^%%%$$$***&&&&^^^%%%$$$***&&&&^^^%%%$$$***&&&&^^^%%%$$$***&&&&^^^%%%$$$
            '# There have two method to check the record status.
            '# You only can chooce one.
            '# _ChkRecord() is filename-oriented.
            '# _ProChkRecord() is count-oriented.
            '# It choice yourself.
            '***&&&&^^^%%%$$$***&&&&^^^%%%$$$***&&&&^^^%%%$$$***&&&&^^^%%%$$$***&&&&^^^%%%$$$

            Kernal_ProRecChk(i)
            ' Kernal_RecChk(i)

            '***&&&&^^^%%%$$$***&&&&^^^%%%$$$***&&&&^^^%%%$$$***&&&&^^^%%%$$$***&&&&^^^%%%$$$

            If hs_RecStatus(i) = 0 Then

                NoRecCounter += 1
                XML_Add_Log(i, ";hs_RecStatus(i) = " & hs_RecStatus(i) & "Stop Record.", hs_NoRecCounter(i), hs_NoRecComboCounter(i))

                If hs_NoRecCounter(i) = 3 Then

                    Dim sendstrmsg As String = ClientName & ":" & "Channel " & i & " Stop Record. Counter:" & hs_NoRecCounter(i)

                    Sub_SendMsgToMailSrv(sendstrmsg)

                    state = False

                    XML_Add_Log(i, "Send Msg To Mail Server : " & sendstrmsg, hs_NoRecCounter(i), hs_NoRecComboCounter(i))

                End If

                If start_MonitorRecChk_Counter Mod 30 = 0 Then

                    If hs_NoRecSend_key(i) = True Then

                        XML_Add_Log(i, " hs_RecStatus(i) = " & hs_RecStatus(i) & " Stop Record.", hs_NoRecCounter(i), hs_NoRecComboCounter(i))


                        Sub_SendMsgToMailSrv(ClientName & ":" & "Channel " & i & " Stop Record. Counter:" & hs_NoRecCounter(i))

                        state = False

                    End If

                End If

            End If

            updatecounter(i, hs_NoRecCounter(i), hs_NoRecComboCounter(i))

        Next

        If state = True Then

            sendmsg("E000")

        Else

            Dim recStack As String = ""

            For i = 1 To chnMaxNub

                recStack &= (hs_RecStatus(i))

            Next

            sendmsg("E100" & recStack)
        End If
        'Try to Detecting 
        If NoRecCounter > 20 Then

            sendmsg("E199") 'emergency 

        End If

        Try

            StateName = ".\state\" & datedealtominute() & "_SysState.xml"

            dataSset.WriteXml(StateName, XmlWriteMode.IgnoreSchema)

        Catch ex As Exception

            dealwithproblem("Writing State Error:" & ex.ToString)

        End Try

    End Sub

    'Private Sub tmLightControler_Tick(sender As Object, e As EventArgs) Handles tmLightControler.Tick

    '    tmLightControler_Exec()

    'End Sub

    Sub tmLightControler_Exec()

        If StateSW_LightControl = True Then

            If State_CheckerLight = True Then

                lightRunStatus.BackColor = Color.GreenYellow

            Else

                My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Asterisk)
                lightRunStatus.BackColor = Color.Red

            End If

            If State_System = True Then

                lightSysStatus.BackColor = Color.LightGreen

            Else

                My.Computer.Audio.PlaySystemSound(Media.SystemSounds.Asterisk)
                lightSysStatus.BackColor = Color.Red

            End If

            StateSW_LightControl = False

        Else

            lightSysStatus.BackColor = Color.Gray
            lightRunStatus.BackColor = Color.Gray
            StateSW_LightControl = True

        End If

    End Sub

    'Check Program isAlive
    '***************************

    Function _chkPrgAlive(ByVal prgname As String)

        Try
            procsnvr = Process.GetProcessesByName(prgname)

            If procsnvr.Length = 0 Then
                MsgBox("The tracking softwave (" & prgname & ") isn't open.")
                Return 1
            End If
        Catch ex As Exception

            dealwithproblem("_chkPrgAlive() Error:Check Program Alive Error" & ex.ToString)
            Return 0
        End Try

        ' 如果有偵測不到程式，ProAlexMonitor於測5次皆沒回應後，自動啟動程式
        Try

            If procsnvr.LongLength = 0 Then

                PrgStopComboCounter += 1
                PrgStopCounter += 1

                If PrgStopCounter Mod 6 = 0 Then
                    Dim sa As Integer


                    Sub_SendMsgToMailSrv("Program " & prgname & "has not been executing. ProAlex will try to  let it run. ")


                    sa = Process.Start(SoftNVRIAPath).Id


                    Sub_SendMsgToMailSrv("Program " & prgname & " has been restarted. ")


                    Return ("P002:SoftNVRIA")

                End If


            Else
                Try

                    If procsnvr(0).Responding = True Then

                        PrgNoRepComboCounter = 0
                        Return ("P001:SoftNVRIA")

                    Else

                        PrgNoRepCounter += 1
                        PrgNoRepComboCounter += 1
                        If PrgNoRepComboCounter Mod 5 = 0 Then

                            Sub_SendMsgToMailSrv("Program " & prgname & " has been not responding. ProAlex will try to restart. counter = " & PrgNoRepCounter)

                            procsnvr(0).Kill()

                            Process.Start(SoftNVRIAPath)

                            Return ("P003:SoftNVRIA")
                        End If
                        Return ("P003:SoftNVRIA")

                    End If

                Catch ex As Exception


                    dealwithproblem("check Program " & prgname & "error msg " & ex.ToString)

                    Return ("SoftNVRIA_error:" & ex.ToString)

                End Try
                PrgStopCounter = 0

            End If
            'If procsnvr(0).HasExited = True Then


            'End If
        Catch ex As Exception

            dealwithproblem("Program " & prgname & "has some problem. Problem msg : " & ex.ToString)
            Return ("P004:SoftNVRIA")
        End Try

        ' 如果有偵測到程式，但是程式沒有回應，ProAlexMonitor於測5次皆沒回應後，自動重啟程式
        Return 0



    End Function

    'This Part need to resolve the proble "Moth wrong" 

    'Deal with date  & Time format
    '***************************
    Function datedealtominute(Optional ByVal cmd As Integer = 0)

        Dim year As String = Today.Date.Year
        Dim month As String = Today.Date.Month
        Dim day As String = Today.Date.Day
        Dim hour As String = Now.Hour
        Dim minute As String = Now.Minute

        Dim calmonth As Integer
        Dim calday As Integer
        Dim calhour As Integer


        'time minute = 0 mode 
        Select Case cmd

            Case 0

                Select Case Now.Minute
                    Case 11 To 59
                        minute = Now.Minute - 1

                    Case 1 To 10

                        minute = "0" & Now.Minute - 1

                    Case 0

                        If Now.Hour = 0 Then

                            If Now.Date.Day = 1 Then
                                calmonth = Now.Date.Month - 1
                            Else


                            End If
                            calday = Today.Date.Day - 1
                            calhour = 23

                        Else

                            hour = Now.Hour - 1

                        End If

                        minute = 59
                    Case Else
                        sendmsg("OutOfControl:time")
                        Exit Select
                End Select

            Case 1

                Select Case Now.Minute

                    Case 10 To 59

                        minute = Now.Minute

                    Case 1 To 9

                        minute = "0" & Now.Minute

                    Case 0

                        minute = "00"

                    Case Else

                        sendmsg("OutOfControl:time")
                        Exit Select

                End Select
            Case 2

        End Select

        If Now.Hour < 10 Then
            hour = "0" & hour
        End If

        If Now.Day < 10 Then
            day = "0" & day
        End If

        If Now.Month < 10 Then
            month = "0" & month
        End If


        Dim OKdateNTime As String = year & month & day & hour & minute

        Return OKdateNTime

    End Function

    Function datedealtoday()

        Dim year As String = Today.Date.Year
        Dim month As String = Today.Date.Month
        Dim day As String = Today.Date.Day
        Dim hour As String = Now.Hour
        Dim minute As String = Now.Minute
        If Today.Date.Month < 10 Then
            month = "0" & Today.Date.Month
        End If
        If Today.Date.Day < 10 Then
            day = "0" & Today.Date.Day
        End If

        Dim OKdate As String = year & month & day
        Return OKdate

    End Function
    '***************************
    Sub chk_XMLFileName()

        'Check Log File Exist
        Dim machdate As String

        machdate = Path_Startup & "\log\" & datedealtoday() & "_DetectLog.xml"

        If Not machdate = Path_XmlDetectFile Then

            Path_XmlDetectFile = Path_Startup & "\log\" & datedealtoday() & "_DetectLog.xml"
            Initilize_XML()

        End If
        machdate = Path_Startup & "\log\" & datedealtoday() & "_Log.xml"

        If Not machdate = Path_XmlLogFile Then

            Path_XmlLogFile = Path_Startup & "\log\" & datedealtoday() & "_Log.xml"
            Initilize_XML()

        End If

    End Sub

    'DownloadFile -Use network friend
    '***************************
    Function _iniFileTra()

        Try
            MsgBox(_FindMyIP)
            File.Copy(INI_Path, Path_Startup & "\System.ini.bkp")
            If _FindMyIP() <> 1 Then

                My.Computer.Network.DownloadFile("\\172.16.10.8\share\" & _FindMyIP() & "-System.ini", ".\System.ini", "administrator@tcbrt.com", "Yong0107", True, 500, True)
                sendmsg("UPGRADE Sucess.")
                Return 0
            End If

        Catch ex As Exception

            dealwithproblem("_iniFileTra() Error:UPGRADE Fail:" & ex.ToString, 1)
            Return 1

        End Try
        Return 0



    End Function
    '***************************
    'Return Computer IP Address
    '***************************
    Function _FindMyIP()

        Dim ip() As IPAddress = Dns.GetHostAddresses(Dns.GetHostName)
        For Each it As IPAddress In ip
            If it.AddressFamily = AddressFamily.InterNetwork Then
                Return it.ToString
            End If
        Next

        Return 1

    End Function
    '***************************

    'Read system.ini Device Channel Number
    '***************************


    Sub dealwithproblem(ByVal Message As String, Optional ByRef Tsendserver As Boolean = False)

        MsgBox(Message)
        Try

            Initilize_XML()

        Catch ex As Exception

            MsgBox(ex.ToString)

        End Try


        If Tsendserver = True Then
            sendmsg("System Fail:" & Message)
        End If

        XML_Add_Log(0, "SystemError:" & Message, 0, 0)
        'MsgBox("SORRY it's got some problem. Please contact Alex : alexlin5780@gmail.com")
        State_System = False
    End Sub

    '♤Kernal Function♤
    '***************************'***************************
    '***************************'***************************
    '***************************'***************************
    '***************************'***************************
    'filename-oriented
    Sub Kernal_RecChk(ByVal channelnumber As Integer)

        Dim files() As String

        Dim chstring As String

        Dim statu As Boolean = False

        Dim filenamelike As String

        If channelnumber < 10 Then

            chstring = "0" & channelnumber

        Else

            chstring = channelnumber

        End If

        Try

            files = IO.Directory.GetFiles("E:\MoxaRecordData\" & datedealtoday() & "\CH00" & chstring)




            filenamelike = "CH00" & chstring & "T" & datedealtominute()

            For Each g In files
                Dim finame As String
                finame = Path.GetFileName(g)
                If finame Like filenamelike & "*" Then

                    statu = True
                Else
                    statu = False

                End If

            Next

            If statu = False Then

            End If

        Catch exx As DirectoryNotFoundException
            dealwithproblem("DirectoryNotFoundException")
        Catch ex As Exception
            statu = False

            dealwithproblem("Kernal_RecChk() IO ERROR :" & ex.ToString)
        End Try

        If statu = True Then

            hs_RecStatus.Remove(channelnumber)
            hs_RecStatus.Add(channelnumber, 1)
            hs_NoRecComboCounter.Remove(channelnumber)
            hs_NoRecComboCounter.Add(channelnumber, 0)


        Else
            hs_RecStatus.Remove(channelnumber)
            hs_RecStatus.Add(channelnumber, 0)

            Dim ss As Integer
            Dim aa As Integer
            aa = hs_NoRecComboCounter(channelnumber)
            ss = hs_NoRecCounter(channelnumber)
            aa += 1
            ss += 1
            hs_NoRecComboCounter.Remove(channelnumber)
            hs_NoRecComboCounter.Add(channelnumber, aa)
            hs_NoRecCounter.Remove(channelnumber)
            hs_NoRecCounter.Add(channelnumber, ss)

        End If

    End Sub

    'Count-oriented
    Sub Kernal_ProRecChk(ByVal channelnumber As Integer)

        Dim files() As String
        Dim fileslength As Integer
        Dim chstring As String
        Dim statu As Boolean = False
        If channelnumber < 10 Then

            chstring = "0" & channelnumber

        Else

            chstring = channelnumber

        End If

        Try

            files = IO.Directory.GetFiles("E:\MoxaRecordData\" & datedealtoday() & "\CH00" & chstring)
            fileslength = files.Length
            updatecounter(channelnumber, , , fileslength)

            If fileslength > hs_FileQCounter(channelnumber) Then

                statu = True

            Else
                If Now.Hour = 0 And 0 < Now.Minute < 3 Then

                    statu = True
                Else

                    statu = False

                End If
            End If
        Catch exx As DirectoryNotFoundException

            'Fix 0:00 files was not created Error.
            If Now.Hour = 0 And 0 < Now.Minute < 3 Then

                statu = True

            Else

                statu = False
                dealwithproblem("DirectoryNotFoundException")
                XML_Add_Log(channelnumber, "PathNoFound ", hs_NoRecCounter(channelnumber), hs_NoRecComboCounter(channelnumber))

            End If

        Catch ex As Exception

            statu = False
            dealwithproblem("Kernal_RecChk() IO ERROR :" & ex.ToString, False)

        End Try

        Try

            If statu = True Then

                hs_RecStatus.Remove(channelnumber)
                hs_RecStatus.Add(channelnumber, 1)
                hs_NoRecCounter.Remove(channelnumber)
                hs_NoRecCounter.Add(channelnumber, 0)
                hs_FileQCounter.Remove(channelnumber)
                hs_FileQCounter.Add(channelnumber, fileslength)


            Else

                hs_RecStatus.Remove(channelnumber)
                hs_RecStatus.Add(channelnumber, 0)

                Dim ss As Integer
                Dim aa As Integer
                aa = hs_NoRecComboCounter(channelnumber)
                ss = hs_NoRecCounter(channelnumber)
                aa += 1
                ss += 1
                hs_NoRecComboCounter.Remove(channelnumber)
                hs_NoRecComboCounter.Add(channelnumber, aa)
                hs_NoRecCounter.Remove(channelnumber)
                hs_NoRecCounter.Add(channelnumber, ss)
                hs_FileQCounter.Remove(channelnumber)
                hs_FileQCounter.Add(channelnumber, fileslength)

            End If

        Catch ex As Exception

            dealwithproblem("Kernal_RecChk() ERROR :" & ex.ToString, False)

        End Try

    End Sub
    '***************************'***************************
    '***************************'***************************
    '***************************'***************************

    'Transmission File on Binary to Server  (Mix 1MB)
    '***************************
    Sub Sub_SendFileToSrv(ByVal pathnname As String)

        Try
            '  Dim by(SendFileBuffer_byte) As Byte

        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try

        Skt_rTcp_SendFileToSrv = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        FStream_SendFile = New FileStream(pathnname, FileMode.Open, FileAccess.Read)
        BReader = New BinaryReader(FStream_SendFile)

        Try

            sendmsg("BF-" & BReader.BaseStream.Length)
            MsgBox(BReader.BaseStream.Length)
            Skt_rTcp_SendFileToSrv.Connect(Ipnp_MsgSrv_ListenFile)
            Skt_rTcp_SendFileToSrv.Send(BReader.ReadBytes(BReader.BaseStream.Length))
            Skt_rTcp_SendFileToSrv.Close()

        Catch ex As Exception

            dealwithproblem("_tsFile()" & ex.ToString, False)

        End Try


        's.Send(File.ReadAllBytes("Make.png"))


    End Sub
    '***************************

    'tcp_Client Msg Sender

    Public Sub Sub_SendMsgToMailSrv(Senddata As String)

        If IOKey_SendMail = True Then
            ' sendMailkey = False 
            Dim bb() As Byte = Encoding.ASCII.GetBytes("2~@" & Senddata & vbLf)

            Try

                If SW_SendEmailAlert Then
                    NStream_SendMailToMailSrv.Write(bb, 0, bb.Length)
                End If

            Catch ex As Exception

                dealwithproblem("Server close connect :" & ex.ToString, False)

            End Try
        End If


    End Sub

    Sub StartToMailSrvSkt()

        Try
            If Check_Ping(MailSrvIP.ToString) = "Success" Then

                If SW_SendEmailAlert Then

                    Skt_TCP_Cli_Send_MsgToMailSrv = New TcpClient(AddressFamily.InterNetwork)
                    Skt_TCP_Cli_Send_MsgToMailSrv.Connect(Ipnp_MailSrv_ListenMsgAndSend)
                    NStream_SendMailToMailSrv = Skt_TCP_Cli_Send_MsgToMailSrv.GetStream

                    txtDebug.Text += "StartToMailSrvSkt - completed " & vbCrLf
                End If
            Else
                ' MsgBox("Mail Server Unreachable")
                dealwithproblem("Mail Server Unreachable")
            End If


            'MsgBox(skt_TCP_Cli_SendMsgToMailSrv.Client.RemoteEndPoint.ToString)


        Catch ex As Exception
            txtDebug.Text += "StartToMailSrv Error ：" & ex.ToString & vbCrLf
            dealwithproblem("StartToMailSrv Error ：" & ex.ToString, False)

        End Try

    End Sub

    Sub StopToMailSrvSkt()

        Try

            If Skt_TCP_Cli_Send_MsgToMailSrv.Connected Then
                Skt_TCP_Cli_Send_MsgToMailSrv.Close()
            End If
            txtDebug.Text += "StopToMailSrvSkt_Completed on sub" & vbCrLf
        Catch ex As Exception
            txtDebug.Text += "StopToMailSrv Error ：" & ex.ToString & vbCrLf
            dealwithproblem("StopToMailSrv Error ：" & ex.ToString, False)
        End Try

    End Sub

    Sub btnMonitorbeClick()

        If StateSW_btnStartMonitor = True Then

            StartMonitor()
            StateSW_btnStartMonitor = False
        Else

            StopMonitor()
            StateSW_btnStartMonitor = True
        End If

    End Sub

    Private Sub TSMI_StartMonitor_Click(sender As Object, e As EventArgs) Handles TSMI_StartMonitor.Click

        btnMonitorbeClick()

    End Sub

    Private Sub GetMAC()
        'If you want to use this function shorter than below. You should impoort somethings below:
        'imports system.net.networkinformation

        Dim theNetworkInterfaces() As System.Net.NetworkInformation.NetworkInterface = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()

        For Each currentInterface As System.Net.NetworkInformation.NetworkInterface In theNetworkInterfaces
            If currentInterface.NetworkInterfaceType.ToString = NetworkInformation.NetworkInterfaceType.Ethernet.ToString Then
                If currentInterface.GetIPProperties.DnsSuffix.ToString = "localdomain" Then
                    MessageBox.Show(currentInterface.GetPhysicalAddress().ToString())
                End If

            End If


        Next


    End Sub

    Private Function testfile()

        Try
            Return IO.File.GetCreationTime("X:\BRT\6.0 LAB\Raid.docx")
        Catch ex As Exception
            dealwithproblem("testfile()_Error :" & ex.ToString, False)
            Return ex.ToString
        End Try


    End Function

    Public Function testmulti()

        Dim udpClient As New UdpClient(8999)
        ' Creates an IP address to use to join and drop the multicast group.
        Dim multicastIpAddress As IPAddress = IPAddress.Parse("225.0.0.1")

        Try
            ' The packet dies after 50 router hops.

            udpClient.JoinMulticastGroup(multicastIpAddress, 2)

        Catch e As Exception
            Return e.ToString()
        End Try
        Return "1"
    End Function

    Function Check_Ping(ByVal add As String)

        Dim aabuff(1500) As Byte
        Dim ss As NetworkInformation.Ping
        Dim ass As New NetworkInformation.PingOptions(1, False)
        ss = New NetworkInformation.Ping
        Return ss.Send(IPAddress.Parse(add), 20).Status.ToString

        ' MsgBox(ss.Send(IPAddress.Parse("172.16.4.254"), 20, aabuff, ass).Status)

    End Function

    Public Sub trymulticast()

        Dim bu(1000) As Byte
        Dim buin(10000) As Byte
        Dim buout(10000) As Byte
        Dim multisocket As Socket
        multisocket = New Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP)
        Dim pnp As IPEndPoint
        pnp = New IPEndPoint(IPAddress.Parse("127.0.0.1"), 5556)
        Dim ipadd As IPAddress
        ipadd = IPAddress.Parse("225.21.0.6")
        multisocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, New MulticastOption(ipadd))
        multisocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 1)
        Dim ipep As New IPEndPoint(ipadd, 5556)
        Try
            multisocket.Bind(pnp)
            multisocket.IOControl(IOControlCode.ReceiveAll, buin, buout)
            multisocket.Connect(ipep)
            ' multisocket.Receive(bu)

            MsgBox(bu.ToString)

        Catch ex2 As SocketException

            dealwithproblem("trymulticast Error : " & ex2.ToString)

        Catch ex As Exception

            dealwithproblem("trymulticast Error : " & ex.ToString)
        End Try




    End Sub

    'End Class 'Receive
    Public Sub Write_Registry(ByVal sSegment As String, ByVal sRegName As String, ByVal sRegValue As Object)

        Dim rRoot As RegistryKey
        Dim rReg As RegistryKey
        rRoot = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\", True)
        rReg = IIf(sSegment = "", rRoot, rRoot.OpenSubKey(sSegment, True))
        rReg.SetValue(sRegName, sRegValue)
        rReg.Close()
        rRoot.Close()

    End Sub

    Private Sub ReadRegistry()

        Dim rRoot As RegistryKey
        Dim rLC As RegistryKey

        rRoot = Registry.LocalMachine.OpenSubKey("SOFTWARE\KYO\Test1", False)
        rRoot = Registry.CurrentUser.OpenSubKey("Software\Microsoft\Windows\CurrentVersion\")
        rLC = rRoot.OpenSubKey("Run")

        MsgBox(rLC.GetValue("Line", 1))

    End Sub

    Private Sub TSMI_TestMode_Click(sender As Object, e As EventArgs) Handles TSMI_TestMode.Click

        If SW_TestMode = True Then

            Panel1.Visible = False
            Me.Size = New System.Drawing.Size(782, 383)
            SW_TestMode = False

        Else

            Panel1.Visible = True
            Me.Size = New System.Drawing.Size(766, 462)
            SW_TestMode = True

        End If


    End Sub

    Dim ssasdf As Queue = New Queue

    '***************************
    Private Sub btnDWProblem_Click(sender As Object, e As EventArgs) Handles btnDWProblem.Click

        State_System = True
        ' Check_Ping("172.16.4.250")
        ' pf2 = New PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total")
        'MsgBox(pf2.NextValue)
    End Sub

    Sub Aencrypt(ByVal mes As String)

        ssasd = Encoding.UTF32.GetBytes(mes)
        For i = 0 To ssasd.Length - 1

            MsgBox(ssasd(i))

        Next
    End Sub


    'Need to know what .
    'Function Adecrypt(ByVal mes As String)

    'End Function

    Dim ssasd() As Byte


#Region "參考"

#Region "Send Mail"
    'Sub sendmaaaail()


    '    Dim mail As New MailMessage()
    '    Dim cred As New NetworkCredential("Alex@tcbrt.com", "highboy5780")
    '    '收件者
    '    mail.[To].Add("hwacombrt@gmail.com")
    '    mail.Subject = "subject"
    '    '寄件者
    '    mail.From = New System.Net.Mail.MailAddress("ProAlex@ProAlex.com")
    '    mail.IsBodyHtml = True
    '    mail.Body = "message"
    '    '設定SMTP
    '    Dim smtp As New SmtpClient("172.16.3.21")
    '    smtp.UseDefaultCredentials = False
    '    smtp.EnableSsl = False
    '    'smtp.EnableSsl = True
    '    'smtp.Credentials = cred
    '    smtp.Port = 25
    '    '送出Mail
    '    Try
    '        smtp.Send(mail)
    '    Catch ex As Exception
    '        MsgBox(ex.ToString)
    '    End Try
    'End Sub
#End Region

#Region "initializeProAlex"
    ' Sub initializeProAlex()


    ''form Alex************************
    'Dim inird() As String
    'inird = File.ReadAllLines("system.ini")
    'Dim inidate As String = vbNullString
    'Dim nu As Integer = 0

    'Using Inputini As StreamReader = New StreamReader("system.ini")
    '    Dim data As String
    '    Do Until Inputini.EndOfStream
    '        data = Inputini.ReadLine
    '        If data Like "[" Then
    '            MsgBox(" config= ")
    '            Do Until data = "EOF"
    '                data = Inputini.ReadLine
    '                If data = "EOF" Then
    '                    Exit Do
    '                Else
    '                    MsgBox(data)
    '                End If

    '            Loop

    '        End If

    '    Loop
    'End Using
    ''For Each File In inird
    ''    nu = nu + 1
    ''    If File = "[NVR-A]" Then

    ''    End If
    ''    inidate = inidate & ":" & File
    ''Next
    ''MsgBox(inidate)
    ''form Alex************************
    ' End Sub
#End Region

#Region "Socket Write File"
    '******************************Socket Write File*******************************
    'Dim by(1024000) As Byte
    'Dim s = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    'Dim ipnp = New IPEndPoint(IPAddress.Parse(SrvIP), SrvRecFilePort)
    '    s.Connect(ipnp)
    'Dim gg As New FileStream(pathnname, FileMode.Open, FileAccess.Read)
    'Dim ga As New BinaryReader(gg)

    '    s.Send(ga.ReadBytes(ga.BaseStream.Length))
    ''s.Send(File.ReadAllBytes("Make.png"))
    '    s.Close()
    '******************************Socket Write File*******************************
#End Region

#Region "debugmode"
    '    Sub debugmode(ByVal Pprocessid As Integer, Optional ByVal msg As String = "")
    'stt:
    '        If Debugiokey = True Then
    '            Debugiokey = False
    '            Try
    '                If debugswitch = False Then
    '                    Exit Sub
    '                End If
    '                Using r As IO.StreamWriter = New IO.StreamWriter(".\debug.txt", True)


    '                    Select Case Pprocessid
    '                        Case 0
    '                            r.WriteLine(Now & "[Kernel]" & msg)
    '                        Case 1
    '                            r.WriteLine(Now & "[NetKernel]" & msg)
    '                    End Select
    '                End Using
    '            Catch ex As Exception
    '                dealwithproblem("debugmode() Error:" & ex.ToString, False)

    '            Finally
    '                Debugiokey = True
    '            End Try
    '        Else
    '            'MsgBox("execut goto and iokey value is " & Debugiokey)
    '            GoTo stt
    '        End If


    '    End Sub
#End Region

#End Region

End Class
