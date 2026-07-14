import { Component } from '@angular/core';
import { BookListComponent } from '../../../book/components/book-list/book-list.component';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [BookListComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardPage {}
