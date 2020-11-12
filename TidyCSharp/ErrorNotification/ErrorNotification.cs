using System;

namespace Geeks.GeeksProductivityTools
{
    internal static class ErrorNotification
    {
        internal static void EmailError(string message)
        {
        }

        internal static void EmailError(Exception ex)
        {
            System.Windows.Forms.MessageBox.Show(ex.Message);

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