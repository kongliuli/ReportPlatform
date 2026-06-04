<template>
  <div class="login-view">
    <div class="login-background">
      <div class="bg-shape bg-shape-1"></div>
      <div class="bg-shape bg-shape-2"></div>
      <div class="bg-shape bg-shape-3"></div>
      <div class="bg-grid"></div>
    </div>

    <div class="login-container">
      <div class="login-card animate-slide-up">
        <div class="login-header">
          <div class="logo-wrapper">
            <div class="logo-icon">
              <svg viewBox="0 0 32 32" fill="none" xmlns="http://www.w3.org/2000/svg">
                <rect width="32" height="32" rx="8" fill="currentColor"/>
                <path d="M8 12h16M8 16h12M8 20h8" stroke="white" stroke-width="2" stroke-linecap="round"/>
              </svg>
            </div>
            <span class="logo-text">{{ $t('app.brand') }}</span>
          </div>
          <h1 class="login-title">{{ $t('login.title') }}</h1>
          <p class="login-subtitle">{{ $t('login.subtitle') }}</p>
        </div>

        <el-form
          ref="formRef"
          :model="form"
          :rules="rules"
          @submit.prevent="handleLogin"
          class="login-form"
        >
          <el-form-item prop="username">
            <div class="form-label">{{ $t('login.username') }}</div>
            <el-input
              v-model="form.username"
              :placeholder="$t('login.usernamePlaceholder')"
              size="large"
              :prefix-icon="User"
            />
          </el-form-item>

          <el-form-item prop="password">
            <div class="form-label">{{ $t('login.password') }}</div>
            <el-input
              v-model="form.password"
              type="password"
              :placeholder="$t('login.passwordPlaceholder')"
              size="large"
              :prefix-icon="Lock"
              show-password
            />
          </el-form-item>

          <el-form-item class="form-actions">
            <el-button
              type="primary"
              native-type="submit"
              :loading="loading"
              size="large"
              class="login-button"
            >
              <span v-if="!loading">{{ $t('login.loginButton') }}</span>
              <span v-else>{{ $t('login.loggingIn') }}</span>
            </el-button>
          </el-form-item>
        </el-form>

        <div class="login-footer">
          <p>{{ $t('login.footer') }}</p>
        </div>
      </div>

      <div class="login-info animate-slide-up" style="animation-delay: 100ms">
        <div class="info-content">
          <h2>{{ $t('features.heading') }}</h2>
          <ul class="feature-list">
            <li>
              <div class="feature-icon">
                <el-icon><Document /></el-icon>
              </div>
              <div class="feature-text">
                <strong>{{ $t('features.editor') }}</strong>
                <span>{{ $t('features.editorDesc') }}</span>
              </div>
            </li>
            <li>
              <div class="feature-icon">
                <el-icon><Clock /></el-icon>
              </div>
              <div class="feature-text">
                <strong>{{ $t('features.version') }}</strong>
                <span>{{ $t('features.versionDesc') }}</span>
              </div>
            </li>
            <li>
              <div class="feature-icon">
                <el-icon><Connection /></el-icon>
              </div>
              <div class="feature-text">
                <strong>{{ $t('features.databinding') }}</strong>
                <span>{{ $t('features.databindingDesc') }}</span>
              </div>
            </li>
            <li>
              <div class="feature-icon">
                <el-icon><Download /></el-icon>
              </div>
              <div class="feature-text">
                <strong>{{ $t('features.export') }}</strong>
                <span>{{ $t('features.exportDesc') }}</span>
              </div>
            </li>
          </ul>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { ElMessage } from 'element-plus'
import { User, Lock, Document, Clock, Connection, Download } from '@element-plus/icons-vue'
import $t from '@/locales/zh-CN'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const formRef = ref()
const loading = ref(false)

const form = reactive({
  username: '',
  password: ''
})

const rules = {
  username: [{ required: true, message: $t('login.usernameRequired'), trigger: 'blur' }],
  password: [{ required: true, message: $t('login.passwordRequired'), trigger: 'blur' }]
}

const handleLogin = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  loading.value = true
  try {
    await authStore.login({
      username: form.username,
      password: form.password
    })
    ElMessage.success($t('login.loginSuccess'))
    const redirect = route.query.redirect || '/'
    router.push(redirect)
  } catch (error) {
    ElMessage.error(error.message || $t('login.loginFailed'))
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-view {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-bg-primary);
  position: relative;
  overflow: hidden;
}

.login-background {
  position: absolute;
  inset: 0;
  overflow: hidden;
  pointer-events: none;
}

