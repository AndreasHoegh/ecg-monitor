import { Component, OnInit, OnDestroy } from '@angular/core';
import { interval, Subject, takeUntil } from 'rxjs';
import { EcgService, EcgSummary } from '../../services/ecg.service';

@Component({
  selector: 'app-ecg-list',
  templateUrl: './ecg-list.component.html',
  styleUrl: './ecg-list.component.scss'
})
export class EcgListComponent implements OnInit, OnDestroy {
  allRecords: EcgSummary[] = [];
  loading = true;
  activeTab: 'anomaly' | 'normal' = 'anomaly';
  urgencyFilter = '';
  statusFilter = '';
  private destroy$ = new Subject<void>();

  constructor(private ecgService: EcgService) {}

  ngOnInit(): void {
    this.load();
    interval(15000).pipe(takeUntil(this.destroy$)).subscribe(() => this.load());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  load(): void {
    this.loading = true;
    this.ecgService.getAll(this.statusFilter || undefined, this.urgencyFilter || undefined)
      .subscribe({
        next: data => { this.allRecords = data; this.loading = false; },
        error: () => (this.loading = false)
      });
  }

  get anomalies(): EcgSummary[] {
    return this.allRecords.filter(r => r.isAnomaly);
  }

  get normals(): EcgSummary[] {
    return this.allRecords.filter(r => !r.isAnomaly);
  }

  get activeRecords(): EcgSummary[] {
    return this.activeTab === 'anomaly' ? this.anomalies : this.normals;
  }

  urgencyClass(urgency: string | null): string {
    return urgency ?? 'low';
  }
}
