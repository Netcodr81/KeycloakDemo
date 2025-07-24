import { ApplicationConfig, APP_INITIALIZER, provideZoneChangeDetection } from "@angular/core";
import { provideRouter } from "@angular/router";
import {
  AutoRefreshTokenService,
  createInterceptorCondition,
  INCLUDE_BEARER_TOKEN_INTERCEPTOR_CONFIG,
  IncludeBearerTokenCondition,
  includeBearerTokenInterceptor,
  provideKeycloak,
  UserActivityService,
  withAutoRefreshToken,
} from "keycloak-angular";

import { routes } from "./app.routes";
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptors } from "@angular/common/http";

const urlCondition = createInterceptorCondition<IncludeBearerTokenCondition>({
  urlPattern: /^(http:\/\/localhost:8080)(\/.*)?$/i,
  bearerPrefix: "Bearer",
});

export const provideKeycloakAngular = () =>
  provideKeycloak({
    config: {
      url: "http://localhost:8080",
      realm: "keycloak_demo",
      clientId: "angular_client",
    },
    initOptions: {
      onLoad: "check-sso",
      checkLoginIframe: false,
      // Remove silentCheckSsoRedirectUri to avoid missing file error
      // silentCheckSsoRedirectUri: window.location.origin + '/silent-check-sso.html'
    },
    features: [
      withAutoRefreshToken({
        onInactivityTimeout: "logout",
        sessionTimeout: 60000,
      }),
    ],
    providers: [
      AutoRefreshTokenService,
      UserActivityService,
      {
        provide: INCLUDE_BEARER_TOKEN_INTERCEPTOR_CONFIG,
        useValue: [urlCondition],
      },
    ],
  });

export const appConfig: ApplicationConfig = {
  providers: [
    provideKeycloakAngular(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([includeBearerTokenInterceptor])),
  ],
};
