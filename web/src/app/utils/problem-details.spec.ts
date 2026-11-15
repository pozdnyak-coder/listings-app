import { HttpErrorResponse } from '@angular/common/http';
import { problemErrors } from './problem-details';

describe('problemErrors', () => {
  it('достаёт errors из ответа 400', () => {
    const e = new HttpErrorResponse({
      status: 400,
      error: { title: 'One or more validation errors occurred.', errors: { title: ['Укажите заголовок'] } },
    });
    expect(problemErrors(e)).toEqual({ title: ['Укажите заголовок'] });
  });

  it('возвращает null для других кодов', () => {
    expect(problemErrors(new HttpErrorResponse({ status: 500 }))).toBeNull();
  });

  it('возвращает null, если это вообще не ошибка HTTP', () => {
    expect(problemErrors(new Error('boom'))).toBeNull();
  });
});
