import { Component } from '@angular/core';

@Component({
  selector: 'app-users-page',
  standalone: true,
  template: `
    <section class="users-page">
      <h2>Users</h2>
      <p>List of users will appear here.</p>
    </section>
  `,
  styles: [`
    .users-page {
      padding: 24px;
    }
  `]
})
export class UsersPage {}
