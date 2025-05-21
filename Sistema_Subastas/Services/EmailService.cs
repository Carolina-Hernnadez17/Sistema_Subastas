using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace Sistema_Subastas.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void EnviarCorreo(string destino, string asunto, string cuerpo)
        {
            var smtp = new SmtpClient(_config["EmailSettings:SmtpServer"])
            {
                Port = int.Parse(_config["EmailSettings:SmtpPort"]),
                Credentials = new NetworkCredential(
                    _config["EmailSettings:SenderEmail"],
                    _config["EmailSettings:SenderPassword"]
                ),
                EnableSsl = true
            };

            var mensaje = new MailMessage
            {
                From = new MailAddress(_config["EmailSettings:SenderEmail"], _config["EmailSettings:SenderName"]),
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = true
            };

            mensaje.To.Add(destino);
            smtp.Send(mensaje);
        }
    }
}
