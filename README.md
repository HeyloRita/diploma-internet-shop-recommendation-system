# diploma-internet-shop-recommendation-system

  Автоматизация управления интернет-магазином с интеграцией системы рекомендаций               



ТРЕБОВАНИЯ

  • Docker Desktop  https://www.docker.com/products/docker-desktop/
  • .NET 8 SDK      https://dotnet.microsoft.com/download



ЗАПУСК 

  1. Запустить докер, если нет базы данных - создать:

	cd "E:\путь до проекта\Интернет магазин"

	docker compose up -d

  2. Запустить InternetShop.API
       
  3. Запустить InternetShop.WPF



ТЕСТОВЫЕ АККАУНТЫ

  Администратор:  admin  /  admin


ОЧИСТИТЬ И СОЗДАТЬ БД 

 cd "E:\путь до проекта\Интернет магазин"

 docker compose down -v
 docker compose up -d



ДОСТУП К БАЗЕ ДАННЫХ 

  1. Установить pgAdmin
  2. Подключиться к базе данных

  Host:     localhost
  Port:     5433
  Database: internetshop
  Username: shop_user
  Password: shop_pass
