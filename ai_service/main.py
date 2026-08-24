from fastapi import FastAPI
from pydantic import BaseModel
from typing import List, Optional
from datetime import datetime

app = FastAPI(title="Smart Grievance AI Service")

# Request Models
class ClassificationRequest(BaseModel):
    title: str
    description: str
    submitter_department_id: Optional[int] = None

class PriorityRequest(BaseModel):
    title: str
    description: str
    predicted_category_id: int
    submitter_department_id: Optional[int] = None
    similar_open_count: int = 0

class SimilarityRequest(BaseModel):
    title: str
    description: str

# Response Models
class CategoryCandidate(BaseModel):
    category_id: int
    category_name: str
    confidence: float

class ClassificationResponse(BaseModel):
    top_candidates: List[CategoryCandidate]
    model_version: str = "v1.0"
    timestamp: datetime = datetime.utcnow()

class PriorityResponse(BaseModel):
    predicted_priority: str
    confidence: float
    model_version: str = "v1.0"
    timestamp: datetime = datetime.utcnow()

class SimilarGrievance(BaseModel):
    grievance_id: int
    similarity_score: float

class SimilarityResponse(BaseModel):
    matches: List[SimilarGrievance]
    model_version: str = "v1.0"
    timestamp: datetime = datetime.utcnow()

@app.post("/predict/category", response_model=ClassificationResponse)
def predict_category(request: ClassificationRequest):
    # Mock logic - In reality, you'd vectorize request.title/description and pass through model
    return ClassificationResponse(
        top_candidates=[
            CategoryCandidate(category_id=1, category_name="Network Issue", confidence=0.85),
            CategoryCandidate(category_id=2, category_name="Hardware Issue", confidence=0.10),
            CategoryCandidate(category_id=3, category_name="Software Issue", confidence=0.05)
        ]
    )

@app.post("/predict/priority", response_model=PriorityResponse)
def predict_priority(request: PriorityRequest):
    # Mock logic
    predicted = "High" if "urgent" in request.description.lower() or request.similar_open_count > 2 else "Medium"
    return PriorityResponse(
        predicted_priority=predicted,
        confidence=0.92
    )

@app.post("/similar", response_model=SimilarityResponse)
def find_similar(request: SimilarityRequest):
    # Mock logic
    return SimilarityResponse(
        matches=[
            SimilarGrievance(grievance_id=1, similarity_score=0.95),
            SimilarGrievance(grievance_id=5, similarity_score=0.81)
        ]
    )

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
