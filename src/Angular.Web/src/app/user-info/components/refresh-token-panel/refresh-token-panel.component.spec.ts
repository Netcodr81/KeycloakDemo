import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RefreshTokenPanelComponent } from './refresh-token-panel.component';

describe('RefreshTokenPanelComponent', () => {
  let component: RefreshTokenPanelComponent;
  let fixture: ComponentFixture<RefreshTokenPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RefreshTokenPanelComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RefreshTokenPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
