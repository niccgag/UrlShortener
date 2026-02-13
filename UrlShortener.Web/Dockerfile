# Build stage
FROM node:20-alpine AS build

WORKDIR /app

# Copy package files
COPY UrlShortener.Web/package.json UrlShortener.Web/yarn.lock ./

# Install dependencies
RUN yarn install --frozen-lockfile

# Copy source code
COPY UrlShortener.Web/ .

# Build the application
RUN yarn build

# Production stage
FROM nginx:alpine AS production

# Copy built assets from build stage
COPY --from=build /app/dist /usr/share/nginx/html

# Copy nginx configuration
COPY UrlShortener.Web/nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]

# Development stage
FROM node:20-alpine AS dev

WORKDIR /app

# Copy package files
COPY UrlShortener.Web/package.json UrlShortener.Web/yarn.lock ./

# Install dependencies (including dev dependencies)
RUN yarn install

# Copy source code
COPY UrlShortener.Web/ .

# Expose port 80
EXPOSE 80

# Run in development mode
CMD ["yarn", "dev", "--host", "0.0.0.0", "--port", "80"]
