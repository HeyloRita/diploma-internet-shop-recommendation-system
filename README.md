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

ОЧИСТИТЬ И СОЗДАТЬ БД 

 cd "E:\путь до проекта\Интернет магазин"

 docker compose down -v
 docker compose up -d
