import { Component } from '@angular/core';
import { UserCardComponent } from '../user-card/user-card';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [UserCardComponent],
  template: `
    <section>
      <app-user-card name="Alice Johnson" email="alice@example.com"></app-user-card>
      <app-user-card name="Bob Smith" email="bob@example.com"></app-user-card>
    </section>
  `
})
export class UsersListComponent {}
