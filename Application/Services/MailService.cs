using Application.Domain.Models;
using Google.Api.Gax.ResourceNames;
using MailKit.Net.Smtp;
using Microsoft.CodeAnalysis;
using MimeKit;

namespace Application.Services
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendMailForNewProjectCreated(string projectId, string projectName, string memberEmail, string memberFullname)
        {
            var mailHtml = "<!DOCTYPE html><html><head>    <meta charset=\"utf-8\" />    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />    <style>        body {            font-family: Arial, sans-serif;            background-color: #f0f0f0;            margin: 0;            padding: 0;        }        .container {            max-width: 600px;            margin: 0 auto;        }        .header {            background-color: #007bff;            color: white;            padding: 20px;            text-align: center;        }        .content {            background-color: white;            padding: 20px;        }        .footer {            background-color: #e9ecef;            padding: 10px;            text-align: center;        }        .password {            font-weight: bold;            font-size: 24px;            color: #007bff;        }    </style></head><body>    <div class=\"container\">        <div class=\"header\">            <h1>Unicare</h1>        </div>        <div class=\"content\">            <p>Hi {{fullname}},</p>            <p>You had been chosen to be the project manager for <a href=\"{{projectLink}}\">{{projectName}}</a></p><p>Sincerely,</p><p>Unicare</p>            </div><div class=\"footer\">    <p>&copy; Unicare. All rights reserved.</p></div></div></body></html>";
            mailHtml = mailHtml
                .Replace("{{projectName}}", projectName)
                .Replace("{{projectLink}}", $"https://uniinc-cnb.com/projects/{projectId}")
                .Replace("{{fullname}}", memberFullname);

            await SendMail(memberEmail, memberFullname, $"[Project Mananger] - {projectName}", mailHtml);

            return true;
        }

        public async Task<bool> SendMailForNewPassword(string toEmail, string fullName, string password)
        {
            var mailHtml = "<!DOCTYPE html><html><head>    <meta charset=\"utf-8\" />    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />    <style>        body {            font-family: Arial, sans-serif;            background-color: #f0f0f0;            margin: 0;            padding: 0;        }        .container {            max-width: 600px;            margin: 0 auto;        }        .header {            background-color: #007bff;            color: white;            padding: 20px;            text-align: center;        }        .content {            background-color: white;            padding: 20px;        }        .footer {            background-color: #e9ecef;            padding: 10px;            text-align: center;        }        .password {            font-weight: bold;            font-size: 24px;            color: #007bff;        }    </style></head><body>    <div class=\"container\">        <div class=\"header\">            <h1>Unicare</h1>        </div>        <div class=\"content\">            <p>Hi {{fullname}},</p>            <p>You email had been used to created new admin account on our system</p><p>Email Address: {{email}}</p><p>Password:</p><p>{{newPassword}}</p><p>Please use this password to log in to your account! </p><p>Sincerely,</p><p>Unicare</p>            </div><div class=\"footer\">    <p>&copy; Unicare. All rights reserved.</p></div></div></body></html>";

            mailHtml = mailHtml
                .Replace("{{email}}", toEmail)
                .Replace("{{fullname}}", fullName)
                .Replace("{{newPassword}}", password);

            await SendMail(toEmail, fullName, "Welcome to Unicare System!", mailHtml);

            return true;
        }

        public async Task<bool> SendMailForResetPassword(string toEmail, string fullName, string token)
        {
            var mailHtml = "<!DOCTYPE html><html><head>    <meta charset=\\\"utf-8\\\" />    <meta name=\\\"viewport\\\" content=\\\"width=device-width, initial-scale=1\\\" />    <style>        body {            font-family: Arial, sans-serif;            background-color: #f0f0f0;            margin: 0;            padding: 0;        }        .container {            max-width: 600px;            margin: 0 auto;        }        .header {            background-color: #007bff;            color: white;            padding: 20px;            text-align: center;        }        .content {            background-color: white;            padding: 20px;        }        .footer {            background-color: #e9ecef;            padding: 10px;            text-align: center;        }        .password {            font-weight: bold;            font-size: 24px;            color: #007bff;        }    </style></head><body>    <div class=\\\"container\\\">        <div class=\\\"header\\\">            <h1>Unicare</h1>        </div>        <div class=\\\"content\\\">            <p>Hi {{fullname}},</p>            <p>You have requested to reset your password, please click the link below to reset! The link is only valid for 2 hour!</p><a href=\"{{resetLink}}\">Reset your password</a><p>If you can't click the link you can copy the following URL<p>{{resetLink}}</p><p>Sincerly,</p>   <p>Unicare</p>            </div><div class=\\\"footer\\\">    <p>&copy; Unicare. All rights reserved.</p></div></div></body></html>\"";

            mailHtml = mailHtml
                .Replace("{{email}}", toEmail)
                .Replace("{{fullname}}", fullName)
                .Replace("{{resetLink}}", $"https://uniinc-cnb.com/reset-password?token={token}");

            await SendMail(toEmail, fullName, "Unicare Password reset", mailHtml);

            return true;
        }

        public async Task<bool> SendMail(string toEmail, string toFullname, string subject, string content)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Admin", "admin@uniinc-cnb.com"));
            message.To.Add(new MailboxAddress(toFullname, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = content };

            // send the message
            var client = new SmtpClient();

            var host = _configuration.GetValue<string>("mail:smtp:host");
            var port = _configuration.GetValue<int>("mail:smtp:port");
            var username = _configuration.GetValue<string>("mail:smtp:username");
            var smptpassword = _configuration.GetValue<string>("mail:smtp:password");

            await client.ConnectAsync(host, port);
            await client.AuthenticateAsync(username, smptpassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return true;
        }
    }
}