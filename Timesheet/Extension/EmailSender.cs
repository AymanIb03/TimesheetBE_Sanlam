using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Timesheet.Extension
{
    public class EmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendEmailAsync(string email, string subject, string messageType, string callbackUrl = null, string userName = null, string password = null)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var portString = _configuration["EmailSettings:Port"];
            var username = _configuration["EmailSettings:Username"];
            var appPassword = _configuration["EmailSettings:AppPassword"];

            if (string.IsNullOrWhiteSpace(smtpServer))
                throw new ArgumentNullException(nameof(smtpServer), "SMTP Server is not configured");
            if (string.IsNullOrWhiteSpace(portString))
                throw new ArgumentNullException(nameof(portString), "Port is not configured");
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username), "Username is not configured");
            if (string.IsNullOrWhiteSpace(appPassword))
                throw new ArgumentNullException(nameof(appPassword), "App Password is not configured");

            if (!int.TryParse(portString, out int port))
                throw new ArgumentException("Invalid port number", nameof(portString));

            var client = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(username, appPassword),
                EnableSsl = true
            };

            string htmlMessage = string.Empty;

            // Construire le message en fonction du type
            if (messageType == "ResetPassword")
            {
                htmlMessage = $@"
        <html>
        <body>
            <div style='font-family: Arial, sans-serif; color: #333;'>
                <h2>Bonjour,</h2>
                <p>Vous avez demandé à réinitialiser votre mot de passe. Veuillez cliquer sur le bouton ci-dessous pour réinitialiser votre mot de passe :</p>
                <a href='{callbackUrl}' style='display: inline-block; padding: 10px 20px; font-size: 16px; color: #fff; background-color: #007bff; text-decoration: none; border-radius: 5px;'>Réinitialiser votre mot de passe</a>
                <p>Si vous n'avez pas demandé cette réinitialisation, veuillez ignorer cet e-mail.</p>
            </div>
        </body>
        </html>";
            }
            else if (messageType == "CreateUser")
            {
                htmlMessage = $@"
        <html>
        <body>
            <div style='font-family: Arial, sans-serif; color: #333;'>
                <h2>Bonjour {userName},</h2>
                <p>Votre compte a été créé avec succès.</p>
                <p>Nom d'utilisateur: {userName}</p>
                <p>Mot de passe: {password}</p>
                <p>Veuillez vous connecter et changer votre mot de passe dès que possible.</p>
                <p>Cordialement,</p>
                <p>L'équipe Timesheet</p>
            </div>
        </body>
        </html>";
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }



    }
}
