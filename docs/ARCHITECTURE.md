# Congress.Ai Architecture

## System Overview

Congress.Ai follows a modern, distributed architecture pattern combining mobile, cloud, and AI services:

### Core Components

- **.NET MAUI Mobile App**: Cross-platform frontend providing user interface and interaction
- **Azure Cloud Backend**: Scalable services handling data storage and processing
- **Python Data Collection**: Automated scripts for gathering and processing congressional data
- **OpenAI Integration**: AI-powered bill analysis and summary generation

## Component Architecture

### Mobile App (.NET MAUI)
- **MVVM Pattern**: Clear separation of concerns between Views, ViewModels, and Models
- **Dependency Injection**: Services registered and injected via Microsoft.Extensions.DependencyInjection
- **State Management**: Robust state handling using custom StateService
- **Data Binding**: Two-way binding between Views and ViewModels

### Backend Services
- **Azure Cosmos DB**: Primary data store with optimized partitioning for bill data
- **Azure Blob Storage**: Binary storage for bill text and related documents
- **Azure Functions**: Serverless compute for data processing tasks
- **OpenAI API**: GPT-based text analysis and summary generation

### Data Collection System
- **Python Scripts**: Automated collection from Congress.gov API
- **Azure Scheduled Tasks**: Timed execution of data collection
- **Data Processing Pipeline**: Transformation and enrichment of raw congressional data

## Data Flow

1. **Data Collection**
   - Python scripts fetch data from Congress.gov API
   - Raw data is processed and enriched
   - Bills and related data stored in Cosmos DB

2. **AI Processing**
   - Bill text sent to OpenAI for analysis
   - Summaries and key points generated
   - Results stored alongside bill data

3. **Mobile App Data Access**
   - App requests bill data from Cosmos DB
   - Cached locally for performance
   - Updates fetched periodically

## Performance Considerations

- **Lazy Loading**: Bill details fetched on demand
- **Caching**: Local storage of frequently accessed data
- **Async Operations**: Non-blocking UI during data operations
- **Batch Processing**: Efficient handling of large datasets

## Security Implementation

- **Azure Key Vault**: Secure storage of API keys and secrets
- **HTTPS**: Encrypted data transmission
- **Azure AD**: Authentication for backend services
- **Role-Based Access**: Controlled access to sensitive operations
