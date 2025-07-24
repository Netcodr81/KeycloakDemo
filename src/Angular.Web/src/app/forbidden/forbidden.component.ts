import { Component } from "@angular/core";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";

@Component({
  selector: "app-forbidden",
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container mx-auto p-8 text-center">
      <h1 class="text-4xl font-bold text-red-600 mb-4">403 - Forbidden</h1>
      <p class="text-lg text-gray-700 mb-6">You don't have permission to access this resource.</p>
      <a routerLink="/" class="bg-blue-500 hover:bg-blue-700 text-white font-bold py-2 px-4 rounded"> Go Home </a>
    </div>
  `,
})
export class ForbiddenComponent {}
