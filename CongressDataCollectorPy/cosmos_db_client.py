from azure.cosmos.aio import CosmosClient
from azure.cosmos import PartitionKey
import logging
from models import Bill

logger = logging.getLogger(__name__)

class CosmosDbClient:
    def __init__(self, endpoint, key, database_name, container_name):
        self.client = CosmosClient(endpoint, key)
        self.database = self.client.get_database_client(database_name)
        self.container = self.database.get_container_client(container_name)

    async def store_bill(self, bill_data):
        try:
            bill = Bill(**bill_data)
            if 'id' not in bill_data:
                bill_data['id'] = f"{bill.type}{bill.number}-{bill.congress}"
            await self.container.upsert_item(bill_data)
            logger.info(f"Stored bill {bill_data['id']} in Cosmos DB.")
        except Exception as e:
            logger.error(f"Failed to store bill in Cosmos DB: {str(e)}")

    async def get_date_range(self):
        query = "SELECT MIN(c.updateDate) as earliest, MAX(c.updateDate) as latest FROM c"
        async for item in self.container.query_items(query, enable_cross_partition_query=True):
            return item['earliest'], item['latest']
        return None, None
