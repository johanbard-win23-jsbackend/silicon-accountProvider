using Data.Entities;
using Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text;
using System;
using Data.Contexts;

namespace Data.Services;

public interface IAccountService
{
    public Task<DeleteResponse> DeleteAccountAsync(DeleteRequest delReq);
    public Task<UpdatePasswordResponse> UpdatePasswordAsync(UpdatePasswordRequest upReq);
}

public class AccountService(UserManager<UserEntity> userManager) : IAccountService
{
    private readonly UserManager<UserEntity> _userManager = userManager;

    public async Task<DeleteResponse> DeleteAccountAsync(DeleteRequest delReq)
    {
        try
        {
            if (delReq != null && delReq.Id != null!)
            {
                var user = await _userManager.FindByIdAsync(delReq.Id);
                if (user != null)
                {
                    var resAccount = await _userManager.DeleteAsync(user);
                    if (resAccount.Succeeded)
                    {
                        var subscriberEntity = new SubscriberEntity
                        {
                            Email = user.Email!,
                        };

                        using (var client = new HttpClient())
                        {
                            //client.BaseAddress = new Uri("");

                            var json = JsonConvert.SerializeObject(subscriberEntity);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            var result = await client.PostAsync("http://localhost:7239/api/DeleteSubscriber", content);
                            if (!result.IsSuccessStatusCode) { return new DeleteResponse { Status = "500", Error = result.Content.ToString() }; }
                        }

                        AddressModel addressModel = new AddressModel
                        {
                            Id = user.AddressId
                        };

                        using (var client = new HttpClient())
                        {
                            //client.BaseAddress = new Uri("");

                            var json = JsonConvert.SerializeObject(addressModel);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            var result = await client.PostAsync("http://localhost:7225/api/DeleteAddress", content);
                            if (!result.IsSuccessStatusCode) { return new DeleteResponse { Status = "500", Error = result.Content.ToString() }; }
                        }
                        return new DeleteResponse { Status = "200" };
                    }
                }
            }
            return new DeleteResponse { Status = "400", Error = "Bad request" };

        }
        catch (Exception ex)
        {
            return new DeleteResponse { Status = "500", Error = ex.Message };
        }
    }

    public async Task<UpdatePasswordResponse> UpdatePasswordAsync(UpdatePasswordRequest upReq)
    {
        try
        {
            if (upReq != null && !upReq.Id.IsNullOrEmpty() && !upReq.OldPassword.IsNullOrEmpty() && !upReq.NewPassword.IsNullOrEmpty() && !upReq.ConfirmPassword.IsNullOrEmpty() && upReq.NewPassword == upReq.ConfirmPassword)
            {
                var user = await _userManager.FindByIdAsync(upReq.Id);
                if (user != null)
                {
                    var res = await _userManager.ChangePasswordAsync(user, upReq.OldPassword, upReq.NewPassword);
                    if (res.Succeeded)
                    {
                        return new UpdatePasswordResponse { Status = "200" };
                    }
                    else
                    {
                        return new UpdatePasswordResponse { Status = "400", Error = JsonConvert.SerializeObject(res.Errors) };
                    }
                }
            }
            return new UpdatePasswordResponse { Status = "400", Error = "Bad request" };

        }
        catch (Exception ex)
        {
            return new UpdatePasswordResponse { Status = "500", Error = ex.Message };
        }
    }
}
