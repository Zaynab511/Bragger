import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ExportService {

  constructor() { }

  exportToPDF(data: any): void {
    // Implement PDF export functionality
    console.log('Export to PDF');
  }

  exportToWord(data: any): void {
    // Implement Word export functionality
    console.log('Export to Word');
  }

  exportToCSV(data: any): void {
    // Implement CSV export functionality
    console.log('Export to CSV');
  }
}
