﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace LORModingBase.CustomExtensions
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class IEnumerable
    {
        /// <summary>
        /// Safely foreach IEnumberable object
        /// </summary>
        public static void ForEachSafe<T>(this IEnumerable<T> source, Action<T> foreachCallBack)
        {
            foreach (T eachObject in source ?? Enumerable.Empty<T>())
                foreachCallBack(eachObject);
        }

        /// <summary>
        /// Find string for given string
        /// </summary>
        public static List<string> FindAll_Contains(this string[] source, string strToSearch, bool ignoreCase = false)
        {
            List<string> findList = new List<string>();
            foreach (string eachStr in source)
            {
                if (!ignoreCase && eachStr.Contains(strToSearch))
                    findList.Add(eachStr);
                if (ignoreCase && eachStr.ToLower().Contains(strToSearch.ToLower()))
                    findList.Add(eachStr);
            }
            return findList;
        }
    }
}
