using System;
using System.Collections.Generic;
using System.Linq;

namespace Tool
{
    public static class ListTool
    {
        public static List<T> RandomSort<T>(this List<T> list) => list.OrderBy(_ => new Random().Next()).ToList();
    }
}
