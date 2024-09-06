import asyncio
from config import load_config
from api_client import CongressApiClient
from cosmos_db_client import CosmosDbClient
from datetime import datetime, timezone, timedelta
import logging
import time

logger = logging.getLogger(__name__)

async def fetch_recent_bills(max_runtime=1500):  # Default to 25 minutes max runtime
    logger.debug(f"Starting fetch_recent_bills with max_runtime={max_runtime}")
    start_time = time.time()
    config = load_config()
    logger.debug(f"Loaded configuration: {config}")
    
    async with CongressApiClient(config['congress_api_key']) as api_client:
        logger.debug("Initialized CongressApiClient")
        cosmos_client = CosmosDbClient(config['cosmos_endpoint'], config['cosmos_key'], config['cosmos_database'], config['cosmos_container'])
        logger.debug("Initialized CosmosDbClient")

        try:
            # Get the latest date from the database
            _, latest_date = await cosmos_client.get_date_range()
            logger.debug(f"Retrieved latest date from database: {latest_date}")
        except Exception as e:
            logger.error(f"Error retrieving date range from Cosmos DB: {str(e)}")
            latest_date = None

        if latest_date:
            start_date = (datetime.fromisoformat(latest_date) + timedelta(days=1)).strftime('%Y-%m-%d')
        else:
            start_date = (datetime.now(timezone.utc) - timedelta(days=30)).strftime('%Y-%m-%d')
        logger.debug(f"Using start_date: {start_date}")

        bill_count = 0
        async for bill in api_client.fetch_bills(from_date=latest_date, sort="updateDate+desc", max_runtime=max_runtime):
            if time.time() - start_time > max_runtime:
                logger.warning(f"Reached maximum runtime of {max_runtime} seconds. Stopping processing.")
                break

            bill_count += 1
            logger.debug(f"Processing bill {bill_count}: {bill.get('id', 'Unknown ID')}")
            bill_details = await api_client.fetch_bill_details(bill)
            await cosmos_client.store_bill(bill_details)
            logger.info(f"Stored bill {bill_count} in CosmosDB")

            if bill_count % 10 == 0:  # Log every 10 bills
                logger.info(f"Processed and stored {bill_count} bills so far")

        logger.info(f"Processed and stored a total of {bill_count} bills")
        logger.debug(f"Total runtime: {time.time() - start_time:.2f} seconds")

if __name__ == "__main__":
    logger.info("Starting daily bill fetch script")
    asyncio.run(fetch_recent_bills())
    logger.info("Daily bill fetch script completed")
