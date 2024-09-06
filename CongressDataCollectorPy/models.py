from typing import List, Optional, Union
from pydantic import BaseModel

class Action(BaseModel):
    actionDate: str
    text: str
    type: str
    actionCode: Optional[str] = None
    sourceSystem: dict

class Cosponsor(BaseModel):
    bioguideId: str
    fullName: str
    party: str
    state: str
    district: Optional[Union[str, int]]
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
    title: str
    latestAction: dict
    updateDate: str
    originChamber: str
    detailedActions: List[Action]
    detailedCosponsors: List[Cosponsor]
    detailedRelatedBills: Optional[List[RelatedBill]] = None
    detailedSubjects: List[Subject]
    detailedSummaries: Optional[List[Summary]] = None
    detailedTextVersions: List[TextVersion]
    fullText: Optional[str] = None
    openAiSummaries: Optional[OpenAiSummaries] = None
    url: Optional[str] = None

    class Config:
        extra = 'allow'

class Cosponsor(BaseModel):
    bioguideId: str
    fullName: str
    party: str
    state: str
    district: Optional[str]
    sponsorshipDate: str
