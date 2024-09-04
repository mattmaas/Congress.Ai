import aiohttp
from datetime import datetime, timezone
import logging

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

    async def fetch_bills(self, from_date=None, to_date=None, sort="updateDate+desc"):
        congress = 118  # Current congress number
        url = f"{self.BASE_URL}/{congress}?format=json&limit=250&sort={sort}&api_key={self.api_key}"
        
        if from_date:
            url += f"&fromDateTime={from_date.isoformat()}Z"
        if to_date:
            url += f"&toDateTime={to_date.isoformat()}Z"

        async with self.session.get(url) as response:
            response.raise_for_status()
            data = await response.json()
            return data['bills']

    async def fetch_bill_details(self, bill):
        url = f"{bill['url']}&api_key={self.api_key}"
        async with self.session.get(url) as response:
            response.raise_for_status()
            data = await response.json()
            return data['bill']

    async def fetch_bill_text(self, text_version):
        url = text_version['formats'][0]['url']
        async with self.session.get(url) as response:
            response.raise_for_status()
            return await response.text()
