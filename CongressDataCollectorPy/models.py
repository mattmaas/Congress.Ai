from typing import List, Optional
from pydantic import BaseModel

class Action(BaseModel):
    actionDate: str
    text: str
    type: str
    actionCode: str
    sourceSystem: dict

class Cosponsor(BaseModel):
    bioguideId: str
    fullName: str
    party: str
    state: str
    district: Optional[str]
    sponsorshipDate: str

class RelatedBill(BaseModel):
    congress: int
    number: str
    type: str
    relationshipDetails: List[dict]

class Subject(BaseModel):
    name: str

class Summary(BaseModel):
    text: str
    updateDate: str

class TextVersion(BaseModel):
    date: str
    type: str
    formats: List[dict]

class OpenAiSummaries(BaseModel):
    summary: str
    keyChanges: List[str]

from typing import Union

class Bill(BaseModel):
    congress: int
    type: str
    number: int
    url: Optional[str]
    title: str
    latestAction: dict
    updateDate: str
    originChamber: str
    detailedActions: Optional[List[Action]]
    detailedCosponsors: Optional[List[Cosponsor]]
    detailedRelatedBills: Optional[List[RelatedBill]]
    detailedSubjects: Optional[List[Subject]]
    detailedSummaries: Optional[List[Summary]]
    detailedTextVersions: Optional[List[TextVersion]]
    fullText: Optional[str]
    openAiSummaries: Optional[OpenAiSummaries]

    class Config:
        extra = 'ignore'

class Cosponsor(BaseModel):
    bioguideId: str
    fullName: str
    party: str
    state: str
    district: Optional[str]
    sponsorshipDate: str
