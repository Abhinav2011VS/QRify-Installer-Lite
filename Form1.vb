Imports System.Net
Imports System.ComponentModel
Imports System.IO
Imports Newtonsoft.Json.Linq
Imports System.Reflection.Emit
Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class Form1
    Private filePath As String
    Private url As String

    ' GitHub repository API URL for the latest release
    Private repoUrl As String = "https://api.github.com/repos/Abhinav2011VS/QRify-App/releases/latest"

    ' Form Load event to set the title and initiate the download
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Set the form title to "Qrify Lite Installer"
        Me.Text = "Qrify Lite Installer"

        ' Create a temporary file path to store the downloaded installer
        filePath = Path.Combine(Path.GetTempPath(), "Qrify-Lite-installer.exe")

        ' Fetch the latest release data from GitHub
        FetchLatestRelease()
    End Sub

    ' Fetch the latest release data from GitHub using the API
    Private Sub FetchLatestRelease()
        Using client As New WebClient()
            ' Set user-agent for GitHub API requests
            client.Headers.Add("User-Agent", "VB-Downloader")
            AddHandler client.DownloadStringCompleted, AddressOf DownloadReleaseInfoCompleted

            ' Start the request to get the latest release info
            client.DownloadStringAsync(New Uri(repoUrl))
        End Using
    End Sub

    ' Callback when the release info is fetched from GitHub
    Private Sub DownloadReleaseInfoCompleted(sender As Object, e As DownloadStringCompletedEventArgs)
        If e.Error IsNot Nothing Then
            Label1.Text = "Error fetching release info."
            Return
        End If

        Try
            ' Parse the JSON response to get the download URL for the .exe file
            Dim jsonResponse As JObject = JObject.Parse(e.Result)
            Dim assets = jsonResponse("assets")

            ' Loop through the assets to find the .exe file
            For Each asset In assets
                Dim assetName = asset("name").ToString()
                If assetName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) Then
                    url = asset("browser_download_url").ToString()
                    StartDownload()  ' Start downloading the .exe file
                    Exit For
                End If
            Next

            If String.IsNullOrEmpty(url) Then
                Label1.Text = "No .exe file found in the latest release."
            End If
        Catch ex As Exception
            Label1.Text = "Error parsing release info."
        End Try
    End Sub

    ' Start the download using the fetched URL
    Private Sub StartDownload()
        If String.IsNullOrEmpty(url) Then Return

        Using client As New WebClient()
            AddHandler client.DownloadProgressChanged, AddressOf DownloadProgressChanged
            AddHandler client.DownloadFileCompleted, AddressOf DownloadFileCompleted

            ' Start downloading the file asynchronously
            client.DownloadFileAsync(New Uri(url), filePath)
        End Using
    End Sub

    ' Update progress bar and label during download
    Private Sub DownloadProgressChanged(sender As Object, e As DownloadProgressChangedEventArgs)
        ProgressBar1.Value = e.ProgressPercentage
        Label1.Text = $"Downloaded {e.BytesReceived / 1024} KB of {e.TotalBytesToReceive / 1024} KB. {e.ProgressPercentage}% complete."
    End Sub

    ' Callback when the download is completed
    Private Sub DownloadFileCompleted(sender As Object, e As AsyncCompletedEventArgs)
        If e.Error IsNot Nothing Then
            Label1.Text = "Error downloading file."
        Else
            Label1.Text = "Download completed!"
            Process.Start(filePath)  ' Run the downloaded installer

            ' Close the form after the installer runs
            Me.Close()
        End If
    End Sub

    ' Cleanup temporary file after the form is closed
    Private Sub Form1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        ' Delete the temporary file after closing the form
        If File.Exists(filePath) Then
            Try
                File.Delete(filePath)
            Catch ex As Exception
                ' Handle any exceptions that may occur during file deletion (optional)
            End Try
        End If
    End Sub
End Class
