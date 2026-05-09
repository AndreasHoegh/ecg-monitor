import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { interval, Subject, takeUntil } from 'rxjs';
import { DashboardStats, EcgService, EcgSummary } from '../../services/ecg.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit, OnDestroy {
  stats: DashboardStats | null = null;
  recentCritical: EcgSummary[] = [];
  private destroy$ = new Subject<void>();

  constructor(private ecgService: EcgService, private router: Router) {}

  ngOnInit(): void {
    this.load();
    interval(15000).pipe(takeUntil(this.destroy$)).subscribe(() => this.load());
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private load(): void {
    this.ecgService.getStats().subscribe(s => (this.stats = s));
    this.ecgService.getAll('Pending', 'critical').subscribe(list => (this.recentCritical = list.slice(0, 5)));
  }

  goToList(status?: string): void {
    this.router.navigate(['/ecg'], { queryParams: status ? { status } : {} });
  }

  diagnosisEntries(): { key: string; value: number }[] {
    if (!this.stats) return [];
    return Object.entries(this.stats.diagnosisCounts)
      .map(([key, value]) => ({ key, value }))
      .sort((a, b) => b.value - a.value);
  }
}
