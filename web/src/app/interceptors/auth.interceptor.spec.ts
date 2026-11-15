import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';
import { environment } from '../../environments/environment';

describe('authInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  const fakeAuth = { getToken: () => 'abc123', logout: vi.fn() };   // поддельный AuthService

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: fakeAuth },
      ],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
    fakeAuth.logout.mockClear();
  });

  afterEach(() => backend.verify());

  it('добавляет токен к запросам в наш API', () => {
    http.get(`${environment.apiUrl}/api/listings`).subscribe();
    const req = backend.expectOne(`${environment.apiUrl}/api/listings`);
    expect(req.request.headers.get('Authorization')).toBe('Bearer abc123');
    req.flush([]);
  });

  it('не отправляет токен на чужие сайты', () => {
    http.get('https://example.com/data').subscribe();
    const req = backend.expectOne('https://example.com/data');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('на ответ 401 выходит из аккаунта', () => {
    http.get(`${environment.apiUrl}/api/auth/me`).subscribe({ error: () => {} });
    backend.expectOne(`${environment.apiUrl}/api/auth/me`)
      .flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(fakeAuth.logout).toHaveBeenCalledTimes(1);
  });
});
