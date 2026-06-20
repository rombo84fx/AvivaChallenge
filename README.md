# AvivaChallenge — Payment Web API

A .NET 10 Web API that creates and manages payment orders, automatically selecting the optimal payment provider based on the lowest commission fee.

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
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run the API

```bash
cd AvivaChallenge.Api
dotnet run
```

The API will start at `https://localhost:5001` (or the port configured in `launchSettings.json`).

### Run Tests

```bash
dotnet test
```

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
