import { Component } from '@angular/core';

@Component({
  selector: 'app-login-page',
  standalone: true,
  template: `
    <section class="auth-page">
      <h2>Login</h2>
      <p>Welcome to the auth page.</p>
    </section>
  `,
  styles: [`
    .auth-page {
      padding: 24px;
    }
  `]
})
export class LoginPage {}
