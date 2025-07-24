import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from "@angular/router";
import { KeycloakAuthGuard, KeycloakService } from "keycloak-angular";

@Injectable({
  providedIn: "root",
})
export class RoleGuard extends KeycloakAuthGuard {
  constructor(protected override readonly router: Router, protected readonly keycloak: KeycloakService) {
    super(router, keycloak);
  }

  public async isAccessAllowed(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
    // Force the user to log in if currently unauthenticated.
    if (!this.authenticated) {
      await this.keycloak.login({
        redirectUri: window.location.origin + state.url,
      });
    }

    // Get the roles required from the route data
    const requiredRoles = route.data?.["roles"];

    // Allow the user to proceed if no additional roles are required
    if (!requiredRoles || requiredRoles.length === 0) {
      return true;
    }

    // Allow the user to proceed if they have at least one of the required roles
    return requiredRoles.some((role: string) => this.roles.includes(role));
  }
}
