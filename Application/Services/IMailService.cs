namespace Application.Services
{
  public interface IMailService
  {
      Task<bool> SendMailForNewProjectCreated(string projectId, string projectName, string memberEmail,
          string memberFullname);
    Task<bool> SendMailForNewPassword(string toEmail, string fullName, string password);
    Task<bool> SendMailForResetPassword(string toEmail, string fullName, string token);
    Task<bool> SendMail(string toEmail, string toFullname, string subject, string content);
  }
}