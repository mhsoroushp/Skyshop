
import { Component, inject } from '@angular/core';
import { LoadingService } from '../../../core/services/loading.service';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-loading',
  imports: [
    MatProgressSpinnerModule,
    CommonModule
  ],
  templateUrl: './loading.component.html',
  styleUrl: './loading.component.scss'
})
export class LoadingComponent {
    loadingService = inject(LoadingService);
}