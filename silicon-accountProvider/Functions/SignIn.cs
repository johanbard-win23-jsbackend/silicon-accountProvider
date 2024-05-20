using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using silicon_accountProvider.Models;

namespace silicon_accountProvider.Functions
{
    public class SignIn(ILogger<Create> logger, IServiceProvider serviceProvider,SignInManager<UserEntity> signInManager)
    {
        private readonly ILogger<Create> _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly SignInManager<UserEntity> _signInManager = signInManager;
        //private readonly SignInManager<UserEntity> _signInManager = serviceProvider.GetRequiredService<SignInManager<UserEntity>>();

        [Function("SignIn")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            //_signInManager.Context = new DefaultHttpContext {RequestServices = serviceProvider  }; //MISSING SERVICE PROVIDER
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
                UserSignInRequest usir = null!;

                try
                {
                    usir = JsonConvert.DeserializeObject<UserSignInRequest>(body)!;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"JsonConvert.DeserializeObject<UserSignInRequest>(body) :: {ex.Message}");
                }

                if (usir != null && !string.IsNullOrEmpty(usir.Email) && !string.IsNullOrEmpty(usir.Password))
                {
                   try
                    {
                        var result = await _signInManager.PasswordSignInAsync(usir.Email, usir.Password, usir.RememberMe, false);
                        if (result.Succeeded) 
                        {
                            return new OkResult();
                        }
                        else
                        {
                            return new UnauthorizedResult();
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError($"_signInManager.PasswordSignInAsync :: {ex.Message}");
                    }
                }
            }

            return new BadRequestResult();
        }
    }
}
    
