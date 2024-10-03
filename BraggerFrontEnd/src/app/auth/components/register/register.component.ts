import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent {
  email = '';
  password = '';
  confirmPassword = '';
  name = '';
  jobTitle = '';
  industry = '';
  contact = '';
  errorMessage = '';
  successMessage = '';

  constructor(private authService: AuthService, private router: Router) {}

  register(): void {
    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }
  
    const newUser = {
      email: this.email,
      PasswordHash: this.password,
      Name: this.name,
      JobTitle: this.jobTitle,
      Industry: this.industry,
      Contact: this.contact
    };
  
    this.authService.register(newUser).subscribe({
      next: (response: any) => {
        console.log('Registration response:', response);
        if (response && response.message) {
          this.successMessage = 'Successfully Registered'; // Make sure it's a string
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 2000);
        } else {
          this.errorMessage = 'Registration failed. Please try again.';
        }
      },
      error: (err) => {
        console.error('Registration error:', err);
        this.errorMessage = 'An error occurred. Please try again.';
      }
    });
  }
  
  
  loginWithGoogle(): void {
    console.log('Logging in with Google');
  }

  loginWithLinkedIn(): void {
    console.log('Logging in with LinkedIn');
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
