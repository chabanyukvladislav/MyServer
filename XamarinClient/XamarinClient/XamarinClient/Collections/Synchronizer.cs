using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using XamarinClient.DatabaseContext;
using XamarinClient.Enum;
using XamarinClient.Models;

namespace XamarinClient.Collections
{
    internal static class Synchronizer
    {
        private static readonly Context Context;

        static Synchronizer()
        {
            Context = new Context();
        }

        public static List<LocalAction> GetItems()
        {
            try
            {
                return Context.Local.ToList();
            }
            catch (Exception)
            {
                return new List<LocalAction>();
            }
        }

        public static void AddItem(People value)
        {
            try
            {
                Context.Local.Add(new LocalAction() { People = value, Type = TypeOfActions.Add });
                Context.SaveChanges();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static void EditItem(People value)
        {
            try
            {
                Context.Local.Add(new LocalAction() { People = value, Type = TypeOfActions.Add });
                Context.SaveChanges();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static void Clear()
        {
            try
            {
                Context.Database.ExecuteSqlCommand("DELETE FROM [Local]");
            }
            catch (Exception ex)
            {
                // ignored
            }
        }
    }
}
