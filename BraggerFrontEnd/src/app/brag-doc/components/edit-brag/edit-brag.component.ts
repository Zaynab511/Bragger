import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BragDocService } from '../../services/brag-doc.service';
import { BragDoc } from '../../models/brag-doc.model';

@Component({
  selector: 'app-edit-brag',
  templateUrl: './edit-brag.component.html',
  styleUrls: ['./edit-brag.component.css']
})
export class EditBragComponent implements OnInit {
  bragForm: FormGroup;
  bragId: number | null = null;
  successMessage: string = '';
  errorMessage: string = '';

  constructor(
    private fb: FormBuilder,
    private bragDocService: BragDocService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    // Initialize the form with default values and validators
    this.bragForm = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      date: ['', Validators.required],
      tag: ['', Validators.required] // Tag is required
    });
  }

  ngOnInit(): void {
    // Get the ID from the route params and load the brag details
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      if (id) {
        this.bragId = id;
        this.loadBragDetails(id);
      }
    });
  }

  loadBragDetails(bragId: number): void {
    // Fetch the brag details from the service using the brag ID
    this.bragDocService.getBragById(bragId).subscribe(
      (brag: BragDoc | undefined) => {
        if (brag) {
          // Ensure date is correctly formatted for the form input (YYYY-MM-DD)
          const formattedDate = brag.date ? new Date(brag.date).toISOString().split('T')[0] : '';
          // Patch the form with existing values
          this.bragForm.patchValue({
            title: brag.title,
            description: brag.description,
            date: formattedDate, // Set date in proper format
            tag: brag.achievementTags
          });
        } else {
          this.errorMessage = 'Brag document not found!';
        }
      },
      (error) => {
        this.errorMessage = 'Error fetching brag details';
        console.error('Error details:', error);
      }
    );
  }

  onSubmit(): void {
    if (this.bragForm.valid && this.bragId !== null) {
      const bragData: BragDoc = this.bragForm.value;
  
      this.bragDocService.updateBrag(this.bragId, bragData).subscribe(
        (response) => {
          // If the response contains a success message, display it
          this.successMessage = response.message; // Update message based on the response
          
          console.log('Update successful:', response); // Log for debugging
  
          // Navigate to the list of achievements after a short delay
          setTimeout(() => this.router.navigate(['/brags']), 2000);
        },
        (error) => {
          // Display a more specific error message based on the error object
          this.errorMessage = error.message || 'Error updating achievement';
          console.error('Error updating achievement:', error);
        }
      );
    }
  }
  
}
