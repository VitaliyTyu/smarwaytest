using Microsoft.AspNetCore.Identity;
using SmartWay.WebApi.Entities;

namespace SmartWay.WebApi.Interfaces;

public interface ITokenService
{
    string CreateJwtToken(IdentityUser user);
}