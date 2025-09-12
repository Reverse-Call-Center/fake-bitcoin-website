<template>
  <footer class="bg-slate-900 text-white mt-16">
    <div class="container mx-auto px-4 py-12">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
        <div>
          <h3 class="text-lg font-semibold mb-4">Security & Trust</h3>
          <div class="space-y-3">
            <div class="flex items-center space-x-2 text-slate-300">
              <Shield class="h-4 w-4" />
              <span class="text-sm">256-bit SSL Encryption</span>
            </div>
            <div class="flex items-center space-x-2 text-slate-300">
              <Lock class="h-4 w-4" />
              <span class="text-sm">PCI DSS Compliant</span>
            </div>
            <div class="flex items-center space-x-2 text-slate-300">
              <Shield class="h-4 w-4" />
              <span class="text-sm">Cold Storage Security</span>
            </div>
          </div>
        </div>

        <div>
          <h3 class="text-lg font-semibold mb-4">Support</h3>
          <div class="space-y-3">
            <a href="#" class="flex items-center space-x-2 text-slate-300 hover:text-white transition-colors">
              <HelpCircle class="h-4 w-4" />
              <span class="text-sm">Help Center</span>
            </a>
            <a href="#" class="block text-sm text-slate-300 hover:text-white transition-colors">Contact Support</a>
            <a href="#" class="block text-sm text-slate-300 hover:text-white transition-colors">Transaction Status</a>
          </div>
        </div>

        <div>
          <h3 class="text-lg font-semibold mb-4">Important Notice</h3>
          <p class="text-sm text-slate-300 leading-relaxed">
            Always verify your Bitcoin address before submitting. Transactions are irreversible and we cannot recover funds sent to incorrect addresses.
          </p>
        </div>
      </div>

      <div class="border-t border-slate-800 mt-8 pt-8 text-center">
        <p class="text-sm text-slate-400">
          © 2025 BTM Redeem. All rights reserved. |
          <a href="#" class="hover:text-white transition-colors ml-1">Privacy Policy</a> |
          <a href="#" class="hover:text-white transition-colors ml-1">Terms of Service</a>
        </p>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { Shield, Lock, HelpCircle } from 'lucide-vue-next'
</script>
