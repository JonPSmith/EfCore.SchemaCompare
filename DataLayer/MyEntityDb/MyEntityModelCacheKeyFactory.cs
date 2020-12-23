// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DataLayer.MyEntityDb
{
    //see https://docs.microsoft.com/en-us/ef/core/modeling/dynamic-model
    public class MyEntityModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context)
            => context is MyEntityDbContext dynamicContext
                ? (context.GetType(), dynamicContext.Config)
                : (object)context.GetType();
    }
}