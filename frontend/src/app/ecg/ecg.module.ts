import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { EcgRoutingModule } from './ecg-routing.module';
import { EcgListComponent } from './ecg-list/ecg-list.component';
import { EcgDetailComponent } from './ecg-detail/ecg-detail.component';

@NgModule({
  declarations: [EcgListComponent, EcgDetailComponent],
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule, EcgRoutingModule]
})
export class EcgModule {}
