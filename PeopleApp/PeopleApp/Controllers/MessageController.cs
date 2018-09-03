using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using PeopleApp.Attributes;
using PeopleApp.DatabaseContext;
using PeopleApp.Hubs;
using PeopleApp.Models;

namespace PeopleApp.Controllers
{
    [Route("api/Message")]
    public class MessageController : Controller
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly Context _context;

        private static RSACryptoServiceProvider Rsa { get; set; }

        public MessageController(Context context, IHubContext<NotificationHub> hub)
        {
            _context = context;
            _hubContext = hub;
        }

        [HttpGet]
        [Authorized]
        public List<byte[]> Get()
        {
            try
            {
                string userId = Request.Headers["userId"];
                List<Messager> list = _context.Messagers.Where(el => el.FromId == userId || el.ToId == userId).ToList();
                string str = JsonConvert.SerializeObject(list);

                string exponent = Request.Query["exponent"];
                string modul = Request.Query["modul"];

                RijndaelManaged sa = new RijndaelManaged();

                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                RSAParameters parameters = new RSAParameters();
                parameters.Modulus = InBytes(modul);
                parameters.Exponent = InBytes(exponent);
                rsa.ImportParameters(parameters);

                byte[] saKey = rsa.Encrypt(sa.Key, false);

                ICryptoTransform transform = sa.CreateEncryptor();

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();
                formatter.Serialize(stream, str);
                byte[] bytesData = stream.ToArray();

                byte[] rsaData = transform.TransformFinalBlock(bytesData, 0, bytesData.Length);

                Rsa = new RSACryptoServiceProvider();
                RSAParameters clientParam = Rsa.ExportParameters(false);

                List<byte[]> result = new List<byte[]>
                {
                    saKey,
                    sa.IV,
                    rsaData,
                    clientParam.Exponent,
                    clientParam.Modulus
                };

                return result;
            }
            catch (Exception)
            {
                return new List<byte[]>();
            }
        }

        private byte[] InBytes(string val)
        {
            string[] arr = val.TrimEnd().Split(' ');
            byte[] res = new byte[arr.Length];
            for (int i = 0; i < arr.Length; ++i)
            {
                res[i] = Convert.ToByte(arr[i]);
            }
            return res;
        }

        [HttpGet("{id}")]
        [Authorized]
        public List<byte[]> Get(Guid id)
        {
            try
            {
                string userId = Request.Headers["userId"];
                Messager mes = _context.Messagers.FirstOrDefault(el => el.Id == id && (el.FromId == userId || el.ToId == userId));
                string str = JsonConvert.SerializeObject(mes);

                string exponent = Request.Query["exponent"];
                string modul = Request.Query["modul"];

                RijndaelManaged sa = new RijndaelManaged();

                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                RSAParameters parameters = new RSAParameters
                {
                    Modulus = InBytes(modul),
                    Exponent = InBytes(exponent)
                };
                rsa.ImportParameters(parameters);

                byte[] saKey = rsa.Encrypt(sa.Key, false);

                ICryptoTransform transform = sa.CreateEncryptor();

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();
                formatter.Serialize(stream, str);
                byte[] bytesData = stream.ToArray();

                byte[] rsaData = transform.TransformFinalBlock(bytesData, 0, bytesData.Length);

                List<byte[]> result = new List<byte[]>
                {
                    saKey,
                    sa.IV,
                    rsaData
                };

                return result;
            }
            catch (Exception)
            {
                return new List<byte[]>();
            }
        }

        [HttpPost]
        [Authorized]
        public void Post([FromBody]List<byte[]> value)
        {
            try
            {
                byte[] saKey = Rsa.Decrypt(value[0], false);

                RijndaelManaged sa = new RijndaelManaged();
                ICryptoTransform transform = sa.CreateDecryptor(saKey, value[1]);
                byte[] data = transform.TransformFinalBlock(value[2], 0, value[2].Length);

                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                string json = formatter.Deserialize(stream) as string;

                Messager item = JsonConvert.DeserializeObject<Messager>(json,
                    new JsonSerializerSettings() { Error = (sender, args) => { args.ErrorContext.Handled = true; } });

                string userId = Request.Headers["userId"];
                item.FromId = userId;
                item.FromNick = _context.Users.Find(userId).Nickname;
                item.Date = DateTime.Now;
                _context.Messagers.Add(item);
                _context.SaveChanges();
                _hubContext.Clients.All.SendAsync("AddMessage", item.Id);
            }
            catch (Exception ex)
            {
                // ignored
            }
        }
    }
}
