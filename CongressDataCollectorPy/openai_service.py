import openai
import logging

logger = logging.getLogger(__name__)

class OpenAiService:
    def __init__(self, api_key):
        self.api_key = api_key
        openai.api_key = api_key

    async def analyze_bill_text(self, bill_text):
        try:
            response = await openai.ChatCompletion.acreate(
                model="gpt-4-0613",
                messages=[
                    {"role": "system", "content": "You are an AI assistant that summarizes and analyzes bill text."},
                    {"role": "user", "content": f"Please provide a concise summary and analysis of the following bill text:\n\n{bill_text}"}
                ],
                max_tokens=500
            )
            return response.choices[0].message['content'].strip()
        except Exception as e:
            logger.error(f"Error in OpenAI API call: {str(e)}")
            return None
