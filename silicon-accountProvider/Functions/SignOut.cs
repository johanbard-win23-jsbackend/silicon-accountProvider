using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace silicon_accountProvider.Functions;

public class SignOut(ILogger<SignUp> logger, SignInManager<UserEntity> signInManager)
{
    private readonly ILogger _logger = logger;
    private readonly SignInManager<UserEntity> _signInManager = signInManager;

    [Function("SignOut")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        try
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return new SignOutResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"_signInManager.SignOutAsync() :: {ex.Message}");
        }

        return new BadRequestResult();
    }
}