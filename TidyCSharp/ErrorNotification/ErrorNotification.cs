using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;

namespace Geeks.GeeksProductivityTools
{
	internal static class ErrorNotification
	{
		static IVsOutputWindowPane customPane;

		internal static void EmailError(string message)
		{
		}

		static ErrorNotification()
		{
			IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

			Guid customGuid = new Guid("0F44E2D1-F5FA-4d2d-AB30-22BE8ECD9789");
			string customTitle = "TidyC# Debug Output";
			outWindow.CreatePane(ref customGuid, customTitle, 1, 1);

			outWindow.GetPane(ref customGuid, out customPane);
		}
		internal static void WriteErrorToOutputWindow(Exception ex)
		{
			customPane.OutputString(Newtonsoft.Json.JsonConvert.SerializeObject(ex) + "\n");
			customPane.Activate();
		}
		internal static void WriteErrorToFile(Exception ex)
		{
			File.AppendAllText(Path.GetDirectoryName(App.DTE.Solution.FileName) + "\\Tidy.Error.log", Newtonsoft.Json.JsonConvert.SerializeObject(ex));
		}

		internal static void EmailError(Exception ex)
		{
			//System.Windows.Forms.MessageBox.Show(ex.Message);

			// TODO: Add feature to Settings for sending the error log to the dev team if requested by end users

			// var message = new MailMessage("Geeks.Productivity.Tools@gmail.com",
			//                              "ali.ashoori@geeks.ltd.uk",
			//                              $"Error from Geeks Productivity Tools {DateTime.Now.Date}", GenerateErrorBody(ex))
			// {
			//    IsBodyHtml = true
			// };

			// var client = new SmtpClient("smtp.gmail.com", 587)
			// {
			//    Credentials = new NetworkCredential("Geeks.Productivity.Tools@gmail.com", "Espresso123"),
			//    EnableSsl = true
			// };
			// client.Send(message);
		}

		static string GenerateErrorBody(Exception e)
		{
			return "<span style='font-weight: bold'>Message:</span>" + "<p>" + e.Message + "</p>" + "</br>" +
				   "<span style='font-weight: bold'>Inner exception message:</span>" + "<p>" + e.InnerException?.Message + "</p>" + "</br>" +
				   "<span style='font-weight: bold'>Stack Trace:</span>" + "<p>" + e.StackTrace + "</p>";
		}
	}
}