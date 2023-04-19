using System;
using System.Collections.Generic;
using MyCourse.Models.ValueTypes;

namespace MyCourse.Models.Entities
{
    public partial class Course
    {
        public Course(string title, string author)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("The course must have a title");
            }
            if (string.IsNullOrWhiteSpace(author))
            {
                throw new ArgumentException("The course must have an author");
            }
            Title=title;
            Author=author;
            Lessons = new HashSet<Lesson>();
        }

        public long Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Author { get; private set; }
        public string Email { get; private set; }
        public double Rating { get; private set; }
        public Money FullPrice { get; private set; }
        public Money CurrentPrice { get; private set; }

        public void ChangeTitle(string newTitle){
            if(string.IsNullOrEmpty(this.Title))
            {
                throw new ArgumentException("The course must have a title");
            }
            this.Title=newTitle;
        }

        public void ChangePrice(Money newFullPrice, Money NewDiscountPrice)
        {
            if(newFullPrice == null || NewDiscountPrice==null )
            {
                throw new ArgumentException("Prices can't be null");
            }
            if(newFullPrice.Currency != NewDiscountPrice.Currency)
            {
                throw new ArgumentException("Prices can't be null");
            }
            if(newFullPrice.Amount < NewDiscountPrice.Amount){
                throw new ArgumentException("Full price can't be less than current price");
            }
            FullPrice=newFullPrice;
            CurrentPrice= NewDiscountPrice;
        }
        public virtual ICollection<Lesson> Lessons { get; private set; }
    }
}
