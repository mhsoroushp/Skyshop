import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { UserCardComponent } from '../../components/user-card/user-card.component';
import { UserService } from '../../../../core/services/user.service';
import { tap } from 'rxjs/internal/operators/tap';
import { User } from '../../../../core/models/user.model';
import { map } from 'rxjs/internal/operators/map';
import { SnackbarService } from '../../../../core/services/snackbar.service';
@Component({
  selector: 'app-users-page',
  standalone: true,
  imports: [CommonModule, UserCardComponent],
  templateUrl: './users-page.html',
  styleUrl: './users-page.scss'
})
export class UsersPage implements OnInit {

  public userService = inject(UserService); 
  _snackBar = inject(SnackbarService);

  ngOnInit(): void {
    this.userService.getAllUsers().subscribe({
      error: (err) => {
        this._snackBar.error('Failed to fetch users. Please try again.');
      }
    });
  }
}