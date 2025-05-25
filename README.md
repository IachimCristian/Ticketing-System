# Ticketing System

A comprehensive digital platform designed to simplify event ticket booking for customers while providing event organizers with efficient event management tools and administrators with powerful system oversight capabilities.

## Architecture Overview

This project follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────┐    ┌─────────────────┐
│ TicketingSystem │    │ TicketingSystem │
│      .Web       │    │      .API       │
│   (Razor Pages) │    │   (REST API)    │
└─────────┬───────┘    └─────────┬───────┘
          │                      │
          └──────────┬───────────┘
                     │
          ┌─────────────────┐
          │ TicketingSystem │
          │ .Infrastructure │
          │ (Data Access)   │
          └─────────┬───────┘
                    │
          ┌─────────────────┐
          │ TicketingSystem │
          │     .Core       │
          │   (Entities)    │
          └─────────────────┘
```

## Project Structure

The project is organized into four distinct layers:

- **TicketingSystem.Core**: Contains domain entities, interfaces, and business logic contracts
- **TicketingSystem.Infrastructure**: Implements data access, repositories, and external services
- **TicketingSystem.Web**: Razor Pages web application for users (customers, organizers, admins)
- **TicketingSystem.API**: RESTful API endpoints for external integrations and services

## Design Patterns Implemented

### Structural Patterns

1. **Repository Pattern**: Abstracts data access logic
   - `IRepository<T>` - generic repository interface
   - Specific repositories: `EventRepository`, `TicketRepository`, `PaymentRepository`, `CustomerNotificationRepository`
   - Generic implementation with error handling and logging

2. **Facade Pattern**: Provides simplified interfaces to complex subsystems
   - `ITicketPurchaseFacade` - simplifies the ticket purchasing workflow
   - Coordinates multiple services for complex operations

3. **Dependency Injection**: Promotes loose coupling throughout the application
   - Interface-based design with constructor injection
   - Service registration in startup configuration

### Behavioral Patterns

1. **Observer Pattern**: Implements notification system
   - `INotificationSubject` and `INotificationObserver` interfaces
   - Real-time notifications for ticket purchases, event updates, and system alerts

2. **Strategy Pattern**: Handles different payment methods
   - `IPaymentStrategy` interface for payment processing
   - Extensible payment system supporting multiple providers

3. **Command Pattern**: Encapsulates business operations
   - `ICommand` interface for operation encapsulation
   - Supports undo operations and transaction management

### Service Layer Patterns

1. **Service Layer Pattern**: Encapsulates business logic
   - `IEventService` - event management operations
   - `IPaymentService` - payment processing logic
   - `ICustomerNotificationService` - notification management
   - `ITicketValidationService` - ticket validation and QR code processing
   - `ISeatMapService` - seat management for events

## Key Features

### User Management
- **Multi-role Authentication**: Customer, Organizer, Administrator roles
- **Secure Registration & Login**: Password hashing with BCrypt
- **Role-based Authorization**: Different access levels and permissions
- **Profile Management**: User profile updates and preferences

### Event Management
- **Event Creation**: Organizers can create events with detailed information
- **Event Browsing**: Customers can search and filter events
- **Capacity Management**: Maximum capacity of 80 attendees per event
- **Event Status Control**: Active/inactive event management
- **Image Support**: Event images with URL validation

### Ticket System
- **Ticket Purchase**: Secure ticket purchasing workflow
- **QR Code Generation**: Unique QR codes for ticket validation
- **Ticket Validation**: Real-time ticket scanning and validation
- **Purchase History**: Complete ticket purchase tracking
- **Refund Processing**: Ticket cancellation and refund management

### Admin Dashboard
- **User Management**: Create, update, delete users and change roles
- **Event Oversight**: Complete event management and monitoring
- **Transaction Monitoring**: Payment and transaction tracking
- **System Analytics**: User statistics and event metrics
- **Data Protection**: System accounts protection and validation

### Notification System
- **Real-time Notifications**: Instant updates for users
- **Email Notifications**: Automated email alerts
- **Notification Preferences**: User-customizable notification settings
- **Event Reminders**: Automated event reminder system

### Payment Processing
- **Secure Payments**: Encrypted payment processing
- **Multiple Payment Methods**: Support for various payment strategies
- **Transaction History**: Complete payment audit trail
- **Refund Management**: Automated refund processing

## Technical Implementation

### Database Design
- **Entity Framework Core**: Code-first approach with migrations
- **SQL Server LocalDB**: Development database
- **Foreign Key Relationships**: Proper relational database design
- **Data Validation**: Comprehensive model validation

### Security Features
- **Authentication**: Cookie-based authentication
- **Authorization**: Policy-based authorization
- **Password Security**: BCrypt password hashing
- **Session Management**: Secure session handling
- **CSRF Protection**: Cross-site request forgery protection

### User Interface
- **Responsive Design**: Bootstrap 5 for mobile-friendly interface
- **Modern UI/UX**: Clean, professional design
- **Interactive Elements**: Dynamic forms and real-time updates
- **Accessibility**: ARIA labels and keyboard navigation support

## Requirements

- **.NET 7.0** or later
- **Entity Framework Core 7.0**
- **SQL Server** (LocalDB for development)
- **Bootstrap 5** for UI components
- **Font Awesome** for icons
- **BCrypt.Net** for password hashing

## Getting Started

1. **Clone the repository**
2. **Update database**: Run `dotnet ef database update` in the Infrastructure project
3. **Build solution**: `dotnet build`
4. **Run application**: 
   ```bash
   cd TicketingSystem.Web
   dotnet run
   ```
5. **Access application**: Navigate to `https://localhost:5232`

## Database Migration

If you encounter migration issues:
```bash
dotnet ef database drop --force
dotnet ef database update
```

## First Time Setup
- **Register an account** through the web interface
- **Create users** with different roles (Customer, Organizer, Administrator)
- **Admin users** can be created through the registration system 