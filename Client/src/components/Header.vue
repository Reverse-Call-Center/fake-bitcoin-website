<template>
  <header class="bg-white shadow-sm border-b border-slate-200">
    <div class="container mx-auto px-4 py-6">
      <div class="flex items-center justify-between">
        <div class="flex items-center space-x-3">
          <div class="bg-gradient-to-r from-orange-500 to-orange-600 p-2 rounded-lg">
            <Bitcoin class="h-8 w-8 text-white" />
          </div>
          <div>
            <h1 class="text-2xl font-bold text-slate-900">BTM Redeem</h1>
            <p class="text-sm text-slate-600">Secure Bitcoin ATM Voucher Redemption</p>
          </div>
        </div>

        <div class="hidden md:flex items-center space-x-6">
          <div class="flex items-center space-x-2 text-sm text-slate-600">
            <Shield class="h-4 w-4 text-green-500" />
            <span>SSL Secured</span>
          </div>
          <div class="flex items-center space-x-2 text-sm text-slate-600">
            <CheckCircle class="h-4 w-4 text-green-500" />
            <span>Verified Platform</span>
          </div>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { Bitcoin, Shield, CheckCircle } from 'lucide-vue-next'
</script>
