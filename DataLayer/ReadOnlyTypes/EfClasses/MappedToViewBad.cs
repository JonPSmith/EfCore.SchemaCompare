// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.ReadOnlyTypes.EfClasses
{
    [Keyless]
    public class MappedToViewBad
    {
        public int Id { get; set; }

        public DateTime MyDateTime { get; set; }

        public int MyString { get; set; }
    }
}