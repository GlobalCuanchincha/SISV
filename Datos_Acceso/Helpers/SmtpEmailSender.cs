using System.Net;
using System.Net.Mail;
using Capa_Corte_Transversal.Config;

namespace Capa_Corte_Transversal.Helpers
{
    public sealed class SmtpEmailSender
    {
        private readonly SmtpSettings _cfg;

        public SmtpEmailSender(SmtpSettings cfg)
        {
            _cfg = cfg;
        }

        public void Send(string toEmail, string subject, string body)
        {
            using (var msg = new MailMessage())
            {
                msg.From = new MailAddress(_cfg.FromEmail, _cfg.FromName);
                msg.To.Add(new MailAddress(toEmail));
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = false;

                using (var client = new SmtpClient(_cfg.Host, _cfg.Port))
                {
                    client.EnableSsl = _cfg.EnableSsl;
                    client.Credentials = new NetworkCredential(_cfg.User, _cfg.Pass);
                    client.Send(msg);
                }
            }
        }
    }
}
