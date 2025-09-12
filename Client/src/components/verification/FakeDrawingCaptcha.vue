<template>
  <div class="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
    <div class="bg-white rounded-xl shadow-2xl p-8 max-w-lg w-full text-center">
      <h2 class="text-xl font-bold text-slate-900 mb-4">Drawing Verification</h2>
      <p class="text-slate-600 mb-2">
        Please draw: <strong>{{ currentPrompt }}</strong>
      </p>
      <p class="text-sm text-slate-500 mb-6">
        Use your mouse or finger to draw on the canvas below
      </p>

      <div class="mb-6 border-2 border-slate-300 rounded-lg p-4 bg-slate-50">
        <canvas
          ref="canvasRef"
          width="400"
          height="200"
          class="border border-slate-200 bg-white rounded cursor-crosshair mx-auto block"
          @mousedown="startDrawing"
          @mousemove="draw"
          @mouseup="stopDrawing"
          @mouseleave="stopDrawing"
          @touchstart="startDrawing"
          @touchmove="draw"
          @touchend="stopDrawing"
        ></canvas>
      </div>

      <div class="flex gap-4 justify-center">
        <button
          @click="clearCanvas"
          class="bg-slate-500 text-white px-4 py-2 rounded-lg font-semibold hover:bg-slate-600 transition-colors"
        >
          Clear
        </button>
        <button
          @click="submitDrawing"
          :disabled="!hasDrawn"
          class="bg-blue-600 text-white px-6 py-2 rounded-lg font-semibold hover:bg-blue-700 transition-colors disabled:bg-slate-400 disabled:cursor-not-allowed"
        >
          Submit Drawing
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'

interface Props {
  type?: string
}

const props = withDefaults(defineProps<Props>(), {
  type: 'drawing',
})

// List of silly things to draw
const drawingPrompts = [
  'a cat wearing a hat',
  'a dog with sunglasses',
  'a happy banana',
  'a house with a smiley face',
  'a car with wings',
  'a tree with googly eyes',
  'a fish riding a bicycle',
  'a robot dancing',
  'a unicorn eating pizza',
  'a dinosaur wearing a bow tie',
  'a cloud with a mustache',
  'a hamburger with legs',
  'a superhero sandwich',
  'a dancing taco',
  'a grumpy donut',
]

const canvasRef = ref<HTMLCanvasElement>()
const isDrawing = ref(false)
const hasDrawn = ref(false)
const currentPrompt = ref('')

const emit = defineEmits<{
  verified: [{ type: string; value: string }]
}>()

onMounted(() => {
  // Select a random drawing prompt
  const randomIndex = Math.floor(Math.random() * drawingPrompts.length)
  currentPrompt.value = drawingPrompts[randomIndex] || 'a happy face'

  // Setup canvas
  const canvas = canvasRef.value
  if (canvas) {
    const ctx = canvas.getContext('2d')
    if (ctx) {
      ctx.lineCap = 'round'
      ctx.lineJoin = 'round'
      ctx.lineWidth = 2
      ctx.strokeStyle = '#334155' // slate-700
    }
  }
})

const getEventPos = (event: MouseEvent | TouchEvent) => {
  const canvas = canvasRef.value
  if (!canvas) return { x: 0, y: 0 }

  const rect = canvas.getBoundingClientRect()

  if ('touches' in event) {
    // Touch event
    const touch = event.touches[0] || event.changedTouches[0]
    if (touch) {
      return {
        x: touch.clientX - rect.left,
        y: touch.clientY - rect.top,
      }
    }
    return { x: 0, y: 0 }
  } else {
    // Mouse event
    return {
      x: event.clientX - rect.left,
      y: event.clientY - rect.top,
    }
  }
}

const startDrawing = (event: MouseEvent | TouchEvent) => {
  event.preventDefault()
  isDrawing.value = true
  hasDrawn.value = true

  const canvas = canvasRef.value
  const ctx = canvas?.getContext('2d')
  if (ctx) {
    const pos = getEventPos(event)
    ctx.beginPath()
    ctx.moveTo(pos.x, pos.y)
  }
}

const draw = (event: MouseEvent | TouchEvent) => {
  if (!isDrawing.value) return
  event.preventDefault()

  const canvas = canvasRef.value
  const ctx = canvas?.getContext('2d')
  if (ctx) {
    const pos = getEventPos(event)
    ctx.lineTo(pos.x, pos.y)
    ctx.stroke()
  }
}

const stopDrawing = () => {
  isDrawing.value = false
}

const clearCanvas = () => {
  const canvas = canvasRef.value
  const ctx = canvas?.getContext('2d')
  if (ctx && canvas) {
    ctx.clearRect(0, 0, canvas.width, canvas.height)
    hasDrawn.value = false
  }
}

const submitDrawing = () => {
  const canvas = canvasRef.value
  if (canvas && hasDrawn.value) {
    // Convert canvas to base64
    const base64Image = canvas.toDataURL('image/png')

    // Emit the captcha completion with type and base64 image as value
    emit('verified', {
      type: props.type,
      value: base64Image,
    })
  }
}
</script>

<style scoped>
/* Prevent touch actions on canvas for better drawing experience */
canvas {
  touch-action: none;
}
</style>
