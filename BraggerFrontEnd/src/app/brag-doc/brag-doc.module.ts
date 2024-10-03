import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms'; // Import FormsModule and ReactiveFormsModule
import { RouterModule } from '@angular/router'; // Import RouterModule
import { EditorModule } from '@tinymce/tinymce-angular'; // Import EditorModule here
import { ListBragComponent } from './components/list-brag/list-brag.component';
import { CreateBragComponent } from './components/create-edit-brag/create-brag.component';
import { BragDocService } from './services/brag-doc.service'; // Correct import path
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { EditBragComponent } from './components/edit-brag/edit-brag.component';
import { SanitizePipe } from './sanitizer.pipe';


@NgModule({
  declarations: [
    ListBragComponent,
    CreateBragComponent,
    EditBragComponent,
    SanitizePipe, 
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule, // Add ReactiveFormsModule here
    FormsModule, // Add FormsModule here
    EditorModule, // Ensure EditorModule is imported here
    RouterModule,
    // Add RouterModule here (useful if there are child routes)
  ],
  providers: [
    BragDocService // Provide BragDocService
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA], // Add this line if necessary
  exports: [
    ListBragComponent,
    CreateBragComponent,
    EditBragComponent,
    SanitizePipe,
  ]
})
export class BragDocModule { }