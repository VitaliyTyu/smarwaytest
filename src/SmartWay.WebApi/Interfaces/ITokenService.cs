using Microsoft.AspNetCore.Identity;

namespace SmartWay.WebApi.Interfaces;

public interface ITokenService
{
    string CreateJwtToken(IdentityUser user);
}