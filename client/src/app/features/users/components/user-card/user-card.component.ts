import { Component, inject, Input, OnInit, signal } from '@angular/core';
import { UserService } from '../../../../core/services/user.service';
import type { User } from '../../../../core/models/user.model';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { UserInitialsPipe } from '../../../../core/pipes/user-initials.pipe';
import { SnackbarService } from '../../../../core/services/snackbar.service';

@Component({
  selector: 'app-user-card',
  standalone: true,
  templateUrl: './user-card.component.html',
  styleUrl: './user-card.component.scss',
  imports: [
    UserInitialsPipe,
    MatIconModule,
    MatCardModule,
    MatSelectModule
  ]
})
export class UserCardComponent implements OnInit {
  ngOnInit(): void {
    console.log('UserCardComponent initialized with user');
  }
  @Input() user!: User;

  _userService = inject(UserService)
  _snackBar = inject(SnackbarService);

  handleRoleChange(event: any, userId: string) {
    this._userService.addUserRole(userId, event.value).subscribe({
      next: () => {
        this.refreshUserRoles()
      },
      error: (err) => {
        this._snackBar.error('Failed to add role. Please try again.');
      }
    });
  }

  handleRoleRemove(role: string, userid: string) {
    this._userService.removeUserRole(userid, role).subscribe({
      next: () => {
        this.refreshUserRoles();
      },
      error: (err) => {
        this._snackBar.error('Failed to remove role. Please try again.');
      }
    });
  }


  private refreshUserRoles() {
    this._userService.getAllUsers().subscribe({
      error: (err) => {
        this._snackBar.error('Failed to refresh user roles. Please try again.');
      }
    });
  }

}
