// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

namespace EfSchemaCompare.Internal
{
    internal static class AppSettings
    {
        /// <summary>
        /// This will look for a appsettings.json file in the top level of the calling assembly and read content
        /// </summary>
        /// <param name="callingAssembly">If called by an internal method you must provide the other calling assembly</param>
        /// <param name="settingsFilename">This allows you to open a json configuration file of this given name</param>
        /// <returns></returns>
        public static IConfigurationRoot GetConfiguration(Assembly callingAssembly = null, string settingsFilename = "appsettings.json") 
        {
            var callingProjectPath =                      //#B
                GetCallingAssemblyTopLevelDir(callingAssembly ?? Assembly.GetCallingAssembly()); //#B
            var builder = new ConfigurationBuilder()               //#C
                .SetBasePath(callingProjectPath)                   //#C
                .AddJsonFile(settingsFilename, optional: true); //#C
            return builder.Build(); //#D
        }

        /// <summary>
        /// This will return the absolute file path of the calling assembly, or the assembly provided 
        /// </summary>
        /// <param name="callingAssembly">optional: provide the calling assembly. default is to use the current calling assembly</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)] //see https://docs.microsoft.com/en-gb/dotnet/api/system.reflection.assembly.getcallingassembly?view=netstandard-2.0#System_Reflection_Assembly_GetCallingAssembly
        private static string GetCallingAssemblyTopLevelDir(Assembly callingAssembly = null)
        {
            var binDir = $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}";
            var pathToManipulate = (callingAssembly ?? Assembly.GetCallingAssembly()).Location;

            var indexOfPart = pathToManipulate.IndexOf(binDir, StringComparison.OrdinalIgnoreCase);
            if (indexOfPart <= 0)
                throw new Exception($"Did not find '{binDir}' in the assembly. Do you need to provide the callingAssembly parameter?");

            return pathToManipulate.Substring(0, indexOfPart);
        }
    }
}