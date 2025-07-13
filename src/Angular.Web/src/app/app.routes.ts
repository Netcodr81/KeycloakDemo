import { Routes } from '@angular/router';
import { PrivateComponent } from './private/private.component';
import { PublicComponent } from './public/public.component';
import { AuthGuard } from './auth.guard';
import { HomeComponent } from './home/home.component';
import { UserInfoComponent } from './user-info/user-info.component';

export const routes: Routes = [
  {path: '', component: HomeComponent},
  {path: 'user-info', component: UserInfoComponent},
  { path: 'private', component: PrivateComponent, canActivate: [AuthGuard] },
  { path: 'public', component: PublicComponent },
  { path: '**', component: HomeComponent }
];
