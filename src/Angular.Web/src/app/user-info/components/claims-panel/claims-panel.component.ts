import {Component, inject, signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import Keycloak from 'keycloak-js';

@Component({
  selector: 'app-claims-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './claims-panel.component.html',
  styleUrl: './claims-panel.component.css'
})
export class ClaimsPanelComponent {
  claims = signal<{ type: string; value: string }[]>([]);

  private readonly keycloak = inject(Keycloak);

  showClaims(): void {
    const tokenParsed = this.keycloak.tokenParsed;
    if (tokenParsed) {
      this.claims.set(
        Object.entries(tokenParsed).map(([type, value]) => ({
          type,
          value: JSON.stringify(value),
        }))
      );
    }
  }

  clearClaims(): void {
    this.claims.set([]);
  }
}
