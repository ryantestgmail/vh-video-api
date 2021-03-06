﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace VideoApi.Common.Security.CustomToken
{
    public class CustomJwtTokenProvider : ICustomJwtTokenProvider
    {
        private readonly CustomTokenSettings _customTokenSettings;

        public CustomJwtTokenProvider(CustomTokenSettings customTokenSettings)
        {
            _customTokenSettings = customTokenSettings;
        }

        public string GenerateToken(string claims, int expiresInMinutes)
        {
            byte[] key = Convert.FromBase64String(_customTokenSettings.Secret);
            return BuildToken(claims, expiresInMinutes, key);
        }

        // This is for acceptance tests only... do not use this anywhere else.
        public string GenerateTokenForCallbackEndpoint(string claims, int expiresInMinutes)
        {
            byte[] key = new ASCIIEncoding().GetBytes(_customTokenSettings.ThirdPartySecret);
            return BuildToken(claims, expiresInMinutes, key);
        }

        private string BuildToken(string claims, int expiresInMinutes, byte[] key)
        {
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(key);
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {new Claim(ClaimTypes.Name, claims)}),
                IssuedAt = DateTime.UtcNow.AddMinutes(-1),
                NotBefore = DateTime.UtcNow.AddMinutes(-1),
                Issuer = _customTokenSettings.Issuer,
                Expires =  DateTime.UtcNow.AddMinutes(expiresInMinutes + 1),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);
            return handler.WriteToken(token);
        }
    }
}