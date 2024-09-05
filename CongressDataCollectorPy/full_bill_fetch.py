import asyncio
from config import load_config
from api_client import CongressApiClient
from cosmos_db_client import CosmosDbClient
from openai_service import OpenAiService
import logging
import time
import json

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
        openai_service = OpenAiService(config['openai_api_key'])
        logger.debug(f"Initialized OpenAiService with API key: {'*' * len(config['openai_api_key'])}")

        bill_count = 0
        async for bill in api_client.fetch_bills(max_runtime=max_runtime):
            if time.time() - start_time > max_runtime:
                logger.warning(f"Reached maximum runtime of {max_runtime} seconds. Stopping processing.")
                break

            bill_count += 1
            logger.debug(f"Processing bill {bill_count}: {bill.get('id', 'Unknown ID')}")
            try:
                bill_details = await api_client.fetch_bill_details(bill)
                if not bill_details:
                    logger.warning(f"Empty bill details for bill {bill.get('id', 'Unknown ID')}")
                    continue

                if 'fullText' in bill_details:
                    openai_summary = await openai_service.analyze_bill_text(bill_details['fullText'])
                    if openai_summary:
                        try:
                            summary_dict = json.loads(openai_summary)
                            bill_details['openAiSummaries'] = {
                                'summary': summary_dict.get('summary', ''),
                                'keyChanges': summary_dict.get('keyChanges', [])
                            }
                        except json.JSONDecodeError:
                            logger.error(f"Failed to parse OpenAI summary as JSON for bill {bill_details.get('id', 'Unknown ID')}")

                # Ensure actionCode is present in detailedActions
                if 'detailedActions' in bill_details:
                    for action in bill_details['detailedActions']:
                        if 'actionCode' not in action:
                            action['actionCode'] = 'Unknown'  # or any default value

                # Convert district to string in detailedCosponsors and ensure it's present
                if 'detailedCosponsors' in bill_details:
                    for cosponsor in bill_details['detailedCosponsors']:
                        if 'district' not in cosponsor:
                            cosponsor['district'] = 'Unknown'  # or any default value
                        else:
                            cosponsor['district'] = str(cosponsor['district'])

                # Convert number to string in detailedRelatedBills
                if 'detailedRelatedBills' in bill_details:
                    for related_bill in bill_details['detailedRelatedBills']:
                        if 'number' in related_bill:
                            related_bill['number'] = str(related_bill['number'])

                # Ensure required fields are present
                bill_details['url'] = bill_details.get('url', '')
                bill_details['detailedRelatedBills'] = bill_details.get('detailedRelatedBills', [])
                bill_details['openAiSummaries'] = bill_details.get('openAiSummaries', None)

                await cosmos_client.store_bill(bill_details)
                logger.info(f"Stored bill {bill_count} in CosmosDB")

                if bill_count % 10 == 0:  # Log every 10 bills
                    logger.info(f"Processed and stored {bill_count} bills so far")
            except Exception as e:
                logger.error(f"Error processing bill {bill.get('id', 'Unknown ID')}: {str(e)}")

        logger.info(f"Processed and stored a total of {bill_count} bills")
        logger.debug(f"Total runtime: {time.time() - start_time:.2f} seconds")

if __name__ == "__main__":
    logger.info("Starting full bill fetch script")
    asyncio.run(fetch_all_bills())
    logger.info("Full bill fetch script completed")
