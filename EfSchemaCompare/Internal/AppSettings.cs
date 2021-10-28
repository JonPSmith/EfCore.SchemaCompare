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
        /// <returns>Returns the IConfigurationRoot, or null if can't find the directory</returns>
        public static IConfigurationRoot GetConfiguration(Assembly callingAssembly = null, string settingsFilename = "appsettings.json") 
        {
            var callingProjectPath =                  
                GetCallingAssemblyTopLevelDir(callingAssembly ?? Assembly.GetCallingAssembly());
            if (callingProjectPath == null)
                return null;
            
            var builder = new ConfigurationBuilder()   
                .SetBasePath(callingProjectPath)  
                .AddJsonFile(settingsFilename, optional: true); 
            return builder.Build(); 
        }

        /// <summary>
        /// This will return the absolute file path of the calling assembly, or the assembly provided 
        /// </summary>
        /// <param name="callingAssembly">optional: provide the calling assembly. default is to use the current calling assembly</param>
        /// <returns>The directory, or null if can't be found</returns>
        [MethodImpl(MethodImplOptions.NoInlining)] //see https://docs.microsoft.com/en-gb/dotnet/api/system.reflection.assembly.getcallingassembly?view=netstandard-2.0#System_Reflection_Assembly_GetCallingAssembly
        private static string GetCallingAssemblyTopLevelDir(Assembly callingAssembly = null)
        {
            var binDir = $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}";
            var pathToManipulate = (callingAssembly ?? Assembly.GetCallingAssembly()).Location;

            var indexOfPart = pathToManipulate.IndexOf(binDir, StringComparison.OrdinalIgnoreCase);
            if (indexOfPart <= 0)
                //could not find the folder
                return null;

            return pathToManipulate.Substring(0, indexOfPart);
        }
    }
}