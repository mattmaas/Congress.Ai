import asyncio
from config import load_config
from api_client import CongressApiClient
from cosmos_db_client import CosmosDbClient
import logging
import time

logger = logging.getLogger(__name__)

async def fetch_all_bills(max_runtime=3600):  # Default to 1 hour max runtime
    logger.debug(f"Starting fetch_all_bills with max_runtime={max_runtime}")
    start_time = time.time()
    config = load_config()
    logger.debug(f"Loaded configuration: {config}")

    async with CongressApiClient(config['congress_api_key']) as api_client:
        logger.debug("Initialized CongressApiClient")
        cosmos_client = CosmosDbClient(config['cosmos_endpoint'], config['cosmos_key'], config['cosmos_database'], config['cosmos_container'])
        logger.debug("Initialized CosmosDbClient")

        bills = await api_client.fetch_bills(max_runtime=max_runtime)
        logger.info(f"Fetched {len(bills)} bills")

        for i, bill in enumerate(bills, 1):
            if time.time() - start_time > max_runtime:
                logger.warning(f"Reached maximum runtime of {max_runtime} seconds. Stopping processing.")
                break

            logger.debug(f"Processing bill {i}/{len(bills)}: {bill.get('id', 'Unknown ID')}")
            bill_details = await api_client.fetch_bill_details(bill)
            await cosmos_client.store_bill(bill_details)
            logger.info(f"Stored bill {i}/{len(bills)} in CosmosDB")

            if i % 10 == 0:  # Log every 10 bills
                logger.info(f"Processed and stored {i} bills so far")

        logger.info(f"Processed {i} out of {len(bills)} bills")
        logger.debug(f"Total runtime: {time.time() - start_time:.2f} seconds")

if __name__ == "__main__":
    logger.info("Starting full bill fetch script")
    asyncio.run(fetch_all_bills())
    logger.info("Full bill fetch script completed")
