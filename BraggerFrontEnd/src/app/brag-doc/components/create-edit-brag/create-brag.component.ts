import { Component, OnInit } from '@angular/core'; 
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BragDocService } from '../../services/brag-doc.service';
import { BragDoc } from '../../models/brag-doc.model';
import { Tag } from '../../models/tag.model';
import { HttpClient } from '@angular/common/http'; // Import HttpClient

@Component({
  selector: 'app-create-brag',
  templateUrl: './create-brag.component.html',
  styleUrls: ['./create-brag.component.css']
})
export class CreateBragComponent implements OnInit {
  bragForm: FormGroup;
  successMessage: string = '';
  errorMessage: string = '';
  availableTags: Tag[] = [];
  aiTags: string[] = []; // For AI-generated tags
  selectedTags: Set<string> = new Set();

  editorConfig = {
    height: 300,
    menubar: true,
    plugins: 'link image code lists wordcount',
    toolbar: 'undo redo | bold italic | alignleft aligncenter alignright | code | numlist bullist',
    content_css: 'https://www.tiny.cloud/css/codepen.min.css',
    setup: (editor: any) => {
      editor.on('change', () => {
        const content = editor.getContent();
        this.bragForm.get('description')?.setValue(content);
        this.fetchAISuggestions(); // Fetch AI suggestions whenever the description changes
      });
    }
  };

  constructor(
    private fb: FormBuilder,
    private bragDocService: BragDocService,
    private router: Router
  ) {
    this.bragForm = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      date: [new Date().toISOString().split('T')[0], Validators.required],
      tags: [[]]
    });
  }

  ngOnInit(): void {
    this.loadTags();
  }

  loadTags(): void {
    this.bragDocService.getTags().subscribe(
      (response: Tag[]) => {
        this.availableTags = response;
      },
      (error) => {
        this.errorMessage = 'Error loading tags';
        console.error(error);
      }
    );
  }

  // fetchAISuggestions(): void {
  //   const description = this.bragForm.get('description')?.value; // Get the description from the form
  //   if (description) {
  //     this.bragDocService.getAISuggestions(description).subscribe(
  //       (response: string[]) => {
  //         // Assuming response is a single string with tags separated by a comma or another delimiter
  //         this.aiTags = response[0] ? response[0].split(',').map(tag => tag.trim()) : []; // Adjust the delimiter as needed
  //       },
  //       (error) => {
  //         this.errorMessage = 'Error fetching AI-generated tags';
  //         console.error(error);
  //       }
  //     );
  //   }
  // }
  fetchAISuggestions(): void {
    const description = this.bragForm.get('description')?.value; // Get the description from the form
    if (description) {
        this.bragDocService.getAISuggestions(description).subscribe(
            (response: string | string[]) => { // Explicitly type the response
                this.aiTags = []; // Initialize the aiTags array

                // Check if the response is a single string
                if (typeof response === 'string') {
                    // Split the string using commas, periods followed by whitespace, or newlines
                    const tagsString = response[0]; // Get the string from the first element

                    // Split the string using the newline character and then trim each resulting tag
                    this.aiTags = tagsString.split(/\n/).map(tag => tag.trim()).filter(tag => tag.length > 0);
                
                    // Further process the tags to remove leading numbers and dots
                    this.aiTags = this.aiTags.map(tag => tag.replace(/^\d+\.\s*/, '').trim());
                    this.aiTags = response[0] ? response[0].split(',').map(tag => tag.trim()) : []; // Adjust the delimiter as needed
                } else {
                    // If response is an array, trim each tag
                    this.aiTags = response.map((tag: string) => tag.trim());
                    this.aiTags = response[0] ? response[0].split(',').map(tag => tag.trim()) : []; // Adjust the delimiter as needed
                }

                // Remove duplicates if necessary
                this.aiTags = Array.from(new Set(this.aiTags)); // Ensure all tags are unique
            },
            (error) => {
                this.errorMessage = 'Error fetching AI-generated tags';
                console.error(error);
            }
        );
    }
}


  

  onTagChange(tag: string) {
    this.selectedTags.has(tag) ? this.selectedTags.delete(tag) : this.selectedTags.add(tag);
    this.bragForm.get('tags')?.setValue(Array.from(this.selectedTags));
  }

  onAItagChange(tag: string) {
    this.selectedTags.has(tag) ? this.selectedTags.delete(tag) : this.selectedTags.add(tag);
    this.bragForm.get('tags')?.setValue(Array.from(this.selectedTags));
  }

  deselectTag(tag: string) {
    this.selectedTags.delete(tag);
    this.bragForm.get('tags')?.setValue(Array.from(this.selectedTags));
  }

  onSubmit(): void {
    if (this.bragForm.valid) {
      const bragData: BragDoc = {
        ...this.bragForm.value,
        tags: Array.from(this.selectedTags)
      };
  
      this.bragDocService.createBrag(bragData).subscribe(
        (response) => {
          this.successMessage = response.message || 'Achievement created successfully!';
          setTimeout(() => this.router.navigate(['/brags']), 2000);
        },
        (error) => {
          this.errorMessage = 'Error creating achievement';
          console.error(error);
        }
      );
    }
  }
}


