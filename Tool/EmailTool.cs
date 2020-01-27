using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Tool
{
    class EmailTool
    {
        public static bool SendMail(string sendTo, string objectMail, string messageMail, MemoryStream attachment = null, string replyTo = "")
        {
            try
            {
                using (MailMessage message = new MailMessage())
                {
                    string mailInfo = "mailAdressSender";
                    //TODO mail
                    if (replyTo == "")
                        replyTo = mailInfo;
                    message.From = new MailAddress(mailInfo);
                    message.To.Add(sendTo);
                    message.Subject = objectMail;
                    message.ReplyToList.Add(replyTo);
                    message.Body = messageMail;
                    message.IsBodyHtml = false;
                    if (attachment != null)
                    {
                        attachment.Position = 0;
                        Attachment data = new Attachment(attachment, "event.ics", "text/calendar");
                        message.Attachments.Add(data);
                    }

                    using SmtpClient client = new SmtpClient
                    {
                        Host = "HostServerMail",
                        Port = 587,
                        EnableSsl = false,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential("sender", "passwordMail"),
                    };
                    client.Send(message);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
