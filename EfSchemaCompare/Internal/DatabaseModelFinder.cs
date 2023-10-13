// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Scaffolding;

namespace EfSchemaCompare.Internal;

internal static class DatabaseModelFinder
{
	public static IDatabaseModelFactory GetDatabaseModelFactory(this DbContext context)
	{
		var providerName = context.Database.ProviderName;
		var providerAssembly = Assembly.Load(providerName!);
		var factoryType =
			providerAssembly.ExportedTypes.First(x => x.GetInterface(nameof(IDatabaseModelFactory)) is not null);

		var constructor = factoryType.GetConstructors().First(x => x.IsPublic);
		var constructorParameters = constructor.GetParameters();

		if (constructorParameters.Length == 0)
			return (IDatabaseModelFactory)constructor.Invoke(null);

		var dbContextInfrastructure = context.GetInfrastructure();
		var resolvedParameters = new object[constructorParameters.Length];
		for (var i = 0; i < constructorParameters.Length; i++)
		{
			var requestedParameterInfo = constructorParameters[i];
			resolvedParameters[i] = dbContextInfrastructure.GetService(requestedParameterInfo.ParameterType);
		}

		return (IDatabaseModelFactory)constructor.Invoke(resolvedParameters);
	}
}