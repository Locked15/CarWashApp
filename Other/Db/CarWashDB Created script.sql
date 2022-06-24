-- Создание базы данных

/*
Таблица "Роли"
*/
CREATE TABLE [Role]
(
	ID INT PRIMARY KEY IDENTITY,
	RoleName NVARCHAR(20) NOT NULL
);

/*
Таблица "Типы услуг"
*/
CREATE TABLE [ServiceType]
(
	ID INT PRIMARY KEY IDENTITY,
	ServicesType NVARCHAR(20) NOT NULL
);

/*
Таблица "Сотрудники"
*/
CREATE TABLE [Employe]
(
	ID INT PRIMARY KEY IDENTITY,
	LastName NVARCHAR(20) NOT NULL,
	FirstName NVARCHAR(20) NOT NULL,
	Patranymic NVARCHAR(20) NOT NULL,
	PhoneNumber NVARCHAR(20) NOT NULL,
	RoleID INT FOREIGN KEY REFERENCES Role(ID) NOT NULL
);

/*
Таблица "Услуги"
*/
CREATE TABLE [Service]
(
	ID INT PRIMARY KEY IDENTITY,
	Title NVARCHAR(20) NOT NULL,
	Price DECIMAL(10,2) NOT NULL,
	Description NVARCHAR(255),
	ServicesType INT FOREIGN KEY REFERENCES ServiceType(ID) NOT NULL
);

/*
Таблица "Клиенты"
*/
CREATE TABLE [Client]
(
	ID INT IDENTITY,
	CarStateNumber NVARCHAR(10) PRIMARY KEY NOT NULL,
	ClientFullName NVARCHAR(60) NOT NULL,
	PhoneNumber NVARCHAR(12) NOT NULL
);

/*
Таблица "Заказ"
*/
CREATE TABLE [Order]
(
	ID INT PRIMARY KEY IDENTITY,
	EmployeID INT FOREIGN KEY REFERENCES [Employe](ID) NOT NULL,
	CarStateNumber NVARCHAR(10) FOREIGN KEY REFERENCES [Client](CarStateNumber) NOT NULL,
	Address NVARCHAR(60) NOT NULL,
	OrderDateTime DATETIME NOT NULL
);

/*
Таблица "Состав заказа"
*/
CREATE TABLE [OrderServices]
(
	OrderID INT FOREIGN KEY REFERENCES [Order](ID)  NOT NULL,
	ServicesID INT FOREIGN KEY REFERENCES [Service](ID) NOT NULL,
	--PRIMARY KEY (OrderID, ServicesID)
);
