using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using silicon_accountProvider.Models;

namespace silicon_accountProvider.Functions
{
    public class SignUp(ILogger<SignUp> logger, UserManager<UserEntity> userManager)
    {
        private readonly ILogger _logger = logger;
        private readonly UserManager<UserEntity> _userManager = userManager;

        [Function("SignUp")]
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
                UserRegistrationRequest urr = null!;

                try
                {
                    urr = JsonConvert.DeserializeObject<UserRegistrationRequest>(body)!;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"JsonConvert.DeserializeObject<UserRegistrationRequest>(body) :: {ex.Message}");
                }
                
                if (urr != null && !string.IsNullOrEmpty(urr.Email) && !string.IsNullOrEmpty(urr.Password) && urr.Password == urr.ConfirmPassword && urr.Terms == true)
                {
                    if (! await _userManager.Users.AnyAsync(x => x.Email == urr.Email))
                    {
                        var userEntity = new UserEntity
                        {
                            FirstName = urr.FirstName,
                            LastName = urr.LastName,
                            Email = urr.Email,
                            UserName = urr.Email,
                            RegistrationDate = DateTime.Now
                        };

                        try
                        {
                            var result = await _userManager.CreateAsync(userEntity, urr.Password);
                            if (result.Succeeded)
                            {
                                return new OkResult();
                            }
                            else
                            {
                                _logger.LogError($"_userManager.CreateAsync :: ERROR CREATING USER {userEntity.Email}");
                            }
                        }
                        catch (Exception ex) 
                        {
                            _logger.LogError($"_userManager.CreateAsync :: {ex.Message}");
                        }                   
                    }
                    else
                    {
                        return new ConflictResult();
                    }
                }
            }

            return new BadRequestResult();
        }
    }
}
