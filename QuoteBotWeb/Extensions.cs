using Domain.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace QuoteBotWeb
{
    public static class Extensions
    {
        public const string AuthEntryKey = "key";

        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }
        
        public static AuthEntry GetAuthEntry(this HttpContext context)
        {
            return (AuthEntry)context.Items["key"];
        }

        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}
