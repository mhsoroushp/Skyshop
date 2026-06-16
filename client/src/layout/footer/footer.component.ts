import { Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  standalone: true,
  template: `
    <footer class="footer">
      <p>© 2026 SkyShop. All rights reserved.</p>
      <span>Built with Angular</span>
    </footer>
  `,
  styles: [`
    .footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 14px 24px;
      background: #111827;
      color: #f9fafb;
      font-size: 14px;
      border-top: 1px solid #374151;
    }
  `]
})
export class FooterComponent {}


