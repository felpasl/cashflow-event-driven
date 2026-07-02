FROM node:24-alpine AS deps
WORKDIR /app
COPY src/frontend/package.json src/frontend/package-lock.json ./
RUN npm ci

FROM node:24-alpine AS build
WORKDIR /app
ARG NEXT_PUBLIC_LANCAMENTOS_API_URL=http://localhost:5001
ARG NEXT_PUBLIC_CONSOLIDADO_API_URL=http://localhost:5002
ARG NEXT_PUBLIC_MERCHANT_ID=00000000-0000-0000-0000-000000000001
ENV NEXT_PUBLIC_LANCAMENTOS_API_URL=$NEXT_PUBLIC_LANCAMENTOS_API_URL
ENV NEXT_PUBLIC_CONSOLIDADO_API_URL=$NEXT_PUBLIC_CONSOLIDADO_API_URL
ENV NEXT_PUBLIC_MERCHANT_ID=$NEXT_PUBLIC_MERCHANT_ID
COPY --from=deps /app/node_modules ./node_modules
COPY src/frontend ./
RUN npm run build

FROM node:24-alpine AS runner
WORKDIR /app
ENV NODE_ENV=production
ENV HOSTNAME=0.0.0.0
ENV PORT=3000
COPY --from=build /app/.next/standalone ./
COPY --from=build /app/.next/static ./.next/static
COPY --from=build /app/public ./public
EXPOSE 3000
CMD ["node", "server.js"]
