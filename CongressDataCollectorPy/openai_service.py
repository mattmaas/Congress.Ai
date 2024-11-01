from openai import AsyncOpenAI
import logging

logger = logging.getLogger(__name__)

class OpenAiService:
    def __init__(self, api_key):
        self.client = AsyncOpenAI(api_key=api_key)

    async def analyze_bill_text(self, bill_text):
        try:
            response = await self.client.chat.completions.create(
                model="gpt-4o-mini",
                messages=[
                    {"role": "system", "content": self.construct_prompt()},
                    {"role": "user", "content": bill_text}
                ],
                max_tokens=1024,
                temperature=0.7,
                response_format={"type": "json_object"}
            )
            return response.choices[0].message.content.strip()
        except Exception as e:
            logger.error(f"Error in OpenAI API call: {str(e)}")
            return None

    @staticmethod
    def construct_prompt():
        return """
Analyze the provided bill text and generate a response in JSON format that includes a brief summary and the most crucial key changes. The summary should distill the bill's primary goals and effects, excluding specific references like bill numbers which are provided elsewhere. Please start the summary by referencing the title of the bill rather than just calling it the 'bill'. Throughout both sections of your response, remember to use specific language rather than vague. This is for an app that needs to educate a lot of people. Each key change should be concisely described, integrating its broader implications in a single sentence. Where relevant, include affected parties in the same entry.

{
  "summary": "Summarize the bill's main objectives and potential impacts concisely. Avoid referencing specific bill numbers or procedural details, focusing instead on the essence and implications of the legislation.",
  "keyChanges": [
    "List the most significant changes proposed by the bill. Ensure all descriptions are clear and contained within single strings for consistency.",
    "If there are affected parties for a key change, append them to the same entry, starting with ' **Affected parties:**' (including the space and asterisks) followed by who will be impacted.",
    "Aim for conciseness and clarity. Less bullet points is better, so consider grouping related points or removing less important ones.",
    "Example format: 'Key change description. **Affected parties:** Description of affected parties.'"
  ]
}"""
