import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EcgListComponent } from './ecg-list.component';

describe('EcgListComponent', () => {
  let component: EcgListComponent;
  let fixture: ComponentFixture<EcgListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EcgListComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EcgListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
