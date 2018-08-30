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
using PeopleApp.Attributes;
using PeopleApp.DatabaseContext;
using PeopleApp.Models;

namespace PeopleApp.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private const string TokenServer = "https://itstep1511.eu.auth0.com/oauth/token";
        private const string WebRedirectUrl = "http://185.247.21.82:8080/Account/Login";
        //private const string WebRedirectUrl = "http://localhost:3668/Account/Login";
        private const string NativeRedirectUrl = "https://itstep1511.eu.auth0.com/mobile";
        private const string WebClientSecret = "VJJhUoEQxOVQmjiT0DEH6d0o8F0BqQ2b7X6U3G1coMHJGzL8C9od8crSarqbTu7_";
        private const string WebClientId = "ofGlLkCt08XJ3Kgm483nDMDLm13I53Bf";
        private const string NativevClientSecret = "zcVkBGepy2aIzBjfncRz5OLMkFbJc42zB4Efje3nqNz1awGo2wVSz9sMS_f_LSDf";
        private const string NativeClientId = "zIrxMH5oEejRE3eu7AOBajsk5Ad4BXdz";
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
        [Authorized]
        public List<string> GetUsers()
        {
            return _context.Users.Select(el => el.UserId).ToList();
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
            int typeIndex = body.IndexOf("type", StringComparison.Ordinal);
            string typeString = body.Remove(0, typeIndex + 8);
            string type = typeString.Remove(typeString.IndexOf('"'));
            string clientId = "";
            string clientSecret = "";
            string redirectUrl = "";
            switch (type)
            {
                case "native":
                    clientId = NativeClientId;
                    clientSecret = NativevClientSecret;
                    redirectUrl = NativeRedirectUrl;
                    break;
                case "web":
                    clientId = WebClientId;
                    clientSecret = WebClientSecret;
                    redirectUrl = WebRedirectUrl;
                    break;
            }

            var client = new HttpClient();
            string json = "{" +
                          "\"grant_type\": \"authorization_code\"," +
                          "\"client_id\": \"" + clientId + "\"," +
                          "\"client_secret\": \"" + clientSecret + "\"," +
                          "\"code\": \"" + code + "\"," +
                          "\"redirect_uri\": \"" + redirectUrl + "\"" +
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