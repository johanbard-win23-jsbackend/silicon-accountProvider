using Data.Entities;
using Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Mime;
using System.Text;

namespace silicon_accountProvider.Functions;

public class SignIn(ILogger<SignUp> logger, UserManager<UserEntity> userManager, SignInManager<UserEntity> signInManager)
{
    private readonly ILogger<SignUp> _logger = logger;
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
                            try
                            {
                                using (var client = new HttpClient())
                                {
                                    string url = Environment.GetEnvironmentVariable("tokenProviderUrl")!;
                                    var obj = new {
                                        UserId=user!.Id,
	                                    Email=user.Email
                                    };

                                    var jsonOut = JsonConvert.SerializeObject(obj);

                                    var request = new HttpRequestMessage
                                    {
                                        Method = HttpMethod.Post,
                                        RequestUri = new Uri(url),
                                        Content = new StringContent(
                                        jsonOut,
                                        Encoding.UTF8,
                                        MediaTypeNames.Application.Json), // or "application/json" in older versions
                                    };

                                    var response = await client.SendAsync(request);

                                    if (response.IsSuccessStatusCode)
                                    {
                                        var json = await response.Content.ReadAsStringAsync();
                                        var tokens = JsonConvert.DeserializeObject<TokenResponse>(json);

                                        if(tokens != null && tokens.refreshToken != null) 
                                        {
                                            await _userManager.SetAuthenticationTokenAsync(user!, "accountProvider", "authToken", tokens.refreshToken);
                                            //var token = await _userManager.GetAuthenticationTokenAsync(user!, "accountProvider", "authToken");

                                            return new OkObjectResult(json);
                                        }

                                        _logger.LogError($"Failed to recieve token");
                                    }
                                    else
                                    {
                                        _logger.LogError("Unsuccessful response from tokenProvider");
                                    }
                                } 
                            }
                            catch(Exception ex)
                            {
                                _logger.LogError($"_userManager.GenerateUserTokenAsync :: {ex.Message}");
                            }
                            
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

