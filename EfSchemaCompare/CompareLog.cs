// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using EfSchemaCompare.Internal;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Test")]

namespace EfSchemaCompare
{
#pragma warning disable 1591
    /// <summary>
    /// This is used to define what is being compared
    /// </summary>
    public enum CompareType { NoSet,
        MatchAnything,
        //Software side
        DbContext, Entity, Property,
        //Database side (used for ExtraInDatabase)
        Database, Table, Column,
        //Used for both
        PrimaryKey, ForeignKey, Index}
    /// <summary>
    /// This defines the result of a comparision
    /// </summary>
    public enum CompareState { Debug, Ok, NotChecked, Different, NotInDatabase, ExtraInDatabase }
    /// <summary>
    /// This contains extra information on what exactly was compared
    /// </summary>
    public enum CompareAttributes { NotSet,
        //This is used to match any attribute for the ignore list
        MatchAnything,
        //column items
        ColumnName, ColumnType, Nullability, DefaultValueSql, ComputedColumnSql, PersistentComputedColumn, ValueGenerated,
        //Tables
        TableName,
        //keys - primary, foreign, alternative
        PrimaryKey, ConstraintName, IndexConstraintName, Unique, DeleteBehavior,
        //MarkAsNotChecked 
        NotMappedToDatabase
    }
#pragma warning restore 1591

    /// <summary>
    /// This holds the log of each compare done
    /// </summary>
    public class CompareLog
    {
        /// <summary>
        /// This constructor either creates a new log (used internally) or allows the user to create a log for ignore matching
        /// </summary>
        /// <param name="type"></param>
        /// <param name="state"></param>
        /// <param name="name"></param>
        /// <param name="attribute"></param>
        /// <param name="expected"></param>
        /// <param name="found"></param>
        [JsonConstructor]
        public CompareLog(CompareType type, CompareState state, string name, 
            CompareAttributes attribute = CompareAttributes.MatchAnything, string expected = null, string found = null)
        {
            Type = type;
            State = state;
            Name = name;
            Attribute = attribute;
            Expected = expected;
            Found = found;
            SubLogs = new List<CompareLog>();
        }

        /// <summary>
        /// Because an EF Core DbContext has a hierarchy then the logs are also in a hierarchy
        /// For EF Core this is DbContext->Entity classes->Properties
        /// </summary>
        public List<CompareLog> SubLogs { get; }

        /// <summary>
        /// This holds what it is comparing
        /// </summary>
        public CompareType Type { get; }

        /// <summary>
        /// This holds the result of the comparison
        /// </summary>
        public CompareState State { get; }

        /// <summary>
        /// This holds the name of the primary thing it is comparing, e.g. MyEntity, MyProperty
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// This contains extra information to define exactly what is being compared, for instance the ColumnName that a property is mapped to
        /// </summary>
        public CompareAttributes Attribute { get; }

        /// <summary>
        /// This holds what EF Core expects to see
        /// </summary>
        public string Expected { get; }

        /// <summary>
        /// This holds what was found in the database
        /// </summary>
        public string Found { get; }

        /// <summary>
        /// This provides a human-readable version of a CompareLog
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //Typical output would be: DIFFERENT: Column 'Id', column type. Expected = varchar(20), Found = nvarchar(20)
            return ToStringDifferentStart($"{State.SplitCamelCaseToUpper()}: ");
        }

        /// <summary>
        /// This returns all the logs, with an indentation for each level in the hierarchy
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="indent"></param>
        /// <returns></returns>
        public static IEnumerable<string> AllResultsIndented(IReadOnlyList<CompareLog> logs, string indent = "")
        {
            foreach (var log in logs)
            {
                yield return (indent + log);
                if (log.SubLogs.Any())
                {
                    foreach (var text in AllResultsIndented(log.SubLogs, indent + "   "))
                    {
                        yield return text;
                    }
                }
            }
        }

        /// <summary>
        /// This returns a string per error, in human-readable form
        /// </summary>
        /// <param name="logs"></param>
        /// <param name="parentNames"></param>
        /// <returns></returns>
        public static IEnumerable<string> ListAllErrors(IReadOnlyList<CompareLog> logs, Stack<string> parentNames = null)
        {
            //This only includes the DbContext if there were multiple DbContexts at the top layer
            var firstCall = parentNames == null;
            var doPushPop = !(firstCall && logs.GroupBy(x => x.Type).Count() < 2);
            if (firstCall)
            {
                parentNames = new Stack<string>();  
            }

            foreach (var log in logs)
            {

                if (log.State != CompareState.Ok)
                    yield return FormFullRefError(log, parentNames);
                if (log.SubLogs.Any())
                {
                    if (doPushPop) parentNames.Push(log.Name);
                    foreach (var errors in ListAllErrors(log.SubLogs, parentNames))
                    {
                        yield return errors;
                    }
                    if (doPushPop) parentNames.Pop();
                }
            }
        }

        //-------------------------------------------------------
        //internal

        internal bool ShouldIgnoreThisLog(IReadOnlyList<CompareLog> ignoreList)
        {
            return ignoreList.Any() && ignoreList.Any(ShouldBeIgnored);
        }

        //-------------------------------------------------------
        //private

        private bool ShouldBeIgnored(CompareLog ignoreItem)
        {
            if (ignoreItem.State != State)
                return false;

            var result = (ignoreItem.Type == CompareType.MatchAnything || ignoreItem.Type == Type)
                && (ignoreItem.Attribute == CompareAttributes.MatchAnything || ignoreItem.Attribute == Attribute)
                && (ignoreItem.Name == null || ignoreItem.Name == Name)
                && (ignoreItem.Expected == null || ignoreItem.Expected == Expected || ignoreItem.Expected == "<null>")
                && (ignoreItem.Found == null || ignoreItem.Found == Found || ignoreItem.Found == "<null>");

            return result;
        }

        private static string ReplaceNullTokenWithNull(string str)
        {
            return str == "<null>" ? null : str;
        }

        private string ToStringDifferentStart(string start)
        {
            //Typical output would be: Column 'Id', column type is Different : Expected = varchar(20), Found = nvarchar(20)
            var result = $"{start}{Type} '{Name}'";
            if (Attribute != CompareAttributes.NotSet)
                result += $", {Attribute.SplitCamelCaseToLower()}";
            if (State == CompareState.Ok)
                return result;

            var sep = ". F";
            if (State != CompareState.ExtraInDatabase)
            {
                result += $". Expected = {Expected ?? "<null>"}";
                sep = ", f";
            }
            if (Found != null || State == CompareState.Different)
                result += $"{sep}ound = {Found ?? "<null>"}";
            return result;
        }

        private static string FormFullRefError(CompareLog log, Stack<string> parents)
        {
            string start = $"{log.State.SplitCamelCaseToUpper()}: ";
            if (parents.Any())
                start += string.Join("->", parents.ToArray().Reverse()) + "->";
            return log.ToStringDifferentStart(start);
        }
    }  
    
}