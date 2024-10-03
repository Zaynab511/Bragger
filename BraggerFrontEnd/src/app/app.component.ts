import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Brag Document Application';
  content: string = ''; // This will hold the content for TinyMCE
  darkMode: boolean = false; // Track dark mode state

  toggleDarkMode() {
    this.darkMode = !this.darkMode; // Toggle the dark mode state
    const root = document.documentElement;

    // Update class based on dark mode state
    if (this.darkMode) {
      root.classList.add('dark');
    } else {
      root.classList.remove('dark');
    }
  }
}
