# Running the Ticketing System

Follow these steps to set up and run the Ticketing System:

## Prerequisites

1. .NET 7.0 SDK or later: [Download here](https://dotnet.microsoft.com/download/dotnet/7.0)
2. SQL Server (LocalDB is fine for development): [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
3. Visual Studio 2022 or Visual Studio Code (optional but recommended)

## Setup Database

1. Open a command prompt in the project root directory
2. Navigate to the API project:
   ```
   cd TicketingSystem.API
   ```
3. Create a migration:
   ```
   dotnet ef migrations add InitialCreate --project ../TicketingSystem.Infrastructure
   ```
4. Apply the migration to create the database:
   ```
   dotnet ef database update --project ../TicketingSystem.Infrastructure
   ```

## Run the API

1. From the API project directory:
   ```
   dotnet run
   ```
2. The API will start at https://localhost:7001 and http://localhost:5001 (ports may vary)
3. Open a web browser and navigate to https://localhost:7001/swagger to access the Swagger UI for testing the API

## Run the Web Application (Optional)

1. Open a new command prompt and navigate to the Web project:
   ```
   cd TicketingSystem.Web
   ```
2. Run the web application:
   ```
   dotnet run
   ```
3. The web application will start at https://localhost:7002 and http://localhost:5002 (ports may vary)
4. Open a web browser and navigate to https://localhost:7002 to access the web interface

## Testing the System

1. Use the Swagger UI to create:
   - A Customer user
   - An Organizer user
   - Events (as an Organizer)
   - Purchase tickets for events (as a Customer)

2. Test different payment methods (Credit Card, PayPal) to see the Strategy pattern in action

3. Cancel events or tickets to see the Command pattern with undo functionality

## Design Patterns Used

The project implements various design patterns as shown in the README.md file, including:

- Factory Pattern (UserFactory)
- Singleton Pattern (QRCodeGenerator)
- Builder Pattern (EventBuilder)
- Repository Pattern (Repository<T> and its implementations)
- Observer Pattern (NotificationService)
- Strategy Pattern (PaymentStrategies)
- Command Pattern (PurchaseTicketCommand, CancelEventCommand)
- Facade Pattern (TicketPurchaseFacade)

Explore the code to see how these patterns are implemented and interact with each other. 