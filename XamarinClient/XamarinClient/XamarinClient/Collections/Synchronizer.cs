using System;
using System.Collections.Generic;
using System.IO;
using SQLitePCL;
using Xamarin.Forms;
using XamarinClient.Models;

namespace XamarinClient.Collections
{
    static class Synchronizer
    {
        private static readonly string Path;
        private const string FileName = "data.bin";

        static Synchronizer()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), FileName);
                    break;
                case Device.UWP:
                    Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), FileName);
                    break;
                case Device.iOS:
                    Batteries.Init();
                    Path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", FileName);
                    break;
                default:
                    Path = "";
                    break;
            }
        }

        public static List<People> GetItems()
        {
            if (Path == "")
                return new List<People>();
            if (!File.Exists(Path))
                return new List<People>();
            List<People> list = new List<People>();
            StreamReader sr = File.OpenText(Path);
            while (!sr.EndOfStream)
            {
                string name = sr.ReadLine();
                string surname = sr.ReadLine();
                string phone = sr.ReadLine();
                list.Add(new People() {Name = name, Surname = surname, Phone = phone});
            }
            sr.Close();
            return list;
        }

        public static void AddItem(People value)
        {
            if (Path == "")
                return;
            StreamWriter sw = File.AppendText(Path);
            sw.WriteLine(value.Name);
            sw.WriteLine(value.Surname);
            sw.WriteLine(value.Phone);
            sw.Close();
        }

        public static void Clear()
        {
            if (Path == "")
                return;
            if (File.Exists(Path))
                File.Delete(Path);
        }
    }
}
