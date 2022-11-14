using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TripboxSupply_API.Models;
using TripboxSupply_API.Models.Member;

namespace TripboxSupply_API.Token
{
    public class TokenManager
    {
        private static string Secret = "db3OIsj+BXE9NZDy0t8W3TcNekrF+2d/1sFnWG4HnV8TZY30iTOdtVWJG8abWvB1GlOgJuQZdcF2Luqm/hccMw==";
       
        /// <summary>
        /// 액세스 토큰 생성
        /// </summary>        
        /// <param name="supplyID"></param>
        /// <param name="partyID"></param>
        /// <param name="memberID"></param>
        /// <param name="memberName"></param>
        /// <param name="status"></param>
        /// <param name="토큰만료기준"></param>
        /// <param name="토큰만료값"></param>
        /// <param name="토큰타입"></param>        
        /// <returns></returns>        
        public static JwtToken GenerateToken(long partyID, string memberID, string name, string status, long supplyID, string 토큰만료기준, int 토큰만료값, string 토큰타입)
        {
            DateTime _Expires;
            switch (토큰만료기준)
            {
                case "년":
                    _Expires = DateTime.UtcNow.AddYears(토큰만료값);
                    break;
                case "월":
                    _Expires = DateTime.UtcNow.AddMonths(토큰만료값);
                    break;
                case "일":
                    _Expires = DateTime.UtcNow.AddDays(토큰만료값);
                    break;
                case "분":
                    _Expires = DateTime.UtcNow.AddMinutes(토큰만료값);
                    break;
                default:
                    _Expires = DateTime.UtcNow.AddMinutes(토큰만료값);
                    break;
            }

            string _a = "thisisasecretkeyanddontsharewithanyone";
            var key = Encoding.ASCII.GetBytes(_a);
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims: new[] {
                                                             new Claim("PartyID", value: partyID.ToString()),
                                                             new Claim("MemberID", value: memberID.ToString()),
                                                             new Claim("Name", value: name),
                                                             new Claim("Status", value: status.ToString()),                                                             
                                                             new Claim("SupplyID", value: supplyID.ToString())
                    }),
                // Expires =  DateTime.UtcNow.AddMonths(3),// AddMinutes(30),
                Expires = _Expires,// AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)

            };

            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.CreateJwtSecurityToken(descriptor);

            var jwtToken = new JwtToken
            {
                ExpirationTime = _Expires,
                RefreshToken = new RefreshTokenGenerator().GenerateRefreshToken(32),
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };

            jwtToken.RefreshToken = string.Empty;
            return jwtToken;
        }

        public static ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
                if (jwtToken == null)
                    return null;
                byte[] key = Convert.FromBase64String(Secret);
                TokenValidationParameters parameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                SecurityToken securityToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, parameters, out securityToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public static string ValidateToken(string token)
        {
            string username = null;
            ClaimsPrincipal principal = GetPrincipal(token);

            if (principal == null)
                return null;

            ClaimsIdentity identity = null;
            try
            {
                identity = (ClaimsIdentity)principal.Identity;
            }
            catch (NullReferenceException)
            {
                return null;
            }

            Claim usernameClaim = identity.FindFirst(type: ClaimTypes.Name);
            username = usernameClaim.Value;
            return username;
        }
        
    }
}
