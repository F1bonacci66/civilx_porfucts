using System;

namespace CivilX.Shared.Models
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public UserInfo User { get; set; }
        public string ErrorMessage { get; set; }
        
        public static AuthResult CreateSuccess(string token, UserInfo user)
        {
            return new AuthResult
            {
                Success = true,
                Token = token,
                User = user
            };
        }
        
        public static AuthResult CreateError(string errorMessage)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
