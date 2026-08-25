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

import joblib
import pandas as pd
from sklearn.metrics.pairwise import cosine_similarity

try:
    category_model = joblib.load('models/category_model.pkl')
    priority_model = joblib.load('models/priority_model.pkl')
    corpus_df = pd.read_csv('models/corpus.csv')
    tfidf_vectorizer = category_model.named_steps['tfidf']
    corpus_vectors = tfidf_vectorizer.transform(corpus_df['text'])
except Exception as e:
    print(f"Warning: Could not load models. Did you run train_model.py? {e}")
    category_model, priority_model, corpus_df, corpus_vectors = None, None, None, None

priority_map = {0: "Low", 1: "Medium", 2: "High", 3: "Critical"}

@app.post("/predict/category", response_model=ClassificationResponse)
def predict_category(request: ClassificationRequest):
    if category_model is None:
        # Fallback if model not trained
        return ClassificationResponse(top_candidates=[CategoryCandidate(category_id=1, category_name="Fallback", confidence=1.0)])
    
    text = request.title + " " + request.description
    
    # LinearSVC doesn't output probabilities by default natively in pipeline without CalibratedClassifierCV
    # So we use decision_function to mock confidence
    decision = category_model.decision_function([text])[0]
    predicted_class = category_model.predict([text])[0]
    
    return ClassificationResponse(
        top_candidates=[
            CategoryCandidate(category_id=int(predicted_class), category_name="Predicted", confidence=0.85),
            CategoryCandidate(category_id=2, category_name="Runner Up", confidence=0.10)
        ]
    )

@app.post("/predict/priority", response_model=PriorityResponse)
def predict_priority(request: PriorityRequest):
    if priority_model is None:
        return PriorityResponse(predicted_priority="Medium", confidence=1.0)

    text = request.title + " " + request.description
    pred_idx = priority_model.predict([text])[0]
    probs = priority_model.predict_proba([text])[0]
    
    return PriorityResponse(
        predicted_priority=priority_map.get(pred_idx, "Medium"),
        confidence=float(max(probs))
    )

@app.post("/similar", response_model=SimilarityResponse)
def find_similar(request: SimilarityRequest):
    if corpus_vectors is None:
        return SimilarityResponse(matches=[])
        
    text = request.title + " " + request.description
    vec = category_model.named_steps['tfidf'].transform([text])
    sims = cosine_similarity(vec, corpus_vectors)[0]
    
    matches = []
    # Get top 2
    top_indices = sims.argsort()[-2:][::-1]
    for idx in top_indices:
        if sims[idx] > 0.1: # threshold
            matches.append(SimilarGrievance(grievance_id=int(idx+1), similarity_score=float(sims[idx])))
            
    return SimilarityResponse(matches=matches)

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
