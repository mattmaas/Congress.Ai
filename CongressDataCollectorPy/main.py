import asyncio
import argparse
from full_bill_fetch import fetch_all_bills
from daily_bill_fetch import fetch_recent_bills
import logging
import sys

# Configure logging to output to both file and console
logging.basicConfig(level=logging.DEBUG,
                    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
                    handlers=[
                        logging.FileHandler("debug.log"),
                        logging.StreamHandler(sys.stdout)
                    ])
logger = logging.getLogger(__name__)

async def main(full_fetch=False, max_runtime=None):
    logger.debug(f"Starting main function with full_fetch={full_fetch}, max_runtime={max_runtime}")
    try:
        if full_fetch:
            logger.info("Initiating full bill fetch")
            await fetch_all_bills(max_runtime=max_runtime or 3600)
        else:
            logger.info("Initiating daily bill fetch")
            await fetch_recent_bills(max_runtime=max_runtime or 1500)
    except Exception as e:
        logger.error(f"An error occurred in main function: {str(e)}", exc_info=True)
    logger.debug("Main function completed")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Fetch and store congressional bills.")
    parser.add_argument("--full", action="store_true", help="Perform a full fetch of all bills")
    parser.add_argument("--max-runtime", type=int, help="Maximum runtime in seconds")
    args = parser.parse_args()

    logger.debug(f"Parsed arguments: full={args.full}, max_runtime={args.max_runtime}")
    asyncio.run(main(full_fetch=args.full, max_runtime=args.max_runtime))
