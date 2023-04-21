using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MyCourse.Models.Entities;

namespace MyCourse.Models.Services.Infrastructure
{
    public partial class MyCourseDbContext : DbContext
    {
        public MyCourseDbContext(DbContextOptions<MyCourseDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Lesson> Lessons { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //il codice .UseSqlite è stato spostato in Startup.cs
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Courses"); //Se la tabella si chiama uguale alla proprietà espressa qui accanto, allora il codice è superfluo e si può anbche non scrivere
                entity.HasKey(course => course.Id); //Superfluo se la propietà si chiama proprio "Id", oppure "CousesId", in quel caso EF capisce che è una PK
                //In caso di PK con più colonne, si deve scrive il riga di codice commentata qua sotto
                //entity.HasKey(course=> new {course.Id, course.Author});

                /*-----vvv--Mapping per gli owned types--vvvvv-----------------(cosa sono? controllare documentazione lezione 11)--*/
                /*
                Le righe sotto sono superflue, in teoria basterebbe scrivere
                 entity.OwnsOne(course => course.CurrentPrice);
                 In caso che le propietà si chiamino CurrentPrice_qualcosa
                */
                entity.OwnsOne(course => course.CurrentPrice, builder=>{
                    builder.Property(money=> money.Currency) //SU c# CURRENCY è una ENUM, sul DB questa è un campo txt
                    .HasConversion<string>()  //per convertire questa stringa dal db a una ENUM
                    .HasColumnName("CurrentPrice_Currency");
                    builder.Property(money=> money.Amount).HasColumnName("CurrentPrice_Amount");
                });

                //qua sotto viene usata la versione ridotta--vvvv------------------------
                    entity.OwnsOne(course => course.FullPrice, builder=>{
                        builder.Property(money=> money.Currency).HasConversion<string>();
                });
                //-fine versione ridotta--------------------------------------------------------------------


                //-Inizio Mapping per le relazioni-vvvv----------------------
                entity
                    .HasMany(course => course.Lessons)
                    .WithOne(lesson => lesson.Course)
                    .HasForeignKey(lesson => lesson.CourseId); // QUESTA riga è superflua SE ha il sufisso "id": Es. "CourseId"
                //---Fine mapping per le relazioni
                //Questa operazione si poteva fare ANCHE nel modelbuilder di "LESSONS", com'è comentato qua sotto

                #region Mapping generato automaticamente dal tool di reverse engineering
                /*
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Author)
                    .IsRequired()
                    .HasColumnType("TEXT (100)");

                entity.Property(e => e.CurrentPriceAmount)
                    .IsRequired()
                    .HasColumnName("CurrentPrice_Amount")
                    .HasColumnType("NUMERIC")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.CurrentPriceCurrency)
                    .IsRequired()
                    .HasColumnName("CurrentPrice_Currency")
                    .HasColumnType("TEXT(3)")
                    .HasDefaultValueSql("'EUR'");

                entity.Property(e => e.Description).HasColumnType("TEXT (10000)");

                entity.Property(e => e.Email).HasColumnType("TEXT (100)");

                entity.Property(e => e.FullPriceAmount)
                    .IsRequired()
                    .HasColumnName("FullPrice_Amount")
                    .HasColumnType("NUMERIC")
                    .HasDefaultValueSql("0");

                entity.Property(e => e.FullPriceCurrency)
                    .IsRequired()
                    .HasColumnName("FullPrice_Currency")
                    .HasColumnType("TEXT(3)")
                    .HasDefaultValueSql("'EUR'");

                entity.Property(e => e.ImagePath).HasColumnType("TEXT (100)");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnType("TEXT (100)");
                    */
                #endregion
            });

            modelBuilder.Entity<Lesson>(entity =>
            {
                /* Qua è come sarebbe il mapping delle relazione da Lesson, invece che da Course
                entity
                    .HasOne(lesson => lesson.Course)
                    .WithMany(course => course.Lessons)
                */
                #region Mapping generato automaticamente dal tool di reverse engineering
                /*
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Description).HasColumnType("TEXT (10000)");

                entity.Property(e => e.Duration)
                    .IsRequired()
                    .HasColumnType("TEXT (8)")
                    .HasDefaultValueSql("'00:00:00'");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasColumnType("TEXT (100)");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Lessons)
                    .HasForeignKey(d => d.CourseId);
                    */
                    #endregion
            });
        }
    }
}
