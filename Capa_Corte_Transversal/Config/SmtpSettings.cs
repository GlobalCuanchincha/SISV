using System;
using System.Configuration;

namespace Capa_Corte_Transversal.Config
{
    public sealed class SmtpSettings
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public bool EnableSsl { get; private set; }
        public string User { get; private set; }
        public string Pass { get; private set; }
        public string FromEmail { get; private set; }
        public string FromName { get; private set; }

        public static SmtpSettings FromAppConfig()
        {
            string host = ConfigurationManager.AppSettings["Smtp.Host"];
            string portStr = ConfigurationManager.AppSettings["Smtp.Port"];
            string sslStr = ConfigurationManager.AppSettings["Smtp.EnableSsl"];
            string user = ConfigurationManager.AppSettings["Smtp.User"];
            string pass = ConfigurationManager.AppSettings["Smtp.Pass"];
            string fromEmail = ConfigurationManager.AppSettings["Smtp.FromEmail"];
            string fromName = ConfigurationManager.AppSettings["Smtp.FromName"];

            if (string.IsNullOrWhiteSpace(host)) throw new Exception("Falta Smtp.Host en App.config");
            if (!int.TryParse(portStr, out int port)) throw new Exception("Smtp.Port inválido");
            if (!bool.TryParse(sslStr, out bool ssl)) ssl = true;

            if (string.IsNullOrWhiteSpace(fromEmail)) fromEmail = user;
            if (string.IsNullOrWhiteSpace(fromName)) fromName = "SISV";

            return new SmtpSettings
            {
                Host = host,
                Port = port,
                EnableSsl = ssl,
                User = user,
                Pass = pass,
                FromEmail = fromEmail,
                FromName = fromName
            };
        }
    }
}
