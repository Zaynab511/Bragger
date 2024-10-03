import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { User } from '../model/user.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = 'https://localhost:7010/api/User'; // replace with your actual API URL

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<{ success: boolean, message: string }> {
    const loginData = { email, password };
    return this.http.post<any>(`${this.apiUrl}/Login`, loginData).pipe(
      map(response => {
        if (response && response.token) {
          localStorage.setItem('token', response.token); // Store the JWT token in local storage
          return { success: true, message: response.message || 'Login successful' };
        } else {
          return { success: false, message: 'Login failed' };
        }
      }),
      catchError(error => of({ success: false, message: error.error?.message || 'Login failed' }))
    );
  }
  
  register(user: User): Observable<{ success: boolean, message: string }> {
    return this.http.post<any>(`${this.apiUrl}/Register`, user).pipe(
      map(response => ({ success: true, message: response })),  // Adjust according to response format
      catchError(error => of({ success: false, message: error.error?.message || 'Registration failed' }))
    );
  }

  forgotPassword(email: string): Observable<boolean> {
    return this.http.post<any>(`${this.apiUrl}/ForgotPassword`, { email }).pipe(
      map(response => response.success),
      catchError(error => of(false))
    );
  }

  getProfile(email: string): Observable<User | null> {
    return this.http.get<User>(`${this.apiUrl}/Profile/${email}`).pipe(
      catchError(error => of(null))
    );
  }

  updateProfile(email: string, updatedData: Partial<User>): Observable<boolean> {
    return this.http.put<any>(`${this.apiUrl}/Profile/${email}`, updatedData).pipe(
      map(response => response.success),
      catchError(error => of(false))
    );
  }

  logout(): void {
    localStorage.removeItem('token'); // Remove token on logout
  }
  isAuthenticated(): boolean {
    return !!localStorage.getItem('token'); // Check if the token exists
  }
  loginWithGoogle(): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/GoogleLogin`, {}).pipe(
      catchError(error => of(false))
    );
  }

  loginWithLinkedIn(): Observable<boolean> {
    return this.http.post<boolean>(`${this.apiUrl}/LinkedInLogin`, {}).pipe(
      catchError(error => of(false))
    );
  }
}
