<script setup lang="ts">
import { ref, onMounted } from 'vue'
import UrlShortener from './components/UrlShortener.vue'

const isRedirecting = ref(false)

onMounted(() => {
  const path = window.location.pathname
  const code = path.slice(1) // Remove leading slash
  
  // Check if it's a short code (not empty and not a static file)
  if (code && !code.includes('.') && code.length > 0) {
    isRedirecting.value = true
    const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5250'
    // Redirect to backend to handle the redirect
    window.location.href = `${apiUrl}/${code}`
  }
})
</script>

<template>
  <div class="app">
    <div v-if="isRedirecting" class="redirect-message">
      <p>Redirecting...</p>
    </div>
    <UrlShortener v-else />
  </div>
</template>

<style>
.app {
  min-height: 100vh;
  width: 100vw;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.redirect-message {
  color: white;
  font-size: 1.5rem;
}
</style>
