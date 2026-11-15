# listings-app — доска объявлений о квартирах

Семестровый проект «Веб-разработка: C# + Angular» (уроки 1–9).

| Слой | Технологии |
|---|---|
| `api/` | ASP.NET Core Web API (.NET 10), EF Core, FluentValidation, JWT, OpenAPI/Swagger |
| `web/` | Angular 22: standalone-компоненты, сигналы, httpResource, signal forms, маршрутизация |
| База | SQLite локально, PostgreSQL на Render |
| Тесты | `api-tests/` (xUnit), `web/**/*.spec.ts` (Vitest) |
| Деплой | Render (`render.yaml`, `api/Dockerfile`) |

**Пошаговая инструкция — в файле [ИНСТРУКЦИЯ.md](ИНСТРУКЦИЯ.md).**

## Что умеет приложение
- Список объявлений: фильтр по району и цене, сортировка, страницы (6 / 12 / 24 на странице), кнопка «Обновить».
- Страница объявления, фото, «Мои объявления» (`/my`).
- Регистрация и вход (JWT). Создавать может любой вошедший, менять и удалять — только автор.
- Серверная валидация (ProblemDetails) + те же правила в форме на клиенте.

## Запуск локально (кратко)
```bash
# 1. сервер (порт 5000)
cd api
dotnet user-secrets set "Jwt:Key" "любая-случайная-строка-не-короче-32-символов"
dotnet run
# Swagger: http://localhost:5000/swagger

# 2. сайт (порт 4200) — в другом терминале
cd web
npm install
npm start        # http://localhost:4200

# тесты
dotnet test api-tests/ListingsApi.Tests.csproj
cd web && npx ng test --watch=false
```

## API
| Метод | Адрес | Доступ |
|---|---|---|
| GET | `/api/listings?page&pageSize&sort&districtId&maxPrice` | все |
| GET | `/api/listings/{id}` | все |
| GET | `/api/listings/mine` | вошедший |
| POST | `/api/listings` | вошедший |
| PUT / DELETE | `/api/listings/{id}` | автор |
| POST | `/api/listings/{id}/photo` | автор |
| GET | `/api/districts` | все |
| POST | `/api/auth/register`, `/api/auth/login` | все |
| GET | `/api/auth/me` | вошедший |
| GET | `/health` | все (проверка для Render) |

## Что происходит между открытием страницы и появлением данных
1. Браузер загружает с сервера статических файлов Angular-приложение (HTML, JS, CSS) и запускает его.
2. Компонент списка читает сигналы фильтра, страницы и сортировки, и `httpResource` отправляет `GET /api/listings?...` на сервер API.
3. Сервер на C# принимает запрос, EF Core превращает его в SQL-запрос к базе (фильтр, сортировка, `LIMIT/OFFSET`) и получает строки.
4. Строки превращаются в DTO, а ASP.NET Core сериализует их в JSON и отправляет в ответе с кодом 200 и CORS-заголовками.
5. Angular получает JSON, кладёт его в сигнал ресурса, и шаблон с `@for` сам перерисовывает карточки.
