CREATE TABLE "Currencies" (
	"ID"	INTEGER,
	"Code"	TEXT UNIQUE,
	"FullName"	TEXT,
	"Sign"	TEXT,
	PRIMARY KEY("ID" AUTOINCREMENT)
);

INSERT INTO Currencies (ID, Code, FullName, Sign) VALUES
(1, "USD", "US Dollar", "$"),
(2, "EUR", "Euro", "€"),
(3, "JPY", "Yen", "¥"),
(4, "GBP", "Pound Sterling", "£"),
(5, "CNY", "Yuan Renminbi", "¥"),
(6, "CHF", "Swiss Franc", "Fr."),
(7, "AUD", "Australian Dollar", "A$"),
(8, "CAD", "Canadian Dollar", "C$"),
(9, "RUB", "Russian Ruble", "₽"),
(10, "HKD", "Hong Kong Dollar", "H$")