// Copyright (c) 2024 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Test")]

namespace EfSchemaCompare.Internal;

/// <summary>
/// This class handles the ignoring of errors that the users wants to be ignored.
/// </summary>
internal class FindErrorsToIgnore
{
    internal static CompareLog DecodeCompareTextToCompareLog(string errorString)
    {
        errorString = errorString.Trim();

        //These parts of the errors are the same in all the error pattern
        var state = GetEnumType<CompareState>(errorString, null, ": ");
        var attribute = GetEnumType<CompareAttributes>(errorString, " ,", ". ");
        //string items. Some errors don't have Expected or Found, in which case it has null 
        var expected = GetStringInError(errorString, "Expected = ", null);
        var found = GetStringInError(errorString, ". Found = ", null);
        var name = GetStringInError(errorString, "'", "'");

        //The type part has two patterns
        //1. "OK: DbContext ...", where "DbContext" is the type
        //2. "NOT IN DATABASE: BookDetail->ForeignKey ...", where "ForeignKey" is the type
        var beforeString = errorString.Contains("->") ? "->" : ": ";
        var type = GetEnumType<CompareType>(errorString, beforeString, " ");

        //These are either different in the various errors strings, or aren't in some error pattens

        return new CompareLog(type, state, name, attribute, expected, found);
    }

    /// <summary>
    /// This returns a section of the errorString, defined by unique before and end strings
    /// e.g. an errorString of "EXTRA IN DATABASE: Table 'Customers'" you could extract the
    /// "Table" by setting beforeString to ": " and setting afterString to "\'".
    /// NOTE: If the beforeString and afterString aren't found it returns an empty string
    /// </summary>
    /// <param name="errorString">A string containing an error created during  </param>
    /// <param name="beforeString">NOTE: If the beforeString is null/empty it will start at the first letter</param>
    /// <param name="afterString">NOTE: If the beforeString is null/empty it will end it will go to the end </param>
    /// <returns>Returns the string between the two start/end strings.
    /// NOTE: If either the beforeString or afterString aren't found it will return null, i.e. not found</returns>
    private static string GetStringInError(string errorString, string beforeString, string afterString)
    {
        var typeStart = 0;
        if (!String.IsNullOrEmpty(beforeString))
        {
            typeStart = errorString.IndexOf(beforeString) + beforeString.Length;
            if (errorString.IndexOf(beforeString, StringComparison.Ordinal) == -1)
                return null; //Could not find the beforeString
        }

        var typeEnd = errorString.Length;
        if (!String.IsNullOrEmpty(afterString))
        {
            typeEnd = errorString.Substring(typeStart).IndexOf(afterString) + typeStart;
            if (errorString.IndexOf(afterString, StringComparison.Ordinal) == -1)
                return null; //Could not find the beforeString
        }

        string result = null;
        if (String.IsNullOrEmpty(afterString))
        {
            result = errorString.Substring(typeStart);
        }
        else
        {
            result = errorString.Substring(typeStart, typeEnd - typeStart);
        }
        return result.Length == 0 || result == "<null>" ? null : result;
    }

    /// <summary>
    /// This takes a string holding the name of the Enum type and returns the correct Type/Enum.
    /// If there is no Enum Name, then it returns the first Enum
    /// See <see cref="GetStringInError"/> summary for the details of how it got the string to be turned into an Enum
    /// </summary>
    /// <typeparam name="T">Is <see cref="CompareState"/>, <see cref="CompareType"/>, or <see cref="CompareAttributes"/></typeparam>
    /// <param name="errorString">The error message</param>
    /// <param name="beforeString">A unique string that is before the error message</param>
    /// <param name="afterString">A unique string that comes after the error message</param>
    /// <returns>The correct Type/Enum entry</returns>
    private static T GetEnumType<T>(string errorString, string beforeString, string afterString) where T : Enum
    {
        var foundSection = GetStringInError(errorString, beforeString, afterString);
        if (foundSection == null)
            return Enum.GetValues(typeof(T)).Cast<T>().First();
        return (T)Enum.Parse(typeof(T), foundSection.Replace(" ", ""), true);
    }


}