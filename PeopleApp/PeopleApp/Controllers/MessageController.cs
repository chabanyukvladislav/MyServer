using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PeopleApp.Attributes;
using PeopleApp.DatabaseContext;
using PeopleApp.Hubs;
using PeopleApp.Models;

namespace PeopleApp.Controllers
{
    [Route("api/Message")]
    public class MessageController : Controller
    {
        private readonly RSACryptoServiceProvider rsa;
        private readonly RSAParameters param;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly Context _context;

        public MessageController(Context context, IHubContext<NotificationHub> hub)
        {
            rsa = new RSACryptoServiceProvider();
            param = rsa.ExportParameters(false);
            _context = context;
            _hubContext = hub;
        }

        [HttpGet]
        [Authorized]
        public IEnumerable<Messager> Get()
        {
            try
            {
                string userId = Request.Headers["userId"];
                IEnumerable<Messager> list = _context.Messagers.Where(el => el.FromId == userId || el.ToId == userId);
                return list;
            }
            catch (Exception)
            {
                return new List<Messager>();
            }
        }

        //[HttpGet]
        //[Authorized]
        //public byte[] Get()
        //{
        //    try
        //    {
        //        string userId = Request.Headers["userId"];
        //        List<Messager> list = _context.Messagers.Where(el => el.FromId == userId || el.ToId == userId).ToList();
        //        string exponent = Request.Query["exponent"];
        //        string modul = Request.Query["modul"];
        //        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        //        RSAParameters parameters = new RSAParameters();
        //        parameters.Modulus = InBytes(modul);
        //        parameters.Exponent = InBytes(exponent);
        //        rsa.ImportParameters(parameters);
        //        BinaryFormatter formatter = new BinaryFormatter();
        //        MemoryStream stream = new MemoryStream();
        //        formatter.Serialize(stream, list);
        //        return rsa.Encrypt(stream.ToArray(), false);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new byte[0];
        //    }
        //}

        //private byte[] InBytes(string val)
        //{
        //    string[] arr = val.TrimEnd().Split(' ');
        //    byte[] res = new byte[arr.Length];
        //    for (int i = 0; i < arr.Length; ++i)
        //    {
        //        res[i] = Convert.ToByte(arr[i]);
        //    }
        //    return res;
        //}

        [HttpGet("{id}")]
        [Authorized]
        public Messager Get(Guid id)
        {
            try
            {
                string userId = Request.Headers["userId"];
                Messager mes = _context.Messagers.FirstOrDefault(el => el.Id == id && (el.FromId == userId || el.ToId == userId));
                return mes;
            }
            catch (Exception)
            {
                return new Messager();
            }
        }

        [HttpPost]
        [Authorized]
        public void Post([FromBody]Messager value)
        {
            try
            {
                string userId = Request.Headers["userId"];
                value.FromId = userId;
                value.FromNick = _context.Users.Find(userId).Nickname;
                value.Date = DateTime.Now;
                _context.Messagers.Add(value);
                _context.SaveChanges();
                _hubContext.Clients.All.SendAsync("AddMessage", value.Id);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
