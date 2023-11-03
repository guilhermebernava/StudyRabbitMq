using Microsoft.AspNetCore.Mvc;
using Sender.Services;

namespace Sender.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProducerController : ControllerBase
{

    [HttpPost]
    public IActionResult SendMessage([FromServices] IProducerServices services, [FromBody] string message)
    {
        var result = services.SendMessage(message);
        return result ? Ok() : Problem(detail: "Error in rabbitMq");
    }
}
