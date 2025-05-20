# Blocking Detection

Blocking Detection is a .NET-based project designed to monitor and block access to certain countries based on IP geolocation data. It provides APIs for managing blocked countries, logging access attempts, and cleaning up temporary blocks.

## Project Structure

The project is organized into the following main components:

- **BD.APIs**: Contains the API layer of the application, including controllers for handling requests related to countries, IPs, and logs.
- **BD.Core**: Contains core models, interfaces, and enums used across the application.
- **BD.Repository**: Implements repository patterns for managing data access, such as country and logs repositories.
- **BD.Services**: Contains services for IP geolocation and temporal block cleanup.

## Features

- **Country Blocking**: Block access to specific countries based on IP geolocation.
- **IP Geolocation**: Retrieve geolocation data for IP addresses.
- **Logging**: Log access attempts and blocked attempts.
- **Temporary Block Cleanup**: Automatically clean up temporary blocks after a specified duration.

## Prerequisites

- .NET 8.0 SDK
- A geolocation API key (e.g., for IP geolocation services)

## Getting Started

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd BlockingDetection
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run the application:
   ```bash
   dotnet run --project BD.APIs
   ```

## Configuration

- Update `appsettings.json` and `appsettings.Development.json` in the `BD.APIs` project with your geolocation API key and other necessary configurations.

## API Endpoints

- **CountriesController**:
  - `GET /countries`: Retrieve a list of blocked countries.
  - `POST /countries`: Add a country to the block list.
  - `DELETE /countries/{id}`: Remove a country from the block list.

- **IpController**:
  - `GET /ip/geolocation`: Retrieve geolocation data for an IP address.

- **LogsController**:
  - `GET /logs`: Retrieve access logs.

## License

This project is licensed under the MIT License. See the LICENSE file for details.