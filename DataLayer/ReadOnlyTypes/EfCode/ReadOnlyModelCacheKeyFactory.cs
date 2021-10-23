// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace DataLayer.ReadOnlyTypes.EfCode
{
    //see https://docs.microsoft.com/en-us/ef/core/modeling/dynamic-model
    public class ReadOnlyModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context, bool designTime)
            => context is ReadOnlyDbContext dynamicContext
                ? (context.GetType(), dynamicContext.Config, designTime)
                : (object)context.GetType();

        public object Create(DbContext context)
            => Create(context, false);
    }
}