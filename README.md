# Shop


## Стек

| Часть | Технологии |
|---|---|
| Backend | .NET 10, ASP.NET Core Web API, EF Core 10, Npgsql |
| База данных | PostgreSQL 18 |
| Frontend | Angular 22  |
| Инфраструктура | Docker Compose |
| Документация | Swagger, Scalar |

## Структура

```
backend/Shop/
  Shop.Api/
    Services/         бизнес-логика
    Contracts/        DTO 
    Entities/         сущности БД
    Infrastructure/   DbContext и конфигурация БД
  compose.yaml        PostgreSQL + API
```

## Запуск

### API и PostgreSQL поднимаются в контейнере, фронт запускается отдельно



