import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EcgListComponent } from './ecg-list/ecg-list.component';
import { EcgDetailComponent } from './ecg-detail/ecg-detail.component';

const routes: Routes = [
  { path: '', component: EcgListComponent },
  { path: ':id', component: EcgDetailComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EcgRoutingModule {}
