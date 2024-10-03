import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent {
  email: string = '';
  successMessage: string = '';
  errorMessage: string = '';

  constructor(private authService: AuthService, private router: Router) {}

  resetPassword() {
    this.authService.forgotPassword(this.email).subscribe(
      () => {
        this.successMessage = 'Password reset link sent to your email.';
        setTimeout(() => this.router.navigate(['/login']), 2000);
      },
      error => this.errorMessage = error.message
    );
  }
}
