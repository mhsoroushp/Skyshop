import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-user-card',
  standalone: true,
  template: `
    <article class="user-card">
      <h3>{{ name }}</h3>
      <p>{{ email }}</p>
    </article>
  `,
  styles: [`
    .user-card {
      border: 1px solid #e5e7eb;
      border-radius: 10px;
      padding: 14px;
      margin-bottom: 12px;
      background: white;
    }
  `]
})
export class UserCardComponent {
  @Input() name = 'User Name';
  @Input() email = 'user@email.com';
}
