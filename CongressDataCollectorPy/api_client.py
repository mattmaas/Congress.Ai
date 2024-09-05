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
            bill_details = data.get('bill', {})
            
            for field, method in [
                ('actions', self.fetch_detailed_actions),
                ('cosponsors', self.fetch_detailed_cosponsors),
                ('relatedBills', self.fetch_detailed_related_bills),
                ('subjects', self.fetch_detailed_subjects),
                ('summaries', self.fetch_detailed_summaries),
                ('textVersions', self.fetch_detailed_text_versions),
                ('sponsors', self.fetch_detailed_sponsors),
                ('committees', self.fetch_detailed_committees),
            ]:
                if field in bill_details:
                    bill_details[f'detailed{field.capitalize()}'] = await method(bill_details[field])
            
            if 'detailedTextVersions' in bill_details and bill_details['detailedTextVersions']:
                bill_details['fullText'] = await self.fetch_bill_text(bill_details['detailedTextVersions'][-1])
            
            return bill_details

    async def fetch_detailed_sponsors(self, sponsors):
        url = f"{sponsors['url']}&api_key={self.api_key}"
        async with self.session.get(url) as response:
            response.raise_for_status()
            data = await response.json()
            return data.get('sponsors', [])

    async def fetch_detailed_committees(self, committees):
        url = f"{committees['url']}&api_key={self.api_key}"
        async with self.session.get(url) as response:
            response.raise_for_status()
            data = await response.json()
            return data.get('committees', [])

    async def fetch_detailed_actions(self, actions):
        url = f"{actions['url']}&api_key={self.api_key}"
        async with self.session.get(url) as response:
            response.raise_for_status()
            data = await response.json()
            return data.get('actions', [])

    async def fetch_detailed_cosponsors(self, cosponsors):
        url = f"{cosponsors['url']}&api_key={self.api_key}"
        async with self.session.get(url) as response:
            response.raise_for_status()
            data = await response.json()
            return data.get('cosponsors', [])

    async def fetch_detailed_related_bills(self, related_bills):
        url = f"{related_bills['url']}&api_key={self.api_key}"
        async with self.session.get(url) as response:
            response.raise_for_status()
            data = await response.json()
            return data.get('relatedBills', [])

    async def fetch_detailed_subjects(self, subjects):
        url = f"{subjects['url']}&api_key={self.api_key}"
        async with self.session.get(url) as response:
            response.raise_for_status()
            data = await response.json()
            return data.get('subjects', {}).get('legislativeSubjects', [])

    async def fetch_detailed_summaries(self, summaries):
        url = f"{summaries['url']}&api_key={self.api_key}"
        async with self.session.get(url) as response:
            response.raise_for_status()
            data = await response.json()
            return data.get('summaries', [])

    async def fetch_detailed_text_versions(self, text_versions):
        url = f"{text_versions['url']}&api_key={self.api_key}"
        async with self.session.get(url) as response:
            response.raise_for_status()
            data = await response.json()
            return data.get('textVersions', [])

    async def fetch_bill_text(self, text_version):
        formats = text_version.get('formats', [])
        formatted_text = next((f for f in formats if f['type'] == 'Formatted Text'), None)
        if formatted_text:
            url = formatted_text['url']
            async with self.session.get(url) as response:
                response.raise_for_status()
                return await response.text()
        return None
