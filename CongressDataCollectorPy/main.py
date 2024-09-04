import asyncio
from config import load_config
from api_client import CongressApiClient
from cosmos_db_client import CosmosDbClient
from datetime import datetime, timezone
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

async def main():
    config = load_config()
    api_client = CongressApiClient(config['congress_api_key'])
    cosmos_client = CosmosDbClient(config['cosmos_endpoint'], config['cosmos_key'], config['cosmos_database'], config['cosmos_container'])

    # Get the earliest and latest dates from the database
    earliest_date, latest_date = await cosmos_client.get_date_range()

    if earliest_date is None or latest_date is None:
        # If no data in the database, fetch all bills
        bills = await api_client.fetch_bills()
    else:
        # Fetch bills updated since the latest date in our database
        new_bills = await api_client.fetch_bills(from_date=latest_date)
        
        # Fetch older bills (if any) to fill gaps
        old_bills = await api_client.fetch_bills(to_date=earliest_date, sort="updateDate+asc")
        
        bills = new_bills + old_bills

    for bill in bills:
        bill_details = await api_client.fetch_bill_details(bill)
        await cosmos_client.store_bill(bill_details)

    logger.info(f"Processed {len(bills)} bills")

if __name__ == "__main__":
    asyncio.run(main())
