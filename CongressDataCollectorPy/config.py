import os
from dotenv import load_dotenv

def load_config():
    load_dotenv()
    
    return {
        'congress_api_key': os.getenv('CONGRESS_API_KEY'),
        'cosmos_endpoint': os.getenv('COSMOS_ENDPOINT'),
        'cosmos_key': os.getenv('COSMOS_KEY'),
        'cosmos_database': os.getenv('COSMOS_DATABASE'),
        'cosmos_container': os.getenv('COSMOS_CONTAINER'),
        'openai_api_key': os.getenv('OPENAI_API_KEY'),
    }
