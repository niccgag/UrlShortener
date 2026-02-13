<script setup lang="ts">
import { ref } from 'vue';
import { shortenUrl } from '../services/api';

const longUrl = ref('');
const shortUrl = ref('');
const error = ref('');
const isLoading = ref(false);
const copied = ref(false);

const handleSubmit = async () => {
  error.value = '';
  shortUrl.value = '';
  copied.value = false;
  
  if (!longUrl.value.trim()) {
    error.value = 'Please enter a URL';
    return;
  }
  
  isLoading.value = true;
  
  try {
    const result = await shortenUrl(longUrl.value.trim());
    shortUrl.value = result.replace(/^"|"$/g, '');
  } catch (err: any) {
    if (err.response?.status === 400) {
      error.value = 'Invalid URL. Please enter a valid HTTP or HTTPS URL.';
    } else {
      error.value = 'Failed to shorten URL. Please try again.';
    }
  } finally {
    isLoading.value = false;
  }
};

const copyToClipboard = async () => {
  try {
    await navigator.clipboard.writeText(shortUrl.value);
    copied.value = true;
    setTimeout(() => {
      copied.value = false;
    }, 2000);
  } catch {
    error.value = 'Failed to copy to clipboard';
  }
};
</script>

<template>
  <div class="url-shortener">
    <div class="card">
      <h1>URL Shortener</h1>
      <p class="subtitle">Transform your long URLs into short, shareable links</p>
      
      <form @submit.prevent="handleSubmit" class="form">
        <div class="input-group">
          <input
            v-model="longUrl"
            type="text"
            placeholder="Enter your long URL here..."
            class="url-input"
            :disabled="isLoading"
          />
          <button
            type="submit"
            class="submit-btn"
            :disabled="isLoading || !longUrl.trim()"
          >
            {{ isLoading ? 'Shortening...' : 'Shorten URL' }}
          </button>
        </div>
      </form>
      
      <div v-if="error" class="error-message">
        {{ error }}
      </div>
      
      <div v-if="shortUrl" class="result">
        <h3>Your shortened URL:</h3>
        <div class="short-url-container">
          <a :href="shortUrl" target="_blank" class="short-url">{{ shortUrl }}</a>
          <button @click="copyToClipboard" class="copy-btn" :class="{ copied }">
            {{ copied ? 'Copied!' : 'Copy' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.url-shortener {
  width: 100%;
  display: flex;
  justify-content: center;
}

.card {
  background: white;
  border-radius: 20px;
  padding: 4rem 6rem;
  box-shadow: 0 25px 80px rgba(0, 0, 0, 0.35);
  text-align: center;
  width: 80vw;
  max-width: 1400px;
  min-width: 600px;
}

h1 {
  color: #213547;
  font-size: clamp(2.5rem, 5vw, 4rem);
  margin-bottom: 1rem;
  font-weight: 700;
}

.subtitle {
  color: #666;
  margin-bottom: 3rem;
  font-size: clamp(1.1rem, 2vw, 1.5rem);
}

.form {
  margin-bottom: 2rem;
}

.input-group {
  display: flex;
  gap: 1.5rem;
  justify-content: center;
  width: 100%;
}

.url-input {
  flex: 1;
  padding: 1.25rem 2rem;
  font-size: clamp(1rem, 1.5vw, 1.3rem);
  border: 2px solid #e0e0e0;
  border-radius: 12px;
  transition: border-color 0.3s, box-shadow 0.3s;
  min-width: 0;
  width: 100%;
}

.url-input:focus {
  outline: none;
  border-color: #42b883;
  box-shadow: 0 0 0 4px rgba(66, 184, 131, 0.15);
}

.url-input:disabled {
  background-color: #f5f5f5;
  cursor: not-allowed;
}

.submit-btn {
  padding: 1.25rem 3rem;
  font-size: clamp(1rem, 1.5vw, 1.3rem);
  font-weight: 600;
  color: white;
  background-color: #42b883;
  border: none;
  border-radius: 12px;
  cursor: pointer;
  transition: background-color 0.3s, transform 0.2s;
  white-space: nowrap;
}

.submit-btn:hover:not(:disabled) {
  background-color: #369870;
  transform: translateY(-2px);
}

.submit-btn:disabled {
  background-color: #a0d4b8;
  cursor: not-allowed;
}

.error-message {
  color: #e74c3c;
  background-color: #fdf2f2;
  padding: 1.25rem 2rem;
  border-radius: 10px;
  margin-bottom: 2rem;
  font-size: clamp(1rem, 1.5vw, 1.2rem);
}

.result {
  background-color: #f0f9f4;
  padding: 2.5rem;
  border-radius: 15px;
  border: 2px solid #42b883;
  margin-top: 2rem;
}

.result h3 {
  color: #213547;
  margin-bottom: 1.5rem;
  font-size: clamp(1.1rem, 2vw, 1.4rem);
}

.short-url-container {
  display: flex;
  gap: 1.5rem;
  justify-content: center;
  align-items: center;
  flex-wrap: wrap;
}

.short-url {
  color: #42b883;
  font-size: clamp(1.2rem, 2vw, 1.6rem);
  font-weight: 600;
  text-decoration: none;
  word-break: break-all;
}

.short-url:hover {
  text-decoration: underline;
}

.copy-btn {
  padding: 1rem 2rem;
  font-size: clamp(0.9rem, 1.5vw, 1.1rem);
  color: #42b883;
  background-color: white;
  border: 2px solid #42b883;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.3s;
  white-space: nowrap;
}

.copy-btn:hover {
  background-color: #42b883;
  color: white;
}

.copy-btn.copied {
  background-color: #42b883;
  color: white;
}

@media (max-width: 1200px) {
  .card {
    width: 90vw;
    padding: 3rem 4rem;
  }
}

@media (max-width: 768px) {
  .card {
    width: 95vw;
    padding: 2rem;
    min-width: auto;
  }
  
  .input-group {
    flex-direction: column;
  }
  
  .submit-btn {
    width: 100%;
  }
}
</style>
