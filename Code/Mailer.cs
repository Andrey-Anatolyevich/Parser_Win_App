using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Code {

    /// <summary>
    /// Класс для отправки писем на почту
    /// </summary>
    public class Mailer {
        #region Переменные класса
        string SmtpHost;
        int SmtpPort;
        bool SslEnabled;
        string Login;
        string Password;
        string SenderEmail;
        string TargetEmail;
        string MessageTitle;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="smtpHost">SMTP address</param>
        /// <param name="smtpPort">SMTP port</param>
        /// <param name="sslEnabled">SSL is enabled</param>
        /// <param name="senderEmail">Address, message is sent from</param>
        /// <param name="senderPassword">Password of the account</param>
        /// <param name="targetEmail">Where to send message</param>
        /// <param name="messageTitle">Title of the message</param>
        public Mailer(string smtpHost, int smtpPort, bool sslEnabled, string senderEmail, 
            string senderPassword, string targetEmail, string messageTitle) {
            SmtpHost = smtpHost;
            SmtpPort = smtpPort;
            SslEnabled = sslEnabled;
            Login = senderEmail;
            Password = senderPassword;
            SenderEmail = senderEmail;
            TargetEmail = targetEmail;
            MessageTitle = messageTitle;
        }
        // send a message to an e-mail
        public void SendString (string message, int totalUnits) {
            SmtpClient client = new SmtpClient();
            client.Host = SmtpHost;
            client.Port = SmtpPort;
            client.EnableSsl = SslEnabled;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential(Login, Password);

            string Title = MessageTitle + " " + totalUnits + " шт. " + string.Format("{0:yyyy.MM.dd HH:mm}", DateTime.Now);
            MailMessage mm = new MailMessage(SenderEmail, TargetEmail, Title , message);
            mm.IsBodyHtml = true;
            mm.BodyEncoding = Encoding.UTF8;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);
        }
    }
}
