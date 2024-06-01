using Data.Entities;
using Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

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
                    var res = await _userManager.DeleteAsync(user);
                    if (res.Succeeded)
                    {
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
            if (upReq != null && !upReq.Id.IsNullOrEmpty() && !upReq.OldPassword.IsNullOrEmpty() && !upReq.NewPassword.IsNullOrEmpty() && !upReq.ConfirmPassword.IsNullOrEmpty() && upReq.NewPassword == upReq.OldPassword)
            {
                var user = await _userManager.FindByIdAsync(upReq.Id);
                if (user != null)
                {
                    var res = await _userManager.ChangePasswordAsync(user, upReq.OldPassword, upReq.NewPassword);
                    if (res.Succeeded)
                    {
                        return new UpdatePasswordResponse { Status = "200" };
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
