import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AccessTokenPanelComponent } from './access-token-panel.component';

describe('AccessTokenPanelComponent', () => {
  let component: AccessTokenPanelComponent;
  let fixture: ComponentFixture<AccessTokenPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccessTokenPanelComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AccessTokenPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
