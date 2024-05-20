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
using System.Runtime.CompilerServices;

namespace silicon_accountProvider.Functions
{
    public class SignIn(ILogger<Create> logger, UserManager<UserEntity> userManager, SignInManager<UserEntity> signInManager)
    {
        private readonly ILogger<Create> _logger = logger;
        private readonly UserManager<UserEntity> _userManager = userManager;
        private readonly SignInManager<UserEntity> _signInManager = signInManager;

        [Function("SignIn")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
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
                        //var result = await _signInManager.PasswordSignInAsync(usir.Email, usir.Password, usir.RememberMe, false);

                        var user = await _userManager.FindByNameAsync(usir.Email);
                        
                        try
                        {
                            var result = await _signInManager.CheckPasswordSignInAsync(user!, usir.Password, false);

                            if (result.Succeeded)
                            {

                                await _userManager.SetAuthenticationTokenAsync(user, "accountProvider", "authToken", "ABC123");
                                var token = await _userManager.GetAuthenticationTokenAsync(user!, "accountProvider", "authToken");
                                _logger.LogWarning($"TOKEN :: {token}");

                                //string token = "ABC123";
                                return new OkObjectResult(token);
                            }
                            else
                            {
                                return new UnauthorizedResult();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"_signInManager.PasswordSignInAsync :: {ex.Message}");
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
    
