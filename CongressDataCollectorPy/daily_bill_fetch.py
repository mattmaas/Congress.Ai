import asyncio
from config import load_config
from api_client import CongressApiClient
from cosmos_db_client import CosmosDbClient
from datetime import datetime, timezone
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

async def fetch_recent_bills(max_runtime=300):  # Default to 5 minutes max runtime
    config = load_config()
    async with CongressApiClient(config['congress_api_key']) as api_client:
        cosmos_client = CosmosDbClient(config['cosmos_endpoint'], config['cosmos_key'], config['cosmos_database'], config['cosmos_container'])

        # Get the latest date from the database
        _, latest_date = await cosmos_client.get_date_range()

        if latest_date is None:
            logger.warning("No bills in the database. Running full fetch instead.")
            bills = await api_client.fetch_bills(max_runtime=max_runtime)
        else:
            bills = await api_client.fetch_bills(from_date=latest_date, max_runtime=max_runtime)

        for bill in bills:
            bill_details = await api_client.fetch_bill_details(bill)
            await cosmos_client.store_bill(bill_details)

        logger.info(f"Processed {len(bills)} bills")

if __name__ == "__main__":
    asyncio.run(fetch_recent_bills())
