# diploma-internet-shop-recommendation-system


  Автоматизация управления интернет-магазином с интеграцией системы рекомендаций               


ТРЕБОВАНИЯ

  • Docker Desktop  https://www.docker.com/products/docker-desktop/
  
  • .NET 8 SDK      https://dotnet.microsoft.com/download


ЗАПУСК 

  1. Запустить докер, если нет базы данных - создать:

	cd diploma-internet-shop-recommendation-system

	docker compose up -d

  2. Запустить InternetShop.API:
	
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
