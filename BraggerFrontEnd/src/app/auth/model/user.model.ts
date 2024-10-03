export interface User {
  id?: number; 
  email: string;
  PasswordHash: string; // Adjust according to backend requirements
  Name: string;
  JobTitle: string;
  Industry: string;
  Contact: string;
}
