using System;
using System.Collections.Generic;

public static class Extensions
{
    public static IEnumerable<string> SplitBy(this string str, int splitLength)
    {
        if (String.IsNullOrEmpty(str)) throw new ArgumentException();
        if (splitLength < 1) throw new ArgumentException();

        for (int i = 0; i < str.Length; i += splitLength)
        {
            if (splitLength + i > str.Length)
                splitLength = str.Length - 1;

            yield return str.Substring(i, splitLength);
        }
    }
}