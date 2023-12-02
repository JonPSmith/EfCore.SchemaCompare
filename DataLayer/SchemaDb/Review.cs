// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace DataLayer.SchemaDb
{
    public class Review
    {
        public int ReviewId { get; set; }

        public int NumStars { get; set; }

        //-----------------------------------------
        //many-to-many relationships

        public ICollection<Book> Books { get; set; }
    }

}