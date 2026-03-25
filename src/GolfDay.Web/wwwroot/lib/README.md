# Client-Side Libraries

These packages should be installed via `libman` or copied from CDN for production.

## Required Libraries

Place the following in their respective directories:

### Bootstrap 5.3+
- `bootstrap/css/bootstrap.min.css`
- `bootstrap/js/bootstrap.bundle.min.js`

CDN: https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/

### Bootstrap Icons 1.11+
- `bootstrap-icons/bootstrap-icons.min.css`
- `bootstrap-icons/fonts/bootstrap-icons.woff2`

CDN: https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/

### jQuery 3.7+
- `jquery/jquery.min.js`

CDN: https://code.jquery.com/jquery-3.7.1.min.js

### jQuery Validation
- `jquery-validation/jquery.validate.min.js`
- `jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js`

CDN: https://cdnjs.cloudflare.com/ajax/libs/jquery-validate/1.20.0/

### Microsoft SignalR
- `@microsoft/signalr/dist/browser/signalr.min.js`

CDN: https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/8.0.0/

### Chart.js (for stats charts)
- `chart.js/chart.umd.min.js`

CDN: https://cdn.jsdelivr.net/npm/chart.js@4.4.3/

## Using CDN in Development
If not using local copies, the layout can reference CDN URLs.
In production, always use locally bundled files.
