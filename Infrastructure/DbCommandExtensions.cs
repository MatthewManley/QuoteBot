using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class DbCommandExtensions
    {
        public static void AddParameterWithValue(this DbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            cmd.Parameters.Add(param);
        }

        public static async IAsyncEnumerable<T> ReadToEnumerable<T>(this DbDataReader reader, Func<T> func)
        {
            while (await reader.ReadAsync())
            {
                yield return func();
            }
        }

        public static async Task<T> ReadFirstOrDefault<T>(this DbDataReader reader, Func<T> func)
        {
            if (await reader.ReadAsync())
            {
                return func();
            }
            return default(T);
        }
    }
}