using Data.Models;
using Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace silicon_accountProvider.Functions;

public class UpdatePassword(ILogger<UpdatePassword> logger, IAccountService accountService)
{
    private readonly ILogger<UpdatePassword> _logger = logger;
    private readonly IAccountService _accountService = accountService;

    [Function("UpdatePassword")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var upReq = JsonConvert.DeserializeObject<UpdatePasswordRequest>(body);

            if (upReq == null)
                return new BadRequestObjectResult(new { Error = "Please provide a valid request" });

            var res = await _accountService.UpdatePasswordAsync(upReq);

            switch (res.Status)
            {
                case "200":
                    return new OkResult();
                case "400":
                    return new BadRequestObjectResult(new { Error = $"Function Delete failed :: {res.Error}" });
                case "500":
                    return new ObjectResult(new { Error = $"Function Delete failed :: {res.Error}" }) { StatusCode = 500 };
                default:
                    return new ObjectResult(new { Error = $"Function Delete failed :: Unknown Error" }) { StatusCode = 500 };
            }
        }
        catch (Exception ex)
        {
            return new ObjectResult(new { Error = $"Function Delete failed :: {ex.Message}" }) { StatusCode = 500 };
        }
    }
}
