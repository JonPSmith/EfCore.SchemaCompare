// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.DependencyInjection;

namespace EfSchemaCompare.Internal;

internal static class DatabaseModelFinder
{
    /// <summary>
    /// This obtains the <see cref="DatabaseModelFactory"/> which is needed to find the schema of a database.
    /// Thanks to GitHub @bgrauer-atacom and @lweberprb for suggesting that this library could support extra database providers.
    /// See https://github.com/JonPSmith/EfCore.SchemaCompare/pull/26 for more on this
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static IDatabaseModelFactory GetDatabaseModelFactory(this DbContext context)
    {
        // REVIEW: According to my understanding the assembly is where the DBContext is
        var assembly = context.GetType().Assembly;

        // REVIEW: This is not the same behaviour as in the CLI, but we don't know the real startup project. Using the test project
        // assembly as startup assembly hand enables the possibility to define design services inside of the test project for test purposes.
        var startupAssembly = Assembly.GetEntryAssembly() ?? assembly;

        try
        {
            // REVIEW: In my case, the assembly was loaded correct. Unsure if this works in all cases.
            var designAssembly = Assembly.Load("Microsoft.EntityFrameworkCore.Design");

            var reportHandlerType = designAssembly.GetType("Microsoft.EntityFrameworkCore.Design.OperationReportHandler")
                ?? throw new InvalidOperationException("Unable to create an 'OperationReportHandler' instance. Are you using a supported EntityFrameworkCore version?");
            // TODO: Maybe implement handler actions for logging purposes?
            var reportHandler = Activator.CreateInstance(reportHandlerType, null, null, null, null);

            var reporterType = designAssembly.GetType("Microsoft.EntityFrameworkCore.Design.Internal.OperationReporter")
                ?? throw new InvalidOperationException("Unable to create an 'OperationReporter' instance. Are you using a supported EntityFrameworkCore version?");
            var reporter = Activator.CreateInstance(reporterType, reportHandler);

            var serviceBuilderType = designAssembly.GetType($"Microsoft.EntityFrameworkCore.Design.Internal.DesignTimeServicesBuilder")
                ?? throw new InvalidOperationException("Unable to create an 'DesignTimeServicesBuilder' instance. Are you using a supported EntityFrameworkCore version?");
            var serviceBuilder = Activator.CreateInstance(serviceBuilderType, assembly, startupAssembly, reporter, Array.Empty<string>());

            var serviceProvider = (IServiceProvider?)serviceBuilderType.GetMethods()
                .Where(x => {
                    if (x.Name != "Build")
                    {
                        return false;
                    }
                    var pars = x.GetParameters();
                    if (pars.Length != 1)
                    {
                        return false;
                    }
                    return pars[0].ParameterType.Name == "DbContext";
                })
                .FirstOrDefault()
                ?.Invoke(serviceBuilder, new object[] { context })
                ?? throw new InvalidOperationException("Unable to build design time service provider. Are you using a supported EntityFrameworkCore version?"); ;

            return serviceProvider.GetRequiredService<IDatabaseModelFactory>();
        }
        catch (FileNotFoundException ex)
        {
            throw new InvalidOperationException($"Your startup project '{startupAssembly.GetName()}' doesn't reference " +
                "Microsoft.EntityFrameworkCore.Design. This package is required for the SchemaCompare to work. " +
                "Ensure your startup project is correct, install the package, and try again.", ex);
        }
    }
}