.bg-shape {
  position: absolute;
  border-radius: 50%;
  filter: blur(80px);
  opacity: 0.5;
}

.bg-shape-1 {
  width: 600px;
  height: 600px;
  background: linear-gradient(135deg, var(--color-primary-bg) 0%, var(--color-accent-bg) 100%);
  top: -200px;
  right: -100px;
  animation: float 20s ease-in-out infinite;
}

.bg-shape-2 {
  width: 400px;
  height: 400px;
  background: linear-gradient(135deg, var(--color-info-bg) 0%, var(--color-primary-bg) 100%);
  bottom: -100px;
  left: -100px;
  animation: float 15s ease-in-out infinite reverse;
}

.bg-shape-3 {
  width: 300px;
  height: 300px;
  background: linear-gradient(135deg, var(--color-accent-bg) 0%, var(--color-success-bg) 100%);
  top: 50%;
  left: 30%;
  animation: float 18s ease-in-out infinite;
}

@keyframes float {
  0%, 100% { transform: translate(0, 0) scale(1); }
  33% { transform: translate(30px, -30px) scale(1.05); }
  66% { transform: translate(-20px, 20px) scale(0.95); }
}

.bg-grid {
  position: absolute;
  inset: 0;
  background-image: 
    linear-gradient(var(--color-border-light) 1px, transparent 1px),
    linear-gradient(90deg, var(--color-border-light) 1px, transparent 1px);
  background-size: 60px 60px;
  opacity: 0.5;
}

.login-container {
  display: flex;
  gap: var(--spacing-12);
  align-items: center;
  position: relative;
  z-index: 1;
  padding: var(--spacing-8);
}

.login-card {
  width: 420px;
  background: var(--color-bg-secondary);
  border-radius: var(--radius-2xl);
  box-shadow: var(--shadow-xl);
  padding: var(--spacing-10);
  border: 1px solid var(--color-border-light);
}

.login-header {
  text-align: center;
  margin-bottom: var(--spacing-8);
}

.logo-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--spacing-3);
  margin-bottom: var(--spacing-6);
}

.logo-icon {
  width: 40px;
  height: 40px;
  color: var(--color-primary);
}

.logo-icon svg {
  width: 100%;
  height: 100%;
}

.logo-text {
  font-size: var(--font-size-xl);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
}

.login-title {
  font-size: var(--font-size-3xl);
  font-weight: var(--font-weight-bold);
  color: var(--color-text-primary);
  margin: 0 0 var(--spacing-2) 0;
  letter-spacing: var(--letter-spacing-tight);
}

.login-subtitle {
  font-size: var(--font-size-base);
  color: var(--color-text-tertiary);
  margin: 0;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: var(--spacing-4);
}

.form-label {
  font-size: var(--font-size-sm);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-secondary);
  margin-bottom: var(--spacing-2);
}

.form-actions {
  margin-top: var(--spacing-4);
}

.login-button {
  width: 100%;
  height: 48px;
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-semibold);
  border-radius: var(--radius-lg);
  transition: all var(--transition-base);
}

.login-button:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: var(--shadow-glow);
}

.login-footer {
  margin-top: var(--spacing-8);
  text-align: center;
}

.login-footer p {
  font-size: var(--font-size-sm);
  color: var(--color-text-muted);
  margin: 0;
}

.login-info {
  width: 380px;
}

.info-content {
  padding: var(--spacing-8);
}

.info-content h2 {
  font-size: var(--font-size-2xl);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
  margin: 0 0 var(--spacing-6) 0;
}

.feature-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: var(--spacing-5);
}

.feature-list li {
  display: flex;
  align-items: flex-start;
  gap: var(--spacing-4);
}

.feature-icon {
  width: 44px;
  height: 44px;
  background: var(--color-primary-bg);
  border-radius: var(--radius-lg);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-primary);
  flex-shrink: 0;
}

.feature-icon .el-icon {
  font-size: 20px;
}

.feature-text {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.feature-text strong {
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-primary);
}

.feature-text span {
  font-size: var(--font-size-sm);
  color: var(--color-text-tertiary);
}

@media (max-width: 900px) {
  .login-info {
    display: none;
  }
  
  .login-container {
    padding: var(--spacing-4);
  }
  
  .login-card {
    width: 100%;
    max-width: 420px;
    padding: var(--spacing-6);
  }
}

:deep(.el-input__wrapper) {
  padding: 0 16px;
  height: 48px;
}

:deep(.el-input__inner) {
  font-size: var(--font-size-base);
}

:deep(.el-form-item) {
  margin-bottom: 0;
}

:deep(.el-form-item__error) {
  padding-top: 4px;
}
</style>
