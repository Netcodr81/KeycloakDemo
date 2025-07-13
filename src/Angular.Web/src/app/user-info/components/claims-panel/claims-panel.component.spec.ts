import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ClaimsPanelComponent } from './claims-panel.component';

describe('ClaimsPanelComponent', () => {
  let component: ClaimsPanelComponent;
  let fixture: ComponentFixture<ClaimsPanelComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ClaimsPanelComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ClaimsPanelComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
