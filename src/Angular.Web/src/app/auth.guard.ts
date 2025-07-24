import { ActivatedRouteSnapshot, CanActivateFn, Router, RouterStateSnapshot, UrlTree } from "@angular/router";
import { inject } from "@angular/core";
import { AuthGuardData, createAuthGuard, KeycloakService } from "keycloak-angular";

const isAccessAllowed = async (route: ActivatedRouteSnapshot, state: RouterStateSnapshot, authData: AuthGuardData): Promise<boolean | UrlTree> => {
  const { authenticated, grantedRoles } = authData;

  console.log("Auth Guard Debug:", { authenticated, grantedRoles, targetUrl: state.url });

  // If user is not authenticated, redirect to Keycloak login with return URL
  if (!authenticated) {
    console.log("User not authenticated, redirecting to login");
    const keycloakService = inject(KeycloakService);
    await keycloakService.login({
      redirectUri: window.location.origin + state.url,
    });
    return false; // This won't be reached due to redirect, but required for type safety
  }

  const requiredRoles = route.data["roles"] || (route.data["role"] ? [route.data["role"]] : []);
  console.log("Required roles:", requiredRoles);

  if (!requiredRoles || requiredRoles.length === 0) {
    console.log("No roles required, allowing access");
    return true; // If no roles required, allow access for authenticated users
  }

  const hasAnyRequiredRole = (roles: string[]): boolean => {
    for (const role of roles) {
      const hasRole = Object.values(grantedRoles.resourceRoles).some((userRoles) => userRoles.includes(role));
      console.log(`Checking role ${role}:`, hasRole);
      if (hasRole) {
        return true; // User has at least one of the required roles
      }
    }
    console.log("Available roles:", grantedRoles.resourceRoles);
    return false;
  };

  if (hasAnyRequiredRole(requiredRoles)) {
    console.log("User has at least one required role, allowing access");
    return true;
  }

  // User is authenticated but doesn't have required role
  console.log("User lacks required role, redirecting to forbidden");
  const router = inject(Router);
  return router.parseUrl("/forbidden");
};

export const canActivateAuthRole = createAuthGuard<CanActivateFn>(isAccessAllowed);
