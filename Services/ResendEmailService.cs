using System.Text;
using System.Text.Json;

namespace APRegistrationAPI.Services
{
    public interface IEmailService
    {
        Task<bool> SendConfirmationEmailAsync(string toEmail, string firstName, string lastName, 
            string examSection, string grade, string registrationId);
        Task<bool> SendStaffNotificationAsync(string firstName, string lastName, string email, 
            string examSection, string grade, string registrationId);
    }

    public class ResendEmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ResendEmailService> _logger;

        public ResendEmailService(
            HttpClient httpClient, 
            IConfiguration configuration,
            ILogger<ResendEmailService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendConfirmationEmailAsync(
            string toEmail, 
            string firstName, 
            string lastName,
            string examSection, 
            string grade, 
            string registrationId)
        {
            try
            {
                var emailHtml = GetConfirmationEmailTemplate(firstName, lastName, examSection, grade, registrationId);
                
                var emailRequest = new
                {
                    from = _configuration["Email:FromAddress"] ?? "onboarding@resend.dev",
                    to = new[] { toEmail },
                    subject = "AP Exam Registration Confirmation - Amberson High School",
                    html = emailHtml
                };

                return await SendEmailAsync(emailRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending confirmation email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendStaffNotificationAsync(
            string firstName, 
            string lastName, 
            string email,
            string examSection, 
            string grade, 
            string registrationId)
        {
            try
            {
                var staffEmail = _configuration["Email:StaffNotificationEmail"] ?? "staff@ambersonhighschool.com";
                var emailHtml = GetStaffNotificationTemplate(firstName, lastName, email, examSection, grade, registrationId);
                
                var emailRequest = new
                {
                    from = _configuration["Email:FromAddress"] ?? "onboarding@resend.dev",
                    to = new[] { staffEmail },
                    subject = $"New AP Exam Registration - {firstName} {lastName}",
                    html = emailHtml
                };

                return await SendEmailAsync(emailRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending staff notification");
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(object emailRequest)
        {
            var apiKey = _configuration["Email:ResendApiKey"] ?? "YOUR_RESEND_API_KEY";
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var content = new StringContent(
                JsonSerializer.Serialize(emailRequest),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("https://api.resend.com/emails", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send email. Status: {Status}, Error: {Error}", 
                    response.StatusCode, errorContent);
                return false;
            }
        }

        // EDIT THIS TEMPLATE AS NEEDED
        private string GetConfirmationEmailTemplate(string firstName, string lastName, 
            string examSection, string grade, string registrationId)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4F46E5; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9fafb; }}
                        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                        .highlight {{ background-color: #FEF3C7; padding: 15px; margin: 15px 0; border-radius: 5px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>AP Exam Registration Confirmation</h1>
                        </div>
                        <div class='content'>
                            <h2>Dear {firstName} {lastName},</h2>
                            <p>Thank you for registering for the AP Exam 2026 at Amberson High School!</p>
                            
                            <div class='highlight'>
                                <h3>Registration Details:</h3>
                                <p><strong>Registration ID:</strong> {registrationId}</p>
                                <p><strong>Name:</strong> {firstName} {lastName}</p>
                                <p><strong>Grade:</strong> {grade}</p>
                                <p><strong>Exam Section:</strong> {examSection}</p>
                            </div>

                            <h3>Next Steps:</h3>
                            <ol>
                                <li><strong>Payment:</strong> Please make an E-Transfer to <strong>emt@ambersoncollege.ca</strong></li>
                                <li><strong>Security Answer:</strong> Amberson7100</li>
                                <li><strong>Notes:</strong> Include your name and registration ID: {registrationId}</li>
                            </ol>

                            <p><strong>Important Reminder:</strong></p>
                            <ul>
                                <li>Regular fee: CAD $275 (Deadline: November 7, 2025)</li>
                                <li>Late fee: CAD $350 (November 15, 2025 - March 13, 2026)</li>
                                <li>Exam payments are non-refundable</li>
                            </ul>

                            <p>Once we confirm your payment, you will receive another confirmation email.</p>
                            <p>If you have any questions, please contact us.</p>
                        </div>
                        <div class='footer'>
                            <p>Amberson High School<br>
                            AP Exam Registration 2026</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        // EDIT THIS TEMPLATE AS NEEDED
        private string GetStaffNotificationTemplate(string firstName, string lastName, 
            string email, string examSection, string grade, string registrationId)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #059669; color: white; padding: 20px; }}
                        .content {{ padding: 20px; background-color: #f9fafb; }}
                        table {{ width: 100%; border-collapse: collapse; margin: 15px 0; }}
                        th, td {{ padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }}
                        th {{ background-color: #e5e7eb; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>New AP Exam Registration</h2>
                        </div>
                        <div class='content'>
                            <h3>New Student Registration Received</h3>
                            <table>
                                <tr><th>Field</th><th>Value</th></tr>
                                <tr><td>Registration ID</td><td>{registrationId}</td></tr>
                                <tr><td>Student Name</td><td>{firstName} {lastName}</td></tr>
                                <tr><td>Email</td><td>{email}</td></tr>
                                <tr><td>Grade</td><td>{grade}</td></tr>
                                <tr><td>Exam Section</td><td>{examSection}</td></tr>
                                <tr><td>Payment Status</td><td>Pending</td></tr>
                                <tr><td>Submitted At</td><td>{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</td></tr>
                            </table>
                            <p><strong>Action Required:</strong> Monitor for E-Transfer payment from this student.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }
    }
}