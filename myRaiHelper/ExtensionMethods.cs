using myRai.Data.Interface;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace myRaiHelper
{
    public static class ExtensionMethods
    {
        private static string[] unita = { "zero", "uno", "due", "tre", "quattro", "cinque", "sei", "sette", "otto", "nove", "dieci", "undici", "dodici", "tredici", "quattordici", "quindici", "sedici", "diciassette", "diciotto", "diciannove" };
        private static string[] decine = { "", "dieci", "venti", "trenta", "quaranta", "cinquanta", "sessanta", "settanta", "ottanta", "novanta" };
        public static string ConvertNumberToReadableString(this long num)
        {
            string result = "";
            long mod = 0;
            long i = 0;

            if (num > 0 && num < 20)
            {
                result = unita[num];
            }
            else if (num < 100)
            {
                mod = num % 10;
                i = num / 10;
                switch (mod)
                {
                    case 0:
                        result = decine[i];
                        break;
                    case 1:
                        result = decine[i].Substring(0, decine[i].Length - 1) + unita[mod];
                        break;
                    case 8:
                        result = decine[i].Substring(0, decine[i].Length - 1) + unita[mod];
                        break;
                    default:
                        result = decine[i] + unita[mod];
                        break;
                }
            }
            else if (num < 1000)
            {
                mod = num % 100;
                i = (num - mod) / 100;
                switch (i)
                {
                    case 1:
                        result = "cento";
                        break;
                    default:
                        result = unita[i] + "cento";
                        break;
                }
                result = result + ConvertNumberToReadableString(mod);
            }
            else if (num < 10000)
            {
                mod = num % 1000;
                i = (num - mod) / 1000;
                switch (i)
                {
                    case 1:
                        result = "mille";
                        break;
                    default:
                        result = unita[i] + "mila";
                        break;
                }
                result = result + ConvertNumberToReadableString(mod);
            }
            else if (num < 1000000)
            {
                mod = num % 1000;
                i = (num - mod) / 1000;
                switch ((num - mod) / 1000)
                {
                    default:
                        if (i < 20)
                        {
                            result = unita[i] + "mila";
                        }
                        else
                        {
                            result = ConvertNumberToReadableString(i) + "mila";
                        }
                        break;
                }
                result = result + ConvertNumberToReadableString(mod);
            }
            else if (num < 1000000000)
            {
                mod = num % 1000000;
                i = (num - mod) / 1000000;
                switch (i)
                {
                    case 1:
                        result = "unmilione";
                        break;

                    default:
                        result = ConvertNumberToReadableString(i) + "milioni";

                        break;
                }
                result = result + ConvertNumberToReadableString(mod);
            }
            else if (num < 1000000000000)
            {
                mod = num % 1000000000;
                i = (num - mod) / 1000000000;
                switch (i)
                {
                    case 1:
                        result = "unmiliardo";
                        break;

                    default:
                        result = ConvertNumberToReadableString(i) + "miliardi";

                        break;
                }
                result = result + ConvertNumberToReadableString(mod);
            }

            return result;
        }

        public static string AmountToReadableString(this decimal num)
        {
            var parteIntera = (long)Decimal.Truncate(num);
            var parteDecimale = (int)(Math.Round(num - parteIntera, 2) * 100);
            return string.Format("{0}/{1:00}", parteIntera.ConvertNumberToReadableString(), parteDecimale);
        }

        public static int ToMinutes(this string value)
        {
            if (value == null || value.Trim() == "" || value.Trim().Length < 4) return 0;

            string[] array = new string[2];

            if (value.IndexOf(':') > 0)
                array = value.Split(':');
            else
            {
                array[0] = value.Substring(0, 2);
                array[1] = value.Substring(2, 2);
            }
            return Int32.Parse(array[0]) * 60 + Int32.Parse(array[1]);
        }
        public static string ToHH_MM(this int value)
        {
            if (value <= 0) return "00:00";
            int h = (int)value / 60;
            int min = value - (h * 60);
            return h.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0');
        }
        public static string UpperFirst(this string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return value;
            else if (value.Length == 1)
                return value.ToUpper();
            else
            {
                value = char.ToUpper(value[0]) + value.Substring(1).ToLower();
                value = new Regex(@"\b(?!Xi\b)(X|XX|XXX|XL|L|LX|LXX|LXXX|XC|C)?(I|II|III|IV|V|VI|VII|VIII|IX)?\b", RegexOptions.IgnoreCase).Replace(value, match => match.Value.ToUpperInvariant());
                return value;
            }
        }
        public static string TitleCase(this string value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return value;
            else if (value.Length == 1)
                return value.ToUpper();
            else
                return string.Join(" ", value.Split(' ').Select(x => string.Join("'", x.Split('\'').Select(y => y.UpperFirst()))));
        }

        public static void Clear<T>(this DbSet<T> dbSet) where T : class
        {
            IEnumerable<T> list = dbSet.AsEnumerable();
            foreach (var item in list)
            {
                dbSet.Remove(item);
            }
        }
        public static void RemoveWhere<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> expr) where T : class
        {
            List<T> list = dbSet.Where(expr).ToList();
            foreach (var item in list)
            {
                dbSet.Remove(item);
            }
        }

        public static void RemoveWhere<T>(this ICollection<T> collection, Func<T, bool> predicate) where T : class
        {
            IEnumerable<T> list = collection.Where(predicate).ToList();
            foreach (var item in list)
            {
                collection.Remove(item);
            }
        }

        public static int GeneraOid<T>(this DbSet<T> dbSet, Func<T, int> selector, int maxCifre = 0) where T : class
        {
            int oid = 0;
            bool trovato = true;
            Random rnd = new Random();

            int maxNum = Int32.MaxValue;
            if (maxCifre > 0)
                maxNum = (int)Math.Pow(10, maxCifre) - 1;

            while (trovato)
            {
                oid = rnd.Next(maxNum);
                trovato = dbSet.Local.Select(selector).Any(y => y == oid) || dbSet.AsNoTracking().Select(selector).Any(x => x == oid);
            }

            return oid;
        }

        public static int GeneraPrimaryKey<T>(this DbSet<T> dbSet, int maxCifre = 0) where T : class
        {
            int oid = 0;
            bool trovato = true;
            Random rnd = new Random();

            int maxNum = Int32.MaxValue;
            int minNum = 0;
            if (maxCifre > 0)
            {
                maxNum = (int)Math.Pow(10, maxCifre) - 1;
                minNum = (int)Math.Pow(10, maxCifre - 1);
            }

            while (trovato)
            {
                oid = rnd.Next(minNum, maxNum);
                trovato = dbSet.Find(oid) != null;
            }

            return oid;
        }

        public static int GeneraComposedPrimaryKey<T>(this DbSet<T> dbSet, int maxCifre, params object[] composedKey) where T : class
        {
            int oid = 0;
            bool trovato = true;
            Random rnd = new Random();

            int maxNum = Int32.MaxValue;
            int minNum = 0;
            if (maxCifre > 0)
            {
                maxNum = (int)Math.Pow(10, maxCifre) - 1;
                minNum = (int)Math.Pow(10, maxCifre - 1);
            }

            while (trovato)
            {
                oid = rnd.Next(minNum, maxNum);
                var tmp = new object[] { oid }.Union(composedKey).ToArray();
                trovato = dbSet.Find(tmp) != null;
            }

            return oid;
        }


        public static IEnumerable<T> ValidToday<T>(this IEnumerable<T> source)
        {
            if (source == null) return null;
            if (typeof(T).GetProperty("data_inizio_validita") == null
                || typeof(T).GetProperty("data_fine_validita") == null)
                return source;

            ParameterExpression t = Expression.Parameter(typeof(T), "t");
            Expression comparison = Expression.LessThanOrEqual(
                Expression.Property(t, "data_inizio_validita"),
                    Expression.Constant(DateTime.Now));
            Expression prop2 = Expression.Property(t, "data_fine_validita");
            Expression comparison2 = Expression.Equal(
                Expression.Property(t, "data_fine_validita"),
                    Expression.Constant(null));
            Expression prop3 = Expression.Property(t, "data_fine_validita");
            var converted = Expression.Convert(Expression.Property(t, "data_fine_validita"), typeof(DateTime));
            Expression comparison3 = Expression.GreaterThan(
                Expression.Convert(
                    Expression.Property(t, "data_fine_validita"), typeof(DateTime)), Expression.Constant(DateTime.Now));

            Expression datafine = Expression.OrElse(comparison2, comparison3);
            Expression final = Expression.And(comparison, datafine);

            var e = source.Where(Expression.Lambda<Func<T, bool>>(final, t).Compile());
            return e;
        }

        /// <summary>
        /// Aggiunge un Ienumerable<T> alla classe specificata. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbSet"></param>
        /// <param name="collection"></param>
        public static void InsertMany<T>(this DbSet<T> dbSet, IEnumerable<T> collection) where T : class
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            foreach (var item in collection)
            {
                dbSet.Add(item);
            }


        }

        public static void AddWithCheck<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (!dict.TryGetValue(key, out TValue temp))
                dict.Add(key, value);
            else
                dict[key] = value;
        }

        public static T Copy<T>(this T from) where T : new()
        {
            return from.CopyProperty<T, T>();
        }
        public static TDest CopyProperty<TOrig, TDest>(this TOrig fromObj) where TDest : new()
        {
            TDest toObj = default(TDest);

            if (fromObj != null)
            {
                toObj = new TDest();

                Type tOrig = typeof(TOrig);
                Type tDest = typeof(TDest);

                foreach (var item in tOrig.GetProperties())
                {
                    var propDest = tDest.GetProperties().FirstOrDefault(x => x.Name == item.Name && x.PropertyType == item.PropertyType);
                    if (propDest != null)
                        propDest.SetValue(toObj, item.GetValue(fromObj, null), null);
                }
            }

            return toObj;
        }

        #region SINTESI1
        public static string Nominativo(this ISintesi1 x)
        {
            return x != null ? 
                    ((!String.IsNullOrWhiteSpace(x.DES_COGNOMEPERS)?x.DES_COGNOMEPERS.TitleCase():"") 
                    + " " 
                    + (!String.IsNullOrWhiteSpace(x.DES_NOMEPERS) ? x.DES_NOMEPERS.TitleCase() : "")).Trim()
                    : null;
        }
        #endregion

        public static void SetCommandTimeout(this DbContext dbContext, int timeOut)
        {
            ((IObjectContextAdapter)dbContext).ObjectContext.CommandTimeout = timeOut;
        }
    }
}
