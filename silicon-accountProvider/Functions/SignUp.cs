using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Data.Models;
using System.Text;

namespace silicon_accountProvider.Functions
{
    public class SignUp(ILogger<SignUp> logger, UserManager<UserEntity> userManager)
    {
        private readonly ILogger _logger = logger;
        private readonly UserManager<UserEntity> _userManager = userManager;

        [Function("SignUp")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogWarning("Started");
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
                _logger.LogWarning("There is a body");

                try
                {
                    urr = JsonConvert.DeserializeObject<UserRegistrationRequest>(body)!;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"JsonConvert.DeserializeObject<UserRegistrationRequest>(body) :: {ex.Message}");
                }

                _logger.LogWarning("Body deserialized to urr");

                if (urr != null && urr.FirstName.Length > 2 && !string.IsNullOrEmpty(urr.Email) && !string.IsNullOrEmpty(urr.Password) && urr.Password == urr.ConfirmPassword && urr.Terms == true)
                {
                    _logger.LogWarning("Finding other i DB");
                    try
                    {
                        if (!await _userManager.Users.AnyAsync(x => x.Email == urr.Email))
                        {
                            _logger.LogWarning("No other i DB");

                            var subscriberEntity = new SubscriberEntity
                            {
                                Email = urr.Email,
                            };

                            using (var client = new HttpClient())
                            {
                                //client.BaseAddress = new Uri("");

                                var json = JsonConvert.SerializeObject(subscriberEntity);
                                var content = new StringContent(json, Encoding.UTF8, "application/json");

                                var result = await client.PostAsync("https://jb-silicon-subscriberprovider.azurewebsites.net/api/CreateSubscriber?code=v3HS2OrfLumU4Kc-c3aApsTnVbILhI_2_HFMuUcETIfSAzFuAwZ0sQ%3D%3D", content);
                                if (!result.IsSuccessStatusCode) { return new ObjectResult(result); }
                                string resultContent = await result.Content.ReadAsStringAsync();
                                subscriberEntity = JsonConvert.DeserializeObject<SubscriberEntity>(resultContent);
                            }

                            var userEntity = new UserEntity();

                            if (subscriberEntity != null)
                            {
                                userEntity = new UserEntity
                                {
                                    FirstName = urr.FirstName,
                                    LastName = urr.LastName,
                                    Email = urr.Email,
                                    UserName = urr.Email,
                                    RegistrationDate = DateTime.Now,
                                    SubscriberId = subscriberEntity.Id
                                };
                            }
                            
                            try
                            {
                                _logger.LogWarning("Creating in DB");
                                var result = await _userManager.CreateAsync(userEntity, urr.Password);
                                if (result.Succeeded)
                                {
                                    _logger.LogWarning("Sending OK Result");
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
                    catch(Exception ex)
                    {
                        _logger.LogError($"_userManager.Users.AnyAsync :: {ex.Message}");
                    }
                    
                }
            }
            _logger.LogWarning("Sending Bad Request");
            return new BadRequestResult();
        }
    }
}
