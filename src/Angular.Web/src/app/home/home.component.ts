import {Component, computed, inject} from '@angular/core';
import Keycloak from "keycloak-js";


@Component({
  selector: 'app-home',
  imports: [],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {

  keycloak = inject(Keycloak);

  isAuthenticated = computed(() =>{
    return this.keycloak.authenticated;
  })

}
