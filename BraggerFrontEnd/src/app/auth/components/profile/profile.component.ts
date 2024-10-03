import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms'; // Import FormBuilder, FormGroup, Validators
import { AuthService } from '../../services/auth.service';
import { User } from '../../model/user.model'; // Ensure the path is correct

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent implements OnInit {
  profileForm: FormGroup; // Define profileForm
  user: User | null = null;
  errorMessage = '';
  successMessage = '';

  constructor(private authService: AuthService, private fb: FormBuilder) {
    this.profileForm = this.fb.group({
      name: ['', Validators.required],
      jobTitle: [''],
      industry: [''],
      contactInfo: ['']
    });
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    const email = 'user@example.com'; // Hardcoded for now
    this.authService.getProfile(email).subscribe(
      (profile: User | null) => {
        if (profile) {
          this.user = profile;
          this.profileForm.patchValue(profile); // Populate form with user data
        } else {
          this.errorMessage = 'Profile not found.';
        }
      },
      (error: any) => {
        this.errorMessage = 'An error occurred while loading the profile.';
      }
    );
  }

  onSubmit(): void {
    if (this.profileForm.valid) {
      if (this.user) {
        this.authService.updateProfile(this.user.email, this.profileForm.value).subscribe(
          (success: boolean) => {
            if (success) {
              this.successMessage = 'Profile updated successfully.';
            } else {
              this.errorMessage = 'Failed to update profile.';
            }
          },
          (error: any) => {
            this.errorMessage = 'An error occurred while updating the profile.';
          }
        );
      }
    }
  }
}
