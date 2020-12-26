// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;

namespace DataLayer.ReadOnlyTypes.EfClasses
{
    public class MappedToView
    {
        public int Id { get; set; }

        public DateTime MyDateTime { get; set; }

        public int MyInt { get; set; }

        public string MyString { get; set; }
    }
}