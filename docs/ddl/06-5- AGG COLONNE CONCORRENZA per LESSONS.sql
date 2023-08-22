ALTER TABLE Lessons
DROP COLUMN [Order];

ALTER TABLE Lessons
DROP COLUMN RowVersion;

ALTER TABLE Lessons
ADD [Order] INTEGER DEFAULT 1000;

ALTER TABLE Lessons
ADD RowVersion DATETIME;

CREATE TRIGGER LessonsSetRowVersionOnInsert
         AFTER INSERT
            ON Lessons
BEGIN
    UPDATE Lessons
       SET RowVersion = CURRENT_TIMESTAMP
     WHERE Id = NEW.Id;
END;

CREATE TRIGGER LessonsSetRowVersionOnUpdate
         AFTER UPDATE
            ON Lessons
          WHEN NEW.RowVersion <= OLD.RowVersion
BEGIN
    UPDATE Lessons
       SET RowVersion = CURRENT_TIMESTAMP
     WHERE Id = NEW.Id;
END;


UPDATE Lessons SET RowVersion = CURRENT_TIMESTAMP WHERE Id=NEW.Id;