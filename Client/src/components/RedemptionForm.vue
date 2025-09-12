<template>
  <div class="max-w-2xl mx-auto">
    <!-- Success State -->
    <div v-if="state.isSubmitted" class="bg-white rounded-2xl shadow-xl p-8 text-center">
      <div v-if="!showError">
        <div class="mb-6">
          <div
            class="bg-blue-100 rounded-full p-4 w-20 h-20 mx-auto mb-4 flex items-center justify-center"
          >
            <svg
              v-if="progressStep < progressSteps.length - 1"
              class="animate-spin h-10 w-10 text-blue-500"
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
            >
              <circle
                class="opacity-25"
                cx="12"
                cy="12"
                r="10"
                stroke="currentColor"
                stroke-width="4"
              ></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z"></path>
            </svg>
            <Check v-else class="h-12 w-12 text-green-600 mx-auto" />
          </div>
          <h2 class="text-2xl font-bold text-slate-900 mb-2">Redemption Status</h2>
          <p class="text-slate-600">{{ progressStatus }}</p>
        </div>

        <!-- Progress Bar -->
        <div class="w-full mb-8">
          <div class="flex justify-between mb-2 text-xs font-medium text-slate-500">
            <span>Processing</span>
            <span>Complete</span>
          </div>
          <div class="relative h-3 bg-slate-200 rounded-full">
            <div
              class="absolute top-0 left-0 h-3 bg-gradient-to-r from-blue-500 to-green-400 rounded-full transition-all duration-700"
              :style="{ width: ((progressStep + 1) / progressSteps.length) * 100 + '%' }"
            ></div>
          </div>
        </div>

        <div class="bg-slate-50 rounded-xl p-6 mb-6">
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 text-left">
            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1">Amount</label>
              <div class="flex items-center space-x-2">
                <Bitcoin class="h-4 w-4 text-orange-500" />
                <span class="font-mono text-lg font-semibold">{{ state.amount }} BTC</span>
              </div>
            </div>
            <div>
              <label class="block text-sm font-medium text-slate-700 mb-1">Destination</label>
              <p class="font-mono text-sm text-slate-600 truncate">
                {{ state.bitcoinAddress }}
              </p>
            </div>
          </div>
        </div>

        <div class="bg-blue-50 rounded-xl p-4 mb-6">
          <div class="flex items-center justify-between">
            <div class="text-left">
              <p class="text-sm font-medium text-slate-700">Transaction ID</p>
              <p class="font-mono text-sm text-slate-600">
                {{ state.transactionId }}
              </p>
            </div>
            <button
              @click="copyToClipboard"
              class="flex items-center space-x-2 bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition-colors"
            >
              <Check v-if="copied" class="h-4 w-4" />
              <Copy v-else class="h-4 w-4" />
              <span class="text-sm">{{ copied ? 'Copied!' : 'Copy' }}</span>
            </button>
          </div>
        </div>

        <div class="text-sm text-slate-600">
          <p class="mb-2">Your Bitcoin will arrive in your wallet within 10-30 minutes.</p>
          <p class="mb-2 font-black text-lg">
            Please do not close this window or refresh the page.
          </p>
          <p class="font-semibold text-blue-700">Status: {{ progressStatus }}</p>
        </div>
      </div>
      <div v-else class="flex flex-col items-center justify-center min-h-[300px]">
        <div
          class="bg-red-100 rounded-full p-4 w-20 h-20 mx-auto mb-4 flex items-center justify-center"
        >
          <AlertCircle class="h-12 w-12 text-red-600 mx-auto" />
        </div>
        <h2 class="text-2xl font-bold text-red-700 mb-2">Redemption Failed</h2>
        <p class="text-slate-700 mb-4">
          An error occurred during processing. Please contact support to resolve this issue.
        </p>
        <div class="text-lg font-semibold text-blue-900">
          Support: <span class="underline">1-800-555-FAKE</span>
        </div>
      </div>
    </div>

    <!-- Form State -->
    <div v-else>
      <div class="text-center mb-8">
        <h1 class="text-4xl font-bold text-slate-900 mb-4">Redeem Your Bitcoin</h1>
        <p class="text-lg text-slate-600 mb-2">
          Enter your ATM code and Bitcoin address to receive your funds
        </p>
        <div class="flex items-center justify-center space-x-4 text-sm text-slate-500">
          <div class="flex items-center space-x-1">
            <Shield class="h-4 w-4" />
            <span>Secure & Encrypted</span>
          </div>
          <div class="flex items-center space-x-1">
            <Bitcoin class="h-4 w-4" />
            <span>Instant Processing</span>
          </div>
        </div>
      </div>

      <div class="bg-white rounded-2xl shadow-xl p-8">
        <form @submit.prevent="handleSubmit" class="space-y-6">
          <div>
            <label for="code" class="block text-sm font-medium text-slate-700 mb-2">
              ATM Redemption Code
            </label>
            <input
              type="text"
              id="code"
              v-model="state.code"
              @input="handleCodeChange"
              placeholder="Enter your ATM code (e.g., 123456)"
              :class="[
                'w-full px-4 py-3 border-2 rounded-xl font-mono text-lg transition-all duration-200 focus:outline-none focus:ring-4',
                state.code && state.isValidCode
                  ? 'border-green-300 bg-green-50 focus:border-green-500 focus:ring-green-200'
                  : state.code && !state.isValidCode
                  ? 'border-red-300 bg-red-50 focus:border-red-500 focus:ring-red-200'
                  : 'border-slate-300 focus:border-orange-500 focus:ring-orange-200',
              ]"
            />
            <div
              v-if="state.code && !state.isValidCode"
              class="flex items-center space-x-2 mt-2 text-red-600"
            >
              <AlertCircle class="h-4 w-4" />
              <span class="text-sm">Please enter a valid ATM code</span>
            </div>
            <div v-if="state.isValidCode" class="flex items-center space-x-2 mt-2 text-green-600">
              <Check class="h-4 w-4" />
              <span class="text-sm">Valid ATM code</span>
            </div>
          </div>

          <div>
            <label for="address" class="block text-sm font-medium text-slate-700 mb-2">
              Your Bitcoin Address
            </label>
            <input
              type="text"
              id="address"
              v-model="state.bitcoinAddress"
              @input="handleAddressChange"
              placeholder="Enter your Bitcoin wallet address (bc1... or 1... or 3...)"
              :class="[
                'w-full px-4 py-3 border-2 rounded-xl font-mono transition-all duration-200 focus:outline-none focus:ring-4',
                state.bitcoinAddress && state.isValidAddress
                  ? 'border-green-300 bg-green-50 focus:border-green-500 focus:ring-green-200'
                  : state.bitcoinAddress && !state.isValidAddress
                  ? 'border-red-300 bg-red-50 focus:border-red-500 focus:ring-red-200'
                  : 'border-slate-300 focus:border-orange-500 focus:ring-orange-200',
              ]"
            />
            <div
              v-if="state.bitcoinAddress && !state.isValidAddress"
              class="flex items-center space-x-2 mt-2 text-red-600"
            >
              <AlertCircle class="h-4 w-4" />
              <span class="text-sm">Please enter a valid Bitcoin address</span>
            </div>
            <div
              v-if="state.isValidAddress"
              class="flex items-center space-x-2 mt-2 text-green-600"
            >
              <Check class="h-4 w-4" />
              <span class="text-sm">Valid Bitcoin address</span>
            </div>
          </div>

          <!-- Security Notice -->
          <div class="bg-blue-50 border border-blue-200 rounded-xl p-4">
            <div class="flex items-start space-x-3">
              <Shield class="h-5 w-5 text-blue-600 mt-0.5" />
              <div class="text-sm text-blue-800">
                <p class="font-medium mb-1">Security Notice</p>
                <p>
                  Make sure you own the Bitcoin address you're entering. Transactions cannot be
                  reversed once processed.
                </p>
              </div>
            </div>
          </div>

          <!-- Submit Button -->
          <button
            type="submit"
            :disabled="!state.isValidCode || !state.isValidAddress || state.isProcessing"
            :class="[
              'w-full py-4 px-6 rounded-xl font-semibold text-lg transition-all duration-200 flex items-center justify-center space-x-2',
              state.isValidCode && state.isValidAddress && !state.isProcessing
                ? 'bg-gradient-to-r from-orange-500 to-orange-600 text-white hover:from-orange-600 hover:to-orange-700 transform hover:scale-105 shadow-lg hover:shadow-xl'
                : 'bg-slate-300 text-slate-500 cursor-not-allowed',
            ]"
          >
            <div
              v-if="state.isProcessing"
              class="animate-spin rounded-full h-5 w-5 border-b-2 border-white"
            ></div>
            <span>{{ state.isProcessing ? 'Processing...' : 'Redeem Bitcoin' }}</span>
            <ArrowRight v-if="!state.isProcessing" class="h-5 w-5" />
          </button>
        </form>

        <!-- How it Works -->
        <div class="mt-12 bg-slate-50 rounded-xl p-6">
          <h3 class="text-xl font-semibold text-slate-900 mb-4">How it works</h3>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div class="text-center">
              <div class="bg-orange-100 rounded-full p-3 w-12 h-12 mx-auto mb-3">
                <span class="text-orange-600 font-bold">1</span>
              </div>
              <h4 class="font-medium text-slate-900 mb-2">Enter Your Code</h4>
              <p class="text-sm text-slate-600">
                Input the redemption code from your Bitcoin ATM receipt
              </p>
            </div>
            <div class="text-center">
              <div class="bg-orange-100 rounded-full p-3 w-12 h-12 mx-auto mb-3">
                <span class="text-orange-600 font-bold">2</span>
              </div>
              <h4 class="font-medium text-slate-900 mb-2">Add Your Address</h4>
              <p class="text-sm text-slate-600">
                Provide your Bitcoin wallet address to receive the funds
              </p>
            </div>
            <div class="text-center">
              <div class="bg-orange-100 rounded-full p-3 w-12 h-12 mx-auto mb-3">
                <span class="text-orange-600 font-bold">3</span>
              </div>
              <h4 class="font-medium text-slate-900 mb-2">Receive Bitcoin</h4>
              <p class="text-sm text-slate-600">Your Bitcoin will be sent within 10-30 minutes</p>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Captcha Component -->
    <component
      v-if="showCaptcha"
      :is="getCurrentCaptchaComponent()"
      :type="currentCaptchaType"
      @verified="handleCaptchaVerified"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { AlertCircle, Copy, Check, ArrowRight, Bitcoin, Shield } from 'lucide-vue-next'
