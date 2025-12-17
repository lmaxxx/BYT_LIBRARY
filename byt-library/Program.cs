// See https://aka.ms/new-console-template for more information

using byt_library.Domain.Entities;

var person1 = new Person("Stephen", "Kinggggggg", new DateTime(1947, 9, 21), "stephen.king@example.com");
var author1 = new Author(person1, "123");
