using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using silicon_accountProvider.Models;

namespace silicon_accountProvider.Functions;

public class Delete(ILogger<Delete> logger, UserManager<UserEntity> userManager)
{
    private readonly ILogger _logger = logger;
    private readonly UserManager<UserEntity> _userManager = userManager;

    [Function("Delete")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        string body = null!;
        

        try
        {
            body = await new StreamReader(req.Body).ReadToEndAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"StreamReader :: {ex.Message}");
        }

        if (body != null)
        {
            UserEntity user;

            try
            {
                user = JsonConvert.DeserializeObject<UserEntity>(body)!;

                if (user != null)
                {
                    try
                    {
                        var res = await _userManager.DeleteAsync(user);
                        if (res.Succeeded)
                        {
                            return new OkResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"_userManager.DeleteAsync(user) :: {ex.Message}");
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"JsonConvert.DeserializeObject<UserEntity>(body) :: {ex.Message}");
            }
        }

        return new BadRequestResult();
    }
}
