import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  errorMessage = '';
  successMessage: string = '';
  isPasswordVisible: boolean = false;

  constructor(private fb: FormBuilder, private authService: AuthService, private router: Router) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {}

  onSubmit(): void {
    // Check if the form is valid
    if (this.loginForm.invalid) {
      this.errorMessage = 'Please fill in all fields.';
      return; // Exit if form is invalid
    }

    const { email, password } = this.loginForm.value;

    // Call login service
    this.authService.login(email, password).subscribe(
      (response) => {
        // Assuming the response has a success property
        if (response.success) {
          this.successMessage = 'Successfully logged in!';
          setTimeout(() => {
            this.router.navigate(['/dashboard']); // Navigate after 2 seconds
          }, 2000);
        } else {
          this.errorMessage = response.message; // Use the message from the API
        }
      },
      (error) => {
        if (error.status === 401) {
          this.errorMessage = 'Invalid email or password.'; // Specific error message
        } else {
          this.errorMessage = 'An unexpected error occurred. Please try again later.';
        }
      }
    );
  }

  togglePasswordVisibility(): void {
    this.isPasswordVisible = !this.isPasswordVisible;
  }

  goToForgotPassword(): void {
    this.router.navigate(['/forgot-password']);
  }

  goToRegister(): void {
    this.router.navigate(['/register']);
  }
}
