using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using BlazorApp2.Data;

namespace BlazorApp2.Services;

/// <summary>
/// Email sender service that implements IEmailSender for ASP.NET Core Identity.
/// Configure email settings in appsettings.json under "EmailSettings" section.
/// </summary>
public class EmailSenderService : IEmailSender<ApplicationUser>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailSenderService> _logger;

    public EmailSenderService(IEmailSender emailSender, ILogger<EmailSenderService> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var subject = "Confirm your email - Maloy Maintenance";
        var htmlMessage = GenerateEmailTemplate(
            "Confirm Your Email",
            $@"<p>Hello,</p>
               <p>Thank you for registering with Maloy Maintenance! Please confirm your email address by clicking the button below.</p>
               <p style='text-align: center; margin: 30px 0;'>
                   <a href='{confirmationLink}' style='background-color: #d10000; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>Confirm Email</a>
               </p>
               <p>If you didn't create an account with us, you can safely ignore this email.</p>
               <p>If the button doesn't work, copy and paste this link into your browser:</p>
               <p style='word-break: break-all; color: #666;'>{confirmationLink}</p>");

        await _emailSender.SendEmailAsync(email, subject, htmlMessage);
        _logger.LogInformation("Confirmation email sent to {Email}", email);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var subject = "Reset your password - Maloy Maintenance";
        var htmlMessage = GenerateEmailTemplate(
            "Reset Your Password",
            $@"<p>Hello,</p>
               <p>We received a request to reset your password. Click the button below to create a new password.</p>
               <p style='text-align: center; margin: 30px 0;'>
                   <a href='{resetLink}' style='background-color: #d10000; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>Reset Password</a>
               </p>
               <p>If you didn't request a password reset, you can safely ignore this email. Your password will remain unchanged.</p>
               <p>If the button doesn't work, copy and paste this link into your browser:</p>
               <p style='word-break: break-all; color: #666;'>{resetLink}</p>");

        await _emailSender.SendEmailAsync(email, subject, htmlMessage);
        _logger.LogInformation("Password reset email sent to {Email}", email);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var subject = "Password reset code - Maloy Maintenance";
        var htmlMessage = GenerateEmailTemplate(
            "Your Password Reset Code",
            $@"<p>Hello,</p>
               <p>We received a request to reset your password. Use the code below to reset your password:</p>
               <p style='text-align: center; margin: 30px 0;'>
                   <span style='background-color: #f5f5f5; padding: 15px 30px; font-size: 24px; font-weight: bold; letter-spacing: 3px; border-radius: 5px; display: inline-block;'>{resetCode}</span>
               </p>
               <p>If you didn't request a password reset, you can safely ignore this email.</p>");

        await _emailSender.SendEmailAsync(email, subject, htmlMessage);
        _logger.LogInformation("Password reset code sent to {Email}", email);
    }

    private static string GenerateEmailTemplate(string title, string content)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
</head>
<body style='margin: 0; padding: 0; font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; background-color: #f5f5f5;'>
    <table role='presentation' style='width: 100%; border-collapse: collapse;'>
        <tr>
            <td style='padding: 40px 0;'>
                <table role='presentation' style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
                    <!-- Header -->
                    <tr>
                        <td style='background-color: #000000; padding: 30px; text-align: center; border-radius: 8px 8px 0 0;'>
                            <h1 style='margin: 0; color: #ffffff; font-size: 24px; font-weight: 700;'>
                                <span style='color: #d10000;'>Maloy</span> Maintenance
                            </h1>
                        </td>
                    </tr>
                    <!-- Content -->
                    <tr>
                        <td style='padding: 40px 30px;'>
                            <h2 style='margin: 0 0 20px; color: #1a1a1a; font-size: 22px;'>{title}</h2>
                            {content}
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style='background-color: #f9f9f9; padding: 20px 30px; text-align: center; border-radius: 0 0 8px 8px; border-top: 1px solid #eeeeee;'>
                            <p style='margin: 0; color: #999999; font-size: 12px;'>
                                This is an automated message from Maloy Maintenance.<br>
                                Please do not reply directly to this email.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}

/// <summary>
/// SMTP-based email sender using standard .NET mail client.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> settings, ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (string.IsNullOrEmpty(_settings.SmtpHost))
        {
            _logger.LogWarning("SMTP not configured. Email to {Email} with subject '{Subject}' was not sent.", email, subject);
            return;
        }

        try
        {
            using var client = new System.Net.Mail.SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new System.Net.NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword),
                EnableSsl = _settings.EnableSsl
            };

            var mailMessage = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", email);
            throw;
        }
    }
}

/// <summary>
/// Development email sender that logs emails instead of sending them.
/// Use this in development to avoid setting up SMTP.
/// </summary>
public class DevelopmentEmailSender : IEmailSender
{
    private readonly ILogger<DevelopmentEmailSender> _logger;
    private readonly IWebHostEnvironment _env;

    public DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogInformation("=== DEVELOPMENT EMAIL ===");
        _logger.LogInformation("To: {Email}", email);
        _logger.LogInformation("Subject: {Subject}", subject);
        
        // Save to a file in development for easy viewing
        var emailDir = Path.Combine(_env.ContentRootPath, "dev-emails");
        Directory.CreateDirectory(emailDir);
        
        var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{email.Replace("@", "_at_")}.html";
        var filePath = Path.Combine(emailDir, fileName);
        
        await File.WriteAllTextAsync(filePath, htmlMessage);
        _logger.LogInformation("Email saved to: {FilePath}", filePath);
        _logger.LogInformation("=========================");
    }
}

/// <summary>
/// Email configuration settings. Add to appsettings.json under "EmailSettings".
/// </summary>
public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public string FromEmail { get; set; } = "noreply@maloymaintenance.com";
    public string FromName { get; set; } = "Maloy Maintenance";
}
