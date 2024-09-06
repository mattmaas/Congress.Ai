from azure.cosmos.aio import CosmosClient
from azure.cosmos import PartitionKey
import logging
from models import Bill
from pydantic import ValidationError

logger = logging.getLogger(__name__)

class CosmosDbClient:
    def __init__(self, endpoint, key, database_name, container_name):
        self.client = CosmosClient(endpoint, key)
        self.database = self.client.get_database_client(database_name)
        self.container = self.database.get_container_client(container_name)

    async def store_bill(self, bill_data):
        try:
            # Convert all 'number' fields in detailedRelatedBills to strings
            if 'detailedRelatedBills' in bill_data:
                for related_bill in bill_data['detailedRelatedBills']:
                    if 'number' in related_bill:
                        related_bill['number'] = str(related_bill['number'])

            # Remove 'url' field if it's None
            if 'url' in bill_data and bill_data['url'] is None:
                del bill_data['url']

            bill = Bill(**bill_data)
            if 'id' not in bill_data:
                bill_data['id'] = f"{bill.type}{bill.number}-{bill.congress}"
            await self.container.upsert_item(bill_data)
            logger.info(f"Stored bill {bill_data['id']} in Cosmos DB.")
        except ValidationError as ve:
            logger.error(f"Validation error while storing bill: {str(ve)}")
        except Exception as e:
            logger.error(f"Failed to store bill in Cosmos DB: {str(e)}")

    async def get_date_range(self):
        earliest_date = None
        latest_date = None
        query = "SELECT c.updateDate FROM c"
        async for item in self.container.query_items(query, enable_cross_partition_query=True):
            date = item['updateDate']
            if earliest_date is None or date < earliest_date:
                earliest_date = date
            if latest_date is None or date > latest_date:
                latest_date = date
        return earliest_date, latest_date
