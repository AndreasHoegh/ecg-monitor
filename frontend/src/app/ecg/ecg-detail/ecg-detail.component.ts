import { Component, OnInit, AfterViewChecked, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { EcgRecord, EcgService } from '../../services/ecg.service';

Chart.register(...registerables);

@Component({
  selector: 'app-ecg-detail',
  templateUrl: './ecg-detail.component.html',
  styleUrl: './ecg-detail.component.scss'
})
export class EcgDetailComponent implements OnInit, AfterViewChecked, OnDestroy {
  @ViewChild('ecgCanvas') canvasRef!: ElementRef<HTMLCanvasElement>;

  record: EcgRecord | null = null;
  loading = true;
  submitting = false;
  submitError = '';
  chart: Chart | null = null;
  private chartRendered = false;

  reviewForm: FormGroup;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private ecgService: EcgService,
    private fb: FormBuilder
  ) {
    this.reviewForm = this.fb.group({
      doctorName: ['', Validators.required],
      diagnosis: ['', Validators.required],
      notes: [''],
      agreedWithAi: [true]
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.ecgService.getById(id).subscribe({
      next: record => {
        this.record = record;
        this.loading = false;
        if (record.aiDiagnosis) {
          this.reviewForm.patchValue({ diagnosis: record.aiDiagnosis });
        }
      },
      error: () => (this.loading = false)
    });
  }

  ngAfterViewChecked(): void {
    if (this.record && !this.chartRendered && this.canvasRef?.nativeElement) {
      this.chartRendered = true;
      this.renderChart();
    }
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  private renderChart(): void {
    const points = this.record!.dataPoints;
    const rate = this.record!.sampleRateHz;
    const step = Math.max(1, Math.floor(points.length / 1800));
    const sampled = points.filter((_, i) => i % step === 0);
    const labels = sampled.map((_, i) => ((i * step) / rate).toFixed(2));

    this.chart = new Chart(this.canvasRef.nativeElement, {
      type: 'line',
      data: {
        labels,
        datasets: [{
          data: sampled,
          borderColor: '#00c853',
          borderWidth: 1,
          pointRadius: 0,
          tension: 0
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: false,
        plugins: { legend: { display: false } },
        scales: {
          x: {
            ticks: { color: '#aaa', maxTicksLimit: 11 },
            grid: { color: '#2a2a2a' },
            title: { display: true, text: 'Tid (s)', color: '#aaa' }
          },
          y: {
            ticks: { color: '#aaa' },
            grid: { color: '#2a2a2a' },
            title: { display: true, text: 'Amplitude (mV)', color: '#aaa' }
          }
        }
      }
    });
  }

  submitReview(): void {
    if (this.reviewForm.invalid || !this.record) return;
    this.submitting = true;
    this.submitError = '';
    this.ecgService.submitReview(this.record.id, this.reviewForm.value).subscribe({
      next: () => this.router.navigate(['/ecg']),
      error: err => {
        this.submitting = false;
        this.submitError = err.status === 409 ? 'Dette EKG er allerede reviewet.' : 'Fejl ved indsendelse.';
      }
    });
  }

  urgencyLabel(u: string | null): string {
    const map: Record<string, string> = { low: 'Lav', medium: 'Medium', high: 'Høj', critical: 'Kritisk' };
    return map[u ?? ''] ?? u ?? '';
  }
}
