import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  template: `
    <section class="auth-page">
      <mat-card class="auth-card">
        <div class="auth-header">
          <mat-icon>lock</mat-icon>
          <div>
            <h1>{{ isRegisterMode ? 'Create account' : 'Login' }}</h1>
            <p>{{ isRegisterMode ? 'Register to track and secure your orders.' : 'Sign in to access protected orders and checkout data.' }}</p>
          </div>
        </div>

        @if (message) {
          <div class="message success">{{ message }}</div>
        }

        @if (error) {
          <div class="message error">{{ error }}</div>
        }

        <form [formGroup]="authForm" (ngSubmit)="onSubmit()" class="auth-form">
          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="email" placeholder="you@example.com" />
            @if (emailControl.hasError('required') && emailControl.touched) {
              <mat-error>Email is required</mat-error>
            } @else if (emailControl.hasError('email') && emailControl.touched) {
              <mat-error>Enter a valid email</mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Password</mat-label>
            <input matInput type="password" formControlName="password" placeholder="Minimum 6 characters" />
            @if (passwordControl.hasError('required') && passwordControl.touched) {
              <mat-error>Password is required</mat-error>
            } @else if (passwordControl.hasError('minlength') && passwordControl.touched) {
              <mat-error>Password must be at least 6 characters</mat-error>
            }
          </mat-form-field>

          <button mat-raised-button color="primary" type="submit" [disabled]="loading || authForm.invalid">
            @if (loading) {
              <mat-spinner diameter="18"></mat-spinner>
            } @else {
              <span>{{ isRegisterMode ? 'Register' : 'Login' }}</span>
            }
          </button>
        </form>

        <button mat-button type="button" (click)="toggleMode()">
          {{ isRegisterMode ? 'Already have an account? Login' : 'Need an account? Register' }}
        </button>
      </mat-card>
    </section>
  `,
  styles: [`
    .auth-page {
      min-height: 100vh;
      display: grid;
      place-items: center;
      padding: 24px;
      background: linear-gradient(135deg, #f5efe6 0%, #dce8f5 100%);
    }

    .auth-card {
      width: 100%;
      max-width: 460px;
      padding: 24px;
      border-radius: 16px;
    }

    .auth-header {
      display: flex;
      gap: 16px;
      align-items: flex-start;
      margin-bottom: 20px;
    }

    .auth-header h1 {
      margin: 0 0 6px;
      font-size: 28px;
    }

    .auth-header p {
      margin: 0;
      color: #5b6470;
      line-height: 1.4;
    }

    .auth-form {
      display: grid;
      gap: 16px;
      margin-bottom: 8px;
    }

    .message {
      border-radius: 10px;
      padding: 12px 14px;
      margin-bottom: 16px;
      font-size: 14px;
    }

    .message.success {
      background: #e7f6eb;
      color: #176b2c;
    }

    .message.error {
      background: #fdeceb;
      color: #b42318;
    }

    button[mat-raised-button] {
      min-height: 44px;
    }
  `]
})
export class LoginPage {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);

  readonly authForm = this.formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  isRegisterMode = false;
  loading = false;
  error = '';
  message = '';

  get emailControl() {
    return this.authForm.controls.email;
  }

  get passwordControl() {
    return this.authForm.controls.password;
  }

  toggleMode(): void {
    this.isRegisterMode = !this.isRegisterMode;
    this.error = '';
    this.message = '';
  }

  onSubmit(): void {
    if (this.authForm.invalid) {
      this.authForm.markAllAsTouched();
      return;
    }

    const email = this.emailControl.value ?? '';
    const password = this.passwordControl.value ?? '';

    this.loading = true;
    this.error = '';
    this.message = '';

    if (this.isRegisterMode) {
      this.authService.register({ email, password }).subscribe({
        next: () => {
          this.loading = false;
          this.message = 'Registration succeeded. You can log in now.';
          this.isRegisterMode = false;
          this.authForm.controls.password.reset('');
          this.cdr.detectChanges();
        },
        error: (err: { error?: { error?: string } }) => {
          this.loading = false;
          this.error = err?.error?.error || 'Authentication failed';
          this.cdr.detectChanges();
        }
      });

      return;
    }

    this.authService.login({ email, password }).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/shop/home']);
        this.cdr.detectChanges();
      },
      error: (err: { error?: { title?: string; message?: string; detail?: string } }) => {
        this.loading = false;
        this.error = err?.error?.message || 'Authentication failed';
        this.cdr.detectChanges();
      }
    });
  }
}
