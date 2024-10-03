import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { BragDoc } from '../models/brag-doc.model';
import { Tag } from '../models/tag.model';  // Import Tag model

@Injectable({
  providedIn: 'root'
})
export class BragDocService {
  private apiUrl = 'https://localhost:7010/api/Achievements'; // Update with your actual API URL
  private tagApiUrl = 'https://localhost:7010/api/Tags'; // API endpoint for tags

  constructor(private http: HttpClient) {}

  // Create HTTP options dynamically to handle token changes
  private getHttpOptions() {
    const token = localStorage.getItem('token');
    return {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}` // Adjust according to your auth strategy
      })
    };
  }

  // Get all brag documents
  getAllBrags(): Observable<BragDoc[]> {
    return this.http.get<BragDoc[]>(`${this.apiUrl}/GetAll`, this.getHttpOptions()).pipe(
      catchError(this.handleError)
    );
  }

  // Get brag by ID
  getBragById(id: number): Observable<BragDoc> {
    return this.http.get<BragDoc>(`${this.apiUrl}/GetById/${id}`, this.getHttpOptions()).pipe(
      catchError(this.handleError)
    );
  }

  // Create a new brag document
  createBrag(bragDoc: BragDoc): Observable<{ message: string }> {
    return this.http.post<string>(`${this.apiUrl}/Create`, bragDoc, {
      ...this.getHttpOptions(),
      responseType: 'text' as 'json' // Treat the response as text
    }).pipe(
      map((response: string) => {
        return { message: response || 'Achievement created successfully!' };
      }),
      catchError((error) => this.handleTextError(error, 'Error creating achievement'))
    );
  }

  // Update a brag document
  updateBrag(id: number, updatedBrag: BragDoc): Observable<{ message: string }> {
    return this.http.put(`${this.apiUrl}/Update/${id}`, updatedBrag, {
      ...this.getHttpOptions(),
      responseType: 'text' // Expect plain text response
    }).pipe(
      map((response: string) => {
        console.log('Update response:', response);
        return { message: response || 'Achievement updated successfully!' };
      }),
      catchError((error) => {
        console.error('Error in update service:', error);
        return this.handleTextError(error, 'Error updating achievement');
      })
    );
  }

  // Delete a brag document
  deleteBrag(id: number): Observable<{ message: string }> {
    return this.http.delete<string>(`${this.apiUrl}/Delete/${id}`, {
      ...this.getHttpOptions(),
      responseType: 'text' as 'json'
    }).pipe(
      map((response: string) => {
        return { message: response || 'Achievement deleted successfully!' };
      }),
      catchError((error) => this.handleTextError(error, 'Error deleting achievement'))
    );
  }

  // Get all available tags
  getTags(): Observable<Tag[]> {
    return this.http.get<Tag[]>(`${this.apiUrl}/GetTags`, this.getHttpOptions()).pipe(
      catchError(this.handleError)
    );
  }

// Get AI suggestions for tags, accepting a description as a parameter
getAISuggestions(description: string): Observable<string[]> {
  return this.http.get<string[]>(`${this.apiUrl}/GetTagSuggestions`, {
    params: { description }, // Pass description as a query parameter
    ...this.getHttpOptions()
  }).pipe(
    catchError(this.handleError)
  );
}

  // Handle text-based error responses for non-JSON responses
  private handleTextError(error: HttpErrorResponse, fallbackMessage: string): Observable<never> {
    const errorMessage = error.error && typeof error.error === 'string'
      ? error.error // if it's a string error response
      : fallbackMessage;
    return throwError(() => new Error(errorMessage));
  }

  // General error handler for HTTP requests
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Something went wrong; please try again later.';

    if (error.error instanceof ErrorEvent) {
      // Client-side or network error
      errorMessage = `Error: ${error.error.message}`;
    } else if (error.status === 0) {
      // No response from server
      errorMessage = 'No response from the server.';
    } else {
      // Server-side error
      errorMessage = `Server Error: ${error.status}. Message: ${error.message}`;
    }
    
    console.error(errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
