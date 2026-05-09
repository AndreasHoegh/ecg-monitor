import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface EcgSummary {
  id: string;
  patientId: string;
  recordedAt: string;
  heartRateBpm: number;
  isAnomaly: boolean;
  aiDiagnosis: string | null;
  aiUrgency: string | null;
  aiConfidence: number | null;
  status: 'Pending' | 'Reviewed';
}

export interface DoctorReview {
  id: string;
  doctorName: string;
  diagnosis: string;
  notes: string;
  agreedWithAi: boolean;
  reviewedAt: string;
}

export interface EcgRecord {
  id: string;
  patientId: string;
  recordedAt: string;
  sampleRateHz: number;
  dataPoints: number[];
  heartRateBpm: number;
  isAnomaly: boolean;
  aiDiagnosis: string | null;
  aiReasoning: string | null;
  aiConfidence: number | null;
  aiUrgency: string | null;
  status: 'Pending' | 'Reviewed';
  review: DoctorReview | null;
}

export interface CreateReview {
  doctorName: string;
  diagnosis: string;
  notes: string;
  agreedWithAi: boolean;
}

export interface DashboardStats {
  totalAnomalies: number;
  pendingReviews: number;
  reviewedToday: number;
  diagnosisCounts: Record<string, number>;
}

@Injectable({ providedIn: 'root' })
export class EcgService {
  private readonly base = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  getStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.base}/ecg/stats`);
  }

  getAll(status?: string, urgency?: string, page = 1): Observable<EcgSummary[]> {
    let params = new HttpParams().set('page', page).set('pageSize', 20);
    if (status) params = params.set('status', status);
    if (urgency) params = params.set('urgency', urgency);
    return this.http.get<EcgSummary[]>(`${this.base}/ecg`, { params });
  }

  getById(id: string): Observable<EcgRecord> {
    return this.http.get<EcgRecord>(`${this.base}/ecg/${id}`);
  }

  submitReview(ecgId: string, review: CreateReview): Observable<DoctorReview> {
    return this.http.post<DoctorReview>(`${this.base}/ecg/${ecgId}/review`, review);
  }
}
