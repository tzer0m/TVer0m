# TVer0m

[![Deploy](https://github.com/tzer0m/TVer0m/actions/workflows/deploy.yml/badge.svg)](https://github.com/tzer0m/TVer0m/actions/workflows/deploy.yml)

A small ASP.NET Core (.NET 10) minimal API that turns an old iMac running Linux Mint (XFCE, X11) into a TV box. It serves a full-screen launcher for the iMac and a touch remote for a phone.

## Pages

- `/` sends each visitor to the right page. Phones and tablets go to `/remote`, and the kiosk browser (a user agent containing `SMART-TV` or `Tizen`) goes to `/host`. Anything else gets a tiny page that checks for a touch screen under 900px and redirects. `?mode=host` or `?mode=remote` overrides detection.
- `/host` is the TV launcher: dark, large text, a clock, and one tile per entry in `wwwroot/services.json`. Arrow keys move focus, and Enter or a click opens the service.
- `/remote` is the phone remote: a d-pad with OK and Back, volume down, mute and up, and Home. Arrows and volume repeat while held.

Add a service by adding an entry with a `name`, `url` and `colour` to `wwwroot/services.json`. No code change is needed.

## API

All endpoints are `POST` and return JSON in the form `{ "success": true, "message": "..." }`. Names are whitelisted, and anything else returns 400.

| Endpoint | Values | Action |
| --- | --- | --- |
| `/api/key/{name}` | `up`, `down`, `left`, `right`, `ok`, `back` | `xdotool key` with Up, Down, Left, Right, Return or Escape |
| `/api/volume/{action}` | `up`, `down`, `mute` | `pactl` on the default sink, in 5% steps and never above 100% |
| `/api/home` | none | Sends the browser back to `/host` |

Home uses Chromium's DevTools port (`Page.navigate` on `127.0.0.1:9222`) and falls back to typing the URL with xdotool. Chromium must be started with `--remote-debugging-port=9222` for the first method to work.

External programs are started with `ProcessStartInfo` and `ArgumentList`, with no shell, so nothing from a request is ever interpreted as a command. `DISPLAY` defaults to `:0` and `XAUTHORITY` to `/home/tzer0m/.Xauthority` when they aren't set.

## Requirements on the iMac

- .NET is not needed when using the self-contained single-file publish.
- `xdotool` for key presses and Home fallback.
- `pactl` (PulseAudio or PipeWire) for volume.
- Chromium in kiosk mode.

## Running Locally

Off Linux, mock implementations log the commands instead of running them, so both pages can be viewed on a Windows or Mac machine. Run the `TVer0m.Web` project and open `http://localhost:8090/host` and `http://localhost:8090/remote`.

The app listens on `0.0.0.0:8090` unless another URL is supplied. There is no authentication, so keep it on a trusted LAN.

## Project Layout

- `Detection/` holds `DeviceDetector`, which decides where a visitor goes.
- `Services/` holds the process runner and the input, volume and browser controllers, each behind an interface.
- `Mocks/` holds the logging implementations used off Linux.
- `Endpoints/` maps the pages and the API.
- `wwwroot/` holds the pages, stylesheets, icons, manifest and `services.json`.

## Credits

Icon: [Apple tv icons created by Darius Dan - Flaticon](https://www.flaticon.com/free-icons/apple-tv)
