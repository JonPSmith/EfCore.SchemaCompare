// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.OldTestSupportDbs.Issue002
{
    public class PrimaryKeyGuid
    {
        [Key, MaxLength(50)]
        public Guid NormativeReferenceGuidId { get; set; }

        public string Name { get; set; }
    }
}