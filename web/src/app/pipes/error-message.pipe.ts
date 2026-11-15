import { Pipe, PipeTransform } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

export function errorMessage(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    switch (err.status) {
      case 0:   return 'Сервер недоступен. Проверьте соединение и повторите (бесплатный сервер может «просыпаться» до минуты).';
      case 400: return typeof err.error === 'string' ? err.error : 'Проверьте введённые данные.';
      case 401: return 'Войдите, чтобы продолжить.';
      case 403: return 'Это объявление может менять только автор.';
      case 404: return 'Объявление не найдено.';
      case 500: return 'Ошибка на сервере. Попробуйте позже.';
      default:  return `Ошибка ${err.status}.`;
    }
  }
  return 'Что-то пошло не так.';
}

@Pipe({ name: 'errorMessage' })
export class ErrorMessagePipe implements PipeTransform {
  transform(err: unknown): string { return errorMessage(err); }
}
