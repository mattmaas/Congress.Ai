import asyncio
from config import load_config
from api_client import CongressApiClient
from cosmos_db_client import CosmosDbClient
from datetime import datetime, timezone
import logging

logging.basicConfig(level=logging.DEBUG, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

async def fetch_recent_bills(max_runtime=1500):  # Default to 25 minutes max runtime
    logger.debug(f"Starting fetch_recent_bills with max_runtime={max_runtime}")
    config = load_config()
    logger.debug(f"Loaded configuration: {config}")
    
    async with CongressApiClient(config['congress_api_key']) as api_client:
        logger.debug("Initialized CongressApiClient")
        cosmos_client = CosmosDbClient(config['cosmos_endpoint'], config['cosmos_key'], config['cosmos_database'], config['cosmos_container'])
        logger.debug("Initialized CosmosDbClient")

        # Get the latest date from the database
        _, latest_date = await cosmos_client.get_date_range()
        logger.debug(f"Retrieved latest date from database: {latest_date}")

        if latest_date is None:
            logger.warning("No bills in the database. Running full fetch instead.")
            bills = await api_client.fetch_bills(max_runtime=max_runtime)
        else:
            logger.info(f"Fetching bills from {latest_date}")
            bills = await api_client.fetch_bills(from_date=latest_date, max_runtime=max_runtime)

        logger.info(f"Fetched {len(bills)} bills")

        for i, bill in enumerate(bills, 1):
            logger.debug(f"Processing bill {i}/{len(bills)}: {bill.get('id', 'Unknown ID')}")
            bill_details = await api_client.fetch_bill_details(bill)
            await cosmos_client.store_bill(bill_details)

        logger.info(f"Processed {len(bills)} bills")

if __name__ == "__main__":
    logger.info("Starting daily bill fetch script")
    asyncio.run(fetch_recent_bills())
    logger.info("Daily bill fetch script completed")
