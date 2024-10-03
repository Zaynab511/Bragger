import { Component, OnInit } from '@angular/core';
import { BragDocService } from '../../services/brag-doc.service';
import { BragDoc } from '../../models/brag-doc.model';
import { Router } from '@angular/router';
// @ts-ignore
import html2pdf from 'html2pdf.js';
// @ts-ignore
import DOMPurify from 'dompurify';
import { Tag } from '../../models/tag.model';

@Component({
  selector: 'app-list-brag',
  templateUrl: './list-brag.component.html',
  styleUrls: ['./list-brag.component.css']
})
export class ListBragComponent implements OnInit {
  brags: BragDoc[] = [];
  successMessage: string | null = null;
  errorMessage: string | null = null;

  constructor(
    private bragDocService: BragDocService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadBrags();
  }

  loadBrags(): void {
    this.bragDocService.getAllBrags().subscribe(
        (brags: BragDoc[]) => {
            console.log(brags); // Log the API response for debugging
            this.brags = brags.map(brag => ({
                ...brag,
                // No need to set tags explicitly as they are already part of the brag object
            }));
        },
        (error) => {
            this.errorMessage = 'Error fetching achievements';
            console.error(error);
        }
    );
}

  

  navigateToEdit(id: number | undefined): void {
    this.router.navigate(['/brag/edit', id]);
  }

  deleteBrag(id: number | undefined): void {
    if (id !== undefined) {
      this.bragDocService.deleteBrag(id).subscribe(
        (response) => {
          this.successMessage = response.message || 'Achievement deleted successfully!';
          this.loadBrags();
        },
        (error) => {
          this.errorMessage = 'Error deleting achievement';
          console.error(error);
        }
      );
    } else {
      console.error('ID is undefined');
    }
  }

  handleExportPDF() {
    if (this.brags.length === 0) {
      console.log('No achievements to export.');
      return;
    }

    const content = this.brags
      .map(
        (brag) => `
      <div style="font-family: Arial, sans-serif; margin: 20px; padding: 10px; border: 1px solid #ddd; border-radius: 5px;">
        <h1 style="font-size: 24px; color: #333;">Achievement</h1>
        <p style="font-size: 18px; color: #555;"><strong>Date:</strong> ${this.formatDate(brag.date)}</p>
        <p style="font-size: 18px; color: #555;"><strong>Title:</strong> ${brag.title}</p>
        <p style="font-size: 18px; color: #555;"><strong>Description:</strong></p>
        <div style="font-size: 16px; color: #666;">${DOMPurify.sanitize(brag.description)}</div>
        <p style="font-size: 18px; color: #555;"><strong>Tags:</strong> ${this.getTags(brag.achievementTags)}</p>
        <hr style="margin: 20px 0; border-top: 1px solid #eee;">
      </div>
    `
      )
      .join("");

    const element = document.createElement('div');
    element.innerHTML = content;
    document.body.appendChild(element);

    html2pdf()
      .from(element)
      .toPdf()
      .get('pdf')
      .then((pdf: any) => {
        pdf.autoPrint();
        pdf.save('achievements.pdf');
      })
      
      .finally(() => document.body.removeChild(element)); // Clean up the element
  }

  formatDate(date: string): string {
    return new Intl.DateTimeFormat('en-US').format(new Date(date)); // Adjust format as needed
  }

  // Update getTags to extract tagName from achievementTags
 // Modify the getTags method to handle both string and array of tags
 getTags(achievementTags: { id: number; tagName: string }[] | undefined): string {
  // Check if achievementTags is undefined or null
  if (!achievementTags || achievementTags.length === 0) {
    return 'No tags';
  }
  return achievementTags.map(tag => tag.tagName).join(', ');
}




  clearMessages(): void {
    setTimeout(() => {
      this.successMessage = null;
      this.errorMessage = null;
    }, 3000);
  }
}
