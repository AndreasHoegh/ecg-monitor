import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EcgDetailComponent } from './ecg-detail.component';

describe('EcgDetailComponent', () => {
  let component: EcgDetailComponent;
  let fixture: ComponentFixture<EcgDetailComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EcgDetailComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EcgDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
