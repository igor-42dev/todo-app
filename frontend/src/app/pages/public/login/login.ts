import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgIf } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, NgIf],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  senha = '';
  errorMessage = '';

  onLogin(): void {
    this.errorMessage = '';
    this.authService.login({ email: this.email, senha: this.senha }).subscribe({
      next: (response) => {
        localStorage.setItem('token', response.token);
        this.router.navigate(['/tasks']);
      },
      error: (err) => {
        console.error('Login failed', err);
        this.errorMessage = 'Email ou senha inválidos';
      },
    });
  }
}
