import { Component } from '@angular/core';
import { RefreshTokenPanelComponent } from './components/refresh-token-panel/refresh-token-panel.component';
import { AccessTokenPanelComponent } from './components/access-token-panel/access-token-panel.component';
import { ClaimsPanelComponent } from './components/claims-panel/claims-panel.component';

@Component({
  selector: 'app-user-info',
  imports: [RefreshTokenPanelComponent, AccessTokenPanelComponent, ClaimsPanelComponent],
  templateUrl: './user-info.component.html',
  styleUrl: './user-info.component.css'
})
export class UserInfoComponent {

}
