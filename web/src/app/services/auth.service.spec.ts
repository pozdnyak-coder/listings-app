import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from './auth.service';

// «токен» с нужным exp; подпись проверяет сервер, не клиент
function fakeToken(expSeconds: number): string {
  const payload = btoa(JSON.stringify({ sub: '7', email: 'a@b.uz', exp: expSeconds }));
  return `header.${payload}.signature`;
}

describe('AuthService', () => {
  function create(token: string | null): AuthService {
    localStorage.clear();
    if (token) localStorage.setItem('auth_token', token);
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    return TestBed.inject(AuthService);
  }

  it('живой токен: вошёл, userId из sub', () => {
    const inHour = Math.floor(Date.now() / 1000) + 3600;
    const auth = create(fakeToken(inHour));
    expect(auth.isLoggedIn()).toBe(true);
    expect(auth.userId()).toBe(7);
  });

  it('просроченный токен: не вошёл, getToken() = null', () => {
    const hourAgo = Math.floor(Date.now() / 1000) - 3600;
    const auth = create(fakeToken(hourAgo));
    expect(auth.isLoggedIn()).toBe(false);
    expect(auth.getToken()).toBeNull();
  });

  it('мусор вместо токена не ломает приложение', () => {
    const auth = create('not-a-jwt');
    expect(auth.isLoggedIn()).toBe(false);
  });
});
