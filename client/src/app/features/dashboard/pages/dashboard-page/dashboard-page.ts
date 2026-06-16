import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  template: `
    <section class="dashboard-page">
      <h2>Dashboard</h2>
      <p>This is your dashboard page.</p>
    </section>
  `,
  styles: [`
    .dashboard-page {
      padding: 24px;
    }
  `]
})
export class DashboardPage {}
