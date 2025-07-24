import { Routes } from "@angular/router";
import { PrivateComponent } from "./private/private.component";
import { canActivateAuthRole } from "./auth.guard";
import { HomeComponent } from "./home/home.component";
import { UserInfoComponent } from "./user-info/user-info.component";
import { UserRolesComponent } from "./user-roles/user-roles.component";
import { ForbiddenComponent } from "./forbidden/forbidden.component";

export const routes: Routes = [
  { path: "", component: HomeComponent },
  {
    path: "user-info",
    component: UserInfoComponent,
    canActivate: [canActivateAuthRole],
    // No role required - just authentication
  },
  {
    path: "user-roles",
    component: UserRolesComponent,
    canActivate: [canActivateAuthRole],
    // Require admin role
  },
  { path: "forbidden", component: ForbiddenComponent },
  { path: "**", component: HomeComponent },
];
