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

    async def fetch_bills(self, from_date=None, to_date=None, sort="latestAction.actionDate+desc", max_runtime=300):
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
                if isinstance(from_date, str):
                    url += f"&fromActionDate={from_date}"
                else:
                    url += f"&fromActionDate={from_date.strftime('%Y-%m-%d')}"
            if to_date:
                if isinstance(to_date, str):
                    url += f"&toActionDate={to_date}"
                else:
                    url += f"&toActionDate={to_date.strftime('%Y-%m-%d')}"

            logger.debug(f"Fetching bills from URL: {url}")
            async with self.session.get(url) as response:
                if response.status == 400:
                    error_text = await response.text()
                    logger.error(f"Bad request error: {error_text}")
                    raise ValueError(f"Bad request error: {error_text}")
                response.raise_for_status()
                data = await response.json()
                bills = data.get('bills', [])
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
            
            if 'actions' in bill_details:
                bill_details['detailedActions'] = await self.fetch_detailed_actions(bill_details['actions'])
            
            if 'cosponsors' in bill_details:
                bill_details['detailedCosponsors'] = await self.fetch_detailed_cosponsors(bill_details['cosponsors'])
            
            if 'relatedBills' in bill_details:
                bill_details['detailedRelatedBills'] = await self.fetch_detailed_related_bills(bill_details['relatedBills'])
            
            if 'subjects' in bill_details:
                bill_details['detailedSubjects'] = await self.fetch_detailed_subjects(bill_details['subjects'])
            
            if 'summaries' in bill_details:
                bill_details['detailedSummaries'] = await self.fetch_detailed_summaries(bill_details['summaries'])
            
            if 'textVersions' in bill_details:
                bill_details['detailedTextVersions'] = await self.fetch_detailed_text_versions(bill_details['textVersions'])
                if bill_details['detailedTextVersions']:
                    bill_details['fullText'] = await self.fetch_bill_text(bill_details['detailedTextVersions'][-1])
            
            return bill_details

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
