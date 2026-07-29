import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MatIcon } from '@angular/material/icon';
import { MatBadge } from '@angular/material/badge';
import {MatToolbarModule} from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../../app/core/services/auth.service';
import { BasketService } from '../../app/core/services/basket.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [    
    MatIcon,
    MatBadge,
    MatButtonModule,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule, 
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit {

  basketCount = signal(0);

  basketService = inject(BasketService);
  authService = inject(AuthService);
  private readonly router = inject(Router);

  ngOnInit(): void {}


  logout(): void {
    this.authService.logout().subscribe(() => {
      this.router.navigate(['/auth']);
    });
  }
}