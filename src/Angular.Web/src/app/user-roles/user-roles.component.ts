import { CommonModule } from "@angular/common";
import { User } from "./../../models/user";
import { Component, computed, inject, OnInit, signal } from "@angular/core";
import Keycloak from "keycloak-js";

@Component({
  selector: "app-user-roles",
  imports: [CommonModule],
  templateUrl: "./user-roles.component.html",
  styleUrl: "./user-roles.component.css",
})
export class UserRolesComponent implements OnInit {
  private readonly keycloak = inject(Keycloak);
  userRoles = signal<string[]>([]);

  isInAdminRole = computed(() => {
    return this.userRoles().includes("Angular_Client_Admin");
  });

  isInUserRole = computed(() => {
    return this.userRoles().includes("Angular_Client_User");
  });

  ngOnInit(): void {
    this.userRoles.set([...(this.keycloak.resourceAccess?.["angular_client"]?.roles ?? []), ...(this.keycloak.realmAccess?.roles ?? [])]);
  }
}