/*import { Component, OnInit } from '@angular/core'; 
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BragDocService } from '../../services/brag-doc.service';
import { BragDoc } from '../../models/brag-doc.model';
import { Tag } from '../../models/tag.model'; // Assuming you have a Tag model

@Component({
  selector: 'app-create-brag',
  templateUrl: './create-brag.component.html',
  styleUrls: ['./create-brag.component.css']
})
export class CreateBragComponent implements OnInit {
  bragForm: FormGroup;
  successMessage: string = '';
  errorMessage: string = '';
  availableTags: Tag[] = [];  // List of available tags
  selectedTags: Set<string> = new Set(); // Use Set to avoid duplicates

  editorConfig = {
    height: 300,
    menubar: true,
    plugins: 'link image code lists wordcount',
    toolbar: 'undo redo | bold italic | alignleft aligncenter alignright | code | numlist bullist',
    content_css: 'https://www.tiny.cloud/css/codepen.min.css',
    setup: (editor: any) => {
      editor.on('change', () => {
        this.bragForm.get('description')?.setValue(editor.getContent());
      });
    }
  };

  constructor(
    private fb: FormBuilder,
    private bragDocService: BragDocService,
    private router: Router
  ) {
    this.bragForm = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      date: [new Date().toISOString().split('T')[0], Validators.required],
      tags: [[]] // Initialize with empty array
    });
  }

  ngOnInit(): void {
    this.loadTags();  // Load available tags
  }

  loadTags(): void {
    this.bragDocService.getTags().subscribe(
      (response: Tag[]) => { // Ensure you're typing the response
        this.availableTags = response;  // Ensure the response has the correct structure
      },
      (error) => {
        this.errorMessage = 'Error loading tags';
        console.error(error);
      }
    );
  }

  onTagChange(tag: string) {
    if (this.selectedTags.has(tag)) {
      this.selectedTags.delete(tag); // Deselect tag if already selected
    } else {
      this.selectedTags.add(tag); // Select tag
    }
    // Update the form with selected tags
    this.bragForm.get('tags')?.setValue(Array.from(this.selectedTags)); // Convert Set to Array
  }

  deselectTag(tag: string) {
    this.selectedTags.delete(tag); // Remove from selected tags
    // Update the form with selected tags
    this.bragForm.get('tags')?.setValue(Array.from(this.selectedTags)); // Convert Set to Array
  }

  onSubmit(): void {
    if (this.bragForm.valid) {
      const bragData: BragDoc = {
        ...this.bragForm.value,
        tags: Array.from(this.selectedTags) // Include selected tags in the data sent to the backend
      };
  
      this.bragDocService.createBrag(bragData).subscribe(
        (response) => {
          this.successMessage = response.message || 'Achievement created successfully!';
          setTimeout(() => this.router.navigate(['/brags']), 2000);
        },
        (error) => {
          this.errorMessage = 'Error creating achievement';
          console.error(error);
        }
      );
    }
  }
}*/
