using Microsoft.AspNetCore.Mvc;
using TenantDesk.Application.Interfaces.Services;

namespace TenantDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailTestController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailTestController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> SendTestEmail()
    {
        await _emailService.SendAsync(
            "test@example.com",
            "TenantDesk SMTP Test",
            """
            <h1>TenantDesk SMTP Test</h1>
            <p>If you can see this email in Mailtrap, SMTP is working correctly.</p>
            """
        );

        return Ok("Test email sent.");
    }
}