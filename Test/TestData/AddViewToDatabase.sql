-- SQL script to add/update a view

CREATE OR ALTER VIEW MyView AS
SELECT Id, MyDateTime, MyInt, MyString FROM NormalClasses 
GO
