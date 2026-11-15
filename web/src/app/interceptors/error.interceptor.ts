import { HttpInterceptorFn } from '@angular/common/http';
import { tap } from 'rxjs';

// Видит каждый запрос и ответ. Пока — только лог в консоль.
export const errorInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    tap({ error: (err) => console.error(`[API] ${req.method} ${req.url} → ${err.status}`) }),
  );
