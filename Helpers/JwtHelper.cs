//using System;
//using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using Microsoft.IdentityModel.Tokens;
//using ECommerceAPI.Models;
//using Microsoft.Extensions.Options;

//namespace EADWebApplication.Helpers
//{
//    //public class JwtHelper
//    //{
//    //    private readonly JwtSettings _jwtSettings;

//    //    public JwtHelper(IOptions<JwtSettings> jwtSettings)
//    //    {
//    //        _jwtSettings = jwtSettings.Value;
//    //    }

//    //    public string GenerateJwtToken(User user)
//    //    {
//    //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
//    //        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//    //        var claims = new List<Claim>
//    //        {
//    //            new Claim(ClaimTypes.Email, user.Email),
//    //            new Claim(ClaimTypes.Role, user.Role),
//    //            new Claim("UserId", user.Id)  // Include UserId in the token
//    //        };

//    //        var token = new JwtSecurityToken(
//    //            issuer: _jwtSettings.Issuer,
//    //            audience: _jwtSettings.Audience,
//    //            claims: claims,
//    //            expires: DateTime.Now.AddMinutes(_jwtSettings.ExpiryInMinutes),
//    //            signingCredentials: credentials
//    //        );

//    //        return new JwtSecurityTokenHandler().WriteToken(token);
//    //    }
//    //}
//}
