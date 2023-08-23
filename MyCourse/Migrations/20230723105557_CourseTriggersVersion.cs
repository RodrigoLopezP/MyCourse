using Microsoft.EntityFrameworkCore.Migrations;

namespace MyCourse.Migrations
{
     public partial class CourseTriggersVersion : Migration
     {
          protected override void Up(MigrationBuilder migrationBuilder)
          {
               migrationBuilder.Sql(@"CREATE TRIGGER CoursesSetRowVersionOnInsert
                        AFTER INSERT ON Courses
                        BEGIN
                        UPDATE Courses SET RowVersion = CURRENT_TIMESTAMP WHERE Id=NEW.Id;
                        END;");
               migrationBuilder.Sql(@"CREATE TRIGGER CoursesSetRowVersionOnUpdate
                        AFTER UPDATE ON Courses WHEN NEW.RowVersion <= OLD.RowVersion
                        BEGIN
                        UPDATE Courses SET RowVersion = CURRENT_TIMESTAMP WHERE Id=NEW.Id;
                        END;");
               migrationBuilder.Sql(@"UPDATE Courses SET RowVersion = CURRENT_TIMESTAMP;");

               //Lesson order up
               migrationBuilder.AddColumn<int>(
                 name: "Order",
                 table: "Lessons",
                 nullable: false,
                 defaultValue: 1000);

               //Lesson version UP
               migrationBuilder.AddColumn<string>(
                   name: "RowVersion",
                   table: "Lessons",
                   nullable: true);
               migrationBuilder.Sql(@"CREATE TRIGGER LessonsSetRowVersionOnInsert
                                   AFTER INSERT ON Lessons
                                   BEGIN
                                   UPDATE Lessons SET RowVersion = CURRENT_TIMESTAMP WHERE Id=NEW.Id;
                                   END;");
               migrationBuilder.Sql(@"CREATE TRIGGER LessonsSetRowVersionOnUpdate
                                   AFTER UPDATE ON Lessons WHEN NEW.RowVersion <= OLD.RowVersion
                                   BEGIN
                                   UPDATE Lessons SET RowVersion = CURRENT_TIMESTAMP WHERE Id=NEW.Id;
                                   END;");
               migrationBuilder.Sql("UPDATE Lessons SET RowVersion = CURRENT_TIMESTAMP;");
          }

          protected override void Down(MigrationBuilder migrationBuilder)
          {
               migrationBuilder.Sql(@"DROP TRIGGER CoursesSetRowVersionOnInsert");
               migrationBuilder.Sql(@"DROP TRIGGER CoursesSetRowVersionOnUpdate");


               //Lesson Order DOWN
               migrationBuilder.DropColumn(
               name: "Order",
               table: "Lessons");

               //Lesson Version DOWN
               migrationBuilder.DropColumn(
              name: "RowVersion",
              table: "Lessons");
               migrationBuilder.Sql("DROP TRIGGER LessonsSetRowVersionOnInsert;");
               migrationBuilder.Sql("DROP TRIGGER LessonsSetRowVersionOnUpdate;");
          }
     }
}
