import { Component, inject, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import Keycloak from "keycloak-js";

@Component({
  selector: "app-claims-panel",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./claims-panel.component.html",
  styleUrl: "./claims-panel.component.css",
})
export class ClaimsPanelComponent {
  claims = signal<{ type: string; value: string }[]>([]);

  private readonly keycloak = inject(Keycloak);

  showClaims(): void {
    const tokenParsed = this.keycloak.tokenParsed;
    if (tokenParsed) {
      // Filter out realm_access and resource_access from the displayed claims
      const filteredEntries = Object.entries(tokenParsed).filter(([key]) => key !== "realm_access" && key !== "resource_access");

      this.claims.set([
        ...filteredEntries.map(([type, value]) => ({
          type,
          value: JSON.stringify(value),
        })),
        ...(this.keycloak.resourceAccess?.["angular_client"]?.roles.map((role) => ({
          type: "role",
          value: role,
        })) ?? []),
        ...(this.keycloak.realmAccess?.roles.map((role) => ({
          type: "role",
          value: role,
        })) ?? []),
      ]);
    }
  }

  clearClaims(): void {
    this.claims.set([]);
  }
}
