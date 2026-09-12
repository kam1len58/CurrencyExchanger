CREATE TABLE "ExchangeRates" (
	"ID"	INTEGER,
	"BaseCurrencyId"	INTEGER,
	"TargetCurrencyId"	INTEGER,
	"Rate"	REAL,
	UNIQUE("BaseCurrencyId","TargetCurrencyId"),
	PRIMARY KEY("ID" AUTOINCREMENT),
	FOREIGN KEY("BaseCurrencyId") REFERENCES "Currencies"("ID"),
	FOREIGN KEY("TargetCurrencyId") REFERENCES "Currencies"("ID")
);

INSERT INTO ExchangeRates (ID, BaseCurrencyId, TargetCurrencyId, Rate) VALUES
(1, 1, 2, 0.8669),
(2, 1, 3, 157.9401),
(3, 1, 4, 0.7413),
(4, 1, 5, 6.7408),
(5, 1, 6, 0.8082),
(6, 1, 7, 1.4223),
(7, 1, 8, 1.0149),
(8, 1, 9, 0.0173),
(9, 1, 10, 0.1813) 




