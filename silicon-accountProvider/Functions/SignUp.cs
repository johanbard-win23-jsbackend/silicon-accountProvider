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
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            if (body != null)
            {
                var urr = JsonConvert.DeserializeObject<UserRegistrationRequest>(body);
                if (urr != null && !string.IsNullOrEmpty(urr.Email) && !string.IsNullOrEmpty(urr.Password))
                {
                    if (! await _userManager.Users.AnyAsync(x => x.Email == urr.Email))
                    {
                        var userEntity = new UserEntity
                        {
                            FirstName = urr.FirstName,
                            LastName = urr.LastName,
                            Email = urr.Email,
                            UserName = urr.Email,
                        };

                        var result = await _userManager.CreateAsync(userEntity, urr.Password);
                        if (result.Succeeded) 
                        {
                            return new OkResult();
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
