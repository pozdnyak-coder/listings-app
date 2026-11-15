import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { ListingsPage } from './pages/listings-page/listings-page';
import { ListingPage } from './pages/listing-page/listing-page';
import { ListingFormPage } from './pages/listing-form-page/listing-form-page';
import { MyListingsPage } from './pages/my-listings-page/my-listings-page';
import { LoginPage } from './pages/login-page/login-page';
import { RegisterPage } from './pages/register-page/register-page';
import { NotFoundPage } from './pages/not-found-page/not-found-page';

export const routes: Routes = [
  { path: '', component: ListingsPage, title: 'Объявления' },
  { path: 'listings/new', component: ListingFormPage, canActivate: [authGuard], title: 'Новое объявление' },  // выше :id!
  { path: 'listings/:id/edit', component: ListingFormPage, canActivate: [authGuard], title: 'Редактирование' },
  { path: 'listings/:id', component: ListingPage, title: 'Объявление' },
  { path: 'my', component: MyListingsPage, canActivate: [authGuard], title: 'Мои объявления' },
  { path: 'login', component: LoginPage, title: 'Вход' },
  { path: 'register', component: RegisterPage, title: 'Регистрация' },
  { path: '**', component: NotFoundPage, title: 'Страница не найдена' },
];
