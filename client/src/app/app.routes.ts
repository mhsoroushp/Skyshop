import { Routes } from '@angular/router';
import { UsersPage } from './features/users/pages/users-page/users-page';
import { AUTH_ROUTES } from './features/auth/auth.routes';
import { DASHBOARD_ROUTES } from './features/dashboard/dashboard.routes';
import { DashboardPage } from './features/dashboard/pages/dashboard-page/dashboard-page';
import { BookListComponent } from './features/book/components/book-list/book-list.component';
import { authGuard } from './core/guards/auth.guard';
import { LoginPage } from './features/auth/pages/login-page/login-page';
import { canDeactivateGuard } from './core/guards/can-deactivate.guard';
import { ShopHomeComponent } from './features/shop/page/shop-home.component';

export const routes: Routes = [
  {path: 'users', component: UsersPage},

  {path: 'books', 
    loadComponent: () => import('./features/book/components/book-list/book-list.component')
      .then(c => c.BookListComponent),
      canActivate: [authGuard]
  },

  {path: 'shop/home', 
    component: ShopHomeComponent
  },

  // {path:'dashboard', children: DASHBOARD_ROUTES},
  {path: 'dashboard', component: DashboardPage},
  {path: 'create-book', 
    loadComponent: () => import('./features/book/components/create-book/create-book.component')
      .then(c => c.CreateBookComponent),
      canActivate: [authGuard],
      canDeactivate: [canDeactivateGuard]
  },
  {path: 'auth', children: AUTH_ROUTES},
];
