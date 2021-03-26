//using MySql.Data.MySqlClient;
//using System;
//using System.Collections.Generic;
//using System.Data.Common;

//namespace Infrastructure
//{
//    public static class DbCommandExtensions
//    {
//        public static void AddParameterWithValue(this DbCommand cmd, string name, object value)
//        {
//            var param = cmd.CreateParameter();
//            param.ParameterName = name;
//            param.Value = value;
//            cmd.Parameters.Add(param);
//        }

//        public static async IAsyncEnumerable<T> ReadToEnumerable<T>(this MySqlDataReader reader, Func<MySqlDataReader, T> func)
//        {
//            while (await reader.ReadAsync())
//            {
//                yield return func(reader);
//            }
//        }
//    }
//}