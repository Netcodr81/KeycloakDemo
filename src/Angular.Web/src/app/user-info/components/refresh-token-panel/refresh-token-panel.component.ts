import {Component, inject, OnInit, signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {HttpClient, HttpClientModule, HttpParams} from '@angular/common/http';
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
    const refreshToken = this.keycloak.refreshToken;
    console.log('Refresh token:', refreshToken);
    if (!refreshToken) {
      alert('No refresh token available.');
      return;
    }

    const tokenUrl = `${this.keycloak.authServerUrl}/realms/${this.keycloak.realm}/protocol/openid-connect/token`;
    console.log('Refreshing token at URL:', tokenUrl);
    const body = new HttpParams()
      .set('grant_type', 'refresh_token')
      .set('client_id', this.keycloak.clientId as string)
      .set('refresh_token', refreshToken);

    this.http.post(tokenUrl, body, { headers: { 'Content-Type': 'application/x-www-form-urlencoded' } })
      .subscribe({
        next: (response: any) => {
          // Update Keycloak tokens manually
          this.keycloak.token = response.access_token;
          this.keycloak.refreshToken = response.refresh_token;
          this.keycloak.idToken = response.id_token;

          // Update the component state instead of reloading
          try {
            const payload = this.decodeTokenPayload(response.access_token);
            this.tokenExpiration.set(this.getClaimAsDate(payload, 'exp'));
            this.tokenIssuedAt.set(this.getClaimAsDate(payload, 'iat'));
            this.lastAuthenticatedAt.set(this.getClaimAsDate(payload, 'auth_time'));
          } catch (e) {
            console.error('Failed to decode new token', e);
          }

          alert('Token refreshed successfully.');
        },
        error: (err) => {
          alert('Failed to refresh token: ' + JSON.stringify(err));
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
