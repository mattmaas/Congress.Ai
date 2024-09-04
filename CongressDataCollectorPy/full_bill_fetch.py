import asyncio
from config import load_config
from api_client import CongressApiClient
from cosmos_db_client import CosmosDbClient
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

async def fetch_all_bills(max_runtime=3600):  # Default to 1 hour max runtime
    config = load_config()
    async with CongressApiClient(config['congress_api_key']) as api_client:
        cosmos_client = CosmosDbClient(config['cosmos_endpoint'], config['cosmos_key'], config['cosmos_database'], config['cosmos_container'])

        bills = await api_client.fetch_bills(max_runtime=max_runtime)

        for bill in bills:
            bill_details = await api_client.fetch_bill_details(bill)
            await cosmos_client.store_bill(bill_details)

        logger.info(f"Processed {len(bills)} bills")

if __name__ == "__main__":
    asyncio.run(fetch_all_bills())
