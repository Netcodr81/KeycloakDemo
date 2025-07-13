import {Component, inject, OnInit, signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {HttpClient, HttpClientModule} from '@angular/common/http';
import Keycloak from 'keycloak-js';

@Component({
  selector: 'app-refresh-token-panel',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  templateUrl: './refresh-token-panel.component.html',
  styleUrl: './refresh-token-panel.component.css'
})
export class RefreshTokenPanelComponent implements OnInit {
  tokenExpiration = signal<Date | null>(null);
  tokenIssuedAt = signal<Date | null>(null);
  lastAuthenticatedAt = signal<Date | null>(null);

  private readonly keycloak = inject(Keycloak);
  private readonly http = inject(HttpClient);

  ngOnInit(): void {
    const token = this.keycloak.token;
    if (token) {
      try {
        const payload = this.decodeTokenPayload(token);
        this.tokenExpiration.set(this.getClaimAsDate(payload, 'exp'));
        this.tokenIssuedAt.set(this.getClaimAsDate(payload, 'iat'));
        this.lastAuthenticatedAt.set(this.getClaimAsDate(payload, 'auth_time'));
      } catch (e) {
        console.error('Failed to decode token', e);
      }
    }
  }

  public refreshToken(): void {
    this.http.post('/authentication/api/refresh-token', null).subscribe({
      next: () => {
        alert('Token refreshed successfully via manual button.');
        window.location.reload();
      },
      error: async (err) => {
        const errorContent = err.error instanceof Blob ? await err.error.text() : JSON.stringify(err.error);
        alert(`Failed to refresh token. Server responded with: ${errorContent}`);
      }
    });
  }

  private getClaimAsDate(payload: any, claimType: string): Date | null {
    const claim = payload[claimType];
    if (typeof claim === 'number') {
      return new Date(claim * 1000);
    }
    return null;
  }

  private decodeTokenPayload(token: string): any {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  }
}