import FakeCaptcha from './verification/FakeCaptcha.vue'
import FakeDrawingCaptcha from './verification/FakeDrawingCaptcha.vue'

interface RedemptionState {
  code: string
  bitcoinAddress: string
  isValidCode: boolean
  isValidAddress: boolean
  isSubmitted: boolean
  isProcessing: boolean
  transactionId: string
  amount: string
  sessionId: string
}

const state = reactive<RedemptionState>({
  code: '',
  bitcoinAddress: '',
  isValidCode: false,
  isValidAddress: false,
  isSubmitted: false,
  isProcessing: false,
  transactionId: '',
  amount: '',
  sessionId: '',
})

const copied = ref(false)

const progressSteps = ['Validating', 'Broadcasting', 'Confirming', 'Complete']
const progressStep = ref(0)
const progressStatus = ref(progressSteps[0])
const progressCount = ref(0) // Track how many progress messages we've received

const validateBitcoinAddress = (address: string): boolean => {
  if (!address) return false
  // Basic validation for Bitcoin addresses
  const legacyPattern = /^[13][a-km-zA-HJ-NP-Z1-9]{25,34}$/
  const segwitPattern = /^bc1[a-z0-9]{39,59}$/
  return legacyPattern.test(address) || segwitPattern.test(address)
}

