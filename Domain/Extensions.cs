using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public static class Extensions
    {
        public static async Task<List<T>> ToList<T>(this Task<IEnumerable<T>> task) => (await task).ToList();
    }
}
