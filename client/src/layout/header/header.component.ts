import { Component } from '@angular/core';

@Component({
  selector: 'app-header',
  standalone: true,
  template: `
    <header class="header">
      <h2>SkyShop</h2>
      <nav>
        <a href="#">Dashboard</a>
        <a href="#">Users</a>
        <a href="#">Profile</a>
      </nav>
    </header>
  `,
  styles: [`
    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 24px;
      background: #111827;
      color: white;
    }

    nav {
      display: flex;
      gap: 12px;
    }

    a {
      color: white;
      text-decoration: none;
    }
  `]
})
export class HeaderComponent {}