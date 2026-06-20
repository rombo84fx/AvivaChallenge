# AvivaChallenge — Payment Order System

A full-stack payment order system with an **Angular 21** frontend and a **.NET 10** Web API backend. The system creates and manages payment orders, automatically selecting the optimal payment provider based on the lowest commission fee.

## Overview

The API integrates with two external payment providers:

- **PagaFacil** — Supports Cash and Credit Card payments.
- **CazaPagos** — Supports Credit Card and Transfer payments.

When an order is created, the system evaluates the fees from each provider for the given amount and payment mode, then routes the payment through the provider with the lowest commission.

## Fee Structure

### PagaFacil

| Payment Mode | Fee              |
|--------------|------------------|
| Cash         | $15 flat         |
| Credit Card  | 1% of the amount |

### CazaPagos

| Payment Mode | Fee                                                                                             |
|--------------|-------------------------------------------------------------------------------------------------|
| Credit Card  | ≤ $1,000 → 2% · $1,001–$5,000 → 1.5% · > $5,000 → 0.5%                                       |
| Transfer     | ≤ $500 → $5 flat · $501–$1,000 → 2.5% · > $1,000 → 2%                                         |

### Optimal Provider Selection (Credit Card)

| Amount Range | Selected Provider | Reason                        |
|--------------|-------------------|-------------------------------|
| ≤ $5,000     | PagaFacil         | 1% < CazaPagos tiered rates   |
| > $5,000     | CazaPagos         | 0.5% < PagaFacil's 1%         |

Cash always routes to **PagaFacil**; Transfer always routes to **CazaPagos**.

## Tech Stack

### Frontend
- **Angular 21** (LTS) — Standalone components, new control flow syntax
- **TypeScript** — Strongly typed models and services
- **SCSS** — Component-scoped and global styles

### Backend
- **.NET 10** / ASP.NET Core Web API
- **Strategy Pattern** for payment provider abstraction
- **In-memory repository** for order storage
- **xUnit** for unit testing

## Project Structure

```
AvivaChallenge.Api/
├── Controllers/        # REST endpoints
├── Domain/             # Order, Product, Fee, PaymentMode, OrderStatus
├── Models/             # Request/Response DTOs
├── Providers/          # IPaymentProvider, PagaFacilProvider, CazaPagosProvider, PaymentProviderSelector
├── Repositories/       # IOrderRepository, InMemoryOrderRepository
├── Services/           # IOrderService, OrderService
└── Program.cs          # App configuration & DI

AvivaChallenge.Tests/
├── FeeCalculationTests.cs
├── PaymentProviderSelectorTests.cs
└── OrderServiceTests.cs

AvivaChallenge.Web/           # Angular 21 Frontend
├── src/app/
│   ├── components/
│   │   ├── product-list/     # Product catalog & order creation
│   │   ├── order-list/       # Orders grid with pay/cancel actions
│   │   └── order-detail/     # Full order information view
│   ├── models/               # TypeScript interfaces
│   ├── services/             # HttpClient API service
│   ├── app.routes.ts         # Route definitions
│   └── app.config.ts         # App providers (HttpClient, Router)
├── proxy.conf.json           # Dev proxy to API
└── angular.json
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/) (LTS)

### Run the API (Backend)

```bash
cd AvivaChallenge.Api
dotnet run
```

The API will start at `http://localhost:5000` (or the port configured in `launchSettings.json`).

### Run the Frontend

```bash
cd AvivaChallenge.Web
npm install
npm start
```

The Angular app will start at `http://localhost:4200` and proxy API requests to the backend.

### Run Tests

```bash
# Backend tests
dotnet test

# Frontend tests
cd AvivaChallenge.Web
npx ng test
```

## Frontend Screens

| Screen          | Route           | Description                                                  |
|-----------------|-----------------|--------------------------------------------------------------|
| Products        | `/`             | Browse products, select items, choose payment mode, create order |
| Orders          | `/orders`       | View all orders in a grid, pay or cancel pending orders      |
| Order Detail    | `/orders/:id`   | View complete order info including products and fees         |

## API Endpoints

| Method | Endpoint                  | Description         |
|--------|---------------------------|---------------------|
| POST   | `/api/orders`             | Create a new order  |
| GET    | `/api/orders`             | List all orders     |
| GET    | `/api/orders/{id}`        | Get order by ID     |
| PUT    | `/api/orders/{id}/cancel` | Cancel an order     |
| PUT    | `/api/orders/{id}/pay`    | Pay an order        |

### Create Order — Request Body

```json
{
  "paymentMode": "CreditCard",
  "products": [
    { "name": "Laptop", "unitPrice": 15000 },
    { "name": "Mouse", "unitPrice": 500 }
  ]
}
```

Valid `paymentMode` values: `Cash`, `CreditCard`, `Transfer`.

## Configuration

Set the API key and provider URLs in `appsettings.json`:

```json
{
  "PaymentProviders": {
    "ApiKey": "YOUR_API_KEY",
    "PagaFacil": {
      "BaseUrl": "https://app-paga-chg-aviva.azurewebsites.net"
    },
    "CazaPagos": {
      "BaseUrl": "https://app-caza-chg-aviva.azurewebsites.net"
    }
  }
}
```
