import { Component, inject} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../app/core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css'
})
export class SidebarComponent{
  authService = inject(AuthService);

  isUserHasAdminRole(): boolean {
    const authState = this.authService.authState();
    return authState?.roles?.includes('Admin') ?? false;
  } 
}
