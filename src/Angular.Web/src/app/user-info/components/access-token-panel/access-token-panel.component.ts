import {Component, inject, signal} from '@angular/core';
import Keycloak from 'keycloak-js';
import {CommonModule} from "@angular/common";

@Component({
  selector: 'app-access-token-panel',
  imports: [CommonModule],
  templateUrl: './access-token-panel.component.html',
  styleUrl: './access-token-panel.component.css'
})
export class AccessTokenPanelComponent {

  accessToken = signal<string | null>(null);
  decodedToken = signal<string | null>(null);
  showingAccessToken = signal(false);
  showingDecodedToken = signal(false);

  private readonly keycloak = inject(Keycloak);

  public showAccessToken(): void {
    this.decodedToken.set(null);
    this.accessToken.set(this.keycloak.token ?? null);
    this.showingAccessToken.set(true);
    this.showingDecodedToken.set(false);
  }

  public decodeAccessToken(): void {
    const token = this.accessToken();
    if (!token) {
      return;
    }

    try {
      const payload = this.decodeTokenPayload(token);
      this.decodedToken.set(JSON.stringify(payload, null, 2));
      this.showingAccessToken.set(false);
      this.showingDecodedToken.set(true);
    } catch (e) {
      this.accessToken.set(`Error decoding token: ${(e as Error).message}`);
      this.decodedToken.set(null);
    }
  }

  public clearAccessToken(): void {
    this.accessToken.set(null);
    this.decodedToken.set(null);
  }

  public async copyAccessToken(): Promise<void> {
    const decoded = this.decodedToken();
    const access = this.accessToken();

    try {
      if (decoded) {
        await navigator.clipboard.writeText(decoded);
        alert('Decoded Access Token copied to clipboard.');
      } else if (access) {
        await navigator.clipboard.writeText(access);
        alert('Access Token copied to clipboard.');
      }
    } catch (e) {
      console.error('Failed to copy token: ', e);
    }
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
