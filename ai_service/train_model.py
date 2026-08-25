import pandas as pd
import numpy as np
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.svm import LinearSVC
from sklearn.ensemble import RandomForestClassifier
from sklearn.pipeline import Pipeline
import joblib
import os

print("Generating synthetic dataset...")
data = {
    'text': [
        'VPN access unavailable, cannot connect to network',
        'Laptop not starting despite being plugged in',
        'Unable to access HR portal for leave application',
        'Salary not credited for this month',
        'Air conditioning is broken on 2nd floor',
        'Power fluctuation destroying workstations',
        'Need access card for the new office wing',
        'Security concern: unauthorized entry observed'
    ],
    'category_id': [1, 2, 3, 4, 9, 10, 13, 14],
    'priority': [2, 1, 1, 2, 1, 3, 2, 3] # 0=Low, 1=Medium, 2=High, 3=Critical
}

df = pd.DataFrame(data)

print("Training Category Classifier (TF-IDF + LinearSVC)...")
category_pipeline = Pipeline([
    ('tfidf', TfidfVectorizer(stop_words='english')),
    ('clf', LinearSVC(random_state=42))
])
category_pipeline.fit(df['text'], df['category_id'])

print("Training Priority Classifier (TF-IDF + Random Forest)...")
priority_pipeline = Pipeline([
    ('tfidf', TfidfVectorizer(stop_words='english')),
    ('clf', RandomForestClassifier(random_state=42))
])
priority_pipeline.fit(df['text'], df['priority'])

os.makedirs('models', exist_ok=True)
joblib.dump(category_pipeline, 'models/category_model.pkl')
joblib.dump(priority_pipeline, 'models/priority_model.pkl')

# Also dump some sample data for similarity comparison
df.to_csv('models/corpus.csv', index=False)

print("Models trained and saved to /models directory!")
