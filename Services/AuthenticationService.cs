using Authentication.Models;
using Authentication.Data;
using Amazon.DynamoDBv2.Model;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

namespace Authentication.Services
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> CreateAccount(PasswordLoginRequest createAccountRequest);
        Task<LoginResponse> PasswordLogin(PasswordLoginRequest passwordLoginRequest);
        Task<LoginResponse> TokenLogin(TokenLoginRequest tokenLoginRequest);

        Task<string> GetUserName(TokenLoginRequest tokenLoginRequest);
    }
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticationData _authenticationData;
        public AuthenticationService(IAuthenticationData authenticationData)
        {
            _authenticationData = authenticationData;
        }
    

    public async Task<LoginResponse> CreateAccount(PasswordLoginRequest createAccountRequest)
        {
            Profile profile = new Profile() {
            username = createAccountRequest.username,
            password = createAccountRequest.password,
            securitytoken = GenerateToken(createAccountRequest.username+ DateTime.Now.ToString()),
            lastTokenUpdateTime = DateTime.Now.ToString("dd/MM/yyyy h:mm:ss")
            };
            if (await _authenticationData.AddProfileData(profile))
            {
                LoginResponse loginResponse = new LoginResponse()
                {
                    status = "success",
                    token = profile.securitytoken
                };
                return loginResponse;
            }
            else {
                LoginResponse loginResponse = new LoginResponse()
                {
                    status = "fail",
                    token = null
                };
                return loginResponse;
            }
            
        }

        public async Task<LoginResponse> PasswordLogin(PasswordLoginRequest passwordLoginRequest)
        {
            LoginType loginType = new LoginType()
            {
                type = "username",
                value = passwordLoginRequest.username
            };
            Profile profile = await _authenticationData.GetProfileData(loginType);
            if (profile.password == passwordLoginRequest.password)
            {
                return await RefreshToken(profile);
            }
            else {
                LoginResponse loginResponse = new LoginResponse()
                {
                    status = "fail",
                    token = null
                };
                return loginResponse;
            }
        }

        public async Task<LoginResponse> TokenLogin(TokenLoginRequest tokenLoginRequest)
        {
            LoginType loginType = new LoginType()
            {
                type = "securitytoken",
                value = tokenLoginRequest.token
            };
            Profile profile = await _authenticationData.GetProfileData(loginType);
            if (profile.securitytoken == tokenLoginRequest.token)
            {
                return await RefreshToken(profile);
            }
            else
            {
                LoginResponse loginResponse = new LoginResponse()
                {
                    status = "fail",
                    token = null
                };
                return loginResponse;
            }
        }

        public async Task<string> GetUserName(TokenLoginRequest tokenLoginRequest)
        {
            LoginType loginType = new LoginType()
            {
                type = "securitytoken",
                value = tokenLoginRequest.token
            };
            Profile profile = await _authenticationData.GetProfileData(loginType);
            if (profile.securitytoken == tokenLoginRequest.token)
            {
                return profile.username;
        
            }
            else
            {
                return "";
            }
        }

        private async Task<LoginResponse> RefreshToken(Profile profile)
        {
            string format = "dd/MM/yyyy h:mm:ss";
            TimeSpan timeDifference = DateTime.Now - DateTime.ParseExact(profile.lastTokenUpdateTime, format, CultureInfo.InvariantCulture);

            if (Math.Abs(timeDifference.Minutes) < 60)
            {
                LoginResponse loginResponse = new LoginResponse()
                {
                    status = "success",
                    token = profile.securitytoken
                };
                return loginResponse;
            }
            else
            {
                profile.securitytoken = GenerateToken(profile.username + DateTime.Now.ToString());
                profile.lastTokenUpdateTime = DateTime.Now.ToString("dd/MM/yyyy h:mm:ss");
                if (await _authenticationData.UpdateProfileData(profile))
                {
                    LoginResponse loginResponse = new LoginResponse()
                    {
                        status = "success",
                        token = profile.securitytoken
                    };
                    return loginResponse;
                }
                else
                {
                    LoginResponse loginResponse = new LoginResponse()
                    {
                        status = "fail",
                        token = null
                    };
                    return loginResponse;

                }
            }
        }

        private string GenerateToken(string username)
        {

            using (MD5 md5 = MD5.Create())
            {
                // Convert the input string to a byte array and compute the hash
                byte[] inputBytes = Encoding.UTF8.GetBytes(username);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // Format as hexadecimal
                }

                return sb.ToString();
            }
        }
    }

   
}
    
