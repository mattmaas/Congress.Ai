import asyncio
import argparse
from full_bill_fetch import fetch_all_bills
from daily_bill_fetch import fetch_recent_bills
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

async def main(full_fetch=False, max_runtime=None):
    if full_fetch:
        await fetch_all_bills(max_runtime=max_runtime or 3600)
    else:
        await fetch_recent_bills(max_runtime=max_runtime or 300)

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Fetch and store congressional bills.")
    parser.add_argument("--full", action="store_true", help="Perform a full fetch of all bills")
    parser.add_argument("--max-runtime", type=int, help="Maximum runtime in seconds")
    args = parser.parse_args()

    asyncio.run(main(full_fetch=args.full, max_runtime=args.max_runtime))
