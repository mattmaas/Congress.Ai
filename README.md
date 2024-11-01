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
- **Cosmos DB**: Utilized as the backend database to store and retrieve bill data efficiently.
- **AI Integration**: AI services are integrated to generate summaries and key changes for each bill.
- **MVVM Architecture**: Implements the Model-View-ViewModel (MVVM) pattern to promote a clean separation of concerns and facilitate maintainability.
- **Azure Blob Storage**: Used for storing application state and managing blobs via the BlobStorageManager.
- **OpenAI API**: Leveraged to generate AI-based summaries and key changes for each bill.
- **Newtonsoft.Json**: Employed for JSON serialization and deserialization of data.


## Architecture

The application's architecture follows the MVVM pattern, ensuring a clear separation between the user interface and business logic. Here's a high-level overview:

- **Models**: Define the data structures used throughout the app, representing bills, sponsors, actions, and more.
- **Views**: XAML pages that define the layout and presentation of data to the user.
- **ViewModels**: Act as intermediaries between the Views and Models, handling data manipulation, commands, and business logic.
- **Services**: Handle data access and external integrations, such as communicating with Cosmos DB to fetch and store data.
- **Infrastructure**: Manages external resources and configurations, including Cosmos DB initialization and Blob storage management.

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
├── Platforms/
│   └── Windows/
│       └── App.xaml.cs
├── Services/
│   └── CosmosDbService.cs
├── ViewModels/
│   ├── BillDetailsPageViewModel.cs
│   ├── BillListPageViewModel.cs
│   ├── MainPageViewModel.cs
│   ├── SettingsViewModel.cs
│   └── BillViewModel.cs
├── Views/
│   ├── MainPage.xaml.cs
│   ├── BillListPage.xaml.cs
│   ├── BillDetailsPage.xaml.cs
│   ├── BillTextPage.xaml.cs
│   └── SettingsPage.xaml.cs
├── AppShell.xaml.cs
├── App.xaml.cs
├── README.md
CongressDataCollector/
├── CongressDataCollector.Core/
│   └── Models/
│       ├── Bill.cs
│       ├── Cosponsor.cs
│       ├── RelatedBill.cs
│       ├── Summary.cs
│       └── TextVersion.cs
├── CongressDataCollector.Services/
│   └── CosmosDbService.cs
└── README.md
```

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
├── Platforms/
│   └── Windows/
│       └── App.xaml.cs
├── Services/
│   └── CosmosDbService.cs
├── ViewModels/
│   ├── BillDetailsPageViewModel.cs
│   ├── BillListPageViewModel.cs
│   ├── MainPageViewModel.cs
│   ├── SettingsViewModel.cs
│   └── BillViewModel.cs
├── Views/
│   ├── MainPage.xaml.cs
│   ├── BillListPage.xaml.cs
│   ├── BillDetailsPage.xaml.cs
│   ├── BillTextPage.xaml.cs
│   └── SettingsPage.xaml.cs
├── AppShell.xaml.cs
├── App.xaml.cs
├── README.md
CongressDataCollector/
├── CongressDataCollector.Core/
│   └── Models/
│       ├── Bill.cs
│       ├── Cosponsor.cs
│       ├── RelatedBill.cs
│       ├── Summary.cs
│       └── TextVersion.cs
├── CongressDataCollector.Services/
│   └── CosmosDbService.cs
└── README.md
```

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
