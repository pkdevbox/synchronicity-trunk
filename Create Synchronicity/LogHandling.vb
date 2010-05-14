﻿'This file is part of Create Synchronicity.
'
'Create Synchronicity is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
'Create Synchronicity is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'You should have received a copy of the GNU General Public License along with Create Synchronicity.  If not, see <http://www.gnu.org/licenses/>.
'Created by:	Clément Pit--Claudel.
'Web site:		http://synchronicity.sourceforge.net.

Structure LogItem
    Dim Item As SyncingItem
    Dim Side As SideOfSource
    Dim Success As Boolean
    'Dim ErrorId As Integer

    Sub New(ByVal _Item As SyncingItem, ByVal _Side As SideOfSource, ByVal _Success As Boolean) ', Optional ByVal _ErrorId As Integer = -1)
        Item = _Item : Side = _Side : Success = _Success ' : ErrorId = _ErrorId
    End Sub
End Structure

Class LogHandler
    Dim LogName As String
    Public Errors As List(Of Exception)
    Public Log As List(Of LogItem)
#If DEBUG Then
    Public DebugInfo As List(Of String)
#End If

    Dim Translation As LanguageHandler = LanguageHandler.GetSingleton
    Dim ProgramConfig As ConfigHandler = ConfigHandler.GetSingleton

    Private Disposed As Boolean

    Sub New(ByVal _LogName As String)
        IO.Directory.CreateDirectory(ProgramConfig.LogRootDir)

        Disposed = False
        LogName = _LogName
        Errors = New List(Of Exception)
        Log = New List(Of LogItem)

#If DEBUG Then
        DebugInfo = New List(Of String)
#End If
    End Sub

    Sub HandleError(ByVal Ex As Exception, Optional ByVal Details As String = "")
        If TypeOf (Ex) Is Threading.ThreadAbortException Then Exit Sub
        If Not Details = "" Then Ex = New Exception(Ex.Message & Microsoft.VisualBasic.vbNewLine & Details, Ex)
        Errors.Add(Ex)
    End Sub

    Sub LogAction(ByVal Item As SyncingItem, ByVal Side As SideOfSource, ByVal Success As Boolean)
        Log.Add(New LogItem(Item, Side, Success))
    End Sub

#If DEBUG Then
    Sub LogInfo(ByVal Info As String)
        DebugInfo.Add(Info)
    End Sub
#End If

    Sub OpenHTMLHeaders(ByRef LogW As IO.StreamWriter)
        LogW.WriteLine("<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">")
        LogW.WriteLine("<html xmlns=""http://www.w3.org/1999/xhtml"" xml:lang=""en"" encoding=""utf-8"">")
        LogW.WriteLine("	<head>")
        LogW.WriteLine("		<title>Create Synchronicity - Log for " & LogName & "</title>")
        LogW.WriteLine("		<meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"" />")
        LogW.WriteLine("		<style type=""text/css"">")
        LogW.WriteLine("			body {")
        LogW.WriteLine("				font-family: verdana, courier;")
        LogW.WriteLine("				font-size: 0.8em;")
        LogW.WriteLine("				margin: auto;")
        LogW.WriteLine("				width: 60%;")
        LogW.WriteLine("			}")
        LogW.WriteLine("			")
        LogW.WriteLine("			table {")
        LogW.WriteLine("				border-collapse: collapse;")
        LogW.WriteLine("			    width: 100%;")
        LogW.WriteLine("			}")
        LogW.WriteLine("			")
        LogW.WriteLine("			th, td {")
        LogW.WriteLine("				border: solid grey;")
        LogW.WriteLine("				border-width: 1px 0 0 0;")
        LogW.WriteLine("				padding: 1em;")
        LogW.WriteLine("			}")
        LogW.WriteLine("		</style>")
        LogW.WriteLine("	</head>")
        LogW.WriteLine("	<body>")
        LogW.WriteLine(String.Format(Translation.Translate("\LOG_TITLE"), LogName))
    End Sub

    Sub CloseHTMLHeaders(ByRef LogW As IO.StreamWriter)
        LogW.WriteLine("	</body>")
        LogW.WriteLine("</html>")
      End Sub

    Sub PutLine(ByVal Title As String, ByVal Contents As String, ByRef LogW As IO.StreamWriter)
#If DEBUG Then
        LogW.WriteLine(Title & "	" & Contents.Replace(" -> ", "	"))
#Else
        LogW.WriteLine("<tr><td>" & Title & "</td><td>" & Contents & "</td></tr>")
#End If
    End Sub

    Sub SaveAndDispose(ByVal Left As String, ByVal Right As String)
        If Disposed Then Exit Sub
        Disposed = True

        Try
            Dim NewLog As Boolean = Not IO.File.Exists(ProgramConfig.GetLogPath(LogName))
            'TODO: </body> and </html> tags
            Dim LogWriter As New IO.StreamWriter(ProgramConfig.GetLogPath(LogName), True)

            Try
#If Not DEBUG Then
                If NewLog Then OpenHTMLHeaders(LogWriter)
                LogWriter.WriteLine("<h2>" & Microsoft.VisualBasic.DateAndTime.DateString & ", " & Microsoft.VisualBasic.DateAndTime.TimeString & "</h2>")
                LogWriter.WriteLine("<p>")
#End If

                LogWriter.WriteLine(String.Format("{0}: {1}", Translation.Translate("\LEFT"), Left))
#If Not DEBUG Then
                LogWriter.WriteLine("<br />")
#End If
                LogWriter.WriteLine(String.Format("{0}: {1}", Translation.Translate("\RIGHT"), Right))

#If Not DEBUG Then
                LogWriter.WriteLine("</p>")
                LogWriter.WriteLine("<table>") '<tr><th>Type</th><th>Contents</th></tr>
#End If

#If DEBUG Then
                For Each Info As String In DebugInfo
                    PutLine("Info", Info, LogWriter)
                Next
#End If
                For Each Record As LogItem In Log
                    PutLine(If(Record.Success, Translation.Translate("\SUCCEDED"), Translation.Translate("\FAILED")), String.Join(" -> ", New String() {Record.Item.FormatType(), Record.Item.FormatAction(), Record.Item.FormatDirection(Record.Side), Record.Item.Path}), LogWriter)
                Next
                For Each Ex As Exception In Errors
                    PutLine(Translation.Translate("\ERROR"), String.Join(" -> ", New String() {Ex.Message, Ex.StackTrace.Replace(Microsoft.VisualBasic.vbNewLine, "\n")}), LogWriter)
                Next

#If Not DEBUG Then
                LogWriter.WriteLine("</table>")
                If NewLog Then CloseHTMLHeaders(LogWriter)
#End If
            Catch Ex As Exception
                Warning(Translation.Translate("\LOGFILE_WRITE_ERROR"), Ex)

            Finally
                LogWriter.Flush()
                LogWriter.Close()
                LogWriter.Dispose()
            End Try
        Catch Ex As Exception
            Warning(Translation.Translate("\LOGFILE_OPEN_ERROR"), Ex)
        End Try
    End Sub

    Sub Warning(ByVal Message As String, Optional ByVal Ex As Exception = Nothing)
        Interaction.ShowMsg(Message & Microsoft.VisualBasic.vbNewLine & If(Ex Is Nothing, "", Ex.Message))
    End Sub
End Class