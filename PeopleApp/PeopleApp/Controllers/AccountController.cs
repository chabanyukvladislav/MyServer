using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace PeopleApp.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private const string TokenServer = "https://vlad191100.eu.auth0.com/oauth/token";
        private const string RedirectUrl = "http://192.168.1.19:1920/Account/Login";
        private const string ClientSecret = "t5p_QaMMlJ-BOOJQd_EAnSGvLCmnopfeNeqEMVkeyWCK3_UZW7gF8_UVSaaI-fLF";
        private const string ClientId = "Y2uaVJba1Ei6mYa7EaZiLajzlwOTo6jl";

        [HttpPost]
        public string Login()
        {
            StreamReader sr = new StreamReader(Request.Body);
            string body = sr.ReadToEnd();
            sr.Close();
            int codeIndex = body.IndexOf("code", StringComparison.Ordinal);
            string allString = body.Remove(0, codeIndex + 8);
            string code = allString.Remove(allString.IndexOf('"'));
            var client = new HttpClient();
            string json = "{" +
                            "\"grant_type\": \"authorization_code\"," +
                            "\"client_id\": \"" + ClientId + "\"," +
                            "\"client_secret\": \"" + ClientSecret + "\"," +
                            "\"code\": \"" + code + "\"," +
                            "\"redirect_uri\": \"" + RedirectUrl + "\"," +
                          "}";
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(TokenServer, content).Result;
            string data = response.Content.ReadAsStringAsync().Result;
            int tokenIndex = data.IndexOf("id_token", StringComparison.Ordinal);
            string firstString = data.Remove(0, tokenIndex + 11);
            string token = firstString.Remove(firstString.IndexOf('"'));
            JwtSecurityTokenHandler jwt = new JwtSecurityTokenHandler();
            string result = "";
            if (jwt.ReadToken(token) is JwtSecurityToken security)
            {
                string email = security.Claims.FirstOrDefault(el => el.Type == "email")?.Value;
                result = email;
            }
            return result;
        }
    }
}