<template>
  <div class="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
    <div class="bg-white rounded-xl shadow-2xl p-8 max-w-sm w-full text-center">
      <h2 class="text-xl font-bold text-slate-900 mb-4">Verification Required</h2>
      <p class="text-slate-600 mb-6">Please complete the captcha to continue your redemption.</p>
      <div class="mb-6">
        <!-- Fake captcha: simple math question -->
        <label class="block text-sm font-medium text-slate-700 mb-2"
          >What is {{ a }} + {{ b }}?</label
        >
        <input v-model="answer" type="number" class="w-full px-4 py-2 border rounded-lg text-lg" />
        <div v-if="error" class="text-red-600 text-sm mt-2">Incorrect, try again.</div>
      </div>
      <button
        @click="verify"
        class="bg-blue-600 text-white px-6 py-2 rounded-lg font-semibold hover:bg-blue-700 transition-colors"
      >
        Verify
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

interface Props {
  type?: string
}

const props = withDefaults(defineProps<Props>(), {
  type: 'math',
})

const a = Math.floor(Math.random() * 10) + 1
const b = Math.floor(Math.random() * 10) + 1
const answer = ref('')
const error = ref(false)

const emit = defineEmits<{
  verified: [{ type: string; value: string }]
}>()

const verify = () => {
  const userAnswer = parseInt(answer.value)
  if (userAnswer === a + b) {
    // Emit the captcha completion with type and answer as value
    emit('verified', {
      type: props.type,
      value: userAnswer.toString(),
    })
    error.value = false
    answer.value = ''
  } else {
    error.value = true
  }
}
</script>
