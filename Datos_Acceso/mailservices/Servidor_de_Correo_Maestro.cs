using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Datos_Acceso.MailServices
{
    public abstract class Servidor_de_Correo_Maestro
    {
        private SmtpClient smtpClient;
        protected string senderMail { get; set; }
        protected string password { get; set; }
        protected string host { get; set; }
        protected int port { get; set; }
        protected bool ssl { get; set; }

        protected void InicializarSMTP()
        {
            smtpClient = new SmtpClient(host, port);
            smtpClient.EnableSsl = ssl;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(senderMail, password);
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.Timeout = 15000;
        }

        public bool EnviarCorreo(string asunto, string cuerpo, List<string> destinatarios, out string error)
        {
            error = null;
            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderMail),
                Subject = asunto,
                Body = cuerpo,
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8,
                IsBodyHtml = false,
                Priority = MailPriority.Normal
            };
            foreach (var to in destinatarios) mailMessage.To.Add(to);
            try
            {
                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
            finally
            {
                mailMessage.Dispose();
                smtpClient.Dispose();
            }
        }
    }
}
