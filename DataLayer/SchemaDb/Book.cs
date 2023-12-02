// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace DataLayer.SchemaDb
{
    public class Book
    {
        public int BookId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PublishedOn { get; set; }


        //-----------------------------------------
        //one-to-one relationships

        public int AuthorId { get; set; }
        public Author Author { get; set; } 

        //many-to-many relationships
        public ICollection<Book> Books { get; set; }
    }
}