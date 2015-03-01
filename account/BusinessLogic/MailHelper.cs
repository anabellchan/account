using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using account.ViewModels;

namespace account.BusinessLogic
{
    public class MailHelper
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public MailHelper(string to, string subject, string body)
        {
            To = "anabellchan@gmail.com";
            Subject = subject;
            Body = body;
        }


        // This value may/may not be constant. 
        // To get started use one of your email 
        // addresses.
        public bool EmailFromArvixe()
        {

            // Use credentials of the Mail account that you created with the steps above.
            const string FROM = "admin@anabellchan.com";
            const string FROM_PWD = "admin123";
            const bool USE_HTML = true;

            // Get the mail server obtained in the steps described above.
            const string SMTP_SERVER = "mail.anabellchan.com.BROWN.mysitehosted.com";
            try
            {
                MailMessage mailMsg = new MailMessage(FROM, To);
                mailMsg.Subject = Subject;
                mailMsg.Body = Body + "<br/>sent by Arvyx";
                mailMsg.IsBodyHtml = USE_HTML;

                SmtpClient smtp = new SmtpClient();
                smtp.Port = 25;
                smtp.Host = SMTP_SERVER;
                smtp.Credentials = new System.Net.NetworkCredential(FROM, FROM_PWD);
                smtp.Send(mailMsg);
            }
            catch 
            {
                return false;
            }
            return true;
        }
    }
}
