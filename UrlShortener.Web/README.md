# UrlShortener.Web

Vue 3 + TypeScript frontend for the URL Shortener API.

## Getting Started

### Prerequisites

- Node.js 18+
- Yarn
- Backend API running (see main project README)

### Installation

```bash
cd UrlShortener.Web
yarn
```

### Development

```bash
# Run development server
yarn dev
```

The frontend will be available at `http://localhost:5173` (or another port if 5173 is taken).

### Build for Production

```bash
yarn build
```

### Environment Variables

Create a `.env` file (copy from `.env.example`):

```bash
cp .env.example .env
```

Available variables:
- `VITE_API_URL` - URL of the backend API (default: http://localhost:5000)

### Development Server Proxy

During development, API requests are proxied to `http://localhost:5000` automatically via Vite's dev server proxy configuration in `vite.config.ts`.

## Project Structure

```
UrlShortener.Web/
├── src/
│   ├── components/
│   │   └── UrlShortener.vue    # Main URL shortening component
│   ├── services/
│   │   └── api.ts              # API client and services
│   ├── App.vue                 # Root component
│   ├── main.ts                 # Entry point
│   └── style.css               # Global styles
├── .env.example                # Environment variable template
├── vite.config.ts              # Vite configuration
└── package.json
```

## Features

- Simple URL input form
- Real-time validation
- Copy shortened URL to clipboard
- Error handling with user-friendly messages
- Responsive design

## API Integration

The frontend communicates with the backend API at:
- `POST /shorten` - Create a shortened URL

## Technology Stack

- Vue 3 with Composition API
- TypeScript
- Vite
- Axios for HTTP requests
