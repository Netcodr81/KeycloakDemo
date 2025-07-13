import {Component, effect, inject, OnInit, signal} from '@angular/core';
import {Router, RouterModule, RouterOutlet} from '@angular/router';
import {KEYCLOAK_EVENT_SIGNAL, KeycloakEventType, ReadyArgs, typeEventArgs} from 'keycloak-angular';
import Keycloak from 'keycloak-js';
import {User} from '../models/user';


@Component({
  selector: 'app-root',
  imports: [RouterOutlet,
    RouterModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'keycloak-angular-example';
  isAuthenticated = false;
  keycloakStatus: string | undefined;
  private readonly keycloak = inject(Keycloak);
  private readonly keycloakSignal = inject(KEYCLOAK_EVENT_SIGNAL);
  private readonly router = inject(Router);
  user = signal<User | null>(null);

  constructor() {
    effect(() => {
      const keycloakEvent = this.keycloakSignal();

      this.keycloakStatus = keycloakEvent.type;

      if (keycloakEvent.type === KeycloakEventType.Ready) {
        this.isAuthenticated = typeEventArgs<ReadyArgs>(keycloakEvent.args);
      }

      if (keycloakEvent.type === KeycloakEventType.AuthLogout) {
        this.isAuthenticated = false;

      }


    });
  }

  async ngOnInit() {
    if (this.keycloak?.authenticated) {
      const profile = await this.keycloak.loadUserProfile();

      this.user.set({
        name: profile.firstName + ' ' + profile.lastName,
        email: profile.email,
        username: profile.username
      });
    }
  }

  public login(): void {
    this.keycloak.login();
  }

  public logout(): void {
    this.keycloak.logout();
  }


  private toJWTString(token: string) {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );

    return jsonPayload;
  }
}