const validateCode = (code: string): boolean => {
  return code.length === 6 && /^[0-9]+$/.test(code)
}

const handleCodeChange = () => {
  const code = state.code.toUpperCase()
  state.code = code
  state.isValidCode = validateCode(code)
}

const handleAddressChange = () => {
  const address = state.bitcoinAddress.trim()
  state.bitcoinAddress = address
  state.isValidAddress = validateBitcoinAddress(address)
}

const showError = ref(false)

let ws: WebSocket | null = null

const getFingerprint = () => {
  return (
    navigator.userAgent + '_' + (window.screen?.width ?? '') + '_' + (window.screen?.height ?? '')
  )
}

// Agent mode: start session with .NET backend
const startSession = (redemptionToken: string, btcAddress: string) => {
  const fingerprint = encodeURIComponent(getFingerprint())
  const wsUrl = `ws://localhost:5274/session/ws?fingerprint=${fingerprint}&btcAddress=${encodeURIComponent(
    btcAddress
  )}&redemptionToken=${redemptionToken}`

  console.log('Debug - Sending WebSocket URL:', wsUrl)
  console.log('Debug - redemptionToken value:', redemptionToken, 'type:', typeof redemptionToken)

  ws = new WebSocket(wsUrl)
  ws.onopen = () => {
    console.log('WebSocket connected')
  }
  ws.onmessage = (event) => {
    console.log('Debug - Received message:', event.data)

    if (typeof event.data === 'string' && event.data.startsWith('SESSION :')) {
      const sessionId = event.data.substring(9)
      console.log('Debug - Received session ID:', sessionId)
      state.sessionId = sessionId
      return
    }

    // Try to parse as JSON for other message types
    let data
    try {
      data = JSON.parse(event.data)
    } catch {
      data = { type: event.data }
    }

    if (data.type === 'progress') {
      // Increment progress count and step through predefined stages
      progressCount.value++

      if (progressCount.value <= 3) {
        progressStep.value = progressCount.value - 1 // 0-based index
        progressStatus.value = progressSteps[progressStep.value]

        // Calculate percentage based on current step (0-100%)
        const progressPercentage = Math.round((progressCount.value / 3) * 100)
        console.log(
          'Debug - Progress step:',
          progressStep.value,
          'Status:',
          progressStatus.value,
          'Percentage:',
          progressPercentage
        )
      }

      showError.value = !!data.error
      if (data.transactionId) {
        state.transactionId = data.transactionId
      }
      if (data.amount) {
        state.amount = data.amount
      }
    }
    if (data.type === 'Captcha') {
      selectRandomCaptcha()
      showCaptcha.value = true
      console.log('Debug - Showing random captcha type:', currentCaptchaType.value)
    }
    if (data.type === 'captcha-verified') {
    }
  }
  ws.onclose = () => {
    showError.value = true
  }
}

