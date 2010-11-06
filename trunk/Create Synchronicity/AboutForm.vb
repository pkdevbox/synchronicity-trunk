﻿'This file is part of Create Synchronicity.
'
'Create Synchronicity is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'Create Synchronicity is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with Create Synchronicity.  If not, see <http://www.gnu.org/licenses/>.
'Created by:	Clément Pit--Claudel.
'Web site:		http://synchronicity.sourceforge.net.

Public Class AboutForm
    Private Sub SetLinkArea(ByVal Link As LinkLabel)
        If Link.Text.IndexOf("\") = -1 Or Link.Text.IndexOf("/") = -1 Then Exit Sub

        Dim Area As New LinkArea
        Area.Start = Link.Text.IndexOf("\")
        Link.Text = Link.Text.Remove(Area.Start, 1)
        Area.Length = Link.Text.IndexOf("/") - Area.Start
        Link.Text = Link.Text.Remove(Area.Start + Area.Length, 1)
        Link.LinkArea = Area
    End Sub

    Private Sub About_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Translation.TranslateControl(Me)

        About_VersionInfo.Text = About_VersionInfo.Text.Replace("%version%", Application.ProductVersion)

        SetLinkArea(About_BugReport)
        SetLinkArea(About_ContactLink)
        SetLinkArea(About_LinkToLicense)
        SetLinkArea(About_LinkToProductPage)
        SetLinkArea(About_LinkToWebsite)
        SetLinkArea(About_VersionInfo)

        ProgramConfig.LoadProgramSettings()
        Translation.FillLanguageListBox(About_LanguagesList)
        About_Updates.Checked = ProgramConfig.GetProgramSetting(ConfigOptions.AutoUpdates, "False")
    End Sub

    Private Sub About_LinkToProductPage_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles About_LinkToProductPage.LinkClicked
        StartProcess("http://synchronicity.sourceforge.net/")
    End Sub

    Private Sub About_LinkToWebsite_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles About_LinkToWebsite.LinkClicked
        StartProcess("http://createsoftware.users.sourceforge.net/")
    End Sub

    Private Sub About_VersionInfo_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles About_VersionInfo.LinkClicked
        Updates.CheckForUpdates(False)
    End Sub

    Private Sub About_ContactLink_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles About_ContactLink.LinkClicked
        StartProcess("http://createsoftware.users.sourceforge.net/")
    End Sub

    Private Sub About_LinkToLicense_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles About_LinkToLicense.LinkClicked
        StartProcess("http://www.gnu.org/licenses/gpl.html")
    End Sub

    Private Sub About_BugReport_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles About_BugReport.LinkClicked
        StartProcess("http://sourceforge.net/tracker/?group_id=264348&atid=1130882")
    End Sub

    Private Sub AboutForm_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        If About_LanguagesList.SelectedIndex <> -1 Then
            Dim SelectedLanguage As String = About_LanguagesList.SelectedItem.ToString.Split("-")(0).Trim
            Dim LanguageChanged As Boolean = ProgramConfig.GetProgramSetting(ConfigOptions.Language, ConfigOptions.DefaultLanguage) <> SelectedLanguage

            ProgramConfig.SetProgramSetting(ConfigOptions.Language, SelectedLanguage)

            If LanguageChanged Then
                ReloadNeeded = True
                Translation = LanguageHandler.GetSingleton(True)
            End If
        End If

        ProgramConfig.SetProgramSetting(ConfigOptions.AutoUpdates, About_Updates.Checked)
        ProgramConfig.SaveProgramSettings()
    End Sub

    Sub StartProcess(ByVal Address As String)
        Try
            Diagnostics.Process.Start(Address)
        Catch
        End Try
    End Sub
End Class
