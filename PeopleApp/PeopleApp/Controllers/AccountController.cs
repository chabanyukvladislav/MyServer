using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using PeopleApp.DatabaseContext;
using PeopleApp.Models;

namespace PeopleApp.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private const string TokenServer = "https://vlad191100.eu.auth0.com/oauth/token";
        private const string RedirectUrl = "http://192.168.1.19:1920/Account/Login";
        private const string ClientSecret = "t5p_QaMMlJ-BOOJQd_EAnSGvLCmnopfeNeqEMVkeyWCK3_UZW7gF8_UVSaaI-fLF";
        private const string ClientId = "Y2uaVJba1Ei6mYa7EaZiLajzlwOTo6jl";
        private readonly Context _context;
        private string _token;

        public AccountController(Context context)
        {
            _context = context;
        }

        [HttpPost]
        public string Login()
        {
            GetToken();
            JwtSecurityTokenHandler jwt = new JwtSecurityTokenHandler();
            if (!(jwt.ReadToken(_token) is JwtSecurityToken security)) return "";
            User user = GetUser(security);
            AddUser(user);
            string answearToken = GetAnswearToken(user.UserId, user.Name, user.Nickname, user.Picture);
            return answearToken;
        }

        [HttpGet]
        public bool Logout()
        {
            try
            {
                if (!Request.Headers.TryGetValue("UserId", out StringValues userId))
                    return false;
                User user = _context.Users.FirstOrDefault(us => us.UserId == userId);
                if (user == null)
                    return true;
                _context.Users.Remove(user);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void GetToken()
        {
            string data = GetData();
            int tokenIndex = data.IndexOf("id_token", StringComparison.Ordinal);
            string firstString = data.Remove(0, tokenIndex + 11);
            string token = firstString.Remove(firstString.IndexOf('"'));
            _token = token;
        }

        private string GetData()
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
                          "\"redirect_uri\": \"" + RedirectUrl + "\"" +
                          "}";
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync(TokenServer, content).Result;
            string data = response.Content.ReadAsStringAsync().Result;
            return data;
        }

        private User GetUser(JwtSecurityToken security)
        {
            string sub = security.Claims.FirstOrDefault(el => el.Type == "sub")?.Value;
            string name = security.Claims.FirstOrDefault(el => el.Type == "name")?.Value;
            string nickname = security.Claims.FirstOrDefault(el => el.Type == "nickname")?.Value;
            string picture = security.Claims.FirstOrDefault(el => el.Type == "picture")?.Value;
            User user = new User() { UserId = sub, Name = name, Nickname = nickname, Picture = picture, Token = _token };
            return user;
        }

        private void AddUser(User user)
        {
            User db = _context.Users.FirstOrDefault(el => el.UserId == user.UserId);
            if (db != null)
            {
                db.Name = user.Name;
                db.Nickname = user.Nickname;
                db.Picture = user.Picture;
                db.Token = user.Token;
                _context.SaveChanges();
                return;
            }
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        private static string GetAnswearToken(string sub, string name, string nickname, string picture)
        {
            List<Claim> listClaim = new List<Claim>
            {
                new Claim("UserId", sub),
                new Claim("Name", name),
                new Claim("Nickname", nickname),
                new Claim("Picture", picture)
            };
            byte[] key = new HMACSHA256().Key;
            SecurityTokenDescriptor resultSecurityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(listClaim),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            JwtSecurityTokenHandler returnJwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            SecurityToken returnSecurityToken = returnJwtSecurityTokenHandler.CreateToken(resultSecurityTokenDescriptor);
            string returnToken = returnJwtSecurityTokenHandler.WriteToken(returnSecurityToken);
            return returnToken;
        }
    }
}