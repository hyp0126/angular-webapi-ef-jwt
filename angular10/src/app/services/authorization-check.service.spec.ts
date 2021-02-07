import { TestBed } from '@angular/core/testing';

import { AuthorizationCheckService } from './authorization-check.service';

describe('AuthorizationCheckService', () => {
  let service: AuthorizationCheckService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AuthorizationCheckService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
