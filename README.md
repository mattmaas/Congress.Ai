# Congress.Ai

## Overview

Congress.Ai is a cross-platform mobile application designed to provide users with comprehensive information about U.S. Congress bills. The app allows users to browse, search, and view detailed information about various bills, including their sponsors, cosponsors, related actions, summaries, and more. Utilizing AI-generated summaries and key changes, Congress.Ai aims to make legislative information more accessible and understandable.

## Features

- **Bill Listings**: Browse through a list of bills categorized by type (e.g., House, Senate).
- **Bill Details**: View detailed information about each bill, including sponsors, cosponsors, related bills, actions, and full text.
- **AI-Generated Summaries**: Access AI-generated summaries and key changes for each bill to understand their impact quickly.
- **Search Functionality**: Search for bills by number, title, or keywords.
- **View Modes**: Toggle between full and compact view modes for better readability.
- **Settings**: Customize your viewing preferences and app settings.


## Technologies Used

- **.NET MAUI**: The app is built using .NET Multi-platform App UI (MAUI) to ensure it runs seamlessly on Android, iOS, and other platforms.
- **C#**: The primary programming language used for implementing the application's logic.
- **Azure Cosmos DB**: Utilized as the backend database to store and retrieve bill data efficiently.
- **OpenAI AI Integration**: AI services are integrated to generate summaries and key changes for each bill.
- **MVVM Architecture**: Implements the Model-View-ViewModel (MVVM) pattern to promote a clean separation of concerns and facilitate maintainability.
- **Azure Blob Storage**: Used for storing application state and managing blobs via the BlobStorageManager.
- **OpenAI API**: Leveraged to generate AI-based summaries and key changes for each bill.
- **Newtonsoft.Json**: Employed for JSON serialization and deserialization of data.


### Project Structure

```
App/
├── appsettings.json
├── Models/
│   ├── Bill.cs
│   ├── Cosponsor.cs
│   ├── RelatedBill.cs
│   ├── Summary.cs
│   └── TextVersion.cs
[...]
```

## Component Architecture

### Mobile App (.NET MAUI)

- **MVVM Pattern**: Clear separation of concerns between Views, ViewModels, and Models.
- **Dependency Injection**: Services registered and injected via Microsoft.Extensions.DependencyInjection.
- **State Management**: Robust state handling using custom StateService.
- **Data Binding**: Two-way binding between Views and ViewModels.

### Backend Services

- **Azure Cosmos DB**: Primary data store with optimized partitioning for bill data.
- **Azure Blob Storage**: Binary storage for bill text and related documents.
- **Azure Functions**: Serverless compute for data processing tasks.
- **OpenAI API**: GPT-based text analysis and summary generation.

### Data Collection System

- **Python Scripts**: Automated collection from Congress.gov API.
- **Azure Scheduled Tasks**: Timed execution of data collection.
- **Data Processing Pipeline**: Transformation and enrichment of raw congressional data.

## Data Flow

1. **Data Collection**
   - Python scripts fetch data from Congress.gov API.
   - Raw data is processed and enriched.
   - Bills and related data stored in Cosmos DB.

2. **AI Processing**
   - Bill text sent to OpenAI for analysis.
   - Summaries and key points generated.
   - Results stored alongside bill data.

3. **Mobile App Data Access**
   - App requests bill data from Cosmos DB.
   - Cached locally for performance.
   - Updates fetched periodically.
   
# Congress.Ai Architecture

## System Overview

Congress.Ai follows a modern, distributed architecture pattern combining mobile, cloud, and AI services:

### Core Components

- **.NET MAUI Mobile App**: Cross-platform frontend providing user interface and interaction.

- **Azure Cloud Backend**: Scalable services handling data storage and processing.

- **Python Data Collection**: Automated scripts for gathering and processing congressional data.

- **OpenAI Integration**: AI-powered bill analysis and summary generation.


## Performance Considerations

- **Lazy Loading**: Bill details fetched on demand.
- **Caching**: Local storage of frequently accessed data.
- **Async Operations**: Non-blocking UI during data operations.
- **Batch Processing**: Efficient handling of large datasets.

## Security Implementation

- **Azure Key Vault**: Secure storage of API keys and secrets.
- **HTTPS**: Encrypted data transmission.
- **Azure AD**: Authentication for backend services.
- **Role-Based Access**: Controlled access to sensitive operations.

## Getting Started

### Prerequisites

- **.NET SDK**: Ensure you have the latest version of the .NET SDK installed.
- **Visual Studio**: Recommended for development, with the MAUI workload installed.
- **Cosmos DB Account**: Required to store and retrieve bill data.

### Installation

1. **Clone the Repository**

   ```bash
   git clone https://github.com/yourusername/Congress.Ai.git
   cd Congress.Ai
   ```

2. **Configure Cosmos DB**

   - Create an `appsettings.json` file in the project root with the following structure:

     ```json
     {
       "CosmosEndpoint": "your-cosmos-db-endpoint",
       "CosmosKey": "your-cosmos-db-key",
       "CosmosDatabaseId": "YourDatabaseId",
       "CosmosContainerId": "YourContainerId"
     }
     ```

3. **Restore Dependencies**

   ```bash
   dotnet restore
   ```

4. **Run the Application**

   ```bash
   dotnet build
   dotnet run
   ```

## Usage

- **Browse Bills**: Launch the app to view a list of current bills. Use the navigation menu to switch between House and Senate bills.
- **View Bill Details**: Tap on a bill to see detailed information, including sponsors, cosponsors, related bills, and AI-generated summaries.
- **Search Bills**: Use the search feature to find bills by number, title, or keywords.
- **Customize View**: Go to settings to toggle between full and compact view modes.

## Contribution

Contributions are welcome! Please fork the repository and submit a pull request.

## License

This project is licensed under the MIT License.

## Acknowledgements

- Thanks to the .NET MAUI community for their support and resources.
- AI services integrated for generating bill summaries.
