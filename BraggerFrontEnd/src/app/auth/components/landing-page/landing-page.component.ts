// src/app/auth/components/landing-page/landing-page.component.ts
import { Component, OnInit } from '@angular/core';
declare const ScrollReveal: any;

@Component({
  selector: 'app-landing-page',
  templateUrl: './landing-page.component.html',
  styleUrls: []
})
export class LandingPageComponent implements OnInit {
  constructor() {}

  ngOnInit(): void {
    // Initialize scroll animations using ScrollReveal
    const scrollRevealOptions = {
      distance: '50px',
      duration: 1000,
      easing: 'ease-in-out',
    };

    ScrollReveal().reveal('header img', {
      ...scrollRevealOptions,
      origin: 'right',
    });

    ScrollReveal().reveal('header h1', {
      ...scrollRevealOptions,
      origin: 'bottom',
      delay: 500,
    });

    ScrollReveal().reveal('header p', {
      ...scrollRevealOptions,
      origin: 'bottom',
      delay: 1000,
    });

    ScrollReveal().reveal('header form', {
      ...scrollRevealOptions,
      origin: 'bottom',
      delay: 1500,
    });
  }
}
