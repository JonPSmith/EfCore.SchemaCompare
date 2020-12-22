// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLayer.BookApp.EfClasses
{
    public class Book                        
    {
        public int BookId { get; set; }

        [Required] 
        [MaxLength(256)] 
        public string Title { get; set; }

        public string Description { get; set; }
        public DateTime PublishedOn { get; set; }

        [MaxLength(64)] 
        public string Publisher { get; set; }

        public decimal Price { get; set; }

        [MaxLength(512)] 
        public string ImageUrl { get; set; }

        public bool SoftDeleted { get; set; }

        //-----------------------------------------------
        //relationships

        public PriceOffer Promotion { get; set; }
        public ICollection<Review> Reviews { get; set; }

        public ICollection<BookAuthor> 
            AuthorsLink { get; set; }

        public ICollection<Tag> Tags { get; set; }
    }
}