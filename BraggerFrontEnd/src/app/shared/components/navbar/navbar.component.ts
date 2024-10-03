import { Component, EventEmitter, Input, Output } from '@angular/core';
import { AuthService } from '../../../auth/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent {
  @Input() darkMode: boolean = false; // Track dark mode state from parent component
  @Output() toggleDarkModeEvent = new EventEmitter<void>(); // Emit toggle event

  constructor(private authService: AuthService, private router: Router) {}

  isAuthenticated(): boolean {
    return this.authService.isAuthenticated(); // Check authentication using AuthService
  }

  logout(): void {
    console.log('Logging out...');
    this.authService.logout(); // Call the AuthService logout method
    this.router.navigate(['/login']); // Redirect to the login page
  }

  navigateToProfile(): void {
    this.router.navigate(['/profile']); // Navigate to the profile page
  }

  navigateToDashboard(): void {
    this.router.navigate(['/dashboard']); // Navigate to the dashboard page
  }

  toggleDarkMode(): void {
    this.toggleDarkModeEvent.emit(); // Emit the event to toggle dark mode
  }
}
