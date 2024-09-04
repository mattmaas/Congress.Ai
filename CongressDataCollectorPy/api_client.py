import aiohttp
from datetime import datetime, timezone
import logging
import asyncio
import time

logger = logging.getLogger(__name__)

class CongressApiClient:
    BASE_URL = "https://api.congress.gov/v3/bill"
    
    def __init__(self, api_key):
        self.api_key = api_key
        self.session = None

    async def __aenter__(self):
        self.session = aiohttp.ClientSession()
        return self

    async def __aexit__(self, exc_type, exc, tb):
        await self.session.close()

    async def fetch_bills(self, from_date=None, to_date=None, sort="updateDate+desc", max_runtime=300):
        congress = 118  # Current congress number
        offset = 0
        limit = 250
        start_time = time.time()

        while True:
            if time.time() - start_time > max_runtime:
                logger.warning(f"Reached maximum runtime of {max_runtime} seconds. Stopping fetch.")
                break

            url = f"{self.BASE_URL}/{congress}?format=json&limit={limit}&offset={offset}&sort={sort}&api_key={self.api_key}"
            
            if from_date:
                url += f"&fromDateTime={from_date.isoformat()}Z"
            if to_date:
                url += f"&toDateTime={to_date.isoformat()}Z"

            logger.debug(f"Fetching bills from URL: {url}")
            async with self.session.get(url) as response:
                response.raise_for_status()
                data = await response.json()
                bills = data['bills']
                logger.info(f"Fetched {len(bills)} bills")

                for bill in bills:
                    yield bill

                if len(bills) < limit:
                    logger.debug("Reached the end of available bills")
                    break  # We've reached the end of the available bills

                offset += limit

            # Add a small delay to avoid hitting rate limits
            await asyncio.sleep(0.1)

    async def fetch_bill_details(self, bill):
        url = f"{bill['url']}&api_key={self.api_key}"
        async with self.session.get(url) as response:
            response.raise_for_status()
            data = await response.json()
            return data.get('bill', {})

    async def fetch_bill_text(self, text_version):
        url = text_version['formats'][0]['url']
        async with self.session.get(url) as response:
            response.raise_for_status()
            return await response.text()

    async def fetch_bill_text_from_details(self, bill_details):
        if 'textVersions' in bill_details and bill_details['textVersions']:
            latest_version = bill_details['textVersions'][-1]
            return await self.fetch_bill_text(latest_version)
        return None
