# diploma-internet-shop-recommendation-system


  Автоматизация управления интернет-магазином с интеграцией системы рекомендаций               


ТРЕБОВАНИЯ

  • Docker Desktop  https://www.docker.com/products/docker-desktop/
  
  • .NET 8 SDK      https://dotnet.microsoft.com/download


ЗАПУСК 

  1. Запустить докер, если нет базы данных - создать:

	cd diploma-internet-shop-recommendation-system

	docker compose up -d


  2. Создать файл конфигурации API

В папке `src/InternetShop.API` создайте файл `appsettings.json` со следующим содержимым:

	{
  	"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=internetshop;Username=shop_user;Password=shop_pass"
 	 },
  	"Jwt": {
    "Key": "your-super-secret-key-at-least-32-characters-long",
    "Issuer": "InternetShop.API",
    "Audience": "InternetShop.WPF"
  	},
  	"Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  	},
  	"AllowedHosts": "*"
	}

	
  3. Запустить InternetShop.API:
	
	cd diploma-internet-shop-recommendation-system\src\InternetShop.API

	 dotnet run

	   
  4. Запустить InternetShop.WPF:

  	cd diploma-internet-shop-recommendation-system\src\InternetShop.WPF

	 dotnet run


ТЕСТОВЫЕ АККАУНТЫ

  Администратор:  admin@shop.ru  /  admin123
  
  Покупатели: anna@mail.ru, ivan@mail.ru, maria@mail.ru / pass123


ОЧИСТИТЬ И СОЗДАТЬ БД :

 	cd diploma-internet-shop-recommendation-system

 	docker compose down -v

 	docker compose up -d