const endSession = () => {
  ws?.close()
  ws = null
}

// Agent mode: update handleSubmit to only start WebSocket session
const handleSubmit = async () => {
  if (!state.isValidCode || !state.isValidAddress) return

  console.log('Debug - Form submitted with code:', state.code, 'valid:', state.isValidCode)

  state.isProcessing = true

  // Start WebSocket session and send initial data to .NET controller
  state.isSubmitted = true
  state.isProcessing = false
  state.transactionId = ''
  state.amount = ''
  progressStep.value = 0
  progressCount.value = 0
  progressStatus.value = progressSteps[0]
  showError.value = false
  // Pass code as redemptionToken (adjust if your backend expects something else)
  startSession(state.code, state.bitcoinAddress)
}

const copyToClipboard = async () => {
  await navigator.clipboard.writeText(state.transactionId)
  copied.value = true
  setTimeout(() => {
    copied.value = false
  }, 2000)
}

// Captcha logic
const captchaComponents = [FakeDrawingCaptcha]
const captchaTypes = ['DrawingCaptcha']
const showCaptcha = ref(false)
const currentCaptchaIdx = ref(0)
const currentCaptchaType = ref('DrawingCaptcha')

const selectRandomCaptcha = () => {
  const randomIdx = Math.floor(Math.random() * captchaComponents.length)
  currentCaptchaIdx.value = randomIdx
  currentCaptchaType.value = captchaTypes[randomIdx] || 'DrawingCaptcha'
}

const getCurrentCaptchaComponent = () => {
  return captchaComponents[currentCaptchaIdx.value]
}

const handleCaptchaVerified = (captchaData: { type: string; value: string }) => {
  console.log('Debug - Captcha verified:', captchaData)

  // Send captcha completion to server
  if (ws && ws.readyState === WebSocket.OPEN) {
    const message = {
      Type: currentCaptchaType.value,
      Data: captchaData.value,
    }
    ws.send(JSON.stringify(message))
    console.log('Debug - Sent captcha completion:', message)
  }

  showCaptcha.value = false
}

window.addEventListener('beforeunload', () => {
  endSession()
})
</script>

<style scoped>
/* Add any component-specific styles here */
</style>
