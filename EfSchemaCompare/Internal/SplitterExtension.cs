// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Text.RegularExpressions;

namespace EfSchemaCompare.Internal
{
    internal static class SplitterExtension
    {
        private static readonly Regex Reg = new Regex("([a-z,0-9](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", RegexOptions.Compiled);

        /// <summary>
        /// This splits up a string based on capital letters
        /// e.g. "MyAction" would become "My Action" and "My10Action" would become "My10 Action"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SplitCamelCase(this string str)
        {
            return Reg.Replace(str, "$1 ");
        }

        public static string SplitCamelCaseToLower(this string str)
        {
            return str.SplitCamelCase().ToLower();
        }

        public static string SplitCamelCaseToLower(this Enum val)
        {
            return val.ToString().SplitCamelCase().ToLower();
        }

        public static string SplitCamelCaseToUpper(this Enum val)
        {
            return val.ToString().SplitCamelCase().ToUpper();
        }
    }
}
