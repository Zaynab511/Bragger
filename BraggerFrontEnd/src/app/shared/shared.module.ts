import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from './components/navbar/navbar.component';
import { ExportService } from './services/export.service';
import { RouterModule } from '@angular/router'; // Import RouterModule


@NgModule({
  declarations: [
    NavbarComponent,
    
  ],
  imports: [
    CommonModule,
    RouterModule,
  ],
  exports: [
    NavbarComponent,
    
  ],
  providers: [ExportService]
})
export class SharedModule { }