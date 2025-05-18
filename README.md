# Ticketing System

A digital platform designed to simplify event ticket booking for customers while providing event organizers with efficient event management tools.

## Project Structure

The project is organized into the following layers:

- **TicketingSystem.Core**: Contains domain entities, interfaces, and business logic
- **TicketingSystem.Infrastructure**: Implements data access and external services
- **TicketingSystem.API**: RESTful API for the ticketing system
- **TicketingSystem.Web**: Web interface for customers and organizers

## Design Patterns Implemented

### Creational Patterns

1. **Factory Pattern**: Used to create different types of users
   - `UserFactory` - creates Customer, Organizer, and Administrator objects

2. **Singleton Pattern**: Used for services that should have only one instance
   - `QRCodeGenerator` - generates and validates QR codes for tickets

3. **Builder Pattern**: Used for constructing complex objects
   - `EventBuilder` - fluent API for creating Event objects with many optional parameters

### Structural Patterns

1. **Repository Pattern**: Abstracts data access logic
   - `IRepository<T>` - generic repository interface
   - Specific repositories for User, Event, Ticket, and Payment entities

2. **Facade Pattern**: Provides a simplified interface to a complex subsystem
   - `TicketPurchaseFacade` - simplifies the ticket purchasing process

3. **Dependency Injection**: Injects dependencies into classes rather than creating them internally
   - Used throughout the services to promote loose coupling

### Behavioral Patterns

1. **Observer Pattern**: Notifies objects of state changes
   - `INotificationObserver` and `INotificationSubject` - for notification handling
   - `EmailNotificationObserver` and `SMSNotificationObserver` - concrete observers

2. **Strategy Pattern**: Defines a family of algorithms and makes them interchangeable
   - `IPaymentStrategy` - interface for different payment methods
   - `CreditCardPaymentStrategy` and `PayPalPaymentStrategy` - concrete strategies

3. **Command Pattern**: Encapsulates a request as an object
   - `ICommand` - command interface with execute and undo operations
   - `PurchaseTicketCommand` and `CancelEventCommand` - concrete commands

## Features

- User Registration & Login
- Event Browsing and Search
- Ticket Purchase & Management
- Event Creation & Management
- QR Code Generation & Validation
- Payment Processing
- Notifications & Alerts
- Admin Panel for System Management

## Requirements

- .NET 7.0 or later
- Entity Framework Core
- SQL Server 