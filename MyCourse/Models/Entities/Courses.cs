using System;
using System.Collections.Generic;
using MyCourse.Models.Enums;
using MyCourse.Models.ValueTypes;

namespace MyCourse.Models.Entities
{
     public partial class Course
     {
          public Course(string title, string author)
          {
               ChangeTitle(title);
               ChangeAuthor(author);
               Title = title;
               Author = author;
               Lessons = new HashSet<Lesson>();

               FullPrice = new Money(Currency.EUR, 0.00m);
               CurrentPrice = new Money(Currency.EUR, 0.00m);
               ImagePath = "/Courses/default.png";
          }

          public int Id { get; private set; }
          public string Title { get; private set; }
          public string Description { get; private set; }
          public string ImagePath { get; private set; }
          public string Author { get; private set; }
          public string Email { get; private set; }
          public double Rating { get; private set; }
          public Money FullPrice { get; private set; }
          public Money CurrentPrice { get; private set; }
          public string RowVersion { get; set; }

          public void ChangeAuthor(string newAuthor)
          {
               if (string.IsNullOrWhiteSpace(newAuthor))
               {
                    throw new ArgumentException("The author must have a name");
               }
               this.Author = newAuthor;
          }
          public void ChangeTitle(string newTitle)
          {
               if (string.IsNullOrEmpty(newTitle))
               {
                    throw new ArgumentException("The course must have a title");
               }
               this.Title = newTitle;
          }
          public void ChangePrice(Money newFullPrice, Money NewDiscountPrice)
          {
               if (newFullPrice == null || NewDiscountPrice == null)
               {
                    throw new ArgumentException("Prices can't be null");
               }
               if (newFullPrice.Currency != NewDiscountPrice.Currency)
               {
                    throw new ArgumentException("Prices can't be null");
               }
               if (newFullPrice.Amount < NewDiscountPrice.Amount)
               {
                    throw new ArgumentException("Full price can't be less than current price");
               }
               FullPrice = newFullPrice;
               CurrentPrice = NewDiscountPrice;
          }
          public void ChangeDescription(string newDescription)
          {
               if (String.IsNullOrEmpty(newDescription))
               {
                    throw new ArgumentException("Description can't be empty");
               }
               Description = newDescription;
          }
          public void ChangeImagePath(string newImagePath)
          {
               if (String.IsNullOrEmpty(newImagePath))
               {
                    throw new ArgumentException("Image is required");
               }
               ImagePath = newImagePath;
          }
          public void ChangeEmail(string newEmail)
          {
               if (String.IsNullOrEmpty(newEmail))
               {
                    throw new ArgumentException("Email is required");
               }
               Email = newEmail;
          }
          public virtual ICollection<Lesson> Lessons { get; private set; }
     }
}
