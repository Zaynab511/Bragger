// src/app/app-routing.module.ts
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './auth/components/login/login.component';
import { RegisterComponent } from './auth/components/register/register.component';
import { ListBragComponent } from './brag-doc/components/list-brag/list-brag.component';
import { CreateBragComponent } from './brag-doc/components/create-edit-brag/create-brag.component';
import { ForgotPasswordComponent } from './auth/components/forgot-password/forgot-password.component';
import { ProfileComponent } from './auth/components/profile/profile.component';
import { DashboardComponent } from './auth/components/dashboard/dashboard.component';
import { EditBragComponent } from './brag-doc/components/edit-brag/edit-brag.component';
import { AuthGuard } from './auth/auth.guard';
import { LandingPageComponent } from './auth/components/landing-page/landing-page.component';
const routes: Routes = [
  { path: '', component: LandingPageComponent },  // Default route for landing page
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'brags', component: ListBragComponent },
  { path: 'brag/create', component: CreateBragComponent },
  { path: 'brag/edit/:id', component: EditBragComponent },
  { path: 'forgot-password', component: ForgotPasswordComponent },
  { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] },
  { path: 'list-brag', component: ListBragComponent, canActivate: [AuthGuard] },
  { path: '**', redirectTo: 'login' } // Wildcard route for unknown paths
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
