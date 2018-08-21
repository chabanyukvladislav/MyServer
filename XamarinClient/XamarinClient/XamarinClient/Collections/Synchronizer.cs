using System.Collections.Generic;
using System.Linq;
using XamarinClient.DatabaseContext;
using XamarinClient.Enum;
using XamarinClient.Models;

namespace XamarinClient.Collections
{
    static class Synchronizer
    {
        private static readonly Context Context;

        static Synchronizer()
        {
            Context = new Context();
        }

        public static List<LocalAction> GetItems()
        {
            return Context.Local.ToList();
        }

        public static void AddItem(People value)
        {
            Context.Local.Add(new LocalAction() { People = value, Type = TypeOfActions.Add });
            Context.SaveChanges();
        }

        public static void EditItem(People value)
        {
            Context.Local.Add(new LocalAction() { People = value, Type = TypeOfActions.Edit });
            Context.SaveChanges();
        }

        public static void DeleteItem(People value)
        {
            LocalAction la = Context.Local.FirstOrDefault(el => el.People == value && el.Type == TypeOfActions.Add);
            if (la != null)
                Context.Local.Remove(la);
            else
                Context.Local.Add(new LocalAction() { People = value, Type = TypeOfActions.Delete });
            Context.SaveChanges();
        }

        public static void Clear()
        {
            Context.Local.RemoveRange(Context.Local);
        }
    }
}